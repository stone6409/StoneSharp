using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneSharp.Core.Models.ContextItems
{
    public class ContextFile : ContextItem
    {
        public string FilePath { get; set; }

        public string FileContent { get; set; }

        public int StartLine { get; set; } = 0;

        public int EndLine { get; set; } = -1;

        public string CodeLanguage { get; set; }

        public Encoding Encoding { get; set; }

        public override bool Equals(ContextItem other)
        {
            if (other is not ContextFile otherFile)
                return false;

            return FilePath == otherFile.FilePath &&
                   StartLine == otherFile.StartLine &&
                   EndLine == otherFile.EndLine &&
                   CodeLanguage == otherFile.CodeLanguage;
        }
    }
}
