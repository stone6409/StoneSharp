/// <summary>
/// 文件系统权限管理器
/// </summary>
namespace StoneSharp.Core.Tools.Permissions
{
    public class FileSystemPermissionManager
    {
        private readonly List<FileSystemPermission> _permissions = new List<FileSystemPermission>();

        /// <summary>
        /// 添加权限
        /// </summary>
        public void AddPermission(FileSystemPermission permission)
        {
            _permissions.Add(permission);
        }

        /// <summary>
        /// 检查是否允许读取
        /// </summary>
        public bool CanRead(string path, string extension = null, long fileSize = 0)
        {
            foreach (var permission in _permissions)
            {
                if (permission.CanRead(path))
                {
                    if (!string.IsNullOrEmpty(extension) && !permission.IsExtensionAllowed(extension))
                        continue;

                    if (fileSize > 0 && !permission.IsSizeAllowed(fileSize))
                        continue;

                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 检查是否允许写入
        /// </summary>
        public bool CanWrite(string path, string extension = null, long fileSize = 0)
        {
            foreach (var permission in _permissions)
            {
                if (permission.CanWrite(path))
                {
                    if (!string.IsNullOrEmpty(extension) && !permission.IsExtensionAllowed(extension))
                        continue;

                    if (fileSize > 0 && !permission.IsSizeAllowed(fileSize))
                        continue;

                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取所有权限
        /// </summary>
        public IReadOnlyList<FileSystemPermission> GetPermissions()
        {
            return _permissions.AsReadOnly();
        }

        /// <summary>
        /// 清除所有权限
        /// </summary>
        public void Clear()
        {
            _permissions.Clear();
        }
    }
}
