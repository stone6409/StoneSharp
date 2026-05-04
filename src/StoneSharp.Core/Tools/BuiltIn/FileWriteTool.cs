using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text;

namespace StoneSharp.Core.Tools.BuiltIn;

/// <summary>
/// 文件写入插件，基于FileWriteTool.ts重新实现，专注于基础文件写入功能
/// </summary>
public sealed class FileWriteTool
{
    private readonly IFileSystem _fileSystem;

    /// <summary>
    /// 构造函数
    /// </summary>
    public FileWriteTool() : this(new DiskFileSystem())
    {
    }
    
    /// <summary>
    /// 构造函数（支持依赖注入）
    /// </summary>
    public FileWriteTool(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    /// <summary>
    /// 写入文件内容 - 创建或更新文件
    /// </summary>
    [KernelFunction, Description("写入文件内容，创建新文件或更新现有文件")]
    public async Task<string> WriteFileAsync(
        [Description("文件绝对路径")] string filePath,
        [Description("要写入的内容")] string content)
    {
        try
        {
            // 确保文件路径是绝对路径
            if (!Path.IsPathRooted(filePath))
            {
                return "错误：文件路径必须是绝对路径";
            }

            // 检查文件是否存在
            bool fileExists = _fileSystem.FileExists(filePath);
            string operationType = fileExists ? "update" : "create";

            // 确保父目录存在
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !_fileSystem.DirectoryExists(directory))
            {
                _fileSystem.CreateDirectory(directory);
            }

            // 检测文件编码（如果文件存在）或使用UTF-8
            Encoding encoding;
            if (fileExists)
            {
                encoding = _fileSystem.DetectFileEncoding(filePath);
            }
            else
            {
                encoding = Encoding.UTF8;
            }

            // 读取原始内容（如果文件存在）
            string? originalContent = null;
            if (fileExists)
            {
                originalContent = await _fileSystem.ReadAllTextAsync(filePath, encoding);
            }

            // 写入文件内容
            await _fileSystem.WriteAllTextAsync(filePath, content, encoding);

            // 构建返回结果
            var result = new StringBuilder();
            
            if (operationType == "create")
            {
                result.AppendLine($"文件创建成功: {filePath}");
                result.AppendLine($"操作类型: 创建新文件");
                result.AppendLine($"内容长度: {content.Length} 字符");
                result.AppendLine($"编码: {encoding.EncodingName}");
            }
            else // update
            {
                result.AppendLine($"文件更新成功: {filePath}");
                result.AppendLine($"操作类型: 更新现有文件");
                result.AppendLine($"原内容长度: {originalContent?.Length ?? 0} 字符");
                result.AppendLine($"新内容长度: {content.Length} 字符");
                result.AppendLine($"编码: {encoding.EncodingName}");
                
                // 计算简单的差异信息
                if (originalContent != null)
                {
                    int originalLines = CountLines(originalContent);
                    int newLines = CountLines(content);
                    result.AppendLine($"原文件行数: {originalLines}");
                    result.AppendLine($"新文件行数: {newLines}");
                    result.AppendLine($"行数变化: {newLines - originalLines:+0;-0;0}");
                }
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"写入文件时出错: {ex.Message}";
        }
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>删除结果</returns>
    [KernelFunction, Description("删除文件")]
    public string DeleteFile(
        [Description("文件路径")] string filePath)
    {
        if (!_fileSystem.FileExists(filePath))
        {
            return $"文件不存在: {filePath}";
        }

        try
        {
            _fileSystem.DeleteFile(filePath);
            return $"文件删除成功: {filePath}";
        }
        catch (Exception ex)
        {
            return $"删除文件时出错: {ex.Message}";
        }
    }

    /// <summary>
    /// 计算文件行数
    /// </summary>
    private int CountLines(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        int count = 1;
        int position = 0;
        while ((position = text.IndexOf('\n', position)) != -1)
        {
            count++;
            position++;
        }
        return count;
    }
}
