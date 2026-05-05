// ToolSettings.cs
using System;

namespace StoneSharp.Core.Settings
{
    /// <summary>
    /// 工具配置设置
    /// </summary>
    public class ToolSettings
    {
        /// <summary>
        /// 是否启用工具调用
        /// </summary>
        public bool IsToolCallingEnable { get; set; } = true;

        /// <summary>
        /// 允许使用的工具列表（逗号分隔）
        /// </summary>
        public string AllowedTools { get; set; } = string.Empty;

        /// <summary>
        /// 是否启用按需技能功能
        /// </summary>
        public bool IsSkillOnDemandEnabled { get; set; } = true;

        /// <summary>
        /// 验证配置是否有效
        /// </summary>
        public void Validate()
        {
            // 验证AllowedTools格式
            if (!string.IsNullOrEmpty(AllowedTools))
            {
                var tools = AllowedTools.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var tool in tools)
                {
                    if (string.IsNullOrWhiteSpace(tool))
                    {
                        throw new InvalidOperationException("AllowedTools包含空白的工具名称");
                    }
                }
            }
        }
    }
}