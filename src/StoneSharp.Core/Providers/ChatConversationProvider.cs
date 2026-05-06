using StoneSharp.Core.Models;
using StoneSharp.Core.Stores;

namespace StoneSharp.Core.Providers
{
    public class ChatConversationProvider : IChatConversationProvider
    {
        public ChatConversationProvider(string fileName)
        {
            ChatConversationStore = new ChatConversationStore(fileName, true);
        }

        public ChatConversationStore ChatConversationStore { get; private set; }

        public ChatConversationCollection LoadChatConversations()
        {
            return ChatConversationStore.LoadChatConversations();
        }

        public async Task<ChatConversationCollection> LoadChatConversationsAsync()
        {
            return await Task.Run(() => ChatConversationStore.LoadChatConversations());
        }

        public void RemoveChatConversation(string id)
        {
            ChatConversationStore.RemoveChatConversation(id);
        }

        public void UpdateChatConversations(ChatConversationCollection chatConversations)
        {
            ChatConversationStore.UpdateChatConversations(chatConversations);
        }

        Dictionary<string, object> _chatConversationNameMap;

        public Dictionary<string, object> GetChatConversationNameMap()
        {
            if (_chatConversationNameMap == null)
            {
                _chatConversationNameMap = ChatConversationStore.GetChatConversationNameMap();
            }

            return _chatConversationNameMap;
        }
    }
}