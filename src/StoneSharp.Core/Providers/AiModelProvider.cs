using StoneSharp.Core.Helpers;
using StoneSharp.Core.Models;
using StoneSharp.Core.Stores;

namespace StoneSharp.Core.Providers
{
    public class AiModelProvider
    {
        const string DefaultConfigureFileName = "aimodel.xml";

        public AiModelProvider(string fileName = null)
        {
            if (fileName == null)
            {
                fileName = GetDefaultConfigureFile();
            }

            bool isCreateNew = !File.Exists(fileName);

            AiModelStore = new AiModelStore(fileName, true);

            if (isCreateNew)
            {
                AiModelSetCollection aiModelSets = GetSampleAiModeSets();
                UpdateAiModels(aiModelSets);
            }
        }

        #region Singleton pattern

        private static AiModelProvider _current;

        public static AiModelProvider Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new AiModelProvider();
                }

                return _current;
            }
        }

        #endregion

        public static string GetDefaultConfigureFile()
        {
            string folderPath = ApplicationDataHelper.GetAgentFolder();
            string filePath = System.IO.Path.Combine(folderPath, DefaultConfigureFileName);

            return filePath;
        }

        public static AiModelSetCollection GetSampleAiModeSets()
        {
            AiModelSetCollection aiModelSets = new AiModelSetCollection();

            AiModelSet aiModelSet = new AiModelSet()
            {
                Name = "Kimi",
                ApiUrl = "https://api.moonshot.cn/v1/",
                ApiKey = "Your API key",
                AiModels = new AiModelCollection()
                {
                    new AiModel("moonshot-v1-8k"),
                }
            };
            aiModelSets.Add(aiModelSet);

            return aiModelSets;
        }

        public AiModelStore AiModelStore { get; private set; }

        public AiModelSetCollection LoadAiModelSets()
        {
            return AiModelStore.LoadAiModelSets();
        }

        public void UpdateAiModels(AiModelSetCollection aiModelSets)
        {
            AiModelStore.UpdateAiModelSets(aiModelSets);
        }

        Dictionary<string, object> _aiModelNameMap;

        public Dictionary<string, object> GetAiModelNameMap()
        {
            if (_aiModelNameMap == null)
            {
                _aiModelNameMap = AiModelStore.GetAiModelSetNameMap();
            }

            return _aiModelNameMap;
        }
    }
}
