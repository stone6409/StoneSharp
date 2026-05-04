using System;

namespace StoneSharp.Core.Tools.BuiltIn
{
    /// <summary>
    /// 文件大小格式化工具类
    /// </summary>
    public static class FileSizeFormatter
    {
        /// <summary>
        /// 格式化文件大小
        /// </summary>
        /// <param name="bytes">文件大小（字节）</param>
        /// <returns>格式化后的文件大小字符串</returns>
        public static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        /// <summary>
        /// 格式化可空的文件大小
        /// </summary>
        /// <param name="bytes">文件大小（字节），可为空</param>
        /// <returns>格式化后的文件大小字符串，如果为空则返回"未知"</returns>
        public static string FormatFileSize(long? bytes)
        {
            if (bytes.HasValue)
            {
                return FormatFileSize(bytes.Value);
            }
            return "未知";
        }
    }
}