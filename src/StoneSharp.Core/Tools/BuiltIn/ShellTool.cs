using Microsoft.SemanticKernel;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StoneSharp.Core.Tools.BuiltIn;

/// <summary>
/// 命令行插件，提供执行命令行操作的功能
/// </summary>
public sealed class ShellTool
{
    private const int DefaultTimeoutSeconds = 30;
    private const int MaxOutputLength = 8192; // 8KB 最大输出限制

    /// <summary>
    /// 执行简单的命令行命令
    /// </summary>
    /// <param name="command">要执行的命令</param>
    /// <param name="workingDirectory">工作目录（可选，默认为当前目录）</param>
    /// <param name="timeoutSeconds">超时时间（秒，可选，默认为30秒）</param>
    /// <param name="showWindow">是否显示控制台窗口（可选，默认为false）</param>
    /// <param name="waitForExit">是否等待进程结束（可选，默认为true）</param>
    /// <returns>命令执行结果</returns>
    [KernelFunction, Description("执行简单的命令行命令，环境默认为Windows")]
    public async Task<string> ExecuteCommandAsync(
        [Description("要执行的命令")] string command,
        [Description("工作目录（可选，默认为当前目录）")] string workingDirectory = "",
        [Description("超时时间（秒，可选，默认为30秒）")] int timeoutSeconds = DefaultTimeoutSeconds,
        [Description("是否显示控制台窗口（可选，默认为false）")] bool showWindow = false,
        [Description("是否等待进程结束（可选，默认为true）")] bool waitForExit = true)
    {
        if (string.IsNullOrEmpty(command))
        {
            return "命令不能为空";
        }

        if (string.IsNullOrEmpty(workingDirectory))
        {
            workingDirectory = Directory.GetCurrentDirectory();
        }

        if (!Directory.Exists(workingDirectory))
        {
            return $"工作目录不存在: {workingDirectory}";
        }

        try
        {
            var result = new StringBuilder();
            result.AppendLine($"执行命令: {command}");
            result.AppendLine($"工作目录: {workingDirectory}");
            result.AppendLine();

            var startInfo = new ProcessStartInfo
            {
                FileName = GetShellExecutable(),
                Arguments = GetShellArguments(command),
                WorkingDirectory = workingDirectory,
                UseShellExecute = showWindow,  // 如果显示窗口，使用Shell执行
                CreateNoWindow = !showWindow
            };

            // 如果需要显示窗口，则不等待退出
            if (!waitForExit && !showWindow)
            {
                // 不等待退出且不显示窗口，使用Shell执行
                startInfo.UseShellExecute = true;
            }

            using var processRunner = new SafeProcessRunner();
            
            if (waitForExit)
            {
                // 设置超时时间（秒转毫秒）
                int timeoutMilliseconds = timeoutSeconds * 1000;
                
                // 使用安全的进程运行器执行命令
                var processResult = await processRunner.RunAsync(
                    startInfo,
                    timeoutMilliseconds,
                    CancellationToken.None);

                if (processResult.TimedOut)
                {
                    result.AppendLine($"命令执行超时（{timeoutSeconds}秒），进程已强制终止");
                    result.AppendLine($"进程ID: {processResult.ProcessId}");
                    
                    if (!string.IsNullOrEmpty(processResult.Output))
                    {
                        result.AppendLine();
                        result.AppendLine("超时前已获取的输出:");
                        result.AppendLine("```");
                        result.AppendLine(TruncateOutput(processResult.Output));
                        result.AppendLine("```");
                    }
                    
                    return result.ToString();
                }

                string output = showWindow ? "（输出在窗口中显示）" : TruncateOutput(processResult.Output);
                string error = showWindow ? "（错误在窗口中显示）" : TruncateOutput(processResult.Error);

                result.AppendLine($"退出代码: {processResult.ExitCode}");
                result.AppendLine($"进程ID: {processResult.ProcessId}");
                result.AppendLine();

                if (!string.IsNullOrEmpty(output) && output != "（输出在窗口中显示）")
                {
                    result.AppendLine("标准输出:");
                    result.AppendLine("```");
                    result.AppendLine(output);
                    result.AppendLine("```");
                    result.AppendLine();
                }

                if (!string.IsNullOrEmpty(error) && error != "（错误在窗口中显示）")
                {
                    result.AppendLine("标准错误:");
                    result.AppendLine("```");
                    result.AppendLine(error);
                    result.AppendLine("```");
                }

                if (string.IsNullOrEmpty(output) && string.IsNullOrEmpty(error))
                {
                    result.AppendLine("命令执行完成，无输出");
                }
            }
            else
            {
                // 不等待进程结束，使用原始Process直接启动
                using var process = new Process
                {
                    StartInfo = startInfo
                };

                if (!process.Start())
                {
                    return $"无法启动进程: {command}";
                }

                result.AppendLine($"进程已启动（PID: {process.Id}）");
                result.AppendLine($"工作目录: {workingDirectory}");
                result.AppendLine($"显示控制台窗口: {showWindow}");
                result.AppendLine($"使用Shell执行: {startInfo.UseShellExecute}");
                result.AppendLine("注意：未等待进程结束，无法获取退出代码和输出");
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"执行命令时出错: {ex.Message}";
        }
    }

    /// <summary>
    /// 获取可用的Shell执行程序
    /// </summary>
    private string GetShellExecutable()
    {
        if (Environment.OSVersion.Platform == PlatformID.Unix)
        {
            return "/bin/bash";
        }
        else
        {
            return "cmd.exe";
        }
    }

    /// <summary>
    /// 获取Shell参数
    /// </summary>
    private string GetShellArguments(string command)
    {
        if (Environment.OSVersion.Platform == PlatformID.Unix)
        {
            return $"-c \"{EscapeForBash(command)}\"";
        }
        else
        {
            return $"/C \"{command}\"";
        }
    }

    /// <summary>
    /// 为Bash转义命令
    /// </summary>
    private string EscapeForBash(string command)
    {
        return command.Replace("\"", "\\\"");
    }

    /// <summary>
    /// 截断输出，避免过长
    /// </summary>
    private string TruncateOutput(string output)
    {
        if (string.IsNullOrEmpty(output) || output.Length <= MaxOutputLength)
        {
            return output;
        }

        return output.Substring(0, MaxOutputLength) + $"\n...（输出已截断，共 {output.Length} 字符，显示前 {MaxOutputLength} 字符）";
    }
}