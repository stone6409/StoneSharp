using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace StoneSharp.CodeProcessing.CodeBlockExtractor
{
    /// <summary>
    /// 目录结构解析器 - 专门解析树形目录结构文本
    /// </summary>
    public static class DirectoryStructureParser
    {
        /// <summary>
        /// 解析树形目录结构字符串
        /// </summary>
        /// <param name="directoryStructureText">目录结构文本</param>
        /// <returns>文件路径列表</returns>
        public static List<string> ParseDirectoryStructure(string directoryStructureText)
        {
            List<string> filePaths = new List<string>();

            if (string.IsNullOrEmpty(directoryStructureText))
                return filePaths;

            string[] lines = directoryStructureText.Split('\n');
            Stack<string> currentPath = new Stack<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].TrimEnd('\r');

                // 跳过空行和注释行
                if (string.IsNullOrWhiteSpace(line) || line.Contains("(文件夹图标)") || line.Contains("(文件图标)"))
                    continue;

                // 计算缩进级别
                int indentLevel = CalculateIndentLevel(line);

                // 调整当前路径栈
                while (currentPath.Count > indentLevel)
                {
                    currentPath.Pop();
                }

                // 解析当前行
                string itemName = ExtractItemName(line);

                if (string.IsNullOrEmpty(itemName))
                    continue;

                // 判断是文件还是文件夹
                bool isDirectory = IsDirectoryItem(line, i, lines);

                if (isDirectory)
                {
                    // 如果是目录，压入栈
                    currentPath.Push(itemName);
                }
                else
                {
                    // 构建完整路径
                    string fullPath = BuildFullPath(currentPath, itemName);

                    // 如果是文件，添加到列表
                    filePaths.Add(fullPath);
                }
            }

            // 检查解析出的路径是否合法
            if (!IsDirectoryStructureValid(filePaths))
            {
                Console.WriteLine("解析出的目录结构不合法，返回空列表");
                return new List<string>();
            }

            return filePaths;
        }

        /// <summary>
        /// 检查目录结构是否合法
        /// </summary>
        /// <param name="filePaths">解析出的文件路径列表</param>
        /// <returns>如果目录结构合法返回true，否则返回false</returns>
        private static bool IsDirectoryStructureValid(List<string> filePaths)
        {
            // 如果没有任何路径，认为不合法
            if (filePaths == null || filePaths.Count == 0)
            {
                Console.WriteLine("目录结构为空");
                return false;
            }

            // 检查每个路径是否合法
            foreach (string path in filePaths)
            {
                if (!IsPathValid(path))
                {
                    Console.WriteLine($"发现非法路径: {path}");
                    return false;
                }
            }

            // 检查是否有明显的目录结构特征
            bool hasValidExtensions = false;
            foreach (string path in filePaths)
            {
                string fileName = Path.GetFileName(path);
                if (HasFileExtension(fileName))
                {
                    hasValidExtensions = true;
                    break;
                }
            }

            if (!hasValidExtensions)
            {
                Console.WriteLine("没有发现有效的文件扩展名");
                return false;
            }

            Console.WriteLine($"目录结构合法，包含 {filePaths.Count} 个有效路径");
            return true;
        }

        /// <summary>
        /// 检查单个路径是否合法
        /// </summary>
        private static bool IsPathValid(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            // 检查路径是否包含非法字符
            char[] invalidChars = Path.GetInvalidPathChars();
            if (path.IndexOfAny(invalidChars) >= 0)
                return false;

            // 检查路径是否太长
            if (path.Length > 260) // Windows路径最大长度
                return false;

            // 检查路径格式是否合理
            // 路径应该至少包含一个斜杠分隔符
            if (!path.Contains("/") && !path.Contains("\\"))
                return false;

            // 检查路径是否以斜杠开头（相对路径不应该以斜杠开头）
            if (path.StartsWith("/") || path.StartsWith("\\"))
                return false;

            return true;
        }

        /// <summary>
        /// 从目录结构中查找根路径
        /// </summary>
        private static string FindRootPath(string[] lines)
        {
            foreach (string line in lines)
            {
                string trimmedLine = line.TrimEnd('\r').Trim();

                // 查找根目录模式，如 "FtpClientUI/" 或 "ProjectName/"
                if (trimmedLine.EndsWith("/") && !trimmedLine.StartsWith(" ") && !trimmedLine.StartsWith("│") && !trimmedLine.StartsWith("├") && !trimmedLine.StartsWith("└"))
                {
                    return trimmedLine;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 计算缩进级别
        /// </summary>
        private static int CalculateIndentLevel(string line)
        {
            int indent = 0;
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == ' ' || line[i] == '│' || line[i] == '├' || line[i] == '└' || line[i] == '─')
                {
                    // 这些字符表示缩进或连接线
                    indent++;
                }
                else
                {
                    break;
                }
            }
            return indent / 4; // 假设每个级别使用4个空格
        }

        /// <summary>
        /// 提取项目名称
        /// </summary>
        private static string ExtractItemName(string line)
        {
            // 移除缩进和连接字符
            string cleanedLine = line.TrimStart(' ', '│', '├', '└', '─', ' ');

            // 处理各种注释格式
            cleanedLine = RemoveComments(cleanedLine);
            cleanedLine = cleanedLine.TrimEnd('\\', '/');

            return cleanedLine;
        }

        /// <summary>
        /// 移除各种格式的注释
        /// </summary>
        private static string RemoveComments(string line)
        {
            string result = line;

            // 定义各种注释模式及其处理方式
            var commentPatterns = new List<(string, Func<string, int, bool>)>
            {
                (" (", (s, index) => index > 0),  // " (" 格式
                (" #", (s, index) => index == 0 || (index > 0 && char.IsWhiteSpace(s[index - 1]))),  // "#" 注释
                (" //", (s, index) => true),  // "//" 注释
                (" --", (s, index) => true),  // "--" 注释（SQL等）
                (" /*", (s, index) => true)   // "/*" 注释开始
            };

            foreach (var (pattern, isValidComment) in commentPatterns)
            {
                int index = result.IndexOf(pattern);
                if (index >= 0 && isValidComment(result, index))
                {
                    result = result.Substring(0, index).Trim();
                    break; // 找到一个注释就停止，避免重复处理
                }
            }

            return result;
        }

        /// <summary>
        /// 判断是否为目录项目
        /// </summary>
        private static bool IsDirectoryItem(string line, int currentIndex, string[] lines)
        {
            string itemName = ExtractItemName(line);

            // 如果以斜杠结尾，肯定是目录
            if (itemName.EndsWith("/"))
                return true;

            // 检查是否有文件扩展名
            if (HasFileExtension(itemName))
                return false;

            // 检查下一行是否包含子项目
            if (currentIndex + 1 < lines.Length)
            {
                string nextLine = lines[currentIndex + 1].TrimEnd('\r');
                int nextIndent = CalculateIndentLevel(nextLine);
                int currentIndent = CalculateIndentLevel(line);

                // 如果下一行缩进更多，说明当前项目是目录
                return nextIndent > currentIndent;
            }

            return false;
        }

        /// <summary>
        /// 检查是否有文件扩展名
        /// </summary>
        private static bool HasFileExtension(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            int lastDotIndex = fileName.LastIndexOf('.');
            if (lastDotIndex <= 0 || lastDotIndex >= fileName.Length - 1)
                return false;

            string extension = fileName.Substring(lastDotIndex + 1).ToLower();
            string[] validExtensions = {
                "cs", "xaml", "xml", "csproj", "json", "txt", "md",
                "py", "js", "ts", "html", "css", "sql", "java",
                "cpp", "c", "php", "rb", "go", "rs", "swift",
                "kt", "dart", "pl", "r", "scala", "hs", "ex",
                "clj", "fs", "vb", "sh", "ps1", "yml", "yaml", "png"
            };

            return Array.IndexOf(validExtensions, extension) >= 0;
        }

        /// <summary>
        /// 构建完整路径
        /// </summary>
        private static string BuildFullPath(Stack<string> currentPath, string itemName)
        {
            // 反转栈以获取正确顺序
            var pathArray = currentPath.ToArray();
            Array.Reverse(pathArray);

            StringBuilder pathBuilder = new StringBuilder();

            foreach (var pathSegment in pathArray)
            {
                if (!string.IsNullOrEmpty(pathSegment))
                {
                    if (pathBuilder.Length > 0)
                        pathBuilder.Append('\\');
                    pathBuilder.Append(pathSegment);
                }
            }

            if (pathBuilder.Length > 0)
                pathBuilder.Append('\\');
            pathBuilder.Append(itemName);

            return pathBuilder.ToString();
        }

        /// <summary>
        /// 从Markdown内容中提取目录结构部分（增强版，支持继续查找）
        /// </summary>
        /// <param name="markdownContent">Markdown内容</param>
        /// <param name="startIndex">开始搜索的位置</param>
        /// <returns>目录结构提取结果，包含文本和结束位置</returns>
        public static DirectoryStructureExtractionResult ExtractDirectoryStructureFromMarkdown(string markdownContent, int startIndex = 0)
        {
            try
            {
                if (string.IsNullOrEmpty(markdownContent))
                    return DirectoryStructureExtractionResult.Failed();

                string[] lines = markdownContent.Split('\n');
                string content = string.Join("\n", lines);

                // 定义可能的目录结构标题关键词
                string[] directoryKeywords = {
                    "目录结构",
                    "项目结构",
                    "文件结构",
                    "目录树",
                    "项目目录",
                    "文件目录",
                    "directory structure",
                    "project structure",
                    "file structure",
                    "directory tree",
                    "project directory",
                    "file directory"
                };

                int directoryIndex = -1;

                // 从指定位置开始查找关键词
                string contentToSearch = content;
                if (startIndex > 0 && startIndex < content.Length)
                {
                    contentToSearch = content.Substring(startIndex);
                }

                // 尝试查找任何一个关键词
                foreach (string keyword in directoryKeywords)
                {
                    directoryIndex = contentToSearch.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
                    if (directoryIndex != -1)
                    {
                        // 调整索引到原始内容中的位置
                        if (startIndex > 0)
                        {
                            directoryIndex += startIndex;
                        }
                        Console.WriteLine($"找到目录结构标题 '{keyword}' 在位置 {directoryIndex}");
                        break;
                    }
                }

                if (directoryIndex == -1)
                {
                    Console.WriteLine("未找到任何目录结构标题");
                    return DirectoryStructureExtractionResult.Failed();
                }

                // 从该位置开始提取
                string remainingContent = content.Substring(directoryIndex);

                // 查找第一个代码块
                int codeBlockStart = remainingContent.IndexOf("```");
                if (codeBlockStart == -1)
                {
                    Console.WriteLine("未找到代码块开始标记");
                    return DirectoryStructureExtractionResult.Failed();
                }

                // 跳过代码块开始标记
                int contentStart = codeBlockStart + 3;

                // 查找代码块结束标记
                int codeBlockEnd = remainingContent.IndexOf("```", contentStart);
                if (codeBlockEnd == -1)
                {
                    Console.WriteLine("未找到代码块结束标记");
                    return DirectoryStructureExtractionResult.Failed();
                }

                // 提取代码块内容
                string directoryStructure = remainingContent.Substring(contentStart, codeBlockEnd - contentStart).Trim();

                // 计算结束位置：directoryIndex + codeBlockEnd + 3（三个反引号）
                int endPosition = directoryIndex + codeBlockEnd + 3;

                Console.WriteLine($"成功提取目录结构，长度: {directoryStructure.Length}，结束位置: {endPosition}");
                return new DirectoryStructureExtractionResult(directoryStructure, endPosition);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"提取目录结构时出错: {ex.Message}");
                return DirectoryStructureExtractionResult.Failed();
            }
        }

        /// <summary>
        /// 从Markdown内容中提取目录结构部分（向后兼容的版本）
        /// </summary>
        /// <param name="markdownContent">Markdown内容</param>
        /// <returns>目录结构文本</returns>
        public static string ExtractDirectoryStructureFromMarkdown(string markdownContent)
        {
            var result = ExtractDirectoryStructureFromMarkdown(markdownContent, 0);
            return result.DirectoryStructureText;
        }

        /// <summary>
        /// 从Markdown内容中查找合法的目录结构
        /// </summary>
        /// <param name="markdownContent">Markdown内容</param>
        /// <returns>找到的目录路径列表，如果未找到则返回空列表</returns>
        public static List<string> FindDirectoryStructure(string markdownContent)
        {
            List<string> directoryPaths = new List<string>();
            int searchStartIndex = 0;

            // 循环查找目录结构，直到找到合法的目录结构或搜索到文档末尾
            while (true)
            {
                Console.WriteLine($"尝试提取目录结构，搜索起始位置: {searchStartIndex}");

                // 提取目录结构
                DirectoryStructureExtractionResult extractionResult = ExtractDirectoryStructureFromMarkdown(markdownContent, searchStartIndex);

                if (!extractionResult.Success)
                {
                    Console.WriteLine("未找到目录结构，停止查找");
                    break;
                }

                // 解析目录结构
                directoryPaths = ParseDirectoryStructure(extractionResult.DirectoryStructureText);

                // 如果解析出合法的目录结构，停止查找
                if (directoryPaths != null && directoryPaths.Count > 0)
                {
                    Console.WriteLine($"成功找到合法的目录结构，包含 {directoryPaths.Count} 个文件路径");
                    break;
                }
                else
                {
                    Console.WriteLine("解析出的目录结构不合法，继续查找...");

                    // 使用提取结果中的结束位置作为下一次搜索的开始位置
                    if (extractionResult.EndPosition > 0 && extractionResult.EndPosition < markdownContent.Length)
                    {
                        searchStartIndex = extractionResult.EndPosition;
                        Console.WriteLine($"下一次搜索从位置 {searchStartIndex} 开始");

                        // 检查是否已搜索到文档末尾
                        if (searchStartIndex >= markdownContent.Length)
                        {
                            Console.WriteLine("已搜索到文档末尾，停止查找");
                            break;
                        }
                    }
                    else
                    {
                        // 如果结束位置无效，停止查找
                        Console.WriteLine("提取结果中的结束位置无效，停止查找");
                        break;
                    }
                }
            }

            // 如果最终还是没有找到合法的目录结构，记录警告
            if (directoryPaths == null || directoryPaths.Count == 0)
            {
                Console.WriteLine("警告: 未能找到合法的目录结构");
                return new List<string>();
            }
            else
            {
                Console.WriteLine($"找到目录结构，包含 {directoryPaths.Count} 个文件路径");
                return directoryPaths;
            }
        }
    }
}