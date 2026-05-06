using System.Collections.Generic;

namespace StoneSharp.Core.Providers
{
    public interface IChatGroupProvider
    {
        /// <summary>
        /// 重命名文件夹
        /// </summary>
        /// <param name="oldPath">原文件夹路径</param>
        /// <param name="newPath">新文件夹路径</param>
        void RenameFolder(string oldPath, string newPath);
        
        /// <summary>
        /// 获取指定文件夹的子文件夹列表
        /// </summary>
        /// <param name="folderPath">父文件夹路径</param>
        /// <returns>子文件夹路径列表</returns>
        IEnumerable<string> GetSubFolders(string folderPath);
        
        /// <summary>
        /// 创建新文件夹
        /// </summary>
        /// <param name="folderPath">要创建的文件夹路径</param>
        void CreateFolder(string folderPath);
        
        /// <summary>
        /// 删除文件夹
        /// </summary>
        /// <param name="folderPath">要删除的文件夹路径</param>
        /// <param name="recursive">是否递归删除子文件夹和文件</param>
        void DeleteFolder(string folderPath, bool recursive);
        
        /// <summary>
        /// 检查文件夹是否存在
        /// </summary>
        /// <param name="folderPath">文件夹路径</param>
        /// <returns>是否存在</returns>
        bool FolderExists(string folderPath);
        
        /// <summary>
        /// 获取唯一的文件夹名称
        /// </summary>
        /// <param name="parentPath">父文件夹路径</param>
        /// <param name="baseName">基础名称</param>
        /// <returns>唯一的文件夹名称</returns>
        string GetUniqueFolderName(string parentPath, string baseName);
    }
}