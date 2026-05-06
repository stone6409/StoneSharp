using StoneSharp.Core.Models;

namespace StoneSharp.Core.Providers
{
    /// <summary>
    /// 聊天提供者接口
    /// </summary>
    public interface IChatProvider
    {
        /// <summary>
        /// 获取所有聊天列表
        /// </summary>
        List<Chat> Chats { get; set; }

        /// <summary>
        /// 加载所有聊天
        /// </summary>
        List<Chat> LoadChats();

        /// <summary>
        /// 异步加载所有聊天
        /// </summary>
        Task<List<Chat>> LoadChatsAsync();

        /// <summary>
        /// 添加聊天
        /// </summary>
        /// <param name="chat">聊天对象</param>
        /// <returns>是否添加成功</returns>
        bool AddChat(Chat chat);

        /// <summary>
        /// 移除聊天
        /// </summary>
        /// <param name="chat">聊天对象</param>
        /// <returns>是否移除成功</returns>
        bool RemoveChat(Chat chat);

        /// <summary>
        /// 获取聊天文件路径
        /// </summary>
        /// <param name="chat">聊天对象</param>
        /// <returns>文件路径</returns>
        string GetChatFilePath(Chat chat);

        /// <summary>
        /// 搜索聊天
        /// </summary>
        /// <param name="searchTerm">搜索关键词</param>
        /// <returns>匹配的聊天列表</returns>
        List<Chat> SearchChats(string searchTerm);

        /// <summary>
        /// 搜索所有聊天会话
        /// </summary>
        /// <param name="searchTerm">搜索关键词</param>
        /// <returns>搜索结果列表</returns>
        List<SearchResult> SearchAllChatConversations(string searchTerm);
    }
}