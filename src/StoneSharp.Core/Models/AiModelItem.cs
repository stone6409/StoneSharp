using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneSharp.Core.Models
{
    public record AiModelItem
    {
        private string aiModelSetName;

        public string Name { get; set; }

        private string _aliasName;

        public string AliasName 
        { 
            get
            {
                if (_aliasName == null)
                {
                    return Name;
                }
                return _aliasName;
            }
            set => _aliasName = value; 
        }

        public string AiModelSetName 
        { 
            get => aiModelSetName; 
            set => aiModelSetName = value; 
        }

        
        private int _maxTokens;

        public int MaxTokens
        {
            get
            {
                return _maxTokens;
            }
            set
            {
                _maxTokens = value;
            }
        }
    }
}
