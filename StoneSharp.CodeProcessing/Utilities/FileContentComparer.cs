using System;
using System.IO;
using System.Linq;
using System.Text;

namespace StoneSharp.CodeProcessing.Utilities
{
    /// <summary>
    /// 文件内容比较工具类
    /// </summary>
    public static class FileContentComparer
    {
        /// <summary>
        /// 比较代码片段内容与真实文件内容是否发生改变（忽略缩进）
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="snippetContent">代码片段内容</param>
        /// <param name="startLine">起始行号</param>
        /// <param name="endLine">结束行号</param>
        /// <returns>true表示内容已改变，false表示内容未改变</returns>
        public static bool HasContentChanged(string filePath, string snippetContent, int startLine = 0, int endLine = -1)
        {
            // 参数验证
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return true; // 文件不存在，视为内容已改变
            }

            if (snippetContent == null)
            {
                return true; // 片段内容为null，视为内容已改变
            }

            try
            {
                // 读取当前文件内容
                string currentFileContent = File.ReadAllText(filePath);

                // 如果是完整文件内容比较（StartLine=0, EndLine=-1）
                if (startLine == 0 && endLine == -1)
                {
                    return !AreContentsEqualIgnoringIndentation(currentFileContent, snippetContent);
                }

                // 如果是代码片段比较
                string[] fileLines = File.ReadAllLines(filePath);

                // 验证行号范围
                if (startLine < 0 || startLine > fileLines.Length || 
                    (endLine != -1 && (endLine < startLine || endLine > fileLines.Length)))
                {
                    return true; // 行号范围无效，视为内容已改变
                }

                // 计算实际结束行
                int actualEndLine = endLine == -1 ? fileLines.Length : endLine;
                int lineCount = actualEndLine - startLine + 1;

                // 提取当前文件的对应片段内容
                if (startLine > fileLines.Length)
                {
                    return true; // 起始行超出文件范围
                }

                string currentSnippetContent = string.Join(Environment.NewLine, 
                    fileLines.Skip(startLine - 1).Take(lineCount));

                return !AreContentsEqualIgnoringIndentation(currentSnippetContent, snippetContent);
            }
            catch (Exception ex)
            {
                // 文件读取异常，视为内容已改变
                System.Diagnostics.Debug.WriteLine($"比较文件内容时发生异常: {ex.Message}");
                return true;
            }
        }

        /// <summary>
        /// 检查代码片段内容是否与文件内容一致（扩展方法版本）
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="snippetContent">代码片段内容</param>
        /// <returns>true表示内容已改变，false表示内容未改变</returns>
        public static bool HasFullFileContentChanged(string filePath, string snippetContent)
        {
            return HasContentChanged(filePath, snippetContent, 0, -1);
        }

        /// <summary>
        /// 比较两个字符串内容是否相等（忽略每行的缩进）
        /// </summary>
        /// <param name="content1">第一个内容</param>
        /// <param name="content2">第二个内容</param>
        /// <returns>true表示内容相等，false表示内容不相等</returns>
        private static bool AreContentsEqualIgnoringIndentation(string content1, string content2)
        {
            if (content1 == content2)
            {
                return true; // 如果完全相同，直接返回true
            }

            // 分割为行并去除每行的前导空白字符
            var lines1 = content1.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                                .Select(line => line.TrimStart())
                                .ToArray();

            var lines2 = content2.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                                .Select(line => line.TrimStart())
                                .ToArray();

            // 移除前面的空行
            lines1 = TrimEmptyLines(lines1);
            lines2 = TrimEmptyLines(lines2);

            // 如果行数不同，内容肯定不同
            if (lines1.Length != lines2.Length)
            {
                return false;
            }

            // 逐行比较（忽略缩进）
            for (int i = 0; i < lines1.Length; i++)
            {
                if (lines1[i] != lines2[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 移除数组前后的空行
        /// </summary>
        /// <param name="lines">行数组</param>
        /// <returns>处理后的行数组</returns>
        private static string[] TrimEmptyLines(string[] lines)
        {
            if (lines == null || lines.Length == 0)
            {
                return lines;
            }

            // 找到第一个非空行的索引
            int startIndex = 0;
            while (startIndex < lines.Length && string.IsNullOrEmpty(lines[startIndex]))
            {
                startIndex++;
            }

            // 如果所有行都是空行，返回空数组
            if (startIndex >= lines.Length)
            {
                return Array.Empty<string>();
            }

            // 找到最后一个非空行的索引
            int endIndex = lines.Length - 1;
            while (endIndex >= 0 && string.IsNullOrEmpty(lines[endIndex]))
            {
                endIndex--;
            }

            // 计算有效行的数量
            int length = endIndex - startIndex + 1;

            // 提取有效行
            string[] result = new string[length];
            Array.Copy(lines, startIndex, result, 0, length);

            return result;
        }

        /// <summary>
        /// 比较两个字符串内容是否相等（忽略所有空白字符）
        /// </summary>
        /// <param name="content1">第一个内容</param>
        /// <param name="content2">第二个内容</param>
        /// <returns>true表示内容相等，false表示内容不相等</returns>
        public static bool AreContentsEqualIgnoringAllWhitespace(string content1, string content2)
        {
            if (content1 == content2)
            {
                return true;
            }

            // 移除所有空白字符（包括空格、制表符、换行符等）
            string normalized1 = new string(content1.Where(c => !char.IsWhiteSpace(c)).ToArray());
            string normalized2 = new string(content2.Where(c => !char.IsWhiteSpace(c)).ToArray());

            return normalized1 == normalized2;
        }
    }
}