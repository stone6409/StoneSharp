namespace StoneSharp.Core.ChatMessages
{
    public interface IChatMessageService
    {
        void AddMessage(AuthorRole authorRole, string content, string? reasoningContent = null);

        void AddFunctionCallMessage(string functionName, string? pluginName = null, string? callId = null, FunctionArguments? arguments = null, string? reasoningContent = null);

        void AddFunctionResultMessage(string? functionName = null, string? pluginName = null, string? callId = null, object? result = null);

        void AddAllowedTool(string tool);

        Task<ChatMessageContentResult> GetChatMessageContentAsync(CancellationToken cancellationToken = default);

        IAsyncEnumerable<StreamingMessage> GetStreamingChatMessageContentsAsync(CancellationToken cancellationToken = default);
    }
}