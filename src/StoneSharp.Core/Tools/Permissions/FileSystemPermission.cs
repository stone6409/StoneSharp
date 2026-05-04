using System;
using System.Collections.Generic;

namespace StoneSharp.Core.Tools.Permissions
{
    /// <summary>
    /// 文件系统权限规则
    /// </summary>
    public class FileSystemPermission
    {
        /// <summary>
        /// 权限类型
        /// </summary>
        public FileSystemPermissionType Type { get; set; }

        /// <summary>
        /// 路径
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// 是否递归应用于子目录
        /// </summary>
        public bool Recursive { get; set; } = false;

        /// <summary>
        /// 最大文件大小限制（字节）
        /// </summary>
        public long MaxFileSize { get; set; } = 64 * 1024; // 默认64K

        /// <summary>
        /// 允许的文件扩展名（为空表示全部允许）
        /// </summary>
        public List<string> AllowedExtensions { get; set; } = new List<string>();

        /// <summary>
        /// 禁止的文件扩展名（优先级高于允许的扩展名）
        /// </summary>
        public List<string> DeniedExtensions { get; set; } = new List<string>();

        /// <summary>
        /// 检查是否允许读取指定路径
        /// </summary>
        public bool CanRead(string path)
        {
            return (Type & FileSystemPermissionType.Read) == FileSystemPermissionType.Read
                && PathMatchesPattern(path);
        }

        /// <summary>
        /// 检查是否允许写入指定路径
        /// </summary>
        public bool CanWrite(string path)
        {
            return (Type & FileSystemPermissionType.Write) == FileSystemPermissionType.Write
                && PathMatchesPattern(path);
        }

        /// <summary>
        /// 检查文件扩展名是否允许
        /// </summary>
        public bool IsExtensionAllowed(string extension)
        {
            if (string.IsNullOrEmpty(extension))
                return true;

            // 检查是否在禁止列表中
            if (DeniedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                return false;

            // 如果允许列表为空，表示全部允许
            if (AllowedExtensions.Count == 0)
                return true;

            // 检查是否在允许列表中
            return AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 检查文件大小是否允许
        /// </summary>
        public bool IsSizeAllowed(long fileSize)
        {
            return fileSize <= MaxFileSize;
        }

        /// <summary>
        /// 检查路径是否匹配模式
        /// </summary>
        private bool PathMatchesPattern(string path)
        {
            if (string.IsNullOrEmpty(Path) || string.IsNullOrEmpty(path))
                return false;

            // 规范化路径
            string pattern = NormalizePath(Path);
            string normalizedPath = NormalizePath(path);

            // 检查是否递归匹配
            if (Recursive)
            {
                // 递归匹配：D:\src\ 匹配 D:\src\ 及其所有子目录
                return normalizedPath.StartsWith(pattern);
            }
            else
            {
                // 非递归匹配：只匹配当前目录
                if (normalizedPath.StartsWith(pattern))
                {
                    // 检查是否在当前目录下（没有更深层级的目录）
                    string relativePath = normalizedPath.Substring(pattern.Length);
                    return !relativePath.Contains('\\');
                }
                return false;
            }
        }

        /// <summary>
        /// 规范化路径
        /// </summary>
        private string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            // 确保路径以反斜杠结尾
            string normalized = path.Replace('/', '\\').TrimEnd('\\');
            if (!normalized.EndsWith("\\"))
            {
                normalized += "\\";
            }
            return normalized;
        }

        public override string ToString()
        {
            return $"{Type}: {Path} (Recursive: {Recursive}, MaxSize: {MaxFileSize} bytes)";
        }
    }
}