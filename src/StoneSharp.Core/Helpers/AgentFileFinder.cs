using StoneSharp.Core.Utilities;

namespace StoneSharp.Core.Helpers
{
    /// <summary>
    /// Agent文件和文件夹查找器（单例模式）
    /// </summary>
    public static class AgentFileFinder
    {
        private const string AgentFileName = "AGENT.md";
        private const string AgentFolderName = ".agent";
        private const string SettingsFileName = "settings.json";

        /// <summary>
        /// 查找AGENT.md文件，按照优先级顺序查找
        /// </summary>
        /// <returns>AGENT.md文件的完整路径，如果未找到则返回null</returns>
        public static string FindAgentFile()
        {
            // 第1步：在当前目录及其父目录中查找
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = FindAgentFileInDirectoryHierarchy(currentDirectory);

            if (!string.IsNullOrEmpty(filePath))
            {
                return filePath;
            }

            // 第2步：在用户数据文件夹中查找
            filePath = FindAgentFileInUserDataFolder();
            if (!string.IsNullOrEmpty(filePath))
            {
                return filePath;
            }

            // 第3步：在程序数据文件夹中查找
            filePath = FindAgentFileInProgramDataFolder();

            return filePath;
        }

        /// <summary>
        /// 在目录层次结构中查找AGENT.md文件
        /// </summary>
        private static string FindAgentFileInDirectoryHierarchy(string startDirectory)
        {
            string currentDir = startDirectory;

            while (currentDir != null)
            {
                // 检查当前目录是否有AGENT.md
                string agentFilePath = Path.Combine(currentDir, AgentFileName);

                if (File.Exists(agentFilePath))
                {
                    return agentFilePath;
                }

                // 检查当前目录是否有.agent子文件夹，并且.agent子文件夹中有AGENT.md
                string agentFolderPath = Path.Combine(currentDir, AgentFolderName);
                if (Directory.Exists(agentFolderPath))
                {
                    string agentFileInFolder = Path.Combine(agentFolderPath, AgentFileName);
                    if (File.Exists(agentFileInFolder))
                    {
                        return agentFileInFolder;
                    }
                }

                // 获取父目录
                DirectoryInfo parentDir = Directory.GetParent(currentDir);
                if (parentDir == null)
                {
                    // 已经到达顶级目录
                    break;
                }

                currentDir = parentDir.FullName;
            }

            return null;
        }

        /// <summary>
        /// 在用户数据文件夹中查找AGENT.md文件
        /// </summary>
        private static string FindAgentFileInUserDataFolder()
        {
            try
            {
                string userDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                if (string.IsNullOrEmpty(userDataFolder))
                {
                    return null;
                }

                return FindAgentFileInAppFolder(userDataFolder);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"在用户数据文件夹中查找AGENT.md文件时发生错误: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 在程序数据文件夹中查找AGENT.md文件
        /// </summary>
        private static string FindAgentFileInProgramDataFolder()
        {
            try
            {
                string programDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                if (string.IsNullOrEmpty(programDataFolder))
                {
                    return null;
                }

                return FindAgentFileInAppFolder(programDataFolder);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"在程序数据文件夹中查找AGENT.md文件时发生错误: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 在应用程序文件夹中查找AGENT.md文件
        /// </summary>
        private static string FindAgentFileInAppFolder(string baseFolder)
        {
            string applicationName = ApplicationUtility.ApplicationName;

            string appFolderPath = Path.Combine(baseFolder, applicationName);
            if (!Directory.Exists(appFolderPath))
            {
                return null;
            }

            // 检查AGENT.md文件是否存在
            string agentFilePath = Path.Combine(appFolderPath, AgentFileName);
            if (File.Exists(agentFilePath))
            {
                return agentFilePath;
            }

            return null;
        }

        /// <summary>
        /// 获取.agent文件夹路径（如果存在）
        /// 只在找到AGENT.md文件后才查找.agent文件夹
        /// </summary>
        public static string FindDotAgentFolderPath()
        {
            // 第1步：在当前目录及其父目录中查找
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string folderPath = FindDotAgentFolderInDirectoryHierarchy(currentDirectory);

            if (!string.IsNullOrEmpty(folderPath))
            {
                return folderPath;
            }

            // 第2步：在用户数据文件夹中查找
            folderPath = FindDotAgentFolderInUserDataFolder();
            if (!string.IsNullOrEmpty(folderPath))
            {
                return folderPath;
            }

            // 第3步：在程序数据文件夹中查找
            folderPath = FindDotAgentFolderInProgramDataFolder();

            return folderPath;
        }

        /// <summary>
        /// 在目录层次结构中查找.agent文件夹
        /// </summary>
        private static string FindDotAgentFolderInDirectoryHierarchy(string startDirectory)
        {
            string currentDir = startDirectory;

            while (currentDir != null)
            {
                // 检查AGENT.md文件是否存在
                string agentFilePath = Path.Combine(currentDir, AgentFileName);
                if (File.Exists(agentFilePath))
                {
                    // 检查当前目录是否有.agent文件夹
                    string dotAgentFolderPath = Path.Combine(currentDir, AgentFolderName);
                    if (Directory.Exists(dotAgentFolderPath))
                    {
                        return dotAgentFolderPath;
                    }
                }

                // 获取父目录
                DirectoryInfo parentDir = Directory.GetParent(currentDir);
                if (parentDir == null)
                {
                    // 已经到达顶级目录
                    break;
                }

                currentDir = parentDir.FullName;
            }

            return null;
        }

        /// <summary>
        /// 在用户数据文件夹中查找.agent文件夹
        /// </summary>
        private static string FindDotAgentFolderInUserDataFolder()
        {
            try
            {
                string userDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                if (string.IsNullOrEmpty(userDataFolder))
                {
                    return null;
                }

                return FindDotAgentFolderInAppFolder(userDataFolder);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"在用户数据文件夹中查找.agent文件夹时发生错误: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 在程序数据文件夹中查找.agent文件夹
        /// </summary>
        private static string FindDotAgentFolderInProgramDataFolder()
        {
            try
            {
                string programDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                if (string.IsNullOrEmpty(programDataFolder))
                {
                    return null;
                }

                return FindDotAgentFolderInAppFolder(programDataFolder);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"在程序数据文件夹中查找.agent文件夹时发生错误: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 在应用程序文件夹中查找.agent文件夹
        /// </summary>
        private static string FindDotAgentFolderInAppFolder(string baseFolder)
        {
            string applicationName = ApplicationUtility.ApplicationName;

            string appFolderPath = Path.Combine(baseFolder, applicationName);
            if (!Directory.Exists(appFolderPath))
            {
                return null;
            }

            // 检查AGENT.md文件是否存在
            string agentFilePath = Path.Combine(appFolderPath, AgentFileName);
            if (!File.Exists(agentFilePath))
            {
                return null;
            }

            // 检查.agent文件夹是否存在
            string dotAgentFolderPath = Path.Combine(appFolderPath, AgentFolderName);
            if (Directory.Exists(dotAgentFolderPath))
            {
                return dotAgentFolderPath;
            }

            return null;
        }

        /// <summary>
        /// 获取.agent文件夹中的settings.json文件路径
        /// </summary>
        public static string FindAgentSettingsFilePath()
        {
            string agentFolderPath = FindDotAgentFolderPath();
            if (string.IsNullOrEmpty(agentFolderPath))
            {
                return null;
            }

            string settingsFilePath = Path.Combine(agentFolderPath, SettingsFileName);
            if (File.Exists(settingsFilePath))
            {
                return settingsFilePath;
            }

            return null;
        }
    }
}