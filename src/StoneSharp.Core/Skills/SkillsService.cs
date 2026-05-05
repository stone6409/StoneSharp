using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StoneSharp.Core.Skills
{
    /// <summary>
    /// Skills管理器实现
    /// </summary>
    public class SkillsService : ISkillsService
    {
        private readonly Dictionary<string, Skill> _loadedSkills = new Dictionary<string, Skill>();
        private readonly string _skillsDirectory;

        public SkillsService(string skillsDirectory)
        {
            _skillsDirectory = skillsDirectory ?? throw new ArgumentNullException(nameof(skillsDirectory));
        }

        /// <summary>
        /// 获取技能目录的完整路径
        /// </summary>
        private string GetSkillsPath(string projectRoot)
        {
            if (!Path.IsPathRooted(_skillsDirectory))
            {
                return Path.Combine(projectRoot, _skillsDirectory);
            }
            return _skillsDirectory;
        }

        /// <summary>
        /// 检查技能是否被禁用
        /// </summary>
        private bool IsSkillDisabled(Skill skill)
        {
            return skill != null && skill.IsDisabled;
        }

        /// <summary>
        /// 发现项目中的技能
        /// </summary>
        public async Task<List<Skill>> DiscoverSkillsAsync(string projectRoot)
        {
            var skills = new List<Skill>();
            var skillsPath = GetSkillsPath(projectRoot);

            if (!Directory.Exists(skillsPath))
            {
                return skills;
            }

            var skillDirectories = Directory.GetDirectories(skillsPath);

            foreach (var skillDir in skillDirectories)
            {
                var folderName = Path.GetFileName(skillDir);
                var skill = await LoadSkillFromDirectoryAsync(skillDir, folderName);
                if (skill != null)
                {
                    skills.Add(skill);
                }
            }

            return skills;
        }

        /// <summary>
        /// 从目录加载技能
        /// </summary>
        private async Task<Skill> LoadSkillFromDirectoryAsync(string skillDir, string folderName)
        {
            try
            {
                var skillMdPath = Path.Combine(skillDir, "skill.md");
                if (!File.Exists(skillMdPath))
                {
                    return null;
                }

                var skillContent = await File.ReadAllTextAsync(skillMdPath);
                var skill = SkillParser.ParseSkill(skillContent, skillDir, folderName);

                if (skill != null)
                {
                    // 使用技能的实际名称作为主键
                    _loadedSkills[skill.Name] = skill;
                }

                return skill;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"从目录加载技能时发生错误: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 尝试从缓存加载技能
        /// </summary>
        private Skill TryLoadFromCache(string skillName)
        {
            if (_loadedSkills.TryGetValue(skillName, out var cachedSkill) && !IsSkillDisabled(cachedSkill))
            {
                return cachedSkill;
            }
            return null;
        }

        /// <summary>
        /// 在指定目录中查找技能
        /// </summary>
        private async Task<Skill> FindSkillInDirectoryAsync(string skillDir, string skillName)
        {
            var folderName = Path.GetFileName(skillDir);
            var skill = await LoadSkillFromDirectoryAsync(skillDir, folderName);
            
            if (skill != null && (skill.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase) || 
                                  folderName.Equals(skillName, StringComparison.OrdinalIgnoreCase)) && 
                !IsSkillDisabled(skill))
            {
                return skill;
            }
            return null;
        }

        /// <summary>
        /// 加载技能
        /// </summary>
        public async Task<Skill> LoadSkillAsync(string skillName)
        {
            // 首先检查是否已加载该技能
            var cachedSkill = TryLoadFromCache(skillName);
            if (cachedSkill != null)
            {
                return cachedSkill;
            }

            try
            {
                // 获取项目根目录
                var projectRoot = Directory.GetCurrentDirectory();
                var skillsPath = GetSkillsPath(projectRoot);

                if (!Directory.Exists(skillsPath))
                {
                    return null;
                }

                // 在所有技能目录中查找
                var skillDirectories = Directory.GetDirectories(skillsPath);
                foreach (var skillDir in skillDirectories)
                {
                    var foundSkill = await FindSkillInDirectoryAsync(skillDir, skillName);
                    if (foundSkill != null)
                    {
                        return foundSkill;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载技能 {skillName} 时发生错误: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 获取所有技能（包括已禁用的）
        /// </summary>
        public async Task<List<Skill>> GetAllSkillsAsync()
        {
            var projectRoot = Directory.GetCurrentDirectory();
            var skills = await DiscoverSkillsAsync(projectRoot);
            return skills;
        }

        /// <summary>
        /// 获取所有未禁用的技能
        /// </summary>
        public async Task<List<Skill>> GetEnabledSkillsAsync()
        {
            var allSkills = await GetAllSkillsAsync();
            return allSkills.Where(s => !s.IsDisabled).ToList();
        }

        /// <summary>
        /// 获取技能提示语
        /// </summary>
        public async Task<Skill> GetSkillAsync(string skillName)
        {
            var skill = await LoadSkillAsync(skillName);
            return skill;
        }

        /// <summary>
        /// 从用户输入中提取技能名称
        /// </summary>
        private string ExtractSkillNameFromInput(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                return null;

            // 检查 /skill 指令
            if (userInput.StartsWith("/skill ", StringComparison.OrdinalIgnoreCase))
            {
                var parts = userInput.Substring(7).Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                {
                    return parts[0].Trim();
                }
            }
            return null;
        }

        /// <summary>
        /// 从用户输入中匹配技能名称
        /// </summary>
        private async Task<string> MatchSkillNameFromInputAsync(string userInput)
        {
            var enabledSkills = await GetEnabledSkillsAsync();
            var matchedSkills = SkillMatcher.GetMatchedSkills(userInput, enabledSkills);

            if (matchedSkills.Any())
            {
                return matchedSkills.First().Skill.Name;
            }
            return null;
        }

        /// <summary>
        /// 检查用户输入是否为技能指令，并返回对应的技能（异步版本）
        /// </summary>
        /// <param name="userInput">用户输入</param>
        /// <returns>如果是指令则返回对应的技能，否则返回null</returns>
        public async Task<Skill> GetSkillFromCommandAsync(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                return null;

            // 首先尝试从指令中提取技能名称
            var skillName = ExtractSkillNameFromInput(userInput);

            // 如果不是指令，尝试匹配技能
            if (string.IsNullOrEmpty(skillName))
            {
                skillName = await MatchSkillNameFromInputAsync(userInput);
            }

            // 如果找到技能名称，加载并返回技能
            if (!string.IsNullOrEmpty(skillName))
            {
                return await LoadSkillAsync(skillName);
            }

            return null;
        }

        /// <summary>
        /// 检查用户输入是否为技能指令，并返回对应的技能（同步版本）
        /// </summary>
        /// <param name="userInput">用户输入</param>
        /// <returns>如果是指令则返回对应的技能，否则返回null</returns>
        public Skill GetSkillFromCommand(string userInput)
        {
            return Task.Run(async () => await GetSkillFromCommandAsync(userInput)).Result;
        }
    }
}