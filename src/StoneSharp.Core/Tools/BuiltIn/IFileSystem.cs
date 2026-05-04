using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace StoneSharp.Core.Tools.BuiltIn;

/// <summary>
/// 文件系统操作接口
/// </summary>
public interface IFileSystem
{
    // 文件操作
    Task WriteAllTextAsync(string filePath, string content, Encoding encoding);
    Task<string> ReadAllTextAsync(string filePath, Encoding encoding);
    Task<string[]> ReadAllLinesAsync(string filePath, Encoding encoding);
    Task WriteAllLinesAsync(string filePath, string[] lines, Encoding encoding);
    Task AppendAllTextAsync(string filePath, string content, Encoding encoding);
    
    // 文件信息
    bool FileExists(string filePath);
    FileInfo GetFileInfo(string filePath);
    long GetFileSize(string filePath);
    DateTime GetFileCreationTime(string filePath);
    DateTime GetFileLastWriteTime(string filePath);
    
    // 目录操作
    bool DirectoryExists(string directoryPath);
    void CreateDirectory(string directoryPath);
    string[] GetDirectories(string directoryPath, string searchPattern, SearchOption searchOption);
    string[] GetFiles(string directoryPath, string searchPattern, SearchOption searchOption);
    DateTime GetDirectoryLastWriteTime(string directoryPath);

    // 文件管理
    Task DeleteFile(string filePath);
    void DeleteDirectory(string directoryPath, bool recursive);
    void CopyFile(string sourceFilePath, string destinationFilePath, bool overwrite);
    void MoveFile(string sourceFilePath, string destinationFilePath);
    
    // 路径操作
    string GetCurrentDirectory();
    void SetCurrentDirectory(string directoryPath);
    string GetDirectoryName(string path);
    string GetFileName(string path);
    string GetFileNameWithoutExtension(string path);
    string GetExtension(string path);
    string CombinePaths(params string[] paths);
    string GetRelativePath(string relativeTo, string path);
    
    // 编码检测
    Encoding DetectFileEncoding(string filePath);
}