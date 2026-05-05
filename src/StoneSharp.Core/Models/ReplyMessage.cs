using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneSharp.Core.Models
{
    public class ReplyMessage
    {
        public string Result { get; set; }

        public string ReasoningContent { get; set; }

        public DateTime Time { get; set; }

        public string AiModel { get; set; }

        public List<ToolCall> ToolCalls { get; set; } = new List<ToolCall>();
    }
}
