using System.Xml.Serialization;

namespace StoneSharp.Core.Models.ContextItems
{
    public class ContextItemUsage
    {
        public ContextItem ContextItem { get; set; }

        public int Count { get; set; }

        public bool IsPinned { get; set; }
    }
}