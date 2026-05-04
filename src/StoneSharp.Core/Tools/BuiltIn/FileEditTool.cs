using System.ComponentModel;
using System.Text;

namespace StoneSharp.Core.Tools.BuiltIn;

/// <summary>
/// 文件编辑插件，基于FileEditTool.ts重新实现，专注于字符串替换功能
/// </summary>
public sealed class FileEditTool
{
    private readonly IFileSystem _fileSystem;

    /// <summary>
    /// 构造函数
    /// </summary>
    public FileEditTool() : this(new DiskFileSystem())
    {
    }

    /// <summary>
    /// 构造函数（支持依赖注入）
    /// </summary>
    public FileEditTool(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    /// <summary>
    /// 编辑文件内容 - 替换字符串
    /// </summary>
    [KernelFunction("EditFile"), Description("编辑文件内容，替换字符串")]
    public async Task<string> EditFileAsync(
        [Description("文件路径")] string filePath,
        [Description("要替换的旧字符串")] string oldString,
        [Description("替换的新字符串")] string newString,
        [Description("是否替换所有匹配项（可选，默认为false）")] bool replaceAll = false)
    {
        try
        {
            // 检查文件是否存在
            if (!_fileSystem.FileExists(filePath))
            {
                // 如果文件不存在且oldString为空，则创建新文件
                if (string.IsNullOrEmpty(oldString))
                {
                    await CreateNewFile(filePath, newString);
                    return $"创建新文件成功: {filePath}";
                }
                else
                {
                    return $"文件不存在: {filePath}";
                }
            }

            // 读取文件内容
            Encoding fileEncoding = _fileSystem.DetectFileEncoding(filePath);
            var originalContent = await _fileSystem.ReadAllTextAsync(filePath, fileEncoding);

            // 标准化换行符
            originalContent = CodeUtility.NormalizeLineEndings(originalContent);
            oldString = CodeUtility.NormalizeLineEndings(oldString);
            newString = CodeUtility.NormalizeLineEndings(newString);

            // 处理空字符串的情况
            if (string.IsNullOrEmpty(oldString))
            {
                // 如果文件内容为空，则替换
                if (string.IsNullOrEmpty(originalContent.Trim()))
                {
                    await _fileSystem.WriteAllTextAsync(filePath, newString, fileEncoding);
                    return $"文件内容替换成功: {filePath}";
                }
                else
                {
                    return "无法替换：文件已存在且非空，但旧字符串为空";
                }
            }

            // 处理引号样式
            var actualOldString = FindActualString(originalContent, oldString);
            if (string.IsNullOrEmpty(actualOldString))
            {
                return $"未找到要替换的字符串: {oldString}";
            }

            var actualNewString = PreserveQuoteStyle(oldString, actualOldString, newString);

            // 执行替换
            string updatedContent;
            if (replaceAll)
            {
                updatedContent = originalContent.Replace(actualOldString, actualNewString);
            }
            else
            {
                // 只替换第一个匹配项
                int index = originalContent.IndexOf(actualOldString, StringComparison.Ordinal);
                if (index == -1)
                {
                    return $"未找到要替换的字符串: {oldString}";
                }

                updatedContent = originalContent.Remove(index, actualOldString.Length)
                    .Insert(index, actualNewString);
            }

            // 检查是否有多个匹配项但未设置replaceAll
            if (!replaceAll)
            {
                var matches = CountMatches(originalContent, actualOldString);
                if (matches > 1)
                {
                    return $"找到 {matches} 个匹配项，但 replaceAll 为 false。要替换所有匹配项，请设置 replaceAll 为 true。";
                }
            }

            // 写回文件
            await _fileSystem.WriteAllTextAsync(filePath, updatedContent, fileEncoding);

            // 计算替换次数
            int replaceCount = replaceAll ?
                CountMatches(originalContent, actualOldString) :
                originalContent.Contains(actualOldString) ? 1 : 0;

            // 返回结果
            var result = new StringBuilder();
            result.AppendLine($"文件编辑成功: {filePath}");
            result.AppendLine($"替换次数: {replaceCount}");
            result.AppendLine($"旧字符串: {actualOldString}");
            result.AppendLine($"新字符串: {actualNewString}");
            result.AppendLine($"全部替换: {replaceAll}");

            return result.ToString();
        }
        catch (Exception ex)
        {
            return $"编辑文件时出错: {ex.Message}";
        }
    }

    /// <summary>
    /// 创建新文件
    /// </summary>
    private async Task CreateNewFile(string filePath, string content)
    {
        // 确保目录存在
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // 写入文件
        await _fileSystem.WriteAllTextAsync(filePath, content, Encoding.UTF8);
    }

    /// <summary>
    /// 在文件中查找实际的字符串（处理引号样式）
    /// </summary>
    private string FindActualString(string fileContent, string searchString)
    {
        // 如果直接找到，返回原字符串
        if (fileContent.Contains(searchString))
        {
            return searchString;
        }

        // 检查引号样式变体
        var quoteVariants = GetQuoteVariants(searchString);
        foreach (var variant in quoteVariants)
        {
            if (fileContent.Contains(variant))
            {
                return variant;
            }
        }

        return null;
    }

    /// <summary>
    /// 获取字符串的引号样式变体
    /// </summary>
    private List<string> GetQuoteVariants(string text)
    {
        var variants = new List<string>();

        // 直引号和弯引号变体
        variants.Add(text.Replace("'", "‘").Replace("'", "’"));
        variants.Add(text.Replace("\"", "“").Replace("\"", "”"));
        variants.Add(text.Replace("'", "`").Replace("'", "´"));

        // 反向变体
        variants.Add(text.Replace("‘", "'").Replace("’", "'"));
        variants.Add(text.Replace("“", "\"").Replace("”", "\""));

        return variants.Distinct().ToList();
    }

    /// <summary>
    /// 保持引号样式
    /// </summary>
    private string PreserveQuoteStyle(string originalOldString, string actualOldString, string newString)
    {
        // 如果实际找到的字符串与原搜索字符串相同，直接返回新字符串
        if (originalOldString == actualOldString)
        {
            return newString;
        }

        // 检查引号样式的差异
        var result = newString;

        // 处理单引号
        if (originalOldString.Contains("'") && actualOldString.Contains("‘"))
        {
            result = result.Replace("'", "‘");
        }
        else if (originalOldString.Contains("'") && actualOldString.Contains("’"))
        {
            result = result.Replace("'", "’");
        }
        else if (originalOldString.Contains("‘") && actualOldString.Contains("'"))
        {
            result = result.Replace("‘", "'").Replace("’", "'");
        }

        // 处理双引号
        if (originalOldString.Contains("\"") && actualOldString.Contains("“"))
        {
            result = result.Replace("\"", "“");
        }
        else if (originalOldString.Contains("\"") && actualOldString.Contains("”"))
        {
            result = result.Replace("\"", "”");
        }
        else if (originalOldString.Contains("“") && actualOldString.Contains("\""))
        {
            result = result.Replace("“", "\"").Replace("”", "\"");
        }

        return result;
    }

    /// <summary>
    /// 计算匹配次数
    /// </summary>
    private int CountMatches(string text, string search)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(search))
            return 0;

        int count = 0;
        int index = 0;

        while ((index = text.IndexOf(search, index, StringComparison.Ordinal)) != -1)
        {
            count++;
            index += search.Length;
        }

        return count;
    }

    /// <summary>
    /// 查看文件内容（简化版）
    /// </summary>
    //[KernelFunction, Description("查看文件内容")]
    //public async Task<string> ViewFileContentAsync(
    //    [Description("文件路径")] string filePath)
    //{
    //    if (!_fileSystem.FileExists(filePath))
    //    {
    //        return $"文件不存在: {filePath}";
    //    }

    //    try
    //    {
    //        Encoding fileEncoding = _fileSystem.DetectFileEncoding(filePath);
    //        var content = await _fileSystem.ReadAllTextAsync(filePath, fileEncoding);

    //        var result = new StringBuilder();
    //        result.AppendLine($"文件: {filePath}");
    //        result.AppendLine($"编码: {fileEncoding.EncodingName}");
    //        result.AppendLine($"大小: {content.Length} 字符");
    //        result.AppendLine("=".PadRight(80, '='));
    //        result.AppendLine(content);
    //        result.AppendLine("=".PadRight(80, '='));

    //        return result.ToString();
    //    }
    //    catch (Exception ex)
    //    {
    //        return $"查看文件内容时出错: {ex.Message}";
    //    }
    //}
}