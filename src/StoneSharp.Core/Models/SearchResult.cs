using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoneSharp.Core.Models
{
    public class SearchResult
    {
        public Chat Chat { get; set; }

        public ChatConversation ChatConversation { get; set; }

        //private ChatConversationCollection _chatConversations;

        //public ChatConversationCollection ChatConversations
        //{
        //    get
        //    {
        //        if (_chatConversations == null)
        //        {
        //            _chatConversations = new ChatConversationCollection();
        //        }

        //        return _chatConversations;
        //    }
        //    set
        //    {
        //        _chatConversations = value;
        //    }
        //}
    }
}
