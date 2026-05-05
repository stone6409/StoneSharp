using StoneSharp.Core.Tools.Permissions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace StoneSharp.Core.Skills
{
    /// <summary>
    /// 技能解析器 - 静态类，用于解析技能元数据
    /// </summary>
    public static class SkillParser
    {
        /// <summary>
        /// 解析技能元数据
        /// </summary>
        public static Skill ParseSkill(string content, string skillPath, string skillName)
        {
            try
            {
                var skill = new Skill
                {
                    Name = skillName,
                    Directory = skillPath
                };

                // 解析YAML front matter
                var lines = content.Split('\n');
                bool inFrontMatter = false;
                var frontMatterLines = new List<string>();
                var promptLines = new List<string>();

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];

                    if (i == 0 && line.Trim() == "---")
                    {
                        inFrontMatter = true;
                        continue;
                    }

                    if (inFrontMatter)
                    {
                        if (line.Trim() == "---")
                        {
                            inFrontMatter = false;
                            continue;
                        }
                        frontMatterLines.Add(line);
                    }
                    else
                    {
                        promptLines.Add(line);
                    }
                }

                // 解析YAML front matter
                if (frontMatterLines.Count > 0)
                {
                    var frontMatter = string.Join("\n", frontMatterLines);
                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();

                    try
                    {
                        var metadata = deserializer.Deserialize<Dictionary<string, object>>(frontMatter);
                        ParseMetadata(skill, metadata);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"解析技能 {skillName} 的YAML front matter时发生错误: {ex.Message}");
                    }
                }

                // 提取提示语
                skill.Prompt = ExtractPrompt(promptLines);

                return skill;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解析技能元数据时发生错误: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 解析元数据
        /// </summary>
        private static void ParseMetadata(Skill skill, Dictionary<string, object> metadata)
        {
            if (metadata == null) return;

            if (metadata.TryGetValue("name", out var name))
                skill.Name = name.ToString();
            if (metadata.TryGetValue("description", out var description))
                skill.Description = description.ToString();
            if (metadata.TryGetValue("version", out var version))
                skill.Version = version.ToString();
            if (metadata.TryGetValue("author", out var author))
                skill.Author = author.ToString();
            if (metadata.TryGetValue("created", out var created))
                skill.Created = created.ToString();
            if (metadata.TryGetValue("updated", out var updated))
                skill.Updated = updated.ToString();
            if (metadata.TryGetValue("tags", out var tags))
            {
                skill.Tags = ParseTags(tags);
            }
            if (metadata.TryGetValue("category", out var category))
                skill.Category = category.ToString();
            if (metadata.TryGetValue("isDisabled", out var isDisabledByDefault))
            {
                if (bool.TryParse(isDisabledByDefault.ToString(), out var isDisabled))
                    skill.IsDisabled = isDisabled;
            }
            if (metadata.TryGetValue("allowedTools", out var allowedTools))
            {
                skill.AllowedTools = ParseAllowedTools(allowedTools);
            }

            ParseFilePermissions(skill, metadata);
            ParseShellPermissions(skill, metadata);
            ParseWebPermissions(skill, metadata);
        }

        /// <summary>
        /// 解析标签列表
        /// </summary>
        private static List<string> ParseTags(object tags)
        {
            if (tags is List<object> tagList)
                return tagList.Select(t => t.ToString()).ToList();
            else if (tags is string tagString)
                return tagString.Split(',', ';')
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToList();
            else if (tags is IEnumerable<object> enumerable)
                return enumerable.Select(t => t.ToString()).ToList();

            return new List<string>();
        }

        /// <summary>
        /// 解析允许的工具列表
        /// </summary>
        private static List<string> ParseAllowedTools(object allowedTools)
        {
            if (allowedTools is List<object> toolList)
                return toolList.Select(t => t.ToString()).ToList();
            else if (allowedTools is string toolString)
                return toolString.Split(',', ';')
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToList();
            else if (allowedTools is IEnumerable<object> enumerable)
                return enumerable.Select(t => t.ToString()).ToList();

            return new List<string>();
        }

        /// <summary>
        /// 提取提示语
        /// </summary>
        private static string ExtractPrompt(List<string> promptLines)
        {
            if (promptLines == null || promptLines.Count == 0)
                return string.Empty;

            var promptContent = string.Join("\n", promptLines);
            var promptStart = promptContent.IndexOf("# 系统指令");
            if (promptStart >= 0)
            {
                return promptContent.Substring(promptStart).Trim();
            }
            else
            {
                return promptContent.Trim();
            }
        }

        /// <summary>
        /// 从文件路径解析技能
        /// </summary>
        public static async Task<Skill> ParseSkillFromFileAsync(string filePath, string skillName = null)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return null;

            try
            {
                var content = await File.ReadAllTextAsync(filePath);
                var skillPath = Path.GetDirectoryName(filePath);

                if (string.IsNullOrWhiteSpace(skillName))
                {
                    skillName = Path.GetFileName(Path.GetDirectoryName(filePath));
                }

                return ParseSkill(content, skillPath, skillName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"从文件解析技能时发生错误: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 从技能目录解析技能
        /// </summary>
        public static async Task<Skill> ParseSkillFromDirectoryAsync(string skillDirectory, string skillName = null)
        {
            if (string.IsNullOrWhiteSpace(skillDirectory) || !Directory.Exists(skillDirectory))
                return null;

            var skillMdPath = Path.Combine(skillDirectory, "skill.md");
            if (!File.Exists(skillMdPath))
                return null;

            if (string.IsNullOrWhiteSpace(skillName))
            {
                skillName = Path.GetFileName(skillDirectory);
            }

            return await ParseSkillFromFileAsync(skillMdPath, skillName);
        }

        /// <summary>
        /// 验证技能元数据
        /// </summary>
        public static bool ValidateSkillMetadata(Skill skill, out List<string> errors)
        {
            errors = new List<string>();

            if (string.IsNullOrWhiteSpace(skill.Name))
                errors.Add("技能名称不能为空");

            if (string.IsNullOrWhiteSpace(skill.Description))
                errors.Add("技能描述不能为空");

            if (string.IsNullOrWhiteSpace(skill.Prompt))
                errors.Add("技能提示语不能为空");

            return errors.Count == 0;
        }

        /// <summary>
        /// 解析文件系统权限
        /// </summary>
        private static void ParseFilePermissions(Skill skill, Dictionary<string, object> metadata)
        {
            if (metadata == null) return;

            // 清空现有权限
            skill.FilePermissions.Clear();

            // 解析文件权限配置
            if (metadata.TryGetValue("filePermissions", out var filePermissions))
            {
                if (filePermissions is List<object> permissionList)
                {
                    foreach (var permissionObj in permissionList)
                    {
                        if (permissionObj is Dictionary<object, object> permissionDict)
                        {
                            var permission = ParseSingleFilePermission(permissionDict);
                            if (permission != null)
                            {
                                skill.FilePermissions.AddPermission(permission);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 解析单个权限规则
        /// </summary>
        private static FileSystemPermission ParseSingleFilePermission(Dictionary<object, object> permissionDict)
        {
            try
            {
                var permission = new FileSystemPermission();

                if (permissionDict.TryGetValue("type", out var typeValue))
                {
                    string typeStr = typeValue.ToString().ToLower();
                    switch (typeStr)
                    {
                        case "read":
                            permission.Type = FileSystemPermissionType.Read;
                            break;
                        case "write":
                            permission.Type = FileSystemPermissionType.Write;
                            break;
                        case "readwrite":
                            permission.Type = FileSystemPermissionType.ReadWrite;
                            break;
                        default:
                            permission.Type = FileSystemPermissionType.Read;
                            break;
                    }
                }

                if (permissionDict.TryGetValue("path", out var pathValue))
                {
                    permission.Path = pathValue.ToString();
                }

                if (permissionDict.TryGetValue("recursive", out var recursiveValue))
                {
                    if (bool.TryParse(recursiveValue.ToString(), out var recursive))
                        permission.Recursive = recursive;
                }

                if (permissionDict.TryGetValue("maxSize", out var maxSizeValue))
                {
                    if (long.TryParse(maxSizeValue.ToString(), out var maxSize))
                        permission.MaxFileSize = maxSize;
                }

                if (permissionDict.TryGetValue("allowedExtensions", out var allowedExtensions))
                {
                    permission.AllowedExtensions = ParseStringList(allowedExtensions);
                }

                if (permissionDict.TryGetValue("deniedExtensions", out var deniedExtensions))
                {
                    permission.DeniedExtensions = ParseStringList(deniedExtensions);
                }

                return permission;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解析文件权限时发生错误: {ex.Message}");
                return null;
            }
        }

        // ShellTool权限解析方法
        private static void ParseShellPermissions(Skill skill, Dictionary<string, object> metadata)
        {
            if (metadata == null) return;

            // 解析ShellTool权限配置
            if (metadata.TryGetValue("shellPermission", out var shellToolPermissionObj))
            {
                if (shellToolPermissionObj is Dictionary<object, object> permissionDict)
                {
                    var permission = ParseSingleShellPermission(permissionDict);
                    if (permission != null)
                    {
                        skill.ShellPermission = permission;
                    }
                }
            }
        }

        // 解析单个ShellTool权限规则
        private static ShellPermission ParseSingleShellPermission(Dictionary<object, object> permissionDict)
        {
            try
            {
                var permission = new ShellPermission();

                if (permissionDict.TryGetValue("allowedCommands", out var allowedCommands))
                {
                    permission.AllowedCommands = ParseStringList(allowedCommands);
                }

                if (permissionDict.TryGetValue("deniedCommands", out var deniedCommands))
                {
                    permission.DeniedCommands = ParseStringList(deniedCommands);
                }

                if (permissionDict.TryGetValue("maxExecutionTime", out var maxExecutionTime))
                {
                    if (int.TryParse(maxExecutionTime.ToString(), out var time))
                        permission.MaxExecutionTime = time;
                }

                if (permissionDict.TryGetValue("maxOutputLength", out var maxOutputLength))
                {
                    if (int.TryParse(maxOutputLength.ToString(), out var length))
                        permission.MaxOutputLength = length;
                }

                if (permissionDict.TryGetValue("allowAdminExecution", out var allowAdminExecution))
                {
                    if (bool.TryParse(allowAdminExecution.ToString(), out var allowAdmin))
                        permission.AllowAdminExecution = allowAdmin;
                }

                return permission;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解析ShellTool权限时发生错误: {ex.Message}");
                return null;
            }
        }

        // WebFetch权限解析方法
        private static void ParseWebPermissions(Skill skill, Dictionary<string, object> metadata)
        {
            if (metadata == null) return;

            // 解析WebFetch权限配置
            if (metadata.TryGetValue("webPermission", out var webPermissionObj))
            {
                if (webPermissionObj is Dictionary<object, object> permissionDict)
                {
                    var permission = ParseSingleWebPermission(permissionDict);
                    if (permission != null)
                    {
                        skill.WebPermission = permission;
                    }
                }
            }
        }

        // 解析单个WebFetch权限规则
        private static WebPermission ParseSingleWebPermission(Dictionary<object, object> permissionDict)
        {
            try
            {
                var permission = new WebPermission();

                if (permissionDict.TryGetValue("allowedHosts", out var allowedHosts))
                {
                    permission.AllowedHosts = ParseStringList(allowedHosts);
                }

                if (permissionDict.TryGetValue("deniedHosts", out var deniedHosts))
                {
                    permission.DeniedHosts = ParseStringList(deniedHosts);
                }

                if (permissionDict.TryGetValue("maxContentLength", out var maxContentLength))
                {
                    if (long.TryParse(maxContentLength.ToString(), out var length))
                        permission.MaxContentLength = length;
                }

                return permission;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解析WebFetch权限时发生错误: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 解析字符串列表
        /// </summary>
        private static List<string> ParseStringList(object value)
        {
            if (value is List<object> list)
                return list.Select(t => t.ToString()).ToList();
            else if (value is string str)
                return str.Split(',', ';')
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToList();
            else if (value is IEnumerable<object> enumerable)
                return enumerable.Select(t => t.ToString()).ToList();

            return new List<string>();
        }
    }
}