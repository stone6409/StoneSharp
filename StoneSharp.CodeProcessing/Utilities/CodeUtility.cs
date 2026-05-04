using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StoneSharp.CodeProcessing.CodeBlockExtractor;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace StoneSharp.CodeProcessing.Utilities
{
    public static class CodeUtility
    {
        public static bool ContainsCodeBlocks(string text)
        {
            // 检查Markdown代码块：```language ... ```
            Regex markdownCodeBlockRegex = new Regex(@"```\w*\s*\n.*?\n```", RegexOptions.Singleline);
            if (markdownCodeBlockRegex.IsMatch(text))
            {
                return true;
            }

            // 检查缩进的代码块（至少连续3行以4个空格或制表符开头）
            Regex indentedCodeBlockRegex = new Regex(@"(?:^|\n)(?:\s{4,}|\t+).*\n(?:\s{4,}|\t+).*\n(?:\s{4,}|\t+).*", RegexOptions.Multiline);
            return indentedCodeBlockRegex.IsMatch(text);
        }

        public static bool ExtractCodeBlock(string text, out string language, out string codeContent, bool isMatchBegin = false, bool isMatchEnd = false)
        {
            language = string.Empty;
            codeContent = string.Empty;

            // 根据 isMatchBegin 和 isMatchEnd 参数动态构建正则表达式
            string beginPattern = isMatchBegin ? @"^```" : @"```";
            string endPattern = isMatchEnd ? @"```$" : @"```";

            string codeBlockPattern = $@"{beginPattern}(\w+)\s*\r?\n(.*?)\r?\n{endPattern}";

            Regex regex = new Regex(codeBlockPattern, RegexOptions.Singleline);
            Match match = regex.Match(text);

            if (!match.Success)
            {
                return false; // 没有找到任何代码块
            }

            language = match.Groups[1].Value; // 提取语言类型
            codeContent = match.Groups[2].Value.Trim(); // 提取代码内容并去除首尾空格
            return true; // 成功提取代码块
        }

        public static string IndentCode(string code)
        {
            // 将代码拆分为行
            string[] lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            // 初始化最小缩进数
            int minIndentCount = int.MaxValue;

            // 查找最小缩进数
            foreach (string line in lines)
            {
                string trimmedLine = line.TrimStart();
                if (!string.IsNullOrEmpty(trimmedLine))
                {
                    int indentCount = line.Length - trimmedLine.Length;
                    if (indentCount < minIndentCount)
                    {
                        minIndentCount = indentCount;
                    }
                }
            }

            // 移除最小缩进数，确保所有行使用相同缩进
            string indent = new string(' ', minIndentCount);

            // 使用StringBuilder构建缩进后的代码
            StringBuilder indentedCode = new StringBuilder();

            foreach (string line in lines)
            {
                string trimmedLine = line.TrimStart();
                if (string.IsNullOrEmpty(trimmedLine))
                {
                    indentedCode.AppendLine(); // 空行保持不变
                }
                else
                {
                    indentedCode.AppendLine(line); // 非空行移除原有缩进，添加最小缩进
                }
            }

            return indentedCode.ToString();
        }

        public static string GetMatchFile(IEnumerable<string> matchingFiles, IEnumerable<string> projectFolders, string codeContent, string codeLanguage)
        {
            string matchFilePath = GetMatchFileSpecial(matchingFiles, codeContent);
            if (matchFilePath != null)
            {
                return matchFilePath;
            }

            string extractedPath = CodePathExtractor.ExtractPathFromCodeContent(codeContent, codeLanguage, projectFolders);
            if (!string.IsNullOrEmpty(extractedPath))
            {
                foreach (string folderPath in projectFolders)
                {
                    string folderName = Path.GetFileName(folderPath);
                    int firstSeparatorIndex = extractedPath.IndexOfAny(new[] { '/', '\\' });
                    if (firstSeparatorIndex == -1)
                    {
                        continue;
                    }

                    string extractedFolderName = extractedPath.Substring(0, firstSeparatorIndex);
                    if (folderName == extractedFolderName)
                    {
                        string relativePath = extractedPath.Substring(firstSeparatorIndex + 1);
                        return Path.Combine(folderPath, relativePath);
                    }
                }
            }

            string bestMatchFilePath = null;
            double highestSimilarity = 0.0;

            foreach (var filePath in matchingFiles)
            {
                if (File.Exists(filePath))
                {
                    string fileExtension = Path.GetExtension(filePath).ToLower();
                    string fileContent = File.ReadAllText(filePath);

                    // 计算文件内容与 codeContent 的相似度
                    double similarity = CalculateSimilarity(fileContent, codeContent);

                    // 如果相似度高于当前最高相似度，则更新最佳匹配文件
                    if (similarity > highestSimilarity)
                    {
                        highestSimilarity = similarity;
                        bestMatchFilePath = filePath;
                    }
                }
            }

            return bestMatchFilePath;
        }

        // 计算相似度的辅助方法
        private static double CalculateSimilarity(string fileContent, string codeContent)
        {
            // 计算 Levenshtein Distance（编辑距离）
            int distance = LevenshteinDistance(fileContent, codeContent);

            // 计算相似度
            double maxLength = Math.Max(fileContent.Length, codeContent.Length);
            return 1.0 - (distance / maxLength);
        }

        // 计算 Levenshtein Distance（编辑距离）的辅助方法
        private static int LevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] dp = new int[n + 1, m + 1];

            // 初始化 DP 表
            for (int i = 0; i <= n; i++) dp[i, 0] = i;
            for (int j = 0; j <= m; j++) dp[0, j] = j;

            // 填充 DP 表
            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;
                    dp[i, j] = Math.Min(
                        Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                        dp[i - 1, j - 1] + cost
                    );
                }
            }

            return dp[n, m];
        }

        public static string GetMatchFileSpecial(IEnumerable<string> matchingFiles, string codeContent)
        {
            foreach (var filePath in matchingFiles)
            {
                if (File.Exists(filePath))
                {
                    string fileExtension = Path.GetExtension(filePath).ToLower();
                    string fileContent = File.ReadAllText(filePath);

                    // 检查代码块是否与提取的标识符匹配
                    if ((fileExtension == ".cs" && IsCodeMatchCSharpFile(fileContent, codeContent)) ||
                        (fileExtension == ".xaml" && IsCodeMatchXamlFile(fileContent, codeContent)))
                    {
                        return filePath;
                    }
                }
            }

            return null;
        }

        //public static string GetMatchFileExtension(string codeContent)
        //{
        //    if (IsCodeMatchCSharpFile(codeContent))
        //    {
        //        return ".cs";
        //    }
        //    else if (IsCodeMatchXamlFile(codeContent))
        //    {
        //        return ".xaml";
        //    }

        //    return null;
        //}

        private static bool IsCodeMatchCSharpFile(string codeContent1, string codeContent2)
        {
            // 解析文件内容
            var fileSyntaxTree = CSharpSyntaxTree.ParseText(codeContent1);
            var fileRoot = fileSyntaxTree.GetRoot();

            // 解析代码块内容
            var codeSyntaxTree = CSharpSyntaxTree.ParseText(codeContent2);
            var codeRoot = codeSyntaxTree.GetRoot();

            // 提取文件内容中的顶级命名空间和第一个类型声明（类或接口）
            var fileNamespace = fileRoot.DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString();
            var fileTypeNode = fileRoot.DescendantNodes()
                .FirstOrDefault(node => node is ClassDeclarationSyntax || node is InterfaceDeclarationSyntax);
            string fileType = null;
            if (fileTypeNode is ClassDeclarationSyntax classDecl)
                fileType = classDecl.Identifier.Text;
            else if (fileTypeNode is InterfaceDeclarationSyntax interfaceDecl)
                fileType = interfaceDecl.Identifier.Text;

            // 提取代码块内容中的顶级命名空间和第一个类型声明（类或接口）
            var codeNamespace = codeRoot.DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString();
            var codeTypeNode = codeRoot.DescendantNodes()
                .FirstOrDefault(node => node is ClassDeclarationSyntax || node is InterfaceDeclarationSyntax);
            string codeType = null;
            if (codeTypeNode is ClassDeclarationSyntax codeClassDecl)
                codeType = codeClassDecl.Identifier.Text;
            else if (codeTypeNode is InterfaceDeclarationSyntax codeInterfaceDecl)
                codeType = codeInterfaceDecl.Identifier.Text;

            // 比较命名空间和类型名
            return !string.IsNullOrEmpty(codeNamespace) && !string.IsNullOrEmpty(codeType) && fileNamespace == codeNamespace && fileType == codeType;
        }

        private static bool IsCodeMatchCSharpFile(string codeContent)
        {
            // 解析代码块内容
            var codeSyntaxTree = CSharpSyntaxTree.ParseText(codeContent);
            var codeRoot = codeSyntaxTree.GetRoot();

            // 提取代码块内容中的顶级命名空间和第一个类名
            var codeNamespace = codeRoot.DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString();
            var codeClass = codeRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault()?.Identifier.Text;

            // 比较命名空间和类名
            return !string.IsNullOrEmpty(codeNamespace) && !string.IsNullOrEmpty(codeClass);
        }

        private static bool IsCodeMatchXamlFile(string codeContent1, string codeContent2)
        {
            try
            {
                // 解析第一个 XAML 文件
                var xamlDoc1 = XDocument.Parse(codeContent1);
                var rootElement1 = xamlDoc1.Root;

                // 解析第二个 XAML 文件
                var xamlDoc2 = XDocument.Parse(codeContent2);
                var rootElement2 = xamlDoc2.Root;

                // 比较根元素类型
                if (rootElement1?.Name != rootElement2?.Name)
                {
                    return false;
                }

                // 比较 x:Class 属性
                XNamespace xNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
                var xClass1 = rootElement1?.Attribute(xNamespace + "Class")?.Value;
                var xClass2 = rootElement2?.Attribute(xNamespace + "Class")?.Value;

                if (xClass1 != xClass2)
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                // 解析失败时返回 false
                return false;
            }
        }

        private static bool IsCodeMatchXamlFile(string codeContent)
        {
            try
            {
                // 解析第一个 XAML 文件
                var xamlDoc1 = XDocument.Parse(codeContent);
                var rootElement1 = xamlDoc1.Root;

                return rootElement1 != null;
            }
            catch (Exception)
            {
                // 解析失败时返回 false
                return false;
            }
        }

        public static void WriteAllText(string filePath, string content)
        {
            //File.WriteAllText(filePath, content, new UTF8Encoding(false));
            File.WriteAllText(filePath, content, Encoding.UTF8);
        }

        /// <summary>
        /// 写入文本到文件，如果文件已存在则保持原来的编码方式（使用增强编码探测）
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="content">要写入的内容</param>
        public static void WriteAllTextPreservingEncoding(string filePath, string content)
        {
            // 如果文件已经存在，则使用原来的编码方式
            if (File.Exists(filePath))
            {
                // 使用 FileEncodingDetector 的增强版本获取原始编码
                // GB2312为System.Text.DBCSCodePageEncoding
                Encoding originalEncoding = FileEncodingDetector.GetEncodingEnhanced(filePath);
                File.WriteAllText(filePath, content, originalEncoding);
            }
            else
            {
                // 文件不存在，使用 UTF-8 编码创建新文件
                File.WriteAllText(filePath, content, Encoding.UTF8);
            }
        }

        /// <summary>
        /// 异步写入文本到文件，如果文件已存在则保持原来的编码方式（使用增强编码探测）
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="content">要写入的内容</param>
        /// <returns>异步任务</returns>
        public static async Task WriteAllTextPreservingEncodingAsync(string filePath, string content)
        {
            // 如果文件已经存在，则使用原来的编码方式
            if (File.Exists(filePath))
            {
                // 使用 FileEncodingDetector 的增强版本获取原始编码
                // GB2312为System.Text.DBCSCodePageEncoding
                Encoding originalEncoding = FileEncodingDetector.GetEncodingEnhanced(filePath);

                // 异步写入文件
                using (var writer = new StreamWriter(filePath, false, originalEncoding))
                {
                    await writer.WriteAsync(content).ConfigureAwait(false);
                }
            }
            else
            {
                // 文件不存在，使用 UTF-8 编码创建新文件
                using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    await writer.WriteAsync(content).ConfigureAwait(false);
                }
            }
        }

        public static string NormalizeLineEndings(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // 将所有换行符统一替换为 \r\n
            text = text.Replace("\r\n", "\n") // 先替换 \r\n 为 \n
                       .Replace("\r", "\n")   // 再替换单独的 \r 为 \n
                       .Replace("\n", "\r\n"); // 最后将 \n 替换为 \r\n

            return text;
        }
    }
}


