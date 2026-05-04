namespace StoneSharp.CodeProcessing.CodeBlockExtractor
{
    /// <summary>
        /// 文件名来源枚举
        /// </summary>
        public enum FileNameSource
        {
            /// <summary>
            /// 从上一行成功提取的文件名
            /// </summary>
            Extracted,

            /// <summary>
            /// 未找到文件名，自动生成的默认文件名
            /// </summary>
            Generated,

            /// <summary>
            /// 来源未知（默认值）
            /// </summary>
            Unknown
        }
}
