using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StoneSharp.Core.Tools.BuiltIn;

/// <summary>
/// 安全的进程运行器，提供健壮的进程管理和资源清理
/// </summary>
public sealed class SafeProcessRunner : IDisposable
{
    private Process? _process;
    private bool _disposed;
    private readonly StringBuilder _outputBuilder = new();
    private readonly StringBuilder _errorBuilder = new();
    private readonly object _lockObject = new();
    private bool _outputReadingComplete;
    private bool _errorReadingComplete;
    
    /// <summary>
    /// 执行命令并等待完成
    /// </summary>
    public async Task<ProcessResult> RunAsync(
        ProcessStartInfo startInfo,
        int timeoutMilliseconds,
        CancellationToken cancellationToken = default)
    {
        if (startInfo == null)
            throw new ArgumentNullException(nameof(startInfo));

        _process = new Process
        {
            StartInfo = startInfo
        };

        // 配置进程输出重定向
        if (!startInfo.UseShellExecute)
        {
            _process.StartInfo.RedirectStandardOutput = true;
            _process.StartInfo.RedirectStandardError = true;
            _process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            _process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
        }

        try
        {
            // 启动进程
            if (!_process.Start())
            {
                throw new InvalidOperationException("无法启动进程");
            }

            // 开始异步读取输出
            if (!startInfo.UseShellExecute)
            {
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();
                _process.OutputDataReceived += OnOutputDataReceived;
                _process.ErrorDataReceived += OnErrorDataReceived;
            }

            // 等待进程退出或超时
            var exitTask = WaitForExitAsync(cancellationToken);
            var timeoutTask = Task.Delay(timeoutMilliseconds, cancellationToken);
            
            var completedTask = await Task.WhenAny(exitTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                // 超时，强制终止进程
                await KillProcessTreeAsync();
                
                // 等待进程完全退出
                await WaitForProcessExitAsync(TimeSpan.FromSeconds(5));
                
                return new ProcessResult
                {
                    ExitCode = -1,
                    Output = _outputBuilder.ToString(),
                    Error = _errorBuilder.ToString(),
                    TimedOut = true,
                    ProcessId = _process.Id
                };
            }
            else
            {
                // 进程正常退出
                var exitCode = await exitTask;
                
                // 等待输出读取完成
                await WaitForOutputReadingCompleteAsync(TimeSpan.FromSeconds(2));
                
                return new ProcessResult
                {
                    ExitCode = exitCode,
                    Output = _outputBuilder.ToString(),
                    Error = _errorBuilder.ToString(),
                    TimedOut = false,
                    ProcessId = _process.Id
                };
            }
        }
        finally
        {
            CleanupAsync().Wait(); // 同步等待清理完成
        }
    }

    /// <summary>
    /// 异步等待进程退出
    /// </summary>
    private async Task<int> WaitForExitAsync(CancellationToken cancellationToken)
    {
        if (_process == null)
            throw new InvalidOperationException("进程未启动");

        var tcs = new TaskCompletionSource<int>();
        
        // 注册退出事件
        _process.Exited += (sender, args) =>
        {
            tcs.TrySetResult(_process.ExitCode);
        };
        
        // 启用退出事件
        _process.EnableRaisingEvents = true;

        // 如果进程已经退出，直接返回退出代码
        if (_process.HasExited)
        {
            return _process.ExitCode;
        }

        // 等待退出事件或取消
        using (cancellationToken.Register(() => tcs.TrySetCanceled()))
        {
            return await tcs.Task;
        }
    }

    /// <summary>
    /// 杀死进程树（包括所有子进程）
    /// </summary>
    private async Task KillProcessTreeAsync()
    {
        if (_process == null || _process.HasExited)
            return;

        try
        {
            // 首先尝试优雅关闭
            if (!_process.CloseMainWindow())
            {
                // 如果无法优雅关闭，强制终止进程树
                await KillProcessTreeInternalAsync(_process.Id);
            }

            // 等待进程退出
            await WaitForProcessExitAsync(TimeSpan.FromSeconds(5));
        }
        catch (InvalidOperationException)
        {
            // 进程可能已经退出
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"杀死进程时出错: {ex.Message}");
        }
    }

    /// <summary>
    /// 递归杀死进程树（包括所有子进程）
    /// </summary>
    private async Task KillProcessTreeInternalAsync(int parentProcessId)
    {
        try
        {
            // 获取所有子进程
            var childProcesses = GetChildProcesses(parentProcessId);
            
            // 先递归杀死所有子进程
            foreach (var childProcessId in childProcesses)
            {
                await KillProcessTreeInternalAsync(childProcessId);
            }

            // 如果不是当前管理的进程，才需要单独杀死
            if (_process == null || _process.Id != parentProcessId)
            {
                // 杀死当前进程
                try
                {
                    using var process = Process.GetProcessById(parentProcessId);
                    if (!process.HasExited)
                    {
                        process.Kill();
                        process.WaitForExit(1000);
                    }
                }
                catch (ArgumentException)
                {
                    // 进程可能已经不存在了
                }
                catch (InvalidOperationException)
                {
                    // 进程可能已经退出
                }
            }
            else
            {
                // 如果是当前管理的进程，直接杀死
                if (_process != null && !_process.HasExited)
                {
                    try
                    {
                        _process.Kill();
                        _process.WaitForExit(1000);
                    }
                    catch (InvalidOperationException)
                    {
                        // 进程可能已经退出
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"杀死进程树时出错: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取指定进程的所有子进程ID
    /// </summary>
    private List<int> GetChildProcesses(int parentProcessId)
    {
        var childProcesses = new List<int>();
        
        try
        {
            // 使用WMI查询获取指定父进程的所有子进程
            string query = $"SELECT ProcessId FROM Win32_Process WHERE ParentProcessId = {parentProcessId}";
            
            using var searcher = new ManagementObjectSearcher(query);
            using var results = searcher.Get();
            
            foreach (ManagementObject process in results)
            {
                var processId = Convert.ToInt32(process["ProcessId"]);
                childProcesses.Add(processId);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"获取子进程时出错: {ex.Message}");
        }
        
        return childProcesses;
    }

    /// <summary>
    /// 等待进程退出
    /// </summary>
    private async Task WaitForProcessExitAsync(TimeSpan timeout)
    {
        if (_process == null || _process.HasExited)
            return;

        try
        {
            var waitTask = Task.Run(() => _process.WaitForExit((int)timeout.TotalMilliseconds));
            await waitTask;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"等待进程退出时出错: {ex.Message}");
        }
    }

    /// <summary>
    /// 等待输出读取完成
    /// </summary>
    private async Task WaitForOutputReadingCompleteAsync(TimeSpan timeout)
    {
        var startTime = DateTime.UtcNow;
        
        while (DateTime.UtcNow - startTime < timeout)
        {
            lock (_lockObject)
            {
                if (_outputReadingComplete && _errorReadingComplete)
                    return;
            }
            
            await Task.Delay(50);
        }
    }

    /// <summary>
    /// 输出数据接收事件处理
    /// </summary>
    private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data == null)
        {
            lock (_lockObject)
            {
                _outputReadingComplete = true;
            }
            return;
        }

        lock (_lockObject)
        {
            _outputBuilder.AppendLine(e.Data);
        }
    }

    /// <summary>
    /// 错误数据接收事件处理
    /// </summary>
    private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data == null)
        {
            lock (_lockObject)
            {
                _errorReadingComplete = true;
            }
            return;
        }

        lock (_lockObject)
        {
            _errorBuilder.AppendLine(e.Data);
        }
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    private async Task CleanupAsync()
    {
        if (_process != null)
        {
            try
            {
                // 取消事件处理程序
                if (!_process.StartInfo.UseShellExecute)
                {
                    _process.OutputDataReceived -= OnOutputDataReceived;
                    _process.ErrorDataReceived -= OnErrorDataReceived;
                }

                // 确保进程已退出
                if (!_process.HasExited)
                {
                    try
                    {
                        // 杀死整个进程树
                        await KillProcessTreeInternalAsync(_process.Id);
                        _process.WaitForExit(1000);
                    }
                    catch
                    {
                        // 忽略杀死进程时的异常
                    }
                }

                // 释放资源
                _process.Dispose();
            }
            catch
            {
                // 忽略清理时的异常
            }
            finally
            {
                _process = null;
            }
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            CleanupAsync().Wait(); // 同步等待清理完成
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 进程执行结果
    /// </summary>
    public class ProcessResult
    {
        public int ExitCode { get; set; }
        public string Output { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        public bool TimedOut { get; set; }
        public int ProcessId { get; set; }
    }
}