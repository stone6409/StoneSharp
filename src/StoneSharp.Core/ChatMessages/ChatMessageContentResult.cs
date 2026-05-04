namespace StoneSharp.Core.ChatMessages
{
    /// <summary>
    /// 聊天消息内容结果，包含文本内容和推理内容（reasoning_content）
    /// </summary>
    public class ChatMessageContentResult
    {
        /// <summary>
        /// 文本内容
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 推理内容（思考过程），来自 DeepSeek 等模型的 reasoning_content
        /// </summary>
        public string? ReasoningContent { get; set; }

        public ChatMessageContentResult()
        {
        }

        public ChatMessageContentResult(string content, string? reasoningContent = null)
        {
            Content = content;
            ReasoningContent = reasoningContent;
        }
    }
}
