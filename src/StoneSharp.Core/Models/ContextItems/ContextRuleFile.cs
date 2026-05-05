using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneSharp.Core.Models.ContextItems
{
    // 用于提示AI大模型的输出规则
    public class ContextRuleFile : ContextItem
    {
        public string FilePath { get; set; }

        public string FileContent { get; set; }

        public override bool Equals(ContextItem other)
        {
            if (other is not ContextRuleFile otherRule)
                return false;

            return FilePath == otherRule.FilePath;
        }
    }
}
