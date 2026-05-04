using StoneSharp.Core.Tools.BuiltIn;

namespace StoneSharp.Core.Tools
{
    /// <summary>
    /// 工具相关的辅助方法
    /// </summary>
    public static class ToolUtility
    {
        private const char ToolSeparator = ',';

        /// <summary>
        /// 所有可提供的工具列表
        /// </summary>
        private static readonly List<Tool> AvailableTools = new List<Tool>
        {
            new Tool("FileRead", "文件读取", typeof(FileReadTool), true) { Category = "文件操作" },
            new Tool("FileWrite", "文件写入", typeof(FileWriteTool), false) { Category = "文件操作" },
            new Tool("FileEdit", "文件编辑", typeof(FileEditTool), false) { Category = "文件操作" },
            new Tool("FindFiles", "查找文件", typeof(FindFilesTool), true) { Category = "文件操作" },
            new Tool("SearchContent", "搜索内容", typeof(SearchContentTool), true) { Category = "文件操作" },
            new Tool("Shell", "Shell", typeof(ShellTool), false) { Category = "系统工具" },
            new Tool("WebFetch", "网页获取", typeof(WebFetchTool), true) { Category = "网络服务" },
        };

        /// <summary>
        /// 将逗号分隔的工具字符串转换为工具ID列表
        /// </summary>
        /// <param name="toolsString">逗号分隔的工具字符串</param>
        /// <returns>工具ID列表</returns>
        public static List<string> ParseToolIds(string toolsString)
        {
            if (string.IsNullOrEmpty(toolsString))
                return new List<string>();

            return toolsString.Split(ToolSeparator)
                .Select(tool => tool.Trim())
                .Where(tool => !string.IsNullOrEmpty(tool))
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// 将工具ID列表转换为逗号分隔的字符串
        /// </summary>
        /// <param name="toolIds">工具ID列表</param>
        /// <returns>逗号分隔的工具字符串</returns>
        public static string FormatToolIds(List<string> toolIds)
        {
            if (toolIds == null || toolIds.Count == 0)
                return string.Empty;

            return string.Join(ToolSeparator.ToString(), toolIds);
        }

        /// <summary>
        /// 检查指定的工具ID是否在工具字符串中
        /// </summary>
        /// <param name="toolsString">逗号分隔的工具字符串</param>
        /// <param name="toolId">要检查的工具ID</param>
        /// <returns>如果工具ID存在则返回true，否则返回false</returns>
        public static bool IsToolSelected(string toolsString, string toolId)
        {
            if (string.IsNullOrEmpty(toolsString) || string.IsNullOrEmpty(toolId))
                return false;

            var toolIds = ParseToolIds(toolsString);
            return toolIds.Contains(toolId);
        }

        /// <summary>
        /// 根据工具ID获取工具
        /// </summary>
        /// <param name="toolId">工具ID</param>
        /// <returns>工具，如果未找到则返回null</returns>
        public static Tool GetTool(string toolId)
        {
            if (string.IsNullOrEmpty(toolId))
                return null;

            return AvailableTools.FirstOrDefault(t => t.Id == toolId);
        }

        /// <summary>
        /// 获取所有可用的工具列表（副本）
        /// </summary>
        /// <returns>工具列表副本</returns>
        public static List<Tool> GetAllAvailableTools()
        {
            return AvailableTools.Select(t => t.Clone()).ToList();
        }

        // 在 ToolUtility.cs 中添加以下方法

        /// <summary>
        /// 根据工具ID获取工具类型
        /// </summary>
        /// <param name="toolId">工具ID</param>
        /// <returns>工具类型，如果未找到则返回null</returns>
        public static Type GetToolType(string toolId)
        {
            if (string.IsNullOrEmpty(toolId))
                return null;

            var tool = GetTool(toolId);
            return tool?.Type;
        }

        /// <summary>
        /// 获取在规划模式下允许使用的工具ID列表
        /// </summary>
        /// <returns>在规划模式下允许使用的工具ID列表</returns>
        public static List<string> GetToolsAllowedInPlanMode()
        {
            return AvailableTools
                .Where(t => t.IsAllowedInPlanMode)
                .Select(t => t.Id)
                .ToList();
        }

        /// <summary>
        /// 检查指定工具是否在规划模式下允许使用
        /// </summary>
        /// <param name="toolId">工具ID</param>
        /// <returns>如果允许则返回true，否则返回false</returns>
        public static bool IsToolAllowedInPlanMode(string toolId)
        {
            if (string.IsNullOrEmpty(toolId))
                return false;

            var tool = GetTool(toolId);
            return tool?.IsAllowedInPlanMode ?? false;
        }

        /// <summary>
        /// 根据规划模式设置过滤工具ID列表
        /// </summary>
        /// <param name="toolIds">原始工具ID列表</param>
        /// <param name="isReadOnly">是否为只读</param>
        /// <returns>过滤后的工具ID列表</returns>
        public static List<string> FilterToolsByReadOnly(List<string> toolIds, bool isReadOnly)
        {
            if (toolIds == null || toolIds.Count == 0)
                return new List<string>();

            if (!isReadOnly)
                return toolIds;

            return toolIds
                .Where(toolId => IsToolAllowedInPlanMode(toolId))
                .ToList();
        }

        /// <summary>
        /// 根据规划模式设置过滤工具ID列表
        /// </summary>
        /// <param name="toolIds">原始工具ID列表</param>
        /// <param name="isPlanMode">是否为规划模式</param>
        /// <returns>过滤后的工具ID列表</returns>
        public static List<string> FilterToolsByPlanMode(List<string> toolIds, bool isPlanMode)
        {
            if (toolIds == null || toolIds.Count == 0)
                return new List<string>();

            if (!isPlanMode)
                return toolIds;

            return toolIds
                .Where(toolId => IsToolAllowedInPlanMode(toolId))
                .ToList();
        }
    }
}