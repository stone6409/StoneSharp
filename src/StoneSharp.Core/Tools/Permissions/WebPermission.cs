using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace StoneSharp.Core.Tools.Permissions
{
    /// <summary>
    /// WebFetch工具权限规则 - 定义允许/禁止访问的URL主机
    /// </summary>
    public class WebPermission
    {
        /// <summary>
        /// 允许访问的主机名列表（支持通配符 *.example.com）
        /// 空列表表示使用内置默认白名单（PreapprovedHosts）
        /// </summary>
        public List<string> AllowedHosts { get; set; } = new List<string>();

        /// <summary>
        /// 禁止访问的主机名列表（优先级高于允许列表）
        /// </summary>
        public List<string> DeniedHosts { get; set; } = new List<string>();

        /// <summary>
        /// 最大内容长度限制（字节，0表示不限制）
        /// </summary>
        public long MaxContentLength { get; set; } = 0;

        /// <summary>
        /// 检查是否允许访问指定URL
        /// </summary>
        /// <param name="url">要检查的完整URL</param>
        /// <returns>如果允许访问则返回true</returns>
        public bool CanAccessUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return false;

            var host = uri.Host;

            // 1. 优先检查黑名单
            foreach (var deniedPattern in DeniedHosts)
            {
                if (MatchesHost(host, deniedPattern))
                    return false;
            }

            // 2. 如果白名单为空，表示使用内置默认，默认允许（由调用方决定）
            if (AllowedHosts.Count == 0)
                return true;

            // 3. 检查白名单
            foreach (var allowedPattern in AllowedHosts)
            {
                if (MatchesHost(host, allowedPattern))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 检查主机名是否匹配模式（支持通配符）
        /// </summary>
        private static bool MatchesHost(string host, string pattern)
        {
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(pattern))
                return false;

            // 支持 *.example.com 通配符匹配
            if (pattern.StartsWith("*."))
            {
                var suffix = pattern.Substring(1); // 包含开头的点号
                return host.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(host, pattern.Substring(2), StringComparison.OrdinalIgnoreCase);
            }

            // 支持 * 和 ? 通配符的正则匹配
            if (pattern.Contains("*") || pattern.Contains("?"))
            {
                return Regex.IsMatch(
                    host,
                    "^" + Regex.Escape(pattern)
                        .Replace("\\*", ".*")
                        .Replace("\\?", ".") + "$",
                    RegexOptions.IgnoreCase);
            }

            // 精确匹配
            return string.Equals(host, pattern, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            return $"AllowedHosts={AllowedHosts.Count}, DeniedHosts={DeniedHosts.Count}";
        }
    }
}
