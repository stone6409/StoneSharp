// 添加工具调用数据模型

using StoneSharp.Core.ChatMessages;

namespace StoneSharp.Core.Models
{
    public class ToolCall
    {
        public string PluginName { get; set; }
        public string FunctionName { get; set; }
        public string CallId { get; set; }
        public string ReasoningContent { get; set; }
        public FunctionArguments Arguments { get; set; }
        public string Result { get; set; }
        public string Status { get; set; }
        public string Error { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}
