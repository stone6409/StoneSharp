using StoneSharp.Core.Models;
using StoneSharp.Core.Stores;

namespace StoneSharp.Core.Providers
{
    /// <summary>
    /// 聊天会话提供者接口
    /// </summary>
    public interface IChatConversationProvider
    {
        /// <summary>
        /// 获取聊天会话配置文件
        /// </summary>
        ChatConversationStore ChatConversationStore { get; }

        /// <summary>
        /// 加载聊天会话集合
        /// </summary>
        /// <returns>聊天会话集合</returns>
        ChatConversationCollection LoadChatConversations();

        /// <summary>
        /// 异步加载聊天会话集合
        /// </summary>
        /// <returns>聊天会话集合</returns>
        Task<ChatConversationCollection> LoadChatConversationsAsync();

        /// <summary>
        /// 移除指定ID的聊天会话
        /// </summary>
        /// <param name="id">会话ID</param>
        void RemoveChatConversation(string id);

        /// <summary>
        /// 更新聊天会话集合
        /// </summary>
        /// <param name="chatConversations">聊天会话集合</param>
        void UpdateChatConversations(ChatConversationCollection chatConversations);

        /// <summary>
        /// 获取聊天会话名称映射表
        /// </summary>
        /// <returns>名称映射字典</returns>
        Dictionary<string, object> GetChatConversationNameMap();
    }
}