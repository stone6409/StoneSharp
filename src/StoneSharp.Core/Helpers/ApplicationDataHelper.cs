using StoneSharp.Core.Utilities;
using System.IO;

namespace StoneSharp.Core.Helpers
{
    public static class ApplicationDataHelper
    {
        private static string? _customAgentFolder;

        public static string CustomAgentFolder
        {
            get
            {
                return _customAgentFolder;
            }
            set
            {
                _customAgentFolder = value;
            }
        }

        public static string GetAgentFolder()
        {
            // 使用AgentFileFinder获取.agent文件夹路径
            string agentFolderPath = AgentFileFinder.FindDotAgentFolderPath();

            if (!string.IsNullOrEmpty(agentFolderPath))
            {
                return agentFolderPath;
            }

            // 回退到默认的MyAgentFolder
            if (!string.IsNullOrEmpty(CustomAgentFolder) && Directory.Exists(CustomAgentFolder))
            {
                return CustomAgentFolder;
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
            string agentFolderPath = GetAgentFolder();
            string skillsFolderPath = Path.Combine(agentFolderPath, "skills");
            FilePathUtility.EnsureDirectoryExists(skillsFolderPath);

            return skillsFolderPath;
        }

        public static string GetAgentPromptsFolder()
        {
            string agentFolderPath = GetAgentFolder();
            string promptsFolderPath = Path.Combine(agentFolderPath, "prompts");
            FilePathUtility.EnsureDirectoryExists(promptsFolderPath);

            return promptsFolderPath;
        }

        public static string GetAgentRulesFolder()
        {
            string agentFolderPath = GetAgentFolder();
            string rulesFolderPath = Path.Combine(agentFolderPath, "rules");
            FilePathUtility.EnsureDirectoryExists(rulesFolderPath);

            return rulesFolderPath;
        }
    }
}