using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneSharp.Core.Prompts.User
{
    public class AttachedFile
    {
        public string FilePath { get; set; }

        public int StartLine { get; set; }

        public int EndLine { get; set; }

        public string FileContent { get; set; }

        public string CodeLanguage { get; set; }
    }
}
