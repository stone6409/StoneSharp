using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StoneSharp.Core.Tools.BuiltIn;

/// <summary>
/// 文件内容搜索工具，支持文本和正则表达式搜索
/// </summary>
public sealed class SearchContentTool
{
    private readonly IFileSystem _fileSystem;
    
    // 默认排除的目录
    private static readonly HashSet<string> DefaultExcludeDirs = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git", "node_modules", ".vs", "bin", "obj", "packages", "__pycache__"
    };
    
    // 二进制文件扩展名（跳过搜索）
    private static readonly HashSet<string> BinaryExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".dll", ".pdb", ".bin", ".dat", ".obj", ".lib", ".so", ".dylib",
        ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2", ".xz",
        ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".tiff", ".tif", ".ico", ".svg",
        ".mp3", ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv",
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx"
    };
    
    // 默认限制
    private const int DEFAULT_MAX_RESULTS = 50;
    private const long DEFAULT_MAX_FILE_SIZE_BYTES = 10 * 1024 * 1024; // 10MB
    private const int DEFAULT_MAX_LINE_LENGTH = 10000; // 避免处理过长的行
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public SearchContentTool() : this(new DiskFileSystem())
    {
    }
    
    /// <summary>
    /// 构造函数（支持依赖注入）
    /// </summary>
    public SearchContentTool(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }
    
    /// <summary>
    /// 搜索文件内容
    /// </summary>
    [KernelFunction, Description("搜索文件内容，支持文本和正则表达式搜索")]
    public async Task<string> SearchContentAsync(
        [Description("搜索的文本或正则表达式（必须提供）")] string searchPattern,
        [Description("搜索的根目录路径（默认当前目录）")] string searchDirectory = ".",
        [Description("按文件扩展名过滤（如 *.cs, *.{js,ts}）")] string filePattern = "*",
        [Description("是否区分大小写（默认不区分）")] bool caseSensitive = false,
        [Description("是否将pattern解释为正则表达式（默认false）")] bool isRegex = false,
        [Description("最大返回结果数（默认50）")] int maxResults = DEFAULT_MAX_RESULTS,
        [Description("要排除的目录名列表（用逗号分隔）")] string excludeDirectories = null,
        [Description("显示匹配行之前的行数（默认0）")] int beforeContextLines = 0,
        [Description("显示匹配行之后的行数（默认0）")] int afterContextLines = 0,
        [Description("是否跳过大于10MB的文件（默认true）")] bool skipLargeFiles = true)
    {
        try
        {
            // 验证输入
            if (string.IsNullOrWhiteSpace(searchPattern))
            {
                return "错误：搜索模式不能为空";
            }
            
            // 规范化路径
            var normalizedPath = NormalizePath(searchDirectory);
            
            // 验证目录存在
            if (!_fileSystem.DirectoryExists(normalizedPath))
            {
                return $"错误：目录不存在: {searchDirectory}";
            }
            
            // 解析排除目录
            var excludeDirSet = ParseExcludeDirectories(excludeDirectories);
            
            // 准备正则表达式（如果需要）
            Regex regexPattern = null;
            if (isRegex)
            {
                try
                {
                    var options = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                    regexPattern = new Regex(searchPattern, options);
                }
                catch (ArgumentException ex)
                {
                    return $"正则表达式语法错误: {ex.Message}";
                }
            }
            
            // 收集要搜索的文件
            var files = CollectFiles(normalizedPath, filePattern, excludeDirSet, skipLargeFiles);
            
            // 执行搜索
            var searchResults = new List<SearchResult>();
            var stats = new SearchStats
            {
                StartTime = DateTime.UtcNow,
                TotalFiles = files.Count
            };
            
            foreach (var file in files)
            {
                var fileResults = await SearchInFileAsync(
                    file, 
                    searchPattern, 
                    regexPattern, 
                    caseSensitive, 
                    beforeContextLines, 
                    afterContextLines);
                
                if (fileResults.Any())
                {
                    searchResults.AddRange(fileResults);
                    
                    // 检查是否达到最大结果数
                    if (searchResults.Count >= maxResults)
                    {
                        searchResults = searchResults.Take(maxResults).ToList();
                        break;
                    }
                }
                
                stats.FilesProcessed++;
            }
            
            stats.EndTime = DateTime.UtcNow;
            stats.TotalMatches = searchResults.Count;
            
            // 格式化输出
            return FormatResults(searchResults, stats, searchPattern, normalizedPath);
        }
        catch (Exception ex)
        {
            return $"搜索内容时出错: {ex.Message}";
        }
    }
    
    /// <summary>
    /// 收集要搜索的文件
    /// </summary>
    private List<string> CollectFiles(string rootPath, string filePattern, HashSet<string> excludeDirs, bool skipLargeFiles)
    {
        var files = new List<string>();
        
        // 递归收集文件
        CollectFilesRecursive(rootPath, filePattern, excludeDirs, skipLargeFiles, files);
        
        return files;
    }
    
    /// <summary>
    /// 递归收集文件
    /// </summary>
    private void CollectFilesRecursive(string currentPath, string filePattern, HashSet<string> excludeDirs, bool skipLargeFiles, List<string> files)
    {
        try
        {
            // 获取当前目录下的文件
            var currentFiles = _fileSystem.GetFiles(currentPath, filePattern, SearchOption.TopDirectoryOnly);
            foreach (var file in currentFiles)
            {
                // 检查文件扩展名
                var extension = _fileSystem.GetExtension(file).ToLowerInvariant();
                if (BinaryExtensions.Contains(extension))
                {
                    continue;
                }
                
                // 检查文件大小
                if (skipLargeFiles)
                {
                    var fileSize = _fileSystem.GetFileSize(file);
                    if (fileSize > DEFAULT_MAX_FILE_SIZE_BYTES)
                    {
                        continue;
                    }
                }
                
                files.Add(file);
            }
            
            // 递归处理子目录
            var subDirs = _fileSystem.GetDirectories(currentPath, "*", SearchOption.TopDirectoryOnly);
            foreach (var subDir in subDirs)
            {
                var dirName = _fileSystem.GetFileName(subDir);
                
                // 检查是否在排除列表中
                if (excludeDirs.Contains(dirName))
                {
                    continue;
                }
                
                CollectFilesRecursive(subDir, filePattern, excludeDirs, skipLargeFiles, files);
            }
        }
        catch (UnauthorizedAccessException)
        {
            // 跳过无权限访问的目录
        }
        catch (Exception)
        {
            // 跳过其他错误
        }
    }
    
    /// <summary>
    /// 在单个文件中搜索
    /// </summary>
    private async Task<List<SearchResult>> SearchInFileAsync(
        string filePath, 
        string pattern, 
        Regex regexPattern, 
        bool caseSensitive, 
        int beforeContextLines, 
        int afterContextLines)
    {
        var results = new List<SearchResult>();
        
        try
        {
            // 检测文件编码
            var encoding = _fileSystem.DetectFileEncoding(filePath);
            
            // 读取文件所有行
            var lines = await _fileSystem.ReadAllLinesAsync(filePath, encoding);
            
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                
                // 跳过过长的行
                if (line.Length > DEFAULT_MAX_LINE_LENGTH)
                {
                    continue;
                }
                
                bool isMatch = false;
                int matchStart = -1;
                int matchEnd = -1;
                
                if (regexPattern != null)
                {
                    // 使用正则表达式搜索
                    var match = regexPattern.Match(line);
                    if (match.Success)
                    {
                        isMatch = true;
                        matchStart = match.Index;
                        matchEnd = match.Index + match.Length;
                    }
                }
                else
                {
                    // 使用普通文本搜索
                    var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                    var index = line.IndexOf(pattern, comparison);
                    if (index >= 0)
                    {
                        isMatch = true;
                        matchStart = index;
                        matchEnd = index + pattern.Length;
                    }
                }
                
                if (isMatch)
                {
                    // 收集上下文行
                    var contextBefore = GetContextLines(lines, i, beforeContextLines, true);
                    var contextAfter = GetContextLines(lines, i, afterContextLines, false);
                    
                    var result = new SearchResult
                    {
                        FilePath = filePath,
                        LineNumber = i + 1,
                        LineContent = line,
                        MatchStart = matchStart,
                        MatchEnd = matchEnd,
                        ContextBefore = contextBefore,
                        ContextAfter = contextAfter
                    };
                    
                    results.Add(result);
                    
                    // 跳过afterContextLines行，避免重叠匹配
                    i = Math.Min(i + afterContextLines, lines.Length - 1);
                }
            }
        }
        catch (Exception)
        {
            // 跳过读取失败的文件
        }
        
        return results;
    }
    
    /// <summary>
    /// 获取上下文行
    /// </summary>
    private List<string> GetContextLines(string[] lines, int currentIndex, int contextCount, bool before)
    {
        var contextLines = new List<string>();
        
        if (contextCount <= 0)
        {
            return contextLines;
        }
        
        if (before)
        {
            var start = Math.Max(0, currentIndex - contextCount);
            for (int i = start; i < currentIndex; i++)
            {
                contextLines.Add(lines[i]);
            }
        }
        else
        {
            var end = Math.Min(lines.Length - 1, currentIndex + contextCount);
            for (int i = currentIndex + 1; i <= end; i++)
            {
                contextLines.Add(lines[i]);
            }
        }
        
        return contextLines;
    }
    
    /// <summary>
    /// 解析排除目录
    /// </summary>
    private HashSet<string> ParseExcludeDirectories(string excludeDirectories)
    {
        var excludeDirSet = new HashSet<string>(DefaultExcludeDirs, StringComparer.OrdinalIgnoreCase);
        
        if (!string.IsNullOrWhiteSpace(excludeDirectories))
        {
            var dirs = excludeDirectories.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(d => d.Trim())
                                 .Where(d => !string.IsNullOrEmpty(d));
            
            foreach (var dir in dirs)
            {
                excludeDirSet.Add(dir);
            }
        }
        
        return excludeDirSet;
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
    /// 格式化搜索结果
    /// </summary>
    private string FormatResults(List<SearchResult> results, SearchStats stats, string pattern, string searchPath)
    {
        var result = new StringBuilder();
        
        // 头部信息
        result.AppendLine($"搜索内容: '{pattern}'");
        result.AppendLine($"搜索目录: {searchPath}");
        result.AppendLine($"搜索时间: {stats.StartTime:yyyy-MM-dd HH:mm:ss}");
        result.AppendLine($"耗时: {(stats.EndTime - stats.StartTime).TotalMilliseconds:F0}ms");
        result.AppendLine();
        
        // 统计信息
        result.AppendLine("统计信息:");
        result.AppendLine($"  总文件数: {stats.TotalFiles}");
        result.AppendLine($"  已处理文件: {stats.FilesProcessed}");
        result.AppendLine($"  总匹配数: {stats.TotalMatches}");
        result.AppendLine();
        
        if (results.Count == 0)
        {
            result.AppendLine("未找到匹配项。");
            return result.ToString();
        }
        
        // 结果列表
        result.AppendLine($"找到 {results.Count} 个匹配项:");
        result.AppendLine(new string('=', 80));
        
        for (int i = 0; i < results.Count; i++)
        {
            var searchResult = results[i];
            var relativePath = _fileSystem.GetRelativePath(searchPath, searchResult.FilePath);
            
            result.AppendLine($"[{i + 1}] {relativePath}:{searchResult.LineNumber}");
            
            // 显示上下文（如果有）
            if (searchResult.ContextBefore.Any())
            {
                foreach (var line in searchResult.ContextBefore)
                {
                    result.AppendLine($"    {line}");
                }
            }
            
            // 显示匹配行，突出显示匹配部分
            var lineContent = searchResult.LineContent;
            if (searchResult.MatchStart >= 0 && searchResult.MatchEnd > searchResult.MatchStart)
            {
                var beforeMatch = lineContent.Substring(0, searchResult.MatchStart);
                var matchText = lineContent.Substring(searchResult.MatchStart, searchResult.MatchEnd - searchResult.MatchStart);
                var afterMatch = lineContent.Substring(searchResult.MatchEnd);
                
                result.AppendLine($"    {beforeMatch}>>>{matchText}<<<{afterMatch}");
            }
            else
            {
                result.AppendLine($"    {lineContent}");
            }
            
            // 显示后续上下文（如果有）
            if (searchResult.ContextAfter.Any())
            {
                foreach (var line in searchResult.ContextAfter)
                {
                    result.AppendLine($"    {line}");
                }
            }
            
            result.AppendLine();
        }
        
        result.AppendLine(new string('=', 80));
        
        // 添加JSON格式结果（可选，便于程序化处理）
        var jsonResult = new
        {
            stats = new
            {
                total_matches = stats.TotalMatches,
                files_searched = stats.TotalFiles,
                files_processed = stats.FilesProcessed,
                time_ms = (stats.EndTime - stats.StartTime).TotalMilliseconds
            },
            results = results.Select(r => new
            {
                file_path = r.FilePath,
                line_number = r.LineNumber,
                line_content = r.LineContent,
                match_start = r.MatchStart,
                match_end = r.MatchEnd,
                context_before = r.ContextBefore,
                context_after = r.ContextAfter
            }).ToList()
        };
        
        result.AppendLine("JSON格式结果（便于程序化处理）:");
        result.AppendLine(JsonSerializer.Serialize(jsonResult, new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        }));
        
        return result.ToString();
    }
    

    
    /// <summary>
    /// 搜索结果
    /// </summary>
    private class SearchResult
    {
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        public string LineContent { get; set; }
        public int MatchStart { get; set; }
        public int MatchEnd { get; set; }
        public List<string> ContextBefore { get; set; } = new();
        public List<string> ContextAfter { get; set; } = new();
    }
    
    /// <summary>
    /// 搜索统计信息
    /// </summary>
    private class SearchStats
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int TotalFiles { get; set; }
        public int FilesProcessed { get; set; }
        public int TotalMatches { get; set; }
    }
}