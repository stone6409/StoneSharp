using StoneSharp.Core.Models;
using System.Xml;

namespace StoneSharp.Core.Stores
{
    public class ChatConversationStore : XmlStoreBase
    {
        public ChatConversationStore(string configFileName, bool ensureExist = false) : base(configFileName, ensureExist)
        {
        }

        protected override void OnCreateXmlDocument(XmlDocument xmlDocument)
        {
            XmlElement root = xmlDocument.CreateElement("ChatConversations");
            xmlDocument.AppendChild(root);
        }

        private XmlNode SelectChatConversationsNode(XmlDocument xmlDocument)
        {
            XmlNode chatConversationsNode = xmlDocument.SelectSingleNode("/ChatConversations");
            return chatConversationsNode;
        }

        #region CURD

        public Dictionary<string, object> GetChatConversationNameMap()
        {
            XmlDocument xmlDocument = GetXmlDocument();

            XmlNode chatConversationsNode = SelectChatConversationsNode(xmlDocument);
            if (chatConversationsNode != null)
            {
                Dictionary<string, object> nameMap = new Dictionary<string, object>();
                foreach (XmlNode chatConversationNode in chatConversationsNode.ChildNodes)
                {
                    string childName = chatConversationNode.Attributes["Name"].Value;
                    nameMap.Add(childName, null);
                }

                return nameMap;
            }

            return null;
        }

        public ChatConversationCollection LoadChatConversations()
        {
            XmlDocument xmlDocument = GetXmlDocument();

            XmlNode chatConversationsNode = SelectChatConversationsNode(xmlDocument);
            if (chatConversationsNode != null)
            {
                ChatConversationCollection chatConversations = new ChatConversationCollection();
                foreach (XmlNode chatConversationNode in chatConversationsNode.ChildNodes)
                {
                    ChatConversation chatConversation = ChatConversationXmlMaper.ReadChatConversation(chatConversationNode);
                    chatConversations.Add(chatConversation);
                }

                return chatConversations;
            }

            return null;
        }

        public ChatConversation LoadChatConversation(string id)
        {
            XmlDocument xmlDocument = GetXmlDocument();

            XmlNode chatConversationsNode = SelectChatConversationsNode(xmlDocument);
            foreach (XmlNode chatConversationNode in chatConversationsNode.ChildNodes)
            {
                ChatConversation chatConversation = ChatConversationXmlMaper.ReadChatConversation(chatConversationNode);
                if (chatConversation.Id == id)
                {
                    return chatConversation;
                }
            }

            return null;
        }

        public void AddChatConversation(ChatConversation chatConversation)
        {
            XmlDocument xmlDocument = GetXmlDocument();

            XmlNode chatConversationsNode = SelectChatConversationsNode(xmlDocument);
            ChatConversationXmlMaper.WriteChatConversation(chatConversation, chatConversationsNode);

            SaveXmlDocumentSafely(xmlDocument); ;
        }

        public void RemoveChatConversation(string id)
        {
            XmlDocument xmlDocument = GetXmlDocument();

            XmlNode chatConversationsNode = SelectChatConversationsNode(xmlDocument);
            foreach (XmlNode chatConversationNode in chatConversationsNode.ChildNodes)
            {
                ChatConversation chatConversation = ChatConversationXmlMaper.ReadChatConversation(chatConversationNode);
                if (chatConversation.Id == id)
                {
                    chatConversationNode.ParentNode.RemoveChild(chatConversationNode);
                }
            }

            SaveXmlDocumentSafely(xmlDocument); ;
        }

        public void UpdateChatConversation(ChatConversation chatConversation)
        {
            XmlDocument xmlDocument = GetXmlDocument();

            XmlNode chatConversationsNode = SelectChatConversationsNode(xmlDocument);
            ChatConversationXmlMaper.WriteChatConversation(chatConversation, chatConversationsNode);

            XmlNode lastChildNode = chatConversationsNode.LastChild;
            foreach (XmlNode chatConversationNode in chatConversationsNode.ChildNodes)
            {
                ChatConversation chatConversation1 = ChatConversationXmlMaper.ReadChatConversation(chatConversationNode);
                if (chatConversationNode != lastChildNode && chatConversation1.Id == chatConversation.Id)
                {
                    chatConversationsNode.RemoveChild(lastChildNode);
                    chatConversationsNode.ReplaceChild(lastChildNode, chatConversationNode);
                    break;
                }
            }

            SaveXmlDocumentSafely(xmlDocument); ;
        }

        public void UpdateChatConversations(ChatConversationCollection chatConversations)
        {
            XmlDocument xmlDocument = GetXmlDocument();

            XmlNode chatConversationsNode = SelectChatConversationsNode(xmlDocument);
            chatConversationsNode.RemoveAll();
            foreach (ChatConversation chatConversation in chatConversations)
            {
                ChatConversationXmlMaper.WriteChatConversation(chatConversation, chatConversationsNode);
            }

            SaveXmlDocumentSafely(xmlDocument); ;
        }

        #endregion
    }
}
