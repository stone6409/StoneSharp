using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneSharp.Core.Models.ContextItems
{
    public class ContextFolder : ContextItem
    {
        public string FolderPath { get; set; }

        public string FolderSummary { get; set; }

        public override bool Equals(ContextItem other)
        {
            if (other == null || !(other is ContextFolder otherFolder))
                return false;

            return FolderPath == otherFolder.FolderPath &&
                   FolderSummary == otherFolder.FolderSummary;
        }
    }
}