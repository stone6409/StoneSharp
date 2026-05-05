using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneSharp.Core.Models.ContextItems
{
    public class ContextFileSnippet : ContextItem
    {
        public string FilePath { get; set; }

        public string SnippetContent { get; set; }

        public int StartLine { get; set; } = 0;

        public int EndLine { get; set; } = -1;

        public string CodeLanguage { get; set; }

        public override bool Equals(ContextItem other)
        {
            if (other is not ContextFileSnippet otherFile)
                return false;

            return FilePath == otherFile.FilePath &&
                   SnippetContent == otherFile.SnippetContent &&
                   StartLine == otherFile.StartLine &&
                   EndLine == otherFile.EndLine &&
                   CodeLanguage == otherFile.CodeLanguage;
        }
    }
}
