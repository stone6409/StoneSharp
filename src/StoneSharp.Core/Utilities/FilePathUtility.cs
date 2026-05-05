using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StoneSharp.Core.Utilities
{
    public static class FilePathUtility
    {
        private static string _currentApplictionProjectFolder;

        public static string CurrentApplictionProjectFolder
        {
            get
            {
                if (_currentApplictionProjectFolder == null)
                {
                    _currentApplictionProjectFolder = GetApplicationDataFolder();
                }

                return _currentApplictionProjectFolder;
            }
            set
            {
                _currentApplictionProjectFolder = value;
            }
        }

        public static string GetEntryAssemblyPath()
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        }


        public static string GetExecutingAssemblyPath()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public static string ApplcationName { get; set; } = ApplicationUtility.ApplicationName;

        // example: C:\Users\Evan.Qin\AppData\Roaming\MyApplication
        public static string GetApplicationDataFolder()
        {
            string applicationDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string myApplicationDataFolder = Path.Combine(applicationDataFolder, ApplcationName);

            if (!File.Exists(myApplicationDataFolder))
            {
                Directory.CreateDirectory(myApplicationDataFolder);
            }

            return myApplicationDataFolder;
        }

        public static void EnsureDirectoryExists(string path)
        {
            try
            {
                // ... If the directory doesn't exist, create it.
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception)
            {
            }
        }

        public static void OpenFolderInExplorer(string folderPath)
        {
            string cmd = "explorer.exe";
            string arg = folderPath;
            Process.Start(cmd, arg);
        }

        public static void OpenFileInExplorer(string filePath)
        {
            string cmd = "explorer.exe";
            string arg = "/select, " + filePath;
            Process.Start(cmd, arg);
        }

        public static string GetRelativePath(string fromDirectory, string toPath)
        {
            if (fromDirectory == null)
                throw new ArgumentNullException("fromDirectory");

            if (toPath == null)
                throw new ArgumentNullException("toPath");

            bool isRooted = (Path.IsPathRooted(fromDirectory) && Path.IsPathRooted(toPath));

            if (isRooted)
            {
                bool isDifferentRoot = (string.Compare(Path.GetPathRoot(fromDirectory), Path.GetPathRoot(toPath), true) != 0);

                // 如果根目录不相同，则直接返回
                if (isDifferentRoot)
                    return toPath;
            }

            List<string> relativePath = new List<string>();
            string[] fromDirectories = fromDirectory.Split(Path.DirectorySeparatorChar);

            string[] toDirectories = toPath.Split(Path.DirectorySeparatorChar);

            int length = Math.Min(fromDirectories.Length, toDirectories.Length);

            int lastCommonRoot = -1;

            // find common root
            for (int x = 0; x < length; x++)
            {
                if (string.Compare(fromDirectories[x], toDirectories[x], true) != 0)
                    break;

                lastCommonRoot = x;
            }

            if (lastCommonRoot == -1)
                return toPath;

            // add relative folders in from path
            for (int x = lastCommonRoot + 1; x < fromDirectories.Length; x++)
            {
                if (fromDirectories[x].Length > 0)
                    relativePath.Add("..");
            }

            // add to folders to path
            for (int x = lastCommonRoot + 1; x < toDirectories.Length; x++)
            {
                relativePath.Add(toDirectories[x]);
            }

            // create relative path
            string[] relativeParts = new string[relativePath.Count];
            relativePath.CopyTo(relativeParts, 0);

            string newPath = string.Join(Path.DirectorySeparatorChar.ToString(), relativeParts);

            return newPath;
        }

        public static bool IsFileInDirectory(string filePath, string directoryPath, out string relativePath)
        {
            // 标准化路径
            string fullDirectoryPath = Path.GetFullPath(directoryPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            string fullFilePath = Path.GetFullPath(filePath);

            // 确保路径存在
            //if (!Directory.Exists(fullDirectoryPath) || !File.Exists(fullFilePath))
            //{
            //    relativePath = null;
            //    return false;
            //}

            // 判断文件路径是否以目录路径作为前缀
            if (fullFilePath.StartsWith(fullDirectoryPath, StringComparison.OrdinalIgnoreCase))
            {
                // 获取相对路径
                relativePath = GetRelativePath(fullDirectoryPath, fullFilePath);
                return true;
            }

            relativePath = null;
            return false;
        }

        public static bool AreFolderPathsEqual(string path1, string path2)
        {
            if (path1 == null || path2 == null)
            {
                if (path1 == path2)
                    return true;

                return false;
            }

            // 规范化路径
            string normalizedPath1 = Path.GetFullPath(new Uri(path1).LocalPath);
            string normalizedPath2 = Path.GetFullPath(new Uri(path2).LocalPath);

            // 忽略路径末尾的斜杠（如果存在）
            if (normalizedPath1.EndsWith("\\"))
            {
                normalizedPath1 = normalizedPath1.TrimEnd('\\');
            }
            if (normalizedPath2.EndsWith("\\"))
            {
                normalizedPath2 = normalizedPath2.TrimEnd('\\');
            }

            // 使用FileInfo的FullName属性进行比较，它会自动规范化路径
            return new DirectoryInfo(normalizedPath1).FullName.Equals(new DirectoryInfo(normalizedPath2).FullName, StringComparison.OrdinalIgnoreCase);
        }

        public static void CopyDirectory(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string filePath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(filePath, filePath.Replace(sourcePath, targetPath), true);
            }
        }

        public static void ClearDirectory(string folderPath)
        {
            try
            {
                if (Directory.Exists(folderPath))
                {
                    // 删除文件夹中的所有文件和子文件夹
                    foreach (string file in Directory.GetFiles(folderPath))
                    {
                        File.Delete(file);
                    }
                    foreach (string dir in Directory.GetDirectories(folderPath))
                    {
                        Directory.Delete(dir, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing template items folder: {ex.Message}");
            }
        }

        public static void ClearDirectoryFiles(string folderPath, string searchPattern, bool recursive = true)
        {
            try
            {
                if (Directory.Exists(folderPath))
                {
                    // 删除当前文件夹中的文件
                    foreach (string file in Directory.GetFiles(folderPath, searchPattern))
                    {
                        File.Delete(file);
                    }

                    // 如果启用递归，遍历子文件夹
                    if (recursive)
                    {
                        foreach (string subFolder in Directory.GetDirectories(folderPath))
                        {
                            ClearDirectoryFiles(subFolder, searchPattern, recursive);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing directory files: {ex.Message}");
            }
        }

        /// <summary>
        /// 根据类的命名空间获取对应的文件夹路径，并与程序集路径组合。
        /// </summary>
        /// <param name="type">类的类型信息。</param>
        /// <returns>完整的文件夹路径。</returns>
        public static string GetFolderFromNamespace(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type), "类型不能为空。");
            }

            // 获取类的命名空间
            string namespacePath = type.Namespace;

            if (string.IsNullOrEmpty(namespacePath))
            {
                throw new InvalidOperationException("指定的类型没有命名空间。");
            }

            // 获取程序集所在的路径
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (string.IsNullOrEmpty(assemblyPath))
            {
                throw new InvalidOperationException("无法获取程序集路径。");
            }

            // 将命名空间中的点（.）替换为路径分隔符
            string relativeFolderPath = namespacePath.Replace('.', Path.DirectorySeparatorChar);

            // 去掉程序集名称部分
            string assemblyName = type.Assembly.GetName().Name;
            if (relativeFolderPath.StartsWith(assemblyName))
            {
                relativeFolderPath = relativeFolderPath.Substring(assemblyName.Length).TrimStart(Path.DirectorySeparatorChar);
            }

            // 组合程序集路径和命名空间路径
            string fullFolderPath = Path.Combine(assemblyPath, relativeFolderPath);

            return fullFolderPath;
        }

        public static bool MoveFileToDirectory(string sourceFilePath, string targetDirectory)
        {
            try
            {
                // 检查目标目录是否存在
                if (!Directory.Exists(targetDirectory))
                {
                    Console.WriteLine($"目标目录不存在: {targetDirectory}");
                    return false;
                }

                // 获取文件名
                string fileName = Path.GetFileName(sourceFilePath);
                // 构造目标路径
                string targetFilePath = Path.Combine(targetDirectory, fileName);

                // 检查目标文件是否已经存在
                if (File.Exists(targetFilePath))
                {
                    Console.WriteLine($"目标文件已存在，无需移动: {targetFilePath}");
                    return false;
                }

                // 移动文件
                File.Move(sourceFilePath, targetFilePath);
                Console.WriteLine($"文件已成功移动到: {targetFilePath}");
                return true;
            }
            catch (Exception ex)
            {
                // 处理异常，例如记录日志或提示用户
                Console.WriteLine($"移动文件时发生错误: {ex.Message}");
                return false;
            }
        }
    }
}
