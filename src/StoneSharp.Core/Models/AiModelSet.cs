using System;
using System.Collections.Generic;

namespace StoneSharp.Core.Models
{
    public class AiModelSet
    {
        public AiModelSet()
        {

        }
        public string Name { get; set; }

        public string ApiUrl { get; set; }

        public string ApiKey { get; set; }

        private AiModelCollection _aiModels;

        public AiModelCollection AiModels
        {
            get
            {
                if (_aiModels == null)
                {
                    _aiModels = new AiModelCollection();
                }

                return _aiModels;
            }
            set
            {
                _aiModels = value;
            }
        }
    }
}
