using System;
using System.Collections.Generic;
using System.IO;

namespace StoneSharp.CodeProcessing.Utilities
{
    public static class CodeLanguageUtility
    {
        // 静态字典，存储文件扩展名与语言的映射关系
        private static readonly Dictionary<string, string> _languageMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { ".cs", "csharp" },
            { ".xml", "xml" },
            { ".xaml", "xml" },
            //{ ".sln", "xml" },
            { ".slnx", "xml" },
            { ".java", "java" },
            { ".py", "python" },
            { ".js", "javascript" },
            { ".ts", "typescript" },
            { ".html", "html" },
            { ".htm", "html" },
            { ".css", "css" },
            { ".json", "json" },
            { ".sql", "sql" },
            { ".cpp", "cpp" },
            { ".cxx", "cpp" },
            { ".hpp", "cpp" },
            { ".c", "c" },
            { ".h", "c" },
            { ".go", "go" },
            { ".php", "php" },
            { ".rb", "ruby" },
            { ".swift", "swift" },
            { ".kt", "kotlin" },
            { ".kts", "kotlin" },
            { ".sh", "bash" },
            { ".ps1", "powershell" },
            { ".bat", "batch" },
            { ".yaml", "yaml" },
            { ".yml", "yaml" },
            { ".md", "markdown" },
            { ".dockerfile", "dockerfile" }
        };

        /// <summary>
        /// 根据文件扩展名获取语言
        /// </summary>
        /// <param name="fileExtension">文件扩展名</param>
        /// <returns>语言名称，如果未找到则返回 "plaintext"</returns>
        public static string GetLanguageFromFileExtension(string fileExtension)
        {
            if (string.IsNullOrWhiteSpace(fileExtension))
            {
                throw new ArgumentException("文件扩展名不能为空。");
            }

            if (_languageMap.TryGetValue(fileExtension, out var language))
            {
                return language;
            }
            return "plaintext"; // 默认返回纯文本格式
        }

        /// <summary>
        /// 根据文件路径获取语言
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>语言名称，如果未找到则返回 "plaintext"</returns>
        public static string GetLanguageFromFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("文件路径不能为空。");
            }

            // 获取文件扩展名
            string fileExtension = Path.GetExtension(filePath);
            return GetLanguageFromFileExtension(fileExtension);
        }

        /// <summary>
        /// 注册新的语言映射
        /// </summary>
        /// <param name="fileExtension">文件扩展名</param>
        /// <param name="language">语言名称</param>
        public static void RegisterLanguage(string fileExtension, string language)
        {
            if (string.IsNullOrWhiteSpace(fileExtension) || string.IsNullOrWhiteSpace(language))
            {
                throw new ArgumentException("文件扩展名和语言名称不能为空。");
            }

            _languageMap[fileExtension] = language;
        }

        /// <summary>
        /// 移除语言映射
        /// </summary>
        /// <param name="fileExtension">文件扩展名</param>
        public static void UnregisterLanguage(string fileExtension)
        {
            if (string.IsNullOrWhiteSpace(fileExtension))
            {
                throw new ArgumentException("文件扩展名不能为空。");
            }

            _languageMap.Remove(fileExtension);
        }
    }
}