using StoneSharp.Core.Models;
using StoneSharp.Core.Utilities;

namespace StoneSharp.Core.Providers
{
    public class ChatProvider : IChatProvider
    {
        public ChatProvider(string folderPath)
        {
            ChatFileUtility.Initialize(folderPath);
        }

        private List<Chat> _chats;

        public List<Chat> Chats
        {
            get
            {
                if (_chats == null)
                {
                    _chats = ChatFileUtility.GetAllChats();
                }

                return _chats;
            }
            set
            {
                _chats = value;
            }
        }

        public List<Chat> LoadChats()
        {
            Chats = ChatFileUtility.GetAllChats();
            return Chats;
        }

        public async Task<List<Chat>> LoadChatsAsync()
        {
            Chats = await Task.Run(() => ChatFileUtility.GetAllChats());
            return Chats;
        }

        public bool AddChat(Chat chat)
        {
            if (true/*ChatFileUtility.CreateChatFile(chat.Id, chat.Name)*/)
            {
                Chats.Add(chat);
                return true;
            }

            return false;
        }

        public bool RemoveChat(Chat chat)
        {
            if (ChatFileUtility.DeleteChatFile(chat.Id, chat.Name))
            {
                Chats.Remove(chat);
                return true;
            }

            return false;
        }

        public string GetChatFilePath(Chat chat)
        {
            return ChatFileUtility.GetFilePath(chat.Id, chat.Name);
        }

        public List<Chat> SearchChats(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new List<Chat>();
            }

            return ChatFileUtility.SearchChats(searchTerm);
        }

        //public List<SearchResult> SearchAllChatConversations(string searchTerm)
        //{
        //    List<SearchResult> results = new List<SearchResult>();

        //    if (string.IsNullOrWhiteSpace(searchTerm))
        //    {
        //        return results;
        //    }

        //    // 遍历所有 Chat
        //    foreach (var chat in Chats)
        //    {
        //        // 加载当前 Chat 的会话内容
        //        var chatConversationProvider = new ChatConversationProvider(GetFilePath(chat));
        //        ChatConversationCollection chatConversations = chatConversationProvider.LoadChatConversations();

        //        // 过滤匹配的会话
        //        ChatConversationCollection matchedConversations = new ChatConversationCollection();
        //        foreach (ChatConversation conversation in chatConversations)
        //        {
        //            bool isMatched = false;

        //            if (conversation.RequestMessage.Prompt.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
        //            {
        //                isMatched = true;
        //            }

        //            if (conversation.ReplyMessage != null && conversation.ReplyMessage.Result.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
        //            {
        //                isMatched = true;
        //            }

        //            if (isMatched)
        //            {
        //                matchedConversations.Add(conversation);
        //            }
        //        }

        //        // 如果有匹配的会话，添加到结果中
        //        if (matchedConversations.Count > 0)
        //        {
        //            results.Add(new SearchResult
        //            {
        //                Chat = chat,
        //                ChatConversations = matchedConversations
        //            });
        //        }
        //    }

        //    return results;
        //}

        public List<SearchResult> SearchAllChatConversations(string searchTerm)
        {
            List<SearchResult> results = new List<SearchResult>();

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return results;
            }

            // 遍历所有 Chat
            foreach (var chat in Chats)
            {
                // 加载当前 Chat 的会话内容
                var chatConversationProvider = new ChatConversationProvider(GetChatFilePath(chat));
                ChatConversationCollection chatConversations = null;
                try
                {
                    chatConversations = chatConversationProvider.LoadChatConversations();
                }
                catch
                {
                }

                if (chatConversations == null)
                {
                    continue;
                }

                // 过滤匹配的会话
                ChatConversationCollection matchedConversations = new ChatConversationCollection();
                foreach (ChatConversation conversation in chatConversations)
                {
                    bool isMatched = false;

                    if (conversation.RequestMessage.Prompt.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    {
                        isMatched = true;
                    }

                    if (conversation.ReplyMessage != null && conversation.ReplyMessage.Result.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    {
                        isMatched = true;
                    }

                    if (isMatched)
                    {
                        results.Add(new SearchResult
                        {
                            Chat = chat,
                            ChatConversation = conversation
                        });
                    }
                }
            }

            return results;
        }
    }
}
