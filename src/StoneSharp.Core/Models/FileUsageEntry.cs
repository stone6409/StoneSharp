using System.Xml.Serialization;

namespace StoneSharp.Core.Models
{
    public class FileUsageEntry
    {
        [XmlElement("FilePath")]
        public string FilePath { get; set; }

        [XmlElement("Count")]
        public int Count { get; set; }

        [XmlElement("IsPinned")]
        public bool IsPinned { get; set; }
    }
}