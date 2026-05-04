using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace StoneSharp.CodeProcessing.CodeBlockExtractor
{
    /// <summary>
    /// Markdown代码块提取器 - 专门负责从Markdown内容中提取代码块的逻辑
    /// </summary>
    public static class MarkdownCodeBlockExtractor
    {
        /// <summary>
        /// 从Markdown内容中提取所有代码块
        /// </summary>
        public static List<CodeBlock> ExtractCodeBlocks(string markdownContent)
        {
            List<CodeBlock> codeBlocks = new List<CodeBlock>();

            // 按行分割内容，便于查找上一行
            string[] lines = markdownContent.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                // 检查是否以```开头（代码块开始）
                if (line.TrimStart().StartsWith("```"))
                {
                    // 提取语言（如果有）
                    string language = ExtractLanguage(line);

                    // 收集代码块内容
                    StringBuilder codeBuilder = new StringBuilder();
                    int j = i + 1;

                    // 遍历直到找到代码块结束标记
                    while (j < lines.Length && !lines[j].TrimStart().StartsWith("```"))
                    {
                        // 移除行尾的回车符，只保留换行符
                        string cleanLine = lines[j].TrimEnd('\r');
                        codeBuilder.AppendLine(cleanLine);
                        j++;
                    }

                    if (j < lines.Length)
                    {
                        string codeContent = codeBuilder.ToString().TrimEnd('\n', '\r');

                        var (fileName, source) = ExtractFileName(lines, i, codeContent, language, codeBlocks.Count);
                        codeBlocks.Add(new CodeBlock
                        {
                            CodeLanguage = string.IsNullOrEmpty(language) ? "text" : language,
                            CodeContent = codeContent,
                            FileName = fileName,
                            Source = source  // 设置文件名来源
                        });

                        // 更新索引到代码块结束位置
                        i = j;
                    }
                }
            }

            return codeBlocks;
        }

        /// <summary>
        /// 从代码块标记行中提取语言
        /// </summary>
        private static string ExtractLanguage(string codeBlockLine)
        {
            // 移除开头的```和空格
            string line = codeBlockLine.TrimStart();
            if (line.StartsWith("```"))
            {
                line = line.Substring(3).TrimStart();

                // 如果后面有语言标识，提取它
                if (!string.IsNullOrEmpty(line))
                {
                    // 语言标识通常是单个单词或简短标识符
                    Match match = Regex.Match(line, @"^(\w+)");
                    if (match.Success)
                    {
                        return match.Value;
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 从代码块的上一行提取完整文件路径（包括目录结构）
        /// </summary>
        private static (string fileName, FileNameSource source) ExtractFileName(string[] lines, int currentIndex, string codeContent, string language, int blockIndex)
        {
            if (language == "c#" || language == "cs" || language == "csharp")
            {
                string extractedPath = CodePathExtractor.ExtractPathFromCodeContent(codeContent, language);
                if (!string.IsNullOrEmpty(extractedPath))
                {
                    string fileName = Path.GetFileName(extractedPath);
                    return (fileName, FileNameSource.Extracted);
                }

                // 文件非完整片段
            }
            else
            {
                // 从代码块的上一行提取完整文件路径（包括目录结构）
                // 原始方法ExtractFullFilePathFromPreviousLine

                // 检查上一行（跳过空行）
                for (int i = currentIndex - 1; i >= 0; i--)
                {
                    string previousLine = lines[i].Trim();

                    // 跳过空行
                    if (string.IsNullOrEmpty(previousLine))
                        continue;

                    // 尝试从上一行提取文件路径
                    string extractedPath = TryExtractFilePath(previousLine);

                    if (!string.IsNullOrEmpty(extractedPath))
                    {
                        // 检查后缀名是否与语言匹配
                        if (IsFileExtensionMatchingLanguage(extractedPath, language))
                        {
                            return (extractedPath, FileNameSource.Extracted);
                        }
                    }

                    break; // 只检查第一行非空的上行
                }

                // 检查下一行（跳过空行）
                for (int i = currentIndex + 1; i < lines.Length; i++)
                {
                    string nextLine = lines[i].Trim();

                    // 跳过空行
                    if (string.IsNullOrEmpty(nextLine))
                        continue;

                    // 尝试从下一行提取文件路径
                    string extractedPath = TryExtractFilePath(nextLine);

                    if (!string.IsNullOrEmpty(extractedPath))
                    {
                        // 检查后缀名是否与语言匹配
                        if (IsFileExtensionMatchingLanguage(extractedPath, language))
                        {
                            return (extractedPath, FileNameSource.Extracted);
                        }
                    }

                    break; // 只检查第一行非空的下行
                }
            }

            // 如果没有找到文件路径，生成默认文件名
            return (GenerateDefaultFileName(language, blockIndex), FileNameSource.Generated);
        }

        /// <summary>
        /// 尝试从文本行中提取文件路径
        /// </summary>
        private static string TryExtractFilePath(string line)
        {
            const string commonExtensions = @"(?:csproj|cs|xaml|xml|json|txt|md|py|js|ts|html|css|sql|java|cpp|c|php|rb|go|rs|swift|kt|dart|png|jpg|jpeg|gif|ico)";

            // 模式1：括号中的文件路径
            string bracketPattern = $@"[（\(]([^）\)]+\.{commonExtensions})[）\)]";
            Match bracketMatch = Regex.Match(line, bracketPattern, RegexOptions.IgnoreCase);
            if (bracketMatch.Success && bracketMatch.Groups.Count > 1)
            {
                return bracketMatch.Groups[1].Value;
            }

            // 合并的模式：完整文件路径或Markdown标题中的路径
            string pathPattern = $@"([\w\-\./\\]+\.{commonExtensions})";
            Match pathMatch = Regex.Match(line, pathPattern, RegexOptions.IgnoreCase);
            if (pathMatch.Success)
            {
                // 如果是Markdown标题，直接返回
                if (line.StartsWith("#"))
                {
                    return pathMatch.Value;
                }
                // 其他情况也返回（原模式2的逻辑）
                return pathMatch.Value;
            }

            // 模式4：通用文件路径模式
            Match genericMatch = Regex.Match(line, @"([\w\-\./\\]+\.[\w]+)");
            if (genericMatch.Success)
            {
                string potentialPath = genericMatch.Value;
                if (IsValidFilePath(potentialPath))
                {
                    return potentialPath;
                }
            }

            return null;
        }

        /// <summary>
        /// 检查是否为有效的文件路径
        /// </summary>
        private static bool IsValidFilePath(string filePath)
        {
            try
            {
                // 获取文件名部分
                string fileName = Path.GetFileName(filePath);

                // 检查文件名是否有效
                if (string.IsNullOrEmpty(fileName))
                    return false;

                // 检查文件名是否包含非法字符
                char[] invalidChars = Path.GetInvalidFileNameChars();
                foreach (char c in fileName)
                {
                    if (Array.IndexOf(invalidChars, c) >= 0)
                    {
                        return false;
                    }
                }

                // 检查是否有扩展名
                int lastDotIndex = fileName.LastIndexOf('.');
                if (lastDotIndex <= 0 || lastDotIndex >= fileName.Length - 1)
                {
                    return false; // 没有扩展名或扩展名在开头
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 生成默认文件名
        /// </summary>
        private static string GenerateDefaultFileName(string language, int blockIndex)
        {
            string extension = GetFileExtension(language);
            return $"codeblock_{blockIndex + 1}{extension}";
        }

        /// <summary>
        /// 根据语言获取文件扩展名
        /// </summary>
        private static string GetFileExtension(string language)
        {
            if (string.IsNullOrEmpty(language))
                return ".txt";

            string normalizedLanguage = language.ToLower();

            if (normalizedLanguage == "csharp" || normalizedLanguage == "cs")
                return ".cs";
            else if (normalizedLanguage == "xml")
                return ".xml";
            else if (normalizedLanguage == "xaml")
                return ".xaml";
            else if (normalizedLanguage == "javascript" || normalizedLanguage == "js")
                return ".js";
            else if (normalizedLanguage == "typescript" || normalizedLanguage == "ts")
                return ".ts";
            else if (normalizedLanguage == "html")
                return ".html";
            else if (normalizedLanguage == "css")
                return ".css";
            else if (normalizedLanguage == "python" || normalizedLanguage == "py")
                return ".py";
            else if (normalizedLanguage == "java")
                return ".java";
            else if (normalizedLanguage == "cpp" || normalizedLanguage == "c++")
                return ".cpp";
            else if (normalizedLanguage == "c")
                return ".c";
            else if (normalizedLanguage == "php")
                return ".php";
            else if (normalizedLanguage == "sql")
                return ".sql";
            else if (normalizedLanguage == "json")
                return ".json";
            else if (normalizedLanguage == "yaml" || normalizedLanguage == "yml")
                return ".yml";
            else if (normalizedLanguage == "markdown" || normalizedLanguage == "md")
                return ".md";
            else if (normalizedLanguage == "bash" || normalizedLanguage == "shell")
                return ".sh";
            else if (normalizedLanguage == "powershell" || normalizedLanguage == "ps")
                return ".ps1";
            else if (normalizedLanguage == "ruby" || normalizedLanguage == "rb")
                return ".rb";
            else if (normalizedLanguage == "go")
                return ".go";
            else if (normalizedLanguage == "rust" || normalizedLanguage == "rs")
                return ".rs";
            else if (normalizedLanguage == "swift")
                return ".swift";
            else if (normalizedLanguage == "kotlin" || normalizedLanguage == "kt")
                return ".kt";
            else if (normalizedLanguage == "dart")
                return ".dart";
            else if (normalizedLanguage == "lua")
                return ".lua";
            else if (normalizedLanguage == "perl" || normalizedLanguage == "pl")
                return ".pl";
            else if (normalizedLanguage == "r")
                return ".r";
            else if (normalizedLanguage == "scala")
                return ".scala";
            else if (normalizedLanguage == "haskell" || normalizedLanguage == "hs")
                return ".hs";
            else if (normalizedLanguage == "elixir")
                return ".ex";
            else if (normalizedLanguage == "clojure" || normalizedLanguage == "clj")
                return ".clj";
            else if (normalizedLanguage == "fsharp" || normalizedLanguage == "fs")
                return ".fs";
            else if (normalizedLanguage == "vbnet" || normalizedLanguage == "vb")
                return ".vb";
            else
                return ".txt";
        }

        /// <summary>
        /// 检查文件路径后缀名是否与语言标识符匹配
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="language">语言标识符</param>
        /// <returns>如果匹配返回true，否则返回false</returns>
        private static bool IsFileExtensionMatchingLanguage(string filePath, string language)
        {
            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(language))
                return false;

            // 获取文件扩展名（包含点号）
            string extension = Path.GetExtension(filePath).ToLowerInvariant();

            // 如果文件没有扩展名，返回false
            if (string.IsNullOrEmpty(extension))
                return false;

            // 标准化语言标识符
            string normalizedLanguage = language.ToLowerInvariant().Trim();

            // 定义语言标识符与文件扩展名的映射关系
            var languageToExtensions = new Dictionary<string, string[]>
            {
                // C# 相关
                ["cs"] = new[] { ".cs" },
                ["csharp"] = new[] { ".cs" },

                // JavaScript 相关
                ["js"] = new[] { ".js" },
                ["javascript"] = new[] { ".js" },

                // TypeScript
                ["ts"] = new[] { ".ts" },
                ["typescript"] = new[] { ".ts" },

                // Java
                ["java"] = new[] { ".java" },

                // Python
                ["py"] = new[] { ".py" },
                ["python"] = new[] { ".py" },

                // Ruby
                ["rb"] = new[] { ".rb" },
                ["ruby"] = new[] { ".rb" },

                // Go
                ["go"] = new[] { ".go" },

                // Rust
                ["rs"] = new[] { ".rs" },
                ["rust"] = new[] { ".rs" },

                // C/C++
                ["c"] = new[] { ".c" },
                ["cpp"] = new[] { ".cpp", ".cc", ".cxx" },
                ["c++"] = new[] { ".cpp", ".cc", ".cxx" },
                ["h"] = new[] { ".h", ".hpp" },
                ["hpp"] = new[] { ".hpp", ".h" },

                // Web 相关
                ["html"] = new[] { ".html", ".htm" },
                ["htm"] = new[] { ".html", ".htm" },
                ["css"] = new[] { ".css" },

                // 数据格式
                ["xml"] = new[] { ".xml", ".xaml", ".csproj" },
                ["json"] = new[] { ".json" },

                // Markdown
                ["md"] = new[] { ".md", ".markdown" },
                ["markdown"] = new[] { ".md", ".markdown" },

                // SQL
                ["sql"] = new[] { ".sql" },

                // 文本文件
                ["txt"] = new[] { ".txt" },
                ["text"] = new[] { ".txt" },

                // 配置文件
                ["yaml"] = new[] { ".yaml", ".yml" },
                ["yml"] = new[] { ".yaml", ".yml" },
                ["toml"] = new[] { ".toml" },
                ["ini"] = new[] { ".ini" },

                // Shell
                ["sh"] = new[] { ".sh" },
                ["bash"] = new[] { ".sh", ".bash" },
                ["powershell"] = new[] { ".ps1" },
                ["ps1"] = new[] { ".ps1" },

                // Batch
                ["bat"] = new[] { ".bat", ".cmd" },
                ["cmd"] = new[] { ".bat", ".cmd" },
            };

            // 检查映射关系中是否存在该语言
            if (languageToExtensions.TryGetValue(normalizedLanguage, out var validExtensions))
            {
                return Array.Exists(validExtensions, ext => ext == extension);
            }

            // 如果没有明确的映射关系，尝试一些通用的匹配规则
            switch (normalizedLanguage)
            {
                case string lang when lang.StartsWith("c"):
                    // 对于C语言家族，检查常见的C/C++扩展名
                    return extension == ".c" || extension == ".cpp" || extension == ".cc" || extension == ".cxx" || extension == ".h" || extension == ".hpp";

                case string lang when lang.Contains("script"):
                    // 对于脚本语言，检查常见的脚本扩展名
                    return extension == ".js" || extension == ".ts" || extension == ".py" || extension == ".rb" || extension == ".sh";

                default:
                    // 默认情况下，如果语言标识符恰好是扩展名（去掉点号），则认为是匹配的
                    // 例如：language="cs" 匹配 extension=".cs"
                    return extension.Substring(1) == normalizedLanguage;
            }
        }
    }
}