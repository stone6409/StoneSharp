using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text;

namespace StoneSharp.Core.Tools.BuiltIn;

/// <summary>
/// 基于Windows习惯查找文件的工具，支持通配符
/// </summary>
public sealed class FindFilesTool
{
    private readonly IFileSystem _fileSystem;
    
    /// <summary>
    /// 默认最大结果数
    /// </summary>
    private const int DEFAULT_MAX_RESULTS = 100;
    
    /// <summary>
    /// 排序方式枚举
    /// </summary>
    public enum SortBy
    {
        [Description("按路径字母顺序排序")]
        Path,
        
        [Description("按修改时间排序（最新的在前）")]
        LastWriteTime,
        
        [Description("按文件大小排序（最大的在前）")]
        FileSize,
        
        [Description("按类型排序（目录在前，文件在后）")]
        Type
    }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public FindFilesTool() : this(new DiskFileSystem())
    {
    }
    
    /// <summary>
    /// 构造函数（支持依赖注入）
    /// </summary>
    public FindFilesTool(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }
    
    /// <summary>
    /// 查找文件 - Windows风格（使用通配符）
    /// </summary>
    [KernelFunction, Description("查找文件 - Windows风格（使用通配符*和?）")]
    public async Task<string> FindFilesAsync(
        [Description("查找模式（如 *.cs, project?.txt, *test*.*）")] string findPattern,
        [Description("查找目录（可选，默认为当前目录）")] string findDirectory = "",
        [Description("是否递归查找子目录（可选，默认为false）")] bool recursive = false,
        [Description("最大结果数（可选，默认为100）")] int maxResults = DEFAULT_MAX_RESULTS,
        [Description("排序方式（可选，默认为按路径排序）")] SortBy sortBy = SortBy.Path,
        [Description("是否倒序排序（可选，默认为false）")] bool descending = false)
    {
        try
        {
            // 验证输入
            if (string.IsNullOrWhiteSpace(findPattern))
            {
                return "错误：查找模式不能为空";
            }
            
            // 获取查找目录
            if (string.IsNullOrEmpty(findDirectory))
            {
                findDirectory = _fileSystem.GetCurrentDirectory();
            }
            
            // 验证目录存在
            if (!_fileSystem.DirectoryExists(findDirectory))
            {
                return $"错误：目录不存在: {findDirectory}";
            }
            
            // 设置查找选项
            var findOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            
            // 执行查找
            var files = _fileSystem.GetFiles(findDirectory, findPattern, findOption);
            var directories = _fileSystem.GetDirectories(findDirectory, findPattern, findOption);
            
            // 使用HashSet去重，确保路径唯一性
            var uniquePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var uniqueFiles = new List<string>();
            var uniqueDirectories = new List<string>();
            
            // 去重文件
            foreach (var file in files)
            {
                if (uniquePaths.Add(file))
                {
                    uniqueFiles.Add(file);
                }
            }
            
            // 去重目录
            foreach (var directory in directories)
            {
                if (uniquePaths.Add(directory))
                {
                    uniqueDirectories.Add(directory);
                }
            }
            
            // 创建文件和目录的合并列表
            var allItems = uniqueFiles.Select(file => new FileSystemItem
                {
                    Path = file,
                    IsDirectory = false,
                    LastWriteTime = _fileSystem.GetFileLastWriteTime(file),
                    Size = _fileSystem.GetFileSize(file),
                    RelativePath = _fileSystem.GetRelativePath(findDirectory, file)
                })
                .Concat(uniqueDirectories.Select(dir => new FileSystemItem
                {
                    Path = dir,
                    IsDirectory = true,
                    LastWriteTime = _fileSystem.GetDirectoryLastWriteTime(dir),
                    Size = 0,
                    RelativePath = _fileSystem.GetRelativePath(findDirectory, dir)
                }))
                .ToList();
            
            // 根据排序选项排序
            var sortedItems = SortItems(allItems, sortBy, descending)
                .Take(maxResults)
                .ToList();
            
            // 构建结果
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"在目录 {findDirectory} 中查找 '{findPattern}' 的结果:");
            stringBuilder.AppendLine($"查找选项: {(recursive ? "递归查找" : "仅当前目录")}");
            stringBuilder.AppendLine($"排序方式: {GetSortDescription(sortBy, descending)}");
            stringBuilder.AppendLine($"找到 {uniqueFiles.Count} 个文件, {uniqueDirectories.Count} 个目录");
            
            var totalItems = uniqueFiles.Count + uniqueDirectories.Count;
            if (totalItems > maxResults)
            {
                stringBuilder.AppendLine($"(结果被截断，只显示前 {maxResults} 个项)");
            }
            
            stringBuilder.AppendLine();
            
            // 按目录分组显示（当按路径排序时）
            if (sortBy == SortBy.Path)
            {
                DisplayGroupedByDirectory(stringBuilder, sortedItems, findDirectory);
            }
            else
            {
                DisplayFlatList(stringBuilder, sortedItems, findDirectory);
            }
            
            string result = stringBuilder.ToString();
            return result;
        }
        catch (Exception ex)
        {
            return $"查找文件时出错: {ex.Message}";
        }
    }
    
    /// <summary>
    /// 文件系统项信息
    /// </summary>
    private class FileSystemItem
    {
        public string Path { get; set; } = string.Empty;
        public string RelativePath { get; set; } = string.Empty;
        public bool IsDirectory { get; set; }
        public DateTime LastWriteTime { get; set; }
        public long Size { get; set; }
        
        public string DirectoryName => System.IO.Path.GetDirectoryName(RelativePath) ?? string.Empty;
        public string FileName => System.IO.Path.GetFileName(RelativePath);
    }
    
    /// <summary>
    /// 排序项目
    /// </summary>
    private IEnumerable<FileSystemItem> SortItems(List<FileSystemItem> items, SortBy sortBy, bool descending)
    {
        IOrderedEnumerable<FileSystemItem> orderedItems;
        
        switch (sortBy)
        {
            case SortBy.Path:
                orderedItems = descending 
                    ? items.OrderByDescending(item => item.RelativePath, StringComparer.OrdinalIgnoreCase)
                    : items.OrderBy(item => item.RelativePath, StringComparer.OrdinalIgnoreCase);
                break;
                
            case SortBy.LastWriteTime:
                orderedItems = descending
                    ? items.OrderByDescending(item => item.LastWriteTime)
                    : items.OrderBy(item => item.LastWriteTime);
                break;
                
            case SortBy.FileSize:
                orderedItems = descending
                    ? items.OrderByDescending(item => item.Size)
                    : items.OrderBy(item => item.Size);
                break;
                
            case SortBy.Type:
                orderedItems = descending
                    ? items.OrderByDescending(item => item.IsDirectory)
                        .ThenByDescending(item => item.RelativePath, StringComparer.OrdinalIgnoreCase)
                    : items.OrderBy(item => item.IsDirectory)
                        .ThenBy(item => item.RelativePath, StringComparer.OrdinalIgnoreCase);
                break;
                
            default:
                orderedItems = items.OrderBy(item => item.RelativePath, StringComparer.OrdinalIgnoreCase);
                break;
        }
        
        return orderedItems;
    }
    
    /// <summary>
    /// 获取排序描述
    /// </summary>
    private string GetSortDescription(SortBy sortBy, bool descending)
    {
        var direction = descending ? "倒序" : "正序";
        
        return sortBy switch
        {
            SortBy.Path => $"按路径排序 ({direction})",
            SortBy.LastWriteTime => $"按修改时间排序 ({direction})",
            SortBy.FileSize => $"按文件大小排序 ({direction})",
            SortBy.Type => $"按类型排序 ({direction})",
            _ => "按路径排序 (正序)"
        };
    }
    
    /// <summary>
    /// 按目录分组显示
    /// </summary>
    private void DisplayGroupedByDirectory(StringBuilder stringBuilder, List<FileSystemItem> items, string baseDirectory)
    {
        // 按目录分组
        var groupedByDirectory = items
            .GroupBy(item => item.DirectoryName)
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase);
        
        foreach (var group in groupedByDirectory)
        {
            if (!string.IsNullOrEmpty(group.Key))
            {
                stringBuilder.AppendLine($"目录: {group.Key}");
                stringBuilder.AppendLine(new string('-', 80));
            }
            
            // 在组内按名称排序
            var sortedInGroup = group.OrderBy(item => item.FileName, StringComparer.OrdinalIgnoreCase);
            
            foreach (var item in sortedInGroup)
            {
                DisplayItem(stringBuilder, item, baseDirectory);
            }
            
            stringBuilder.AppendLine();
        }
    }
    
    /// <summary>
    /// 平铺列表显示
    /// </summary>
    private void DisplayFlatList(StringBuilder stringBuilder, List<FileSystemItem> items, string baseDirectory)
    {
        foreach (var item in items)
        {
            DisplayItem(stringBuilder, item, baseDirectory);
        }
    }
    
    /// <summary>
    /// 显示单个项目
    /// </summary>
    private void DisplayItem(StringBuilder stringBuilder, FileSystemItem item, string baseDirectory)
    {
        stringBuilder.AppendLine($"  [{(item.IsDirectory ? "目录" : "文件")}] {item.RelativePath}");
        
        if (!item.IsDirectory)
        {
            stringBuilder.AppendLine($"    大小: {FileSizeFormatter.FormatFileSize(item.Size)}");
        }
        
        stringBuilder.AppendLine($"    修改时间: {item.LastWriteTime:yyyy-MM-dd HH:mm:ss}");
        stringBuilder.AppendLine();
    }
}