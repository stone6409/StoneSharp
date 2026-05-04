
namespace StoneSharp.CodeProcessing.CodeBlockExtractor
{
    /// <summary>
    /// 代码块数据结构
    /// </summary>
    public class CodeBlock
    {
        public string CodeLanguage { get; set; }
        public string CodeContent { get; set; }
        public string FileName { get; set; }

        /// <summary>
        /// 文件名来源类型
        /// </summary>
        public FileNameSource Source { get; set; }
    }
}
