using StoneSharp.Core.Models.ContextItems;
using System.Xml;

namespace StoneSharp.Core.Stores
{
    public static class ContextItemUsageXmlMaper
    {
        public static void WriteContextItemUsage(ContextItemUsage contextItemUsage, XmlNode xmlNode)
        {
            XmlElement xmlElement = xmlNode.OwnerDocument.CreateElement("ContextItemUsage");

            if (contextItemUsage.ContextItem != null)
            {
                ContextItemXmlMaper.WriteContextItem(contextItemUsage.ContextItem, xmlElement);
            }

            XmlAttributeHelper.WriteAttribute(xmlElement, "Count", contextItemUsage.Count, 0);
            XmlAttributeHelper.WriteAttribute(xmlElement, "IsPinned", contextItemUsage.IsPinned, false);

            xmlNode.AppendChild(xmlElement);
        }

        public static ContextItemUsage ReadContextItemUsage(XmlNode xmlNode)
        {
            if (xmlNode == null)
                throw new ArgumentNullException(nameof(xmlNode));

            ContextItemUsage contextItemUsage = new ContextItemUsage();

            if (xmlNode.ChildNodes.Count > 0)
            {
                XmlNode childNode = xmlNode.ChildNodes[0];
                contextItemUsage.ContextItem = ContextItemXmlMaper.ReadContextItem(childNode);
            }

            contextItemUsage.Count = XmlAttributeHelper.ReadAttribute(xmlNode, "Count", 0);
            contextItemUsage.IsPinned = XmlAttributeHelper.ReadAttribute(xmlNode, "IsPinned", false);

            return contextItemUsage;
        }
    }
}