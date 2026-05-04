namespace StoneSharp.Core.ChatMessages
{
    /// <summary>
    /// 流式消息，包含文本内容和推理内容（reasoning_content）
    /// </summary>
    public class StreamingMessage
    {
        /// <summary>
        /// 文本内容
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 推理内容（思考过程），来自 DeepSeek 等模型的 reasoning_content
        /// </summary>
        public string? ReasoningContent { get; set; }

        public StreamingMessage()
        {
        }

        public StreamingMessage(string content, string? reasoningContent = null)
        {
            Content = content;
            ReasoningContent = reasoningContent;
        }
    }
}
