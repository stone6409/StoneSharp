using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StoneSharp.Core.Tools.Permissions
{
    /// <summary>
    /// Shell工具权限规则
    /// </summary>
    public class ShellPermission
    {
        /// <summary>
        /// 允许执行的命令模式（支持通配符）
        /// </summary>
        public List<string> AllowedCommands { get; set; } = new List<string>();

        /// <summary>
        /// 禁止执行的命令模式（优先级高于允许的命令）
        /// </summary>
        public List<string> DeniedCommands { get; set; } = new List<string>();

        /// <summary>
        /// 最大执行时间限制（秒）
        /// </summary>
        public int MaxExecutionTime { get; set; } = 30;

        /// <summary>
        /// 最大输出长度限制（字符）
        /// </summary>
        public int MaxOutputLength { get; set; } = 8192;

        /// <summary>
        /// 是否允许管理员权限执行
        /// </summary>
        public bool AllowAdminExecution { get; set; } = false;

        /// <summary>
        /// 检查是否允许执行指定命令
        /// </summary>
        public bool CanExecute(string command)
        {
            // 检查命令权限
            return IsCommandAllowed(command);
        }

        /// <summary>
        /// 检查命令是否允许执行
        /// </summary>
        private bool IsCommandAllowed(string command)
        {
            if (string.IsNullOrEmpty(command))
                return false;

            // 检查是否在禁止列表中
            foreach (var deniedPattern in DeniedCommands)
            {
                if (MatchesPattern(command, deniedPattern))
                    return false;
            }

            // 如果允许列表为空，表示全部允许
            if (AllowedCommands.Count == 0)
                return true;

            // 检查是否在允许列表中
            foreach (var allowedPattern in AllowedCommands)
            {
                if (MatchesPattern(command, allowedPattern))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 检查命令是否匹配模式
        /// </summary>
        private bool MatchesPattern(string command, string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                return false;

            // 支持简单的通配符匹配
            if (pattern.Contains("*") || pattern.Contains("?"))
            {
                return Regex.IsMatch(
                    command,
                    "^" + Regex.Escape(pattern)
                        .Replace("\\*", ".*")
                        .Replace("\\?", ".") + "$",
                    RegexOptions.IgnoreCase);
            }

            // 精确匹配
            return string.Equals(command, pattern, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            return $"AllowedCommands={AllowedCommands.Count}, MaxTime={MaxExecutionTime}s";
        }
    }
}
