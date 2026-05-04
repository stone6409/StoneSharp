
using System.Collections.Generic;

/// <summary>
/// 目录结构信息
/// </summary>
namespace StoneSharp.CodeProcessing.CodeBlockExtractor
{
    public class DirectoryStructure
    {
        public string RootPath { get; set; }
        public List<string> Directories { get; set; } = new List<string>();
        public Dictionary<string, string> FilePaths { get; set; } = new Dictionary<string, string>();
    }
}
