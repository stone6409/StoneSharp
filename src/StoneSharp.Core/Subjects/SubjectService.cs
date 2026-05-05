using System;
using System.IO;
using StoneSharp.Core.Subjects;

namespace StoneSharp.Core.Subjects
{
    /// <summary>
    /// Subject服务，用于查找和解析subject.md文件
    /// </summary>
    public class SubjectService : ISubjectService
    {
        /// <summary>
        /// 从指定的对话文件夹和主题文件夹中查找并解析subject.md文件
        /// </summary>
        /// <param name="chatFolder">对话文件夹路径</param>
        /// <param name="subjectFolder">主题文件夹相对路径（相对于chatFolder）</param>
        /// <returns>解析后的Subject对象，如果未找到则返回null</returns>
        public Subject FindAndParseSubject(string chatFolder, string subjectFolder)
        {
            string filePath = FindSubjectFilePath(chatFolder, subjectFolder);

            if (filePath == null)
                return null;

            try
            {
                string content = File.ReadAllText(filePath);
                return SubjectParser.ParseChatFile(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"读取或解析subject.md文件时发生错误: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 从指定的对话文件夹和主题文件夹中查找subject.md文件路径
        /// </summary>
        /// <param name="chatFolder">对话文件夹路径</param>
        /// <param name="subjectFolder">主题文件夹相对路径（相对于chatFolder）</param>
        /// <returns>找到的subject.md文件路径，如果未找到则返回null</returns>
        public string FindSubjectFilePath(string chatFolder, string subjectFolder)
        {
            if (string.IsNullOrEmpty(chatFolder))
                throw new ArgumentException("对话文件夹路径不能为空", nameof(chatFolder));

            string currentFolder;
            if (!string.IsNullOrEmpty(subjectFolder))
            {
                currentFolder = Path.Combine(chatFolder, subjectFolder);
            }
            else
            {
                currentFolder = chatFolder;
            }

            while (true)
            {
                string subjectFilePath = Path.Combine(currentFolder, "subject.md");

                if (File.Exists(subjectFilePath))
                {
                    return subjectFilePath;
                }

                // 向上移动到父文件夹
                string parentFolder = Path.GetDirectoryName(currentFolder);

                // 如果已经到达对话文件夹或无法获取父目录，则退出循环
                if (string.IsNullOrEmpty(parentFolder) || 
                    !parentFolder.StartsWith(chatFolder, StringComparison.OrdinalIgnoreCase))
                    break;

                currentFolder = parentFolder;
            }

            return null;
        }
    }
}