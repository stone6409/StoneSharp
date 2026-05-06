using StoneSharp.Core.Models;
using System.Xml;

namespace StoneSharp.Core.Stores
{
    public class AiModelStore : XmlStoreBase
    {
        public AiModelStore(string configFileName, bool ensureExist = false) : base(configFileName, ensureExist)
        {
        }

        protected override void OnCreateXmlDocument(XmlDocument xmlDocument)
        {
            XmlElement root = xmlDocument.CreateElement("AiModelSets");
            xmlDocument.AppendChild(root);
        }

        private XmlNode SelectAiModelSetsNode(XmlDocument xmlDocument)
        {
            XmlNode aiModelsNode = xmlDocument.SelectSingleNode("/AiModelSets");
            return aiModelsNode;
        }

        #region CURD

        public Dictionary<string, object> GetAiModelSetNameMap()
        {
            XmlDocument xmlDocument = GetXmlDocument();

            XmlNode aiModelsNode = SelectAiModelSetsNode(xmlDocument);
            if (aiModelsNode != null)
            {
                Dictionary<string, object> nameMap = new Dictionary<string, object>();
                foreach (XmlNode aiModelNode in aiModelsNode.ChildNodes)
                {
                    string childName = aiModelNode.Attributes["Name"].Value;
                    nameMap.Add(childName, null);
                }

                return nameMap;
            }

            return null;
        }

        public AiModelSetCollection LoadAiModelSets()
        {
            XmlDocument xmlDocument = GetXmlDocument();

            XmlNode aiModelsNode = SelectAiModelSetsNode(xmlDocument);
            if (aiModelsNode != null)
            {
                AiModelSetCollection aiModels = new AiModelSetCollection();
                foreach (XmlNode aiModelNode in aiModelsNode.ChildNodes)
                {
                    AiModelSet aiModel = AiModelSetXmlMaper.ReadAiModelSet(aiModelNode);
                    aiModels.Add(aiModel);
                }

                return aiModels;
            }

            return null;
        }

        public AiModelSet LoadAiModelSet(string name)
        {
            XmlDocument xmlDocument = GetXmlDocument();

            XmlNode aiModelsNode = SelectAiModelSetsNode(xmlDocument);
            foreach (XmlNode aiModelNode in aiModelsNode.ChildNodes)
            {
                AiModelSet aiModel = AiModelSetXmlMaper.ReadAiModelSet(aiModelNode);
                if (aiModel.Name == name)
                {
                    return aiModel;
                }
            }

            return null;
        }

        public void AddAiModelSet(AiModelSet aiModel)
        {
            XmlDocument xmlDocument = GetXmlDocument();

            XmlNode aiModelsNode = SelectAiModelSetsNode(xmlDocument);
            AiModelSetXmlMaper.WriteAiModelSet(aiModel, aiModelsNode);

            SaveXmlDocument(xmlDocument);
        }

        public void RemoveAiModelSet(string name)
        {
            XmlDocument xmlDocument = GetXmlDocument();

            XmlNode aiModelsNode = SelectAiModelSetsNode(xmlDocument);
            foreach (XmlNode aiModelNode in aiModelsNode.ChildNodes)
            {
                AiModelSet aiModel = AiModelSetXmlMaper.ReadAiModelSet(aiModelNode);
                if (aiModel.Name == name)
                {
                    aiModelNode.ParentNode.RemoveChild(aiModelNode);
                }
            }

            SaveXmlDocument(xmlDocument);
        }

        public void UpdateAiModelSet(AiModelSet aiModel)
        {
            XmlDocument xmlDocument = GetXmlDocument();

            XmlNode aiModelsNode = SelectAiModelSetsNode(xmlDocument);
            AiModelSetXmlMaper.WriteAiModelSet(aiModel, aiModelsNode);

            XmlNode lastChildNode = aiModelsNode.LastChild;
            foreach (XmlNode aiModelNode in aiModelsNode.ChildNodes)
            {
                AiModelSet aiModel1 = AiModelSetXmlMaper.ReadAiModelSet(aiModelNode);
                if (aiModelNode != lastChildNode && aiModel1.Name == aiModel.Name)
                {
                    aiModelsNode.RemoveChild(lastChildNode);
                    aiModelsNode.ReplaceChild(lastChildNode, aiModelNode);
                    break;
                }
            }

            SaveXmlDocument(xmlDocument);
        }

        public void UpdateAiModelSets(AiModelSetCollection aiModels)
        {
            XmlDocument xmlDocument = GetXmlDocument();

            XmlNode aiModelsNode = SelectAiModelSetsNode(xmlDocument);
            aiModelsNode.RemoveAll();
            foreach (AiModelSet aiModel in aiModels)
            {
                AiModelSetXmlMaper.WriteAiModelSet(aiModel, aiModelsNode);
            }

            SaveXmlDocument(xmlDocument);
        }

        #endregion
    }
}
