using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneSharp.Core.Models.ContextItems
{
    public class ContextKnowledgeBase : ContextItem
    {
        public string Name { get; set; }

        public override bool Equals(ContextItem other)
        {
            if (other is not ContextKnowledgeBase otherKnowledgeBase)
                return false;

            return Name == otherKnowledgeBase.Name;
        }
    }
}
