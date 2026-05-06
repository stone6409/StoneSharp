using StoneSharp.Core.Models;
using System.Xml;

namespace StoneSharp.Core.Stores
{
    public static class AiModelSetXmlMaper
    {
        #region Write Object

        public static void WriteAiModelSet(AiModelSet aiModelSet, XmlNode xmlNode)
        {
            XmlElement xmlElement = xmlNode.OwnerDocument.CreateElement("AiModelSet");

            XmlAttributeHelper.WriteAttribute(xmlElement, "Name", aiModelSet.Name);
            XmlAttributeHelper.WriteAttribute(xmlElement, "ApiUrl", aiModelSet.ApiUrl);
            XmlAttributeHelper.WriteAttribute(xmlElement, "ApiKey", aiModelSet.ApiKey);

            WriteAiModels(aiModelSet.AiModels, xmlElement);

            xmlNode.AppendChild(xmlElement);
        }

        public static void WriteAiModels(IEnumerable<AiModel> aiModels, XmlNode xmlNode)
        {
            foreach (AiModel aiModel in aiModels)
            {
                XmlElement xmlElement = xmlNode.OwnerDocument.CreateElement("AiModel");
                XmlAttributeHelper.WriteAttribute(xmlElement, "Name", aiModel.Name);
                XmlAttributeHelper.WriteAttribute(xmlElement, "AliasName", aiModel.AliasName);
                XmlAttributeHelper.WriteAttribute(xmlElement, "MaxTokens", aiModel.MaxTokens);

                xmlNode.AppendChild(xmlElement);
            }
        }

        #endregion

        #region Raad Object

        public static AiModelSet ReadAiModelSet(XmlNode xmlNode)
        {
            AiModelSet aiModelSet = new AiModelSet();
            aiModelSet.Name = XmlAttributeHelper.ReadAttribute(xmlNode, "Name", null);
            aiModelSet.ApiUrl = XmlAttributeHelper.ReadAttribute(xmlNode, "ApiUrl", null);
            aiModelSet.ApiKey = XmlAttributeHelper.ReadAttribute(xmlNode, "ApiKey", null);

            aiModelSet.AiModels = ReadAiModels(xmlNode);

            return aiModelSet;
        }

        public static AiModelCollection ReadAiModels(XmlNode xmlNode)
        {
            AiModelCollection aiModels = new AiModelCollection();

            XmlNodeList aiModelNodes = xmlNode.SelectNodes("AiModel");
            foreach (XmlNode aiModleNode in aiModelNodes)
            {
                AiModel aiModel = new AiModel();
                aiModel.Name = XmlAttributeHelper.ReadAttribute(aiModleNode, "Name", null);
                aiModel.AliasName = XmlAttributeHelper.ReadAttribute(aiModleNode, "AliasName", null);
                aiModel.MaxTokens = XmlAttributeHelper.ReadAttribute(aiModleNode, "MaxTokens", 0);

                aiModels.Add(aiModel);
            }

            return aiModels;
        }

        #endregion
    }
}
