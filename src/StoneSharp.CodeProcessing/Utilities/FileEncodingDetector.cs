using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

/// <summary>
/// 文件编码探测工具类（静态类）
/// 提供多种方法来探测文件的原始编码，特别优化了对GB2312/GBK等中文编码的支持
/// </summary>

namespace StoneSharp.CodeProcessing.Utilities
{
    public static class FileEncodingDetector
    {
        #region 静态构造函数 - 注册编码提供程序

        static FileEncodingDetector()
        {
            // 注册编码提供程序以支持更多编码
            RegisterEncodingProvider();
        }

        /// <summary>
        /// 注册编码提供程序
        /// </summary>
        private static void RegisterEncodingProvider()
        {
            try
            {
                // 注册 CodePagesEncodingProvider 以支持更多编码
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            }
            catch
            {
                // 如果注册失败，继续使用默认的编码支持
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 获取文件的编码（基础版本）
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>文件的编码，如果无法确定则返回 UTF-8</returns>
        public static Encoding GetEncoding(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return Encoding.UTF8;
            }

            try
            {
                // 1. 检查BOM标记
                Encoding bomEncoding = DetectBOMEncoding(filePath);
                if (bomEncoding != null)
                {
                    return bomEncoding;
                }

                // 2. 尝试使用StreamReader自动探测
                Encoding detectedEncoding = DetectWithStreamReader(filePath);
                if (detectedEncoding != null && detectedEncoding != Encoding.Default)
                {
                    return detectedEncoding;
                }

                // 3. 通过内容分析编码
                return DetectEncodingByContent(filePath);
            }
            catch
            {
                return Encoding.UTF8;
            }
        }

        /// <summary>
        /// 获取文件的编码（增强版本，准确性更高）
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>文件的编码，如果无法确定则返回 UTF-8</returns>
        public static Encoding GetEncodingEnhanced(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return Encoding.UTF8;
            }

            try
            {
                // 1. 检查BOM标记
                Encoding bomEncoding = DetectBOMEncoding(filePath);
                if (bomEncoding != null)
                {
                    return bomEncoding;
                }

                // 2. 尝试多种编码读取
                Encoding detectedEncoding = TryMultipleEncodings(filePath);
                if (detectedEncoding != null)
                {
                    return detectedEncoding;
                }

                // 3. 使用统计分析方法
                return DetectEncodingByStatisticalAnalysis(filePath);
            }
            catch
            {
                return Encoding.UTF8;
            }
        }

        #endregion

        #region 私有方法 - BOM检测

        /// <summary>
        /// 通过BOM标记检测编码
        /// </summary>
        private static Encoding DetectBOMEncoding(string filePath)
        {
            try
            {
                byte[] bom = new byte[4];
                using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    int bytesRead = file.Read(bom, 0, 4);
                    if (bytesRead < 2) return null;
                }

                // UTF-8 BOM: EF BB BF
                if (bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
                {
                    return Encoding.UTF8;
                }
                // UTF-16 Little Endian BOM: FF FE
                else if (bom[0] == 0xFF && bom[1] == 0xFE)
                {
                    return Encoding.Unicode;
                }
                // UTF-16 Big Endian BOM: FE FF
                else if (bom[0] == 0xFE && bom[1] == 0xFF)
                {
                    return Encoding.BigEndianUnicode;
                }
                // UTF-7 BOM: 2B 2F 76
                else if (bom[0] == 0x2B && bom[1] == 0x2F && bom[2] == 0x76)
                {
                    return Encoding.UTF7;
                }
                // UTF-32 Little Endian BOM: FF FE 00 00
                else if (bom[0] == 0xFF && bom[1] == 0xFE && bom[2] == 0x00 && bom[3] == 0x00)
                {
                    return Encoding.UTF32;
                }
                // UTF-32 Big Endian BOM: 00 00 FE FF
                else if (bom[0] == 0x00 && bom[1] == 0x00 && bom[2] == 0xFE && bom[3] == 0xFF)
                {
                    return Encoding.GetEncoding(12001); // UTF-32BE
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region 私有方法 - 编码探测

        /// <summary>
        /// 使用StreamReader自动探测编码
        /// </summary>
        private static Encoding DetectWithStreamReader(string filePath)
        {
            try
            {
                using (var reader = new StreamReader(filePath, Encoding.Default, true))
                {
                    reader.ReadToEnd();
                    return reader.CurrentEncoding;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 通过内容分析编码
        /// </summary>
        private static Encoding DetectEncodingByContent(string filePath)
        {
            try
            {
                byte[] buffer = ReadFileBytes(filePath, 4096);
                if (buffer == null || buffer.Length == 0)
                {
                    return Encoding.UTF8;
                }

                // 尝试不同的编码
                var encodingsToTry = GetTestEncodings();

                foreach (var encoding in encodingsToTry)
                {
                    try
                    {
                        string content = encoding.GetString(buffer);
                        if (IsValidTextContent(content, encoding))
                        {
                            return encoding;
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                return Encoding.UTF8;
            }
            catch
            {
                return Encoding.UTF8;
            }
        }

        /// <summary>
        /// 尝试多种编码读取文件
        /// </summary>
        private static Encoding TryMultipleEncodings(string filePath)
        {
            try
            {
                byte[] buffer = ReadFileBytes(filePath, 8192);
                if (buffer == null || buffer.Length == 0)
                {
                    return null;
                }

                List<Encoding> encodingsToTry = new List<Encoding>
                {
                    Encoding.UTF8,
                    Encoding.GetEncoding("GB2312"),
                    Encoding.GetEncoding("GBK"),
                    Encoding.Default,
                    Encoding.GetEncoding("Big5"),
                    Encoding.GetEncoding("shift_jis"),
                    Encoding.GetEncoding("euc-kr"),
                    Encoding.Unicode,
                    Encoding.BigEndianUnicode
                };

                Encoding bestEncoding = null;
                double bestScore = -1;

                foreach (var encoding in encodingsToTry)
                {
                    try
                    {
                        string content = encoding.GetString(buffer);
                        double score = CalculateEncodingScore(content, encoding, buffer);

                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestEncoding = encoding;
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                return bestEncoding;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 通过统计分析探测编码
        /// </summary>
        private static Encoding DetectEncodingByStatisticalAnalysis(string filePath)
        {
            try
            {
                byte[] buffer = ReadFileBytes(filePath, 16384);
                if (buffer == null || buffer.Length == 0)
                {
                    return Encoding.UTF8;
                }

                Dictionary<Encoding, double> scores = new Dictionary<Encoding, double>();

                // 使用安全的编码获取方式
                var testEncodings = GetTestEncodings();

                foreach (var encoding in testEncodings)
                {
                    try
                    {
                        string decoded = encoding.GetString(buffer);
                        double score = CalculateEncodingScore(decoded, encoding, buffer);
                        scores[encoding] = score;
                    }
                    catch
                    {
                        scores[encoding] = 0;
                    }
                }

                // 返回得分最高的编码
                var bestEncoding = scores.OrderByDescending(kv => kv.Value).FirstOrDefault();
                return bestEncoding.Value > 0 ? bestEncoding.Key : Encoding.UTF8;
            }
            catch
            {
                return Encoding.UTF8;
            }
        }

        #endregion

        #region 私有方法 - 辅助函数

        /// <summary>
        /// 读取文件字节
        /// </summary>
        private static byte[] ReadFileBytes(string filePath, int maxBytes)
        {
            try
            {
                using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    int bytesToRead = Math.Min((int)file.Length, maxBytes);
                    byte[] buffer = new byte[bytesToRead];
                    file.Read(buffer, 0, bytesToRead);
                    return buffer;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 检查文本内容是否有效
        /// </summary>
        private static bool IsValidTextContent(string text, Encoding encoding)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            // 检查控制字符比例
            int controlCount = 0;
            int replacementCount = 0;

            foreach (char c in text)
            {
                if (char.IsControl(c) && c != '\r' && c != '\n' && c != '\t')
                {
                    controlCount++;
                }

                if (c == '\uFFFD') // Unicode替换字符
                {
                    replacementCount++;
                }
            }

            double controlRatio = (double)controlCount / text.Length;
            double replacementRatio = (double)replacementCount / text.Length;

            // 如果控制字符或替换字符太多，认为无效
            return controlRatio < 0.05 && replacementRatio < 0.05;
        }

        /// <summary>
        /// 计算编码得分（改进版本）
        /// </summary>
        /// <param name="text">解码后的文本</param>
        /// <param name="encoding">当前测试的编码</param>
        /// <param name="rawBytes">原始字节数据</param>
        /// <returns>编码得分</returns>
        private static double CalculateEncodingScore(string text, Encoding encoding, byte[] rawBytes = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            double score = 0;
            int totalChars = text.Length;

            // 1. 可打印字符比例
            int printableCount = 0;
            int chineseCount = 0;
            int invalidCount = 0;
            int asciiCount = 0;
            int highAsciiCount = 0;

            foreach (char c in text)
            {
                // 可打印字符
                if (!char.IsControl(c) || c == '\r' || c == '\n' || c == '\t')
                {
                    printableCount++;
                }

                // ASCII字符（0-127）
                if (c >= 0 && c <= 127)
                {
                    asciiCount++;
                }
                // 高ASCII字符（128-255）
                else if (c >= 128 && c <= 255)
                {
                    highAsciiCount++;
                }

                // 中文字符
                if (c >= 0x4E00 && c <= 0x9FFF)
                {
                    chineseCount++;
                }

                // 无效字符
                if (c == '\uFFFD' || (char.IsControl(c) && c != '\r' && c != '\n' && c != '\t'))
                {
                    invalidCount++;
                }
            }

            // 可打印字符比例加分
            double printableRatio = (double)printableCount / totalChars;
            score += printableRatio * 50;

            // ASCII字符比例加分（对于UTF-8和ASCII文件很重要）
            double asciiRatio = (double)asciiCount / totalChars;
            score += asciiRatio * 30;

            // 中文字符比例加分（针对中文编码）
            double chineseRatio = (double)chineseCount / totalChars;
            score += chineseRatio * 100;

            // 无效字符比例减分
            double invalidRatio = (double)invalidCount / totalChars;
            score -= invalidRatio * 200;

            // 常见中文词汇加分
            string[] commonChinese = { "的", "一", "是", "在", "不", "了", "有", "和", "人", "这" };
            foreach (var word in commonChinese)
            {
                if (text.Contains(word))
                {
                    score += 10;
                }
            }

            // 根据编码类型调整权重
            if (encoding.CodePage == 65001) // UTF-8
            {
                // UTF-8对于ASCII文件应该有优势
                if (asciiRatio > 0.9)
                {
                    score += 100; // 给UTF-8额外加分
                }

                // 检查UTF-8字节序列的有效性
                if (rawBytes != null && IsValidUTF8Sequence(rawBytes))
                {
                    score += 150; // 有效的UTF-8序列额外加分
                }
            }
            else if (encoding.CodePage == 1200) // UTF-16 LE (Encoding.Unicode)
            {
                // UTF-16对于纯ASCII文件通常会有很多0字节，应该减分
                if (rawBytes != null && asciiRatio > 0.8)
                {
                    // 检查是否有大量0字节（UTF-16的特征）
                    int zeroByteCount = 0;
                    for (int i = 0; i < rawBytes.Length; i++)
                    {
                        if (rawBytes[i] == 0)
                        {
                            zeroByteCount++;
                        }
                    }

                    double zeroByteRatio = (double)zeroByteCount / rawBytes.Length;
                    if (zeroByteRatio > 0.3) // 如果超过30%是0字节，很可能是UTF-16
                    {
                        score += 50; // 确实是UTF-16文件
                    }
                    else
                    {
                        score -= 100; // 不是UTF-16文件，减分
                    }
                }
            }
            else if (encoding.CodePage == 936 || encoding.CodePage == 54936) // GB2312/GBK
            {
                if (chineseRatio > 0.1)
                {
                    score += 50;
                }
            }

            // 检查JSON文件特征（对于package.json这样的文件）
            if (text.Contains("{") && text.Contains("}") && text.Contains("\""))
            {
                // JSON文件通常是UTF-8
                if (encoding.CodePage == 65001) // UTF-8
                {
                    score += 80;
                }
            }

            return Math.Max(0, score);
        }

        /// <summary>
        /// 检查字节序列是否是有效的UTF-8序列
        /// </summary>
        private static bool IsValidUTF8Sequence(byte[] data)
        {
            if (data == null || data.Length == 0)
                return true;

            for (int i = 0; i < data.Length; i++)
            {
                byte b = data[i];

                // 单字节字符 (0xxxxxxx)
                if (b <= 0x7F)
                {
                    continue;
                }

                // 多字节字符
                int followingBytes = 0;
                if ((b & 0xE0) == 0xC0) // 110xxxxx
                {
                    followingBytes = 1;
                }
                else if ((b & 0xF0) == 0xE0) // 1110xxxx
                {
                    followingBytes = 2;
                }
                else if ((b & 0xF8) == 0xF0) // 11110xxx
                {
                    followingBytes = 3;
                }
                else
                {
                    return false; // 无效的UTF-8起始字节
                }

                // 检查后续字节
                for (int j = 1; j <= followingBytes; j++)
                {
                    if (i + j >= data.Length)
                        return false;

                    if ((data[i + j] & 0xC0) != 0x80) // 后续字节必须是10xxxxxx
                        return false;
                }

                i += followingBytes;
            }

            return true;
        }

        /// <summary>
        /// 获取要测试的编码列表（安全版本）
        /// </summary>
        private static List<Encoding> GetTestEncodings()
        {
            var encodings = new List<Encoding>();

            // 总是添加 UTF-8
            encodings.Add(Encoding.UTF8);

            // 添加默认编码（通常是系统默认编码）
            encodings.Add(Encoding.Default);

            // 添加 Unicode 编码
            encodings.Add(Encoding.Unicode);
            encodings.Add(Encoding.BigEndianUnicode);

            // 尝试添加中文编码（使用安全方式）
            TryAddEncoding(encodings, "GB2312");
            TryAddEncoding(encodings, "GBK");
            TryAddEncoding(encodings, "GB18030"); // GB18030 是 GBK 的超集

            // 尝试添加其他语言编码
            TryAddEncoding(encodings, "Big5"); // 繁体中文
            TryAddEncoding(encodings, "shift_jis"); // 日文
            TryAddEncoding(encodings, "euc-kr"); // 韩文
            TryAddEncoding(encodings, "windows-1252"); // 西欧
            TryAddEncoding(encodings, "iso-8859-1"); // 拉丁文

            return encodings;
        }

        /// <summary>
        /// 安全地尝试添加编码
        /// </summary>
        private static void TryAddEncoding(List<Encoding> encodings, string encodingName)
        {
            try
            {
                var encoding = Encoding.GetEncoding(encodingName);
                if (encoding != null && !encodings.Any(e => e.CodePage == encoding.CodePage))
                {
                    encodings.Add(encoding);
                }
            }
            catch
            {
                // 如果编码不支持，则跳过
            }
        }

        #endregion
    }
}