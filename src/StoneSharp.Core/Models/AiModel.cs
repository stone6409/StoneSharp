using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneSharp.Core.Models
{
    public class AiModel
    {
        public AiModel()
        {
        }

        public AiModel(string name, int maxTokens = 0)
        {
            Name = name;
            MaxTokens = maxTokens;
        }

        public AiModel(string name, string aliasName, int maxTokens = 0)
        {
            Name = name;
            AliasName = aliasName;
            MaxTokens = maxTokens;
        }

        public string Name { get; set; }

        public string AliasName { get; set; }

        public int MaxTokens { get; set; }
    }
}
