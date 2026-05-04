using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace StoneSharp.Core.Tools.BuiltIn;

/// <summary>
/// 带备份功能的磁盘文件系统实现
/// </summary>
public sealed class BackupDiskFileSystem : DiskFileSystem
{
    private readonly string _backupDirectory;
    private readonly Dictionary<string, string> _backupFileMap = new();

    /// <summary>
    /// 初始化备份文件系统
    /// </summary>
    public BackupDiskFileSystem()
    {
        _backupDirectory = Path.Combine(Path.GetTempPath(), "FileSystemBackup", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_backupDirectory);
    }

    /// <summary>
    /// 初始化备份文件系统，指定备份目录
    /// </summary>
    /// <param name="backupDirectory">备份目录路径</param>
    public BackupDiskFileSystem(string backupDirectory)
    {
        _backupDirectory = backupDirectory ?? throw new ArgumentNullException(nameof(backupDirectory));
        Directory.CreateDirectory(_backupDirectory);
    }

    /// <summary>
    /// 写入文本到文件（带备份）
    /// </summary>
    public override async Task WriteAllTextAsync(string filePath, string content, Encoding encoding)
    {
        await BackupFileIfNeeded(filePath);
        await base.WriteAllTextAsync(filePath, content, encoding);
        RecordFileOperation(filePath);
    }

    /// <summary>
    /// 写入所有行到文件（带备份）
    /// </summary>
    public override async Task WriteAllLinesAsync(string filePath, string[] lines, Encoding encoding)
    {
        await BackupFileIfNeeded(filePath);
        await base.WriteAllLinesAsync(filePath, lines, encoding);
        RecordFileOperation(filePath);
    }

    /// <summary>
    /// 追加文本到文件（带备份）
    /// </summary>
    public override async Task AppendAllTextAsync(string filePath, string content, Encoding encoding)
    {
        await BackupFileIfNeeded(filePath);
        await base.AppendAllTextAsync(filePath, content, encoding);
        RecordFileOperation(filePath);
    }

    /// <summary>
    /// 删除文件（带备份）
    /// </summary>
    public override async Task DeleteFile(string filePath)
    {
        await BackupFileIfNeeded(filePath);
        base.DeleteFile(filePath);
        // 删除文件后，我们仍然保留记录，但标记为已删除
        RecordFileOperation(filePath);
    }

    /// <summary>
    /// 复制文件（带备份）
    /// </summary>
    public override void CopyFile(string sourceFilePath, string destinationFilePath, bool overwrite)
    {
        if (overwrite && FileExists(destinationFilePath))
        {
            BackupFileIfNeeded(destinationFilePath).Wait();
        }
        base.CopyFile(sourceFilePath, destinationFilePath, overwrite);
        RecordFileOperation(destinationFilePath);
    }

    /// <summary>
    /// 移动文件（带备份）
    /// </summary>
    public override void MoveFile(string sourceFilePath, string destinationFilePath)
    {
        if (FileExists(destinationFilePath))
        {
            BackupFileIfNeeded(destinationFilePath).Wait();
        }
        base.MoveFile(sourceFilePath, destinationFilePath);
        RecordFileOperation(destinationFilePath);
        
        // 如果源文件在备份列表中，更新映射
        if (_backupFileMap.TryGetValue(sourceFilePath, out var backupPath))
        {
            _backupFileMap.Remove(sourceFilePath);
            _backupFileMap[destinationFilePath] = backupPath;
        }
        else
        {
            // 如果源文件是新增文件，也记录目标文件为新增
            RecordFileOperation(destinationFilePath);
        }
    }

    /// <summary>
    /// 如果需要，备份文件
    /// </summary>
    private async Task BackupFileIfNeeded(string filePath)
    {
        if (!FileExists(filePath) || _backupFileMap.ContainsKey(filePath))
            return;

        var backupFilePath = GenerateBackupFilePath(filePath);
        await CopyFileToBackupAsync(filePath, backupFilePath);
        _backupFileMap[filePath] = backupFilePath;
    }

    /// <summary>
    /// 记录文件操作
    /// </summary>
    private void RecordFileOperation(string filePath)
    {
        // 如果文件已经存在备份，就不需要再次记录
        if (!_backupFileMap.ContainsKey(filePath))
        {
            // 对于新增文件，我们记录null值表示没有备份
            _backupFileMap[filePath] = null;
        }
    }

    /// <summary>
    /// 生成备份文件路径
    /// </summary>
    private string GenerateBackupFilePath(string originalFilePath)
    {
        var fileName = Path.GetFileName(originalFilePath);
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
        var backupFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{timestamp}{Path.GetExtension(fileName)}";
        return Path.Combine(_backupDirectory, backupFileName);
    }

    /// <summary>
    /// 复制文件到备份目录
    /// </summary>
    private async Task CopyFileToBackupAsync(string sourceFilePath, string backupFilePath)
    {
        using var sourceStream = File.OpenRead(sourceFilePath);
        using var backupStream = File.Create(backupFilePath);
        await sourceStream.CopyToAsync(backupStream);
    }

    /// <summary>
    /// 获取文件的备份路径
    /// </summary>
    /// <param name="filePath">原始文件路径</param>
    /// <returns>备份文件路径，如果没有备份则返回null</returns>
    public string GetBackupFilePath(string filePath)
    {
        return _backupFileMap.TryGetValue(filePath, out var backupPath) ? backupPath : null;
    }

    /// <summary>
    /// 比较原始文件和当前文件的差异
    /// </summary>
    /// <param name="filePath">要比较的文件路径</param>
    /// <returns>
    /// 返回差异信息：
    /// - 如果没有备份且是新增文件，返回 "New file (no backup available)"
    /// - 如果没有备份，返回 "No backup found"
    /// - 如果文件被删除，返回 "File has been deleted"
    /// - 如果文件内容相同，返回 "No changes detected"
    /// - 如果文件内容不同，返回 "Files are different"
    /// </returns>
    public async Task<string> CompareWithBackupAsync(string filePath)
    {
        if (!_backupFileMap.TryGetValue(filePath, out var backupPath))
            return "No backup found";

        if (backupPath == null)
            return "New file (no backup available)";

        if (!FileExists(filePath))
            return "File has been deleted";

        if (!File.Exists(backupPath))
            return "Backup file not found";

        var originalContent = await ReadAllTextAsync(filePath, Encoding.UTF8);
        var backupContent = await File.ReadAllTextAsync(backupPath, Encoding.UTF8);

        return originalContent == backupContent ? "No changes detected" : "Files are different";
    }

    /// <summary>
    /// 还原文件到备份版本
    /// </summary>
    /// <param name="filePath">要还原的文件路径</param>
    /// <returns>是否还原成功</returns>
    public async Task<bool> RestoreFromBackupAsync(string filePath)
    {
        if (!_backupFileMap.TryGetValue(filePath, out var backupPath))
            return false;

        // 新增文件没有备份，无法还原
        if (backupPath == null)
            return false;

        if (!File.Exists(backupPath))
            return false;

        try
        {
            var backupContent = await File.ReadAllTextAsync(backupPath, Encoding.UTF8);
            await WriteAllTextAsync(filePath, backupContent, Encoding.UTF8);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 获取所有操作过的文件列表
    /// </summary>
    public IReadOnlyDictionary<string, string> GetAllOperatedFiles()
    {
        return new Dictionary<string, string>(_backupFileMap);
    }

    /// <summary>
    /// 获取所有有备份的文件列表
    /// </summary>
    public IReadOnlyDictionary<string, string> GetAllBackedUpFiles()
    {
        return _backupFileMap
            .Where(kvp => kvp.Value != null)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    /// <summary>
    /// 获取所有新增的文件列表
    /// </summary>
    public IReadOnlyCollection<string> GetAllNewFiles()
    {
        return _backupFileMap
            .Where(kvp => kvp.Value == null)
            .Select(kvp => kvp.Key)
            .ToList();
    }

    /// <summary>
    /// 检查文件是否是新增的
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>如果是新增文件则返回true</returns>
    public bool IsNewFile(string filePath)
    {
        return _backupFileMap.TryGetValue(filePath, out var backupPath) && backupPath == null;
    }

    /// <summary>
    /// 检查文件是否被删除
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>如果文件已被删除则返回true</returns>
    public bool IsFileDeleted(string filePath)
    {
        return _backupFileMap.ContainsKey(filePath) && !FileExists(filePath);
    }

    /// <summary>
    /// 清理所有备份文件
    /// </summary>
    public void CleanupBackups()
    {
        // 清理有备份的文件
        foreach (var backupPath in _backupFileMap.Values.Where(path => path != null))
        {
            try
            {
                if (File.Exists(backupPath))
                    File.Delete(backupPath);
            }
            catch
            {
                // 忽略删除失败的文件
            }
        }
        _backupFileMap.Clear();

        try
        {
            if (Directory.Exists(_backupDirectory))
                Directory.Delete(_backupDirectory, true);
        }
        catch
        {
            // 忽略删除失败的情况
        }

        // 清理FileSystemBackup文件夹下超过1天的文件和文件夹
        CleanupOldBackupsInFileSystemBackup();
    }

    /// <summary>
    /// 清理FileSystemBackup文件夹下超过1天的文件和文件夹
    /// </summary>
    private void CleanupOldBackupsInFileSystemBackup()
    {
        try
        {
            var fileSystemBackupRoot = Path.Combine(Path.GetTempPath(), "FileSystemBackup");
            if (!Directory.Exists(fileSystemBackupRoot))
                return;

            var cutoffTime = DateTime.Now.AddDays(-1);
            
            // 清理超过1天的文件
            var files = Directory.GetFiles(fileSystemBackupRoot, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                try
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.LastWriteTime < cutoffTime)
                    {
                        File.Delete(file);
                    }
                }
                catch
                {
                    // 忽略删除失败的文件
                }
            }

            // 清理超过1天的空文件夹
            var directories = Directory.GetDirectories(fileSystemBackupRoot, "*", SearchOption.AllDirectories);
            foreach (var directory in directories)
            {
                try
                {
                    var dirInfo = new DirectoryInfo(directory);
                    if (dirInfo.LastWriteTime < cutoffTime)
                    {
                        // 检查文件夹是否为空
                        if (!Directory.EnumerateFileSystemEntries(directory).Any())
                        {
                            Directory.Delete(directory, false);
                        }
                    }
                }
                catch
                {
                    // 忽略删除失败的文件夹
                }
            }
        }
        catch
        {
            // 忽略清理过程中的异常
        }
    }

    /// <summary>
    /// 析构函数，自动清理备份
    /// </summary>
    ~BackupDiskFileSystem()
    {
        try
        {
            CleanupBackups();
        }
        catch
        {
            // 忽略析构函数中的异常
        }
    }
}