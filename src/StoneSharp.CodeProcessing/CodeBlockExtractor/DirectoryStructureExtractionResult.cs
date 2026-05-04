using System;

namespace StoneSharp.CodeProcessing.CodeBlockExtractor
{
    /// <summary>
    /// 目录结构提取结果
    /// </summary>
    public class DirectoryStructureExtractionResult
    {
        /// <summary>
        /// 提取到的目录结构文本
        /// </summary>
        public string DirectoryStructureText { get; set; }

        /// <summary>
        /// 在原始内容中的结束位置
        /// </summary>
        public int EndPosition { get; set; }

        /// <summary>
        /// 是否成功提取到目录结构
        /// </summary>
        public bool Success => !string.IsNullOrEmpty(DirectoryStructureText);

        /// <summary>
        /// 构造函数
        /// </summary>
        public DirectoryStructureExtractionResult(string text, int endPosition)
        {
            DirectoryStructureText = text ?? string.Empty;
            EndPosition = endPosition;
        }

        /// <summary>
        /// 创建失败结果
        /// </summary>
        public static DirectoryStructureExtractionResult Failed()
        {
            return new DirectoryStructureExtractionResult(string.Empty, -1);
        }
    }
}