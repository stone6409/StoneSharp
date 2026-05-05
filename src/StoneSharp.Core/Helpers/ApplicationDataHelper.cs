using StoneSharp.Core.Utilities;
using System.IO;

namespace StoneSharp.Core.Helpers
{
    public static class ApplicationDataHelper
    {
        private static string? _customMyAgentFolder;

        public static string MyAgentFolder
        {
            get
            {
                return _customMyAgentFolder;
            }
            set
            {
                _customMyAgentFolder = value;
            }
        }

        public static string GetMyAgentFolder()
        {
            // 使用AgentFileFinder获取.agent文件夹路径
            string agentFolderPath = AgentFileFinder.FindDotAgentFolderPath();

            if (!string.IsNullOrEmpty(agentFolderPath))
            {
                return agentFolderPath;
            }

            // 回退到默认的MyAgentFolder
            if (!string.IsNullOrEmpty(_customMyAgentFolder) && Directory.Exists(MyAgentFolder))
            {
                return MyAgentFolder;
            }

            agentFolderPath = Path.Combine(GetApplicationDataFolder(), ".agent");
            FilePathUtility.EnsureDirectoryExists(agentFolderPath);
            return agentFolderPath;
        }

        public static string GetApplicationDataFolder()
        {
            string applicationName = ApplicationUtility.ApplicationName;
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            folderPath = Path.Combine(folderPath, applicationName);

            return folderPath;
        }

        public static string GetAgentSkillsFolder()
        {
            // 首先尝试从.agent文件夹中获取skills文件夹
            string agentFolderPath = GetMyAgentFolder();
            string skillsFolderPath = Path.Combine(agentFolderPath, "skills");
            FilePathUtility.EnsureDirectoryExists(skillsFolderPath);

            return skillsFolderPath;
        }

        public static string GetAgentPromptsFolder()
        {
            string agentFolderPath = GetMyAgentFolder();
            string promptsFolderPath = Path.Combine(agentFolderPath, "prompts");
            FilePathUtility.EnsureDirectoryExists(promptsFolderPath);

            return promptsFolderPath;
        }

        public static string GetAgentRulesFolder()
        {
            string agentFolderPath = GetMyAgentFolder();
            string rulesFolderPath = Path.Combine(agentFolderPath, "rules");
            FilePathUtility.EnsureDirectoryExists(rulesFolderPath);

            return rulesFolderPath;
        }
    }
}