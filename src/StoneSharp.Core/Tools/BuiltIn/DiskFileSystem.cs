using StoneSharp.CodeProcessing.Utilities;
using System.Text;

namespace StoneSharp.Core.Tools.BuiltIn;

/// <summary>
/// 磁盘文件系统实现
/// </summary>
public class DiskFileSystem : IFileSystem
{
    /// <summary>
    /// 写入文本到文件
    /// </summary>
    public virtual async Task WriteAllTextAsync(string filePath, string content, Encoding encoding)
    {
        content = CodeUtility.NormalizeLineEndings(content);
        await File.WriteAllTextAsync(filePath, content, encoding);
    }

    /// <summary>
    /// 读取文件所有文本
    /// </summary>
    public async Task<string> ReadAllTextAsync(string filePath, Encoding encoding)
    {
        return await File.ReadAllTextAsync(filePath, encoding);
    }

    /// <summary>
    /// 读取文件所有行
    /// </summary>
    public async Task<string[]> ReadAllLinesAsync(string filePath, Encoding encoding)
    {
        return await File.ReadAllLinesAsync(filePath, encoding);
    }

    /// <summary>
    /// 写入所有行到文件
    /// </summary>
    public virtual async Task WriteAllLinesAsync(string filePath, string[] lines, Encoding encoding)
    {
        await File.WriteAllLinesAsync(filePath, lines, encoding);
    }

    /// <summary>
    /// 追加文本到文件
    /// </summary>
    public virtual async Task AppendAllTextAsync(string filePath, string content, Encoding encoding)
    {
        content = CodeUtility.NormalizeLineEndings(content);
        await File.AppendAllTextAsync(filePath, content, encoding);
    }

    /// <summary>
    /// 检查文件是否存在
    /// </summary>
    public bool FileExists(string filePath)
    {
        return File.Exists(filePath);
    }

    /// <summary>
    /// 获取文件信息
    /// </summary>
    public FileInfo GetFileInfo(string filePath)
    {
        return new FileInfo(filePath);
    }

    /// <summary>
    /// 获取文件大小
    /// </summary>
    public long GetFileSize(string filePath)
    {
        return new FileInfo(filePath).Length;
    }

    /// <summary>
    /// 获取文件创建时间
    /// </summary>
    public DateTime GetFileCreationTime(string filePath)
    {
        return File.GetCreationTime(filePath);
    }

    /// <summary>
    /// 获取文件最后修改时间
    /// </summary>
    public DateTime GetFileLastWriteTime(string filePath)
    {
        return File.GetLastWriteTime(filePath);
    }

    /// <summary>
    /// 获取目录最后修改时间
    /// </summary>
    public DateTime GetDirectoryLastWriteTime(string directoryPath)
    {
        return Directory.GetLastWriteTime(directoryPath);
    }

    /// <summary>
    /// 检查目录是否存在
    /// </summary>
    public bool DirectoryExists(string directoryPath)
    {
        return Directory.Exists(directoryPath);
    }

    /// <summary>
    /// 创建目录
    /// </summary>
    public void CreateDirectory(string directoryPath)
    {
        Directory.CreateDirectory(directoryPath);
    }

    /// <summary>
    /// 获取子目录列表（支持搜索选项）
    /// </summary>
    public string[] GetDirectories(string directoryPath, string searchPattern, SearchOption searchOption)
    {
        return Directory.GetDirectories(directoryPath, searchPattern, searchOption);
    }

    /// <summary>
    /// 获取文件列表（支持搜索选项）
    /// </summary>
    public string[] GetFiles(string directoryPath, string searchPattern, SearchOption searchOption)
    {
        return Directory.GetFiles(directoryPath, searchPattern, searchOption);
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    public virtual async Task DeleteFile(string filePath)
    {
        File.Delete(filePath);
    }

    /// <summary>
    /// 删除目录
    /// </summary>
    public void DeleteDirectory(string directoryPath, bool recursive)
    {
        Directory.Delete(directoryPath, recursive);
    }

    /// <summary>
    /// 复制文件
    /// </summary>
    public virtual void CopyFile(string sourceFilePath, string destinationFilePath, bool overwrite)
    {
        File.Copy(sourceFilePath, destinationFilePath, overwrite);
    }

    /// <summary>
    /// 移动文件
    /// </summary>
    public virtual void MoveFile(string sourceFilePath, string destinationFilePath)
    {
        File.Move(sourceFilePath, destinationFilePath);
    }

    /// <summary>
    /// 获取当前工作目录
    /// </summary>
    public string GetCurrentDirectory()
    {
        return Directory.GetCurrentDirectory();
    }

    /// <summary>
    /// 设置当前工作目录
    /// </summary>
    public void SetCurrentDirectory(string directoryPath)
    {
        Directory.SetCurrentDirectory(directoryPath);
    }

    /// <summary>
    /// 获取目录名
    /// </summary>
    public string GetDirectoryName(string path)
    {
        return Path.GetDirectoryName(path);
    }

    /// <summary>
    /// 获取文件名
    /// </summary>
    public string GetFileName(string path)
    {
        return Path.GetFileName(path);
    }

    /// <summary>
    /// 获取不带扩展名的文件名
    /// </summary>
    public string GetFileNameWithoutExtension(string path)
    {
        return Path.GetFileNameWithoutExtension(path);
    }

    /// <summary>
    /// 获取扩展名
    /// </summary>
    public string GetExtension(string path)
    {
        return Path.GetExtension(path);
    }

    /// <summary>
    /// 合并路径
    /// </summary>
    public string CombinePaths(params string[] paths)
    {
        return Path.Combine(paths);
    }

    /// <summary>
    /// 获取相对路径
    /// </summary>
    public string GetRelativePath(string relativeTo, string path)
    {
        return Path.GetRelativePath(relativeTo, path);
    }

    /// <summary>
    /// 检测文件编码
    /// </summary>
    public Encoding DetectFileEncoding(string filePath)
    {
        return FileEncodingDetector.GetEncodingEnhanced(filePath);
    }
}