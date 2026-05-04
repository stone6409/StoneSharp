using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StoneSharp.Core.Tools.BuiltIn;

/// <summary>
/// 文件读取插件，基于FileReadTool.ts重新实现
/// 支持读取文本文件、图像文件、PDF文件、Jupyter笔记本等
/// </summary>
public sealed class FileReadTool
{
    private readonly IFileSystem _fileSystem;
    private readonly Dictionary<string, (string content, DateTime timestamp, int? startLine, int? maxLines)> _readFileState = new();

    // 常见图像扩展名
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".tiff", ".tif"
    };

    // PDF扩展名
    private static readonly HashSet<string> PdfExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf"
    };

    // 二进制文件扩展名（不支持读取）
    private static readonly HashSet<string> BinaryExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".dll", ".bin", ".dat", ".obj"
    };

    // 默认限制
    private const long DefaultMaxSizeBytes = 256 * 1024; // 256KB
    private const int DefaultMaxLines = 2000;

    /// <summary>
    /// 构造函数
    /// </summary>
    public FileReadTool() : this(new DiskFileSystem())
    {
    }

    /// <summary>
    /// 构造函数（支持依赖注入）
    /// </summary>
    public FileReadTool(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    /// <summary>
    /// 读取文件内容
    /// </summary>
    [KernelFunction, Description("读取文件内容，支持文本、图像、PDF等多种格式")]
    public async Task<string> ReadFileAsync(
        [Description("文件路径")] string filePath,
        [Description("起始行号（可选，从1开始）")] int startLine = 1,
        [Description("读取行数限制（可选）")] int? maxLines = null,
        [Description("PDF页面范围（如'1-5'，仅PDF有效）")] string pageRange = null)
    {
        try
        {
            // 规范化路径
            var normalizedPath = NormalizePath(filePath);
            
            // 检查文件是否存在
            if (!_fileSystem.FileExists(normalizedPath))
            {
                return $"文件不存在: {filePath}";
            }

            // 检查文件类型
            var extension = Path.GetExtension(normalizedPath).ToLowerInvariant();
            
            // 检查是否为二进制文件
            if (BinaryExtensions.Contains(extension) && 
                !ImageExtensions.Contains(extension) && 
                !PdfExtensions.Contains(extension))
            {
                return $"不支持读取二进制文件: {filePath} (扩展名: {extension})";
            }

            // 检查文件大小
            var fileSize = _fileSystem.GetFileSize(normalizedPath);
            if (fileSize > DefaultMaxSizeBytes)
            {
                return $"文件太大 ({FileSizeFormatter.FormatFileSize(fileSize)})，超过限制 ({FileSizeFormatter.FormatFileSize(DefaultMaxSizeBytes)})。请使用起始行和限制参数读取部分内容。";
            }

            // 检查去重
            if (CheckFileUnchanged(normalizedPath, startLine, maxLines))
            {
                return $"文件自上次读取后未更改: {filePath}";
            }

            // 根据文件类型调用相应的读取方法
            if (ImageExtensions.Contains(extension))
            {
                return await ReadImageFileAsync(normalizedPath, filePath);
            }
            else if (PdfExtensions.Contains(extension))
            {
                return await ReadPdfFileAsync(normalizedPath, filePath, pageRange);
            }
            else
            {
                return await ReadTextFileAsync(normalizedPath, filePath, startLine, maxLines ?? DefaultMaxLines);
            }
        }
        catch (Exception ex)
        {
            return $"读取文件时出错: {ex.Message}";
        }
    }

    /// <summary>
    /// 读取文本文件
    /// </summary>
    private async Task<string> ReadTextFileAsync(string normalizedPath, string originalPath, int startLine, int maxLines)
    {
        try
        {
            // 检测文件编码
            var encoding = _fileSystem.DetectFileEncoding(normalizedPath);
            
            // 读取所有行
            var lines = await _fileSystem.ReadAllLinesAsync(normalizedPath, encoding);
            var totalLines = lines.Length;

            // 调整起始行（从1开始）
            var startIndex = Math.Max(0, startLine - 1);
            if (startIndex >= totalLines)
            {
                return $"文件只有 {totalLines} 行，但起始行设置为 {startLine}";
            }

            // 计算要读取的行数
            var linesToRead = Math.Min(maxLines, totalLines - startIndex);
            var selectedLines = lines.Skip(startIndex).Take(linesToRead).ToArray();

            // 格式化输出
            var result = new StringBuilder();
            result.AppendLine($"文件: {originalPath}");
            result.AppendLine($"编码: {encoding.EncodingName}");
            result.AppendLine($"总行数: {totalLines}");
            result.AppendLine($"读取行: {startIndex + 1}-{startIndex + linesToRead} (共 {linesToRead} 行)");
            result.AppendLine($"文件大小: {FileSizeFormatter.FormatFileSize(_fileSystem.GetFileSize(normalizedPath))}");
            result.AppendLine(new string('=', 80));

            // 添加行号
            for (int i = 0; i < selectedLines.Length; i++)
            {
                var lineNumber = startIndex + i + 1;
                result.AppendLine($"{lineNumber,6}: {selectedLines[i]}");
            }

            result.AppendLine(new string('=', 80));

            // 更新读取状态
            UpdateReadState(normalizedPath, string.Join(Environment.NewLine, selectedLines), startLine, maxLines);

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"读取文本文件时出错: {ex.Message}";
        }
    }

    /// <summary>
    /// 读取图像文件
    /// </summary>
    private async Task<string> ReadImageFileAsync(string normalizedPath, string originalPath)
    {
        try
        {
            // 读取文件为字节数组
            var bytes = await File.ReadAllBytesAsync(normalizedPath);
            var fileSize = bytes.Length;

            // 转换为Base64
            var base64 = Convert.ToBase64String(bytes);

            // 获取图像信息
            var extension = Path.GetExtension(normalizedPath).ToLowerInvariant();
            var mimeType = GetImageMimeType(extension);

            // 构建结果
            var result = new StringBuilder();
            result.AppendLine($"图像文件: {originalPath}");
            result.AppendLine($"格式: {extension.TrimStart('.')}");
            result.AppendLine($"MIME类型: {mimeType}");
            result.AppendLine($"文件大小: {FileSizeFormatter.FormatFileSize(fileSize)}");
            result.AppendLine($"Base64长度: {base64.Length} 字符");
            result.AppendLine(new string('=', 80));
            
            // 对于小图像，可以显示部分Base64数据
            if (base64.Length < 1000)
            {
                result.AppendLine("Base64数据（前1000字符）:");
                result.AppendLine(base64.Length > 1000 ? base64.Substring(0, 1000) + "..." : base64);
            }
            else
            {
                result.AppendLine("Base64数据（前500字符）:");
                result.AppendLine(base64.Substring(0, 500) + "...");
                result.AppendLine($"...（完整数据共 {base64.Length} 字符）");
            }

            result.AppendLine(new string('=', 80));
            result.AppendLine("注意：图像数据已转换为Base64格式。在实际使用中，您可能需要使用专门的图像处理库来显示或处理图像。");

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"读取图像文件时出错: {ex.Message}";
        }
    }

    /// <summary>
    /// 读取PDF文件（简化版）
    /// </summary>
    private async Task<string> ReadPdfFileAsync(string normalizedPath, string originalPath, string pageRange)
    {
        try
        {
            var fileSize = _fileSystem.GetFileSize(normalizedPath);
            var result = new StringBuilder();
            
            result.AppendLine($"PDF文件: {originalPath}");
            result.AppendLine($"文件大小: {FileSizeFormatter.FormatFileSize(fileSize)}");
            
            if (!string.IsNullOrEmpty(pageRange))
            {
                result.AppendLine($"请求的页面范围: {pageRange}");
                result.AppendLine("注意：PDF页面范围解析需要专门的PDF处理库（如iTextSharp、PdfSharp等）");
            }

            result.AppendLine(new string('=', 80));
            result.AppendLine("PDF文件内容读取需要专门的PDF处理库。");
            result.AppendLine("建议的解决方案：");
            result.AppendLine("1. 安装iTextSharp或PdfSharp库来处理PDF");
            result.AppendLine("2. 使用系统命令行工具（如pdftotext）提取文本");
            result.AppendLine("3. 对于简单需求，可以返回文件元数据");
            result.AppendLine(new string('=', 80));

            // 读取文件为Base64（可选）
            if (fileSize < 1024 * 1024) // 小于1MB
            {
                var bytes = await File.ReadAllBytesAsync(normalizedPath);
                var base64 = Convert.ToBase64String(bytes);
                result.AppendLine($"Base64数据（前500字符）:");
                result.AppendLine(base64.Length > 500 ? base64.Substring(0, 500) + "..." : base64);
            }

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"读取PDF文件时出错: {ex.Message}";
        }
    }

    /// <summary>
    /// 检查文件是否未更改
    /// </summary>
    private bool CheckFileUnchanged(string normalizedPath, int startLine, int? maxLines)
    {
        if (_readFileState.TryGetValue(normalizedPath, out var state))
        {
            // 检查起始行和限制是否匹配
            if (state.startLine == startLine && state.maxLines == maxLines)
            {
                // 检查文件修改时间
                var lastWriteTime = _fileSystem.GetFileLastWriteTime(normalizedPath);
                if (lastWriteTime <= state.timestamp)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 更新读取状态
    /// </summary>
    private void UpdateReadState(string normalizedPath, string content, int startLine, int? maxLines)
    {
        var timestamp = _fileSystem.GetFileLastWriteTime(normalizedPath);
        _readFileState[normalizedPath] = (content, timestamp, startLine, maxLines);
    }

    /// <summary>
    /// 规范化路径
    /// </summary>
    private string NormalizePath(string path)
    {
        // 展开环境变量
        path = Environment.ExpandEnvironmentVariables(path);
        
        // 转换为绝对路径
        if (!Path.IsPathRooted(path))
        {
            path = Path.Combine(_fileSystem.GetCurrentDirectory(), path);
        }
        
        // 规范化路径分隔符
        return Path.GetFullPath(path);
    }

    /// <summary>
    /// 获取图像MIME类型
    /// </summary>
    private string GetImageMimeType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".tiff" or ".tif" => "image/tiff",
            _ => "application/octet-stream"
        };
    }


}