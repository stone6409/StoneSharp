using System;
using StoneSharp.Core.Subjects;

namespace StoneSharp.Core.Subjects
{
    /// <summary>
    /// Subject服务接口，用于查找和解析subject.md文件
    /// </summary>
    public interface ISubjectService
    {
        /// <summary>
        /// 从指定的对话文件夹和主题文件夹中查找并解析subject.md文件
        /// </summary>
        /// <param name="chatFolder">对话文件夹路径</param>
        /// <param name="subjectFolder">主题文件夹相对路径（相对于chatFolder）</param>
        /// <returns>解析后的Subject对象，如果未找到则返回null</returns>
        Subject FindAndParseSubject(string chatFolder, string subjectFolder);

        /// <summary>
        /// 从指定的对话文件夹和主题文件夹中查找subject.md文件路径
        /// </summary>
        /// <param name="chatFolder">对话文件夹路径</param>
        /// <param name="subjectFolder">主题文件夹相对路径（相对于chatFolder）</param>
        /// <returns>找到的subject.md文件路径，如果未找到则返回null</returns>
        string FindSubjectFilePath(string chatFolder, string subjectFolder);
    }
}