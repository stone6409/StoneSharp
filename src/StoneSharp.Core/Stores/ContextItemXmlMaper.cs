using StoneSharp.Core.Models.ContextItems;
using System.Xml;

namespace StoneSharp.Core.Stores
{
    public static class ContextItemXmlMaper
    {
        #region Write Object

        private delegate void WriteContextItemMethod(ContextItem contextItem, XmlNode xmlNode);

        private static Dictionary<Type, WriteContextItemMethod> _writeContextItemMethods;

        private static Dictionary<Type, WriteContextItemMethod> WriteContextItemMethods
        {
            get
            {
                if (_writeContextItemMethods == null)
                {
                    _writeContextItemMethods = new Dictionary<Type, WriteContextItemMethod>
                    {
                        { typeof(ContextFile), WriteContextFile },
                        { typeof(ContextFileSnippet), WriteContextFileSnippet },
                        { typeof(ContextFolder), WriteContextFolder },
                        { typeof(ContextKnowledgeBase), WriteContextKnowledgeBase },
                        { typeof(ContextRuleFile), WriteContextRuleFile },
                    };
                }

                return _writeContextItemMethods;
            }
        }

        public static void WriteContextItem(ContextItem contextItem, XmlNode xmlNode)
        {
            if (contextItem == null)
                throw new ArgumentNullException(nameof(contextItem));

            if (xmlNode == null)
                throw new ArgumentNullException(nameof(xmlNode));

            WriteContextItemMethod writeContextItemMethod = WriteContextItemMethods[contextItem.GetType()];
            writeContextItemMethod(contextItem, xmlNode);
        }

        private static void WriteContextFile(ContextItem contextItem, XmlNode xmlNode)
        {
            ContextFile contextFile = contextItem as ContextFile;
            XmlElement xmlElement = xmlNode.OwnerDocument.CreateElement("ContextFile");

            XmlAttributeHelper.WriteAttribute(xmlElement, "FilePath", contextFile.FilePath);
            XmlAttributeHelper.WriteAttribute(xmlElement, "FileContent", contextFile.FileContent);
            XmlAttributeHelper.WriteAttribute(xmlElement, "StartLine", contextFile.StartLine);
            XmlAttributeHelper.WriteAttribute(xmlElement, "EndLine", contextFile.EndLine);
            XmlAttributeHelper.WriteAttribute(xmlElement, "CodeLanguage", contextFile.CodeLanguage);

            xmlNode.AppendChild(xmlElement);
        }

        private static void WriteContextFileSnippet(ContextItem contextItem, XmlNode xmlNode)
        {
            ContextFileSnippet contextFileSnippet = contextItem as ContextFileSnippet;
            XmlElement xmlElement = xmlNode.OwnerDocument.CreateElement("ContextFileSnippet");

            XmlAttributeHelper.WriteAttribute(xmlElement, "FilePath", contextFileSnippet.FilePath);
            XmlAttributeHelper.WriteAttribute(xmlElement, "SnippetContent", contextFileSnippet.SnippetContent);
            XmlAttributeHelper.WriteAttribute(xmlElement, "StartLine", contextFileSnippet.StartLine);
            XmlAttributeHelper.WriteAttribute(xmlElement, "EndLine", contextFileSnippet.EndLine);
            XmlAttributeHelper.WriteAttribute(xmlElement, "CodeLanguage", contextFileSnippet.CodeLanguage);

            xmlNode.AppendChild(xmlElement);
        }

        private static void WriteContextFolder(ContextItem contextItem, XmlNode xmlNode)
        {
            ContextFolder contextFolder = contextItem as ContextFolder;
            XmlElement xmlElement = xmlNode.OwnerDocument.CreateElement("ContextFolder");

            XmlAttributeHelper.WriteAttribute(xmlElement, "FolderPath", contextFolder.FolderPath);
            XmlAttributeHelper.WriteAttribute(xmlElement, "FolderSummary", contextFolder.FolderSummary);

            xmlNode.AppendChild(xmlElement);
        }

        private static void WriteContextKnowledgeBase(ContextItem contextItem, XmlNode xmlNode)
        {
            ContextKnowledgeBase contextKnowledgeBase = contextItem as ContextKnowledgeBase;
            XmlElement xmlElement = xmlNode.OwnerDocument.CreateElement("ContextKnowledgeBase");

            XmlAttributeHelper.WriteAttribute(xmlElement, "Name", contextKnowledgeBase.Name);

            xmlNode.AppendChild(xmlElement);
        }

        private static void WriteContextRuleFile(ContextItem contextItem, XmlNode xmlNode)
        {
            ContextRuleFile contextRuleFile = contextItem as ContextRuleFile;
            XmlElement xmlElement = xmlNode.OwnerDocument.CreateElement("ContextRuleFile");

            XmlAttributeHelper.WriteAttribute(xmlElement, "FilePath", contextRuleFile.FilePath);

            xmlNode.AppendChild(xmlElement);
        }

        #endregion

        #region Read Object

        private delegate ContextItem ReadContextItemMethod(XmlNode xmlNode);

        private static Dictionary<string, ReadContextItemMethod> _readContextItemMethods;

        private static Dictionary<string, ReadContextItemMethod> ReadContextItemMethods
        {
            get
            {
                if (_readContextItemMethods == null)
                {
                    _readContextItemMethods = new Dictionary<string, ReadContextItemMethod>
                    {
                        { "ContextFile", ReadContextFile },
                        { "ContextFileSnippet", ReadContextFileSnippet },
                        { "ContextFolder", ReadContextFolder },
                        { "ContextKnowledgeBase", ReadContextKnowledgeBase },
                        { "ContextRuleFile", ReadContextRuleFile },
                    };
                }

                return _readContextItemMethods;
            }
        }

        public static ContextItem ReadContextItem(XmlNode xmlNode)
        {
            string typeName = xmlNode.Name;
            if (ReadContextItemMethods.TryGetValue(typeName, out var readContextItemMethod))
            {
                try
                {
                    return readContextItemMethod(xmlNode);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to read ContextItem of type '{typeName}'.", ex);
                }
            }

            return null;
        }

        private static ContextFile ReadContextFile(XmlNode xmlNode)
        {
            ContextFile contextFile = new ContextFile();
            contextFile.FilePath = XmlAttributeHelper.ReadAttribute(xmlNode, "FilePath", null);
            contextFile.FileContent = XmlAttributeHelper.ReadAttribute(xmlNode, "FileContent", null);
            contextFile.StartLine = XmlAttributeHelper.ReadAttribute(xmlNode, "StartLine", 0);
            contextFile.EndLine = XmlAttributeHelper.ReadAttribute(xmlNode, "EndLine", -1);
            contextFile.CodeLanguage = XmlAttributeHelper.ReadAttribute(xmlNode, "CodeLanguage", null);

            return contextFile;
        }

        private static ContextFileSnippet ReadContextFileSnippet(XmlNode xmlNode)
        {
            ContextFileSnippet contextFileSnippet = new ContextFileSnippet();
            contextFileSnippet.FilePath = XmlAttributeHelper.ReadAttribute(xmlNode, "FilePath", null);
            contextFileSnippet.SnippetContent = XmlAttributeHelper.ReadAttribute(xmlNode, "SnippetContent", null);
            contextFileSnippet.StartLine = XmlAttributeHelper.ReadAttribute(xmlNode, "StartLine", 0);
            contextFileSnippet.EndLine = XmlAttributeHelper.ReadAttribute(xmlNode, "EndLine", -1);
            contextFileSnippet.CodeLanguage = XmlAttributeHelper.ReadAttribute(xmlNode, "CodeLanguage", null);

            return contextFileSnippet;
        }

        private static ContextFolder ReadContextFolder(XmlNode xmlNode)
        {
            ContextFolder contextFolder = new ContextFolder();
            contextFolder.FolderPath = XmlAttributeHelper.ReadAttribute(xmlNode, "FolderPath", null);
            contextFolder.FolderSummary = XmlAttributeHelper.ReadAttribute(xmlNode, "FolderSummary", null);

            return contextFolder;
        }

        private static ContextKnowledgeBase ReadContextKnowledgeBase(XmlNode xmlNode)
        {
            ContextKnowledgeBase contextKnowledgeBase = new ContextKnowledgeBase();
            contextKnowledgeBase.Name = XmlAttributeHelper.ReadAttribute(xmlNode, "Name", null);

            return contextKnowledgeBase;
        }

        private static ContextRuleFile ReadContextRuleFile(XmlNode xmlNode)
        {
            ContextRuleFile contextRuleFile = new ContextRuleFile();
            contextRuleFile.FilePath = XmlAttributeHelper.ReadAttribute(xmlNode, "FilePath", null);

            return contextRuleFile;
        }

        #endregion
    }
}