using StoneSharp.Core.Tools.Permissions;

namespace StoneSharp.Core.Subjects
{
    /// <summary>
    /// AGENT.md文件的元数据
    /// </summary>
    public class Subject
    {
        /// <summary>
        /// 系统提示语内容（去除YAML前端元数据后的Markdown内容）
        /// </summary>
        public string Prompt { get; set; } = string.Empty;

        /// <summary>
        /// 文件系统权限管理器
        /// </summary>
        public FileSystemPermissionManager FilePermissions { get; set; } = new FileSystemPermissionManager();

        /// <summary>
        /// 是否成功解析了YAML前端元数据
        /// </summary>
        public bool HasYamlMetadata { get; set; }

        /// <summary>
        /// 原始文件内容（包含YAML前端元数据）
        /// </summary>
        public string RawContent { get; set; } = string.Empty;

        /// <summary>
        /// 创建包含默认提示语的ChatMetadata
        /// </summary>
        public static Subject CreateDefault(string defaultPrompt)
        {
            return new Subject
            {
                Prompt = defaultPrompt,
                HasYamlMetadata = false,
                RawContent = defaultPrompt
            };
        }
    }
}