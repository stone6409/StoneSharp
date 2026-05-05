using StoneSharp.Core.Tools.Permissions;

namespace StoneSharp.Core.Skills
{
    /// <summary>
    /// 技能元数据
    /// </summary>
    public class Skill
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Version { get; set; } = "1.0.0";
        public string Author { get; set; } = string.Empty;
        public string Created { get; set; } = string.Empty;
        public string Updated { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new List<string>();
        public string Category { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
        public string Directory { get; set; } = string.Empty;
        public bool IsDisabled { get; set; } = false;
        public List<string> AllowedTools { get; set; } = new List<string>();

        public FileSystemPermissionManager FilePermissions { get; set; } = new FileSystemPermissionManager();
        public ShellPermission ShellPermission { get; set; } = new ShellPermission();
        public WebPermission WebPermission { get; set; } = new WebPermission();

        public override string ToString() => $"{Name} v{Version} - {Description}";
    }
}
