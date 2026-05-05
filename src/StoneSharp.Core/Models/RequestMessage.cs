using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StoneSharp.Core.Models.ContextItems;

namespace StoneSharp.Core.Models
{
    public class RequestMessage
    {
        public string Prompt { get; set; }

        public List<ContextItem> ContextItems { get; set; } = new List<ContextItem>();

        public DateTime Time { get; set; }
    }
}
