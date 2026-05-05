using StoneSharp.Core.Tools.Permissions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace StoneSharp.Core.Subjects
{
    /// <summary>
    /// AGENT.md文件解析器
    /// </summary>
    public static class SubjectParser
    {
        /// <summary>
        /// 解析AGENT.md文件内容
        /// </summary>
        public static Subject ParseChatFile(string content)
        {
            try
            {
                var metadata = new Subject
                {
                    RawContent = content
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

                // 解析YAML前端元数据
                if (frontMatterLines.Count > 0)
                {
                    metadata.HasYamlMetadata = true;
                    var frontMatter = string.Join("\n", frontMatterLines);
                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();

                    try
                    {
                        var yamlMetadata = deserializer.Deserialize<Dictionary<string, object>>(frontMatter);
                        ParseFilePermissions(metadata, yamlMetadata);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"解析AGENT.md的YAML前端元数据时发生错误: {ex.Message}");
                    }
                }

                // 提取提示语
                metadata.Prompt = ExtractPrompt(promptLines);

                return metadata;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解析AGENT.md文件时发生错误: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 解析文件系统权限
        /// </summary>
        private static void ParseFilePermissions(Subject metadata, Dictionary<string, object> yamlMetadata)
        {
            if (yamlMetadata == null) return;

            // 清空现有权限
            metadata.FilePermissions.Clear();

            // 解析文件权限配置
            if (yamlMetadata.TryGetValue("filePermissions", out var filePermissions))
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
                                metadata.FilePermissions.AddPermission(permission);
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

        /// <summary>
        /// 解析字符串列表
        /// </summary>
        private static List<string> ParseStringList(object value)
        {
            var list = new List<string>();

            if (value is List<object> objectList)
            {
                foreach (var item in objectList)
                {
                    list.Add(item.ToString());
                }
            }
            else if (value is string str)
            {
                // 尝试解析逗号分隔的字符串
                var items = str.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in items)
                {
                    list.Add(item.Trim());
                }
            }

            return list;
        }

        /// <summary>
        /// 提取提示语
        /// </summary>
        private static string ExtractPrompt(List<string> promptLines)
        {
            // 移除开头的空行
            while (promptLines.Count > 0 && string.IsNullOrWhiteSpace(promptLines[0]))
            {
                promptLines.RemoveAt(0);
            }

            // 移除结尾的空行
            while (promptLines.Count > 0 && string.IsNullOrWhiteSpace(promptLines[promptLines.Count - 1]))
            {
                promptLines.RemoveAt(promptLines.Count - 1);
            }

            return string.Join("\n", promptLines);
        }
    }
}