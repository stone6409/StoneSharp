using StoneSharp.Core.Models.ContextItems;
using System.Xml;

namespace StoneSharp.Core.Stores
{
    public class ContextItemUsageStore : XmlStoreBase
    {
        public ContextItemUsageStore(string configFileName, bool ensureExist = false) : base(configFileName, ensureExist)
        {
        }

        protected override void OnCreateXmlDocument(XmlDocument xmlDocument)
        {
            XmlElement root = xmlDocument.CreateElement("ContextItemUsages");
            xmlDocument.AppendChild(root);
        }

        private XmlNode SelectContextItemUsagesNode(XmlDocument xmlDocument)
        {
            XmlNode contextItemUsagesNode = xmlDocument.SelectSingleNode("/ContextItemUsages");
            return contextItemUsagesNode;
        }

        #region CURD

        public ContextItemUsageCollection LoadContextItemUsages()
        {
            XmlDocument xmlDocument = GetXmlDocument();

            XmlNode contextItemUsagesNode = SelectContextItemUsagesNode(xmlDocument);
            if (contextItemUsagesNode != null)
            {
                ContextItemUsageCollection contextItemUsages = new ContextItemUsageCollection();
                foreach (XmlNode contextItemUsageNode in contextItemUsagesNode.ChildNodes)
                {
                    ContextItemUsage contextItemUsage = ContextItemUsageXmlMaper.ReadContextItemUsage(contextItemUsageNode);
                    contextItemUsages.Add(contextItemUsage);
                }

                return contextItemUsages;
            }

            return null;
        }

        public void AddContextItemUsage(ContextItemUsage contextItemUsage)
        {
            XmlDocument xmlDocument = GetXmlDocument();

            XmlNode contextItemUsagesNode = SelectContextItemUsagesNode(xmlDocument);
            ContextItemUsageXmlMaper.WriteContextItemUsage(contextItemUsage, contextItemUsagesNode);

            SaveXmlDocumentSafely(xmlDocument);
        }

        public void UpdateContextItemUsages(ContextItemUsageCollection contextItemUsages)
        {
            XmlDocument xmlDocument = GetXmlDocument();

            XmlNode contextItemUsagesNode = SelectContextItemUsagesNode(xmlDocument);
            contextItemUsagesNode.RemoveAll();
            foreach (ContextItemUsage contextItemUsage in contextItemUsages)
            {
                ContextItemUsageXmlMaper.WriteContextItemUsage(contextItemUsage, contextItemUsagesNode);
            }

            SaveXmlDocumentSafely(xmlDocument);
        }

        #endregion
    }
}
