using System;

namespace StoneSharp.Core.Models
{
    public class ChatConversation
    {
        public ChatConversation()
        {

        }

        public string Id { get; set; }

        public RequestMessage RequestMessage { get; set; }

        public ReplyMessage ReplyMessage { get; set; }
    }
}
