using System.Collections.Generic;
using System.Threading.Tasks;

namespace StoneSharp.Core.Skills
{
    /// <summary>
    /// Skills管理器接口
    /// </summary>
    public interface ISkillsService
    {
        /// <summary>
        /// 发现项目中的技能
        /// </summary>
        Task<List<Skill>> DiscoverSkillsAsync(string projectRoot);

        /// <summary>
        /// 获取所有技能（包括已禁用的）
        /// </summary>
        Task<List<Skill>> GetAllSkillsAsync();

        /// <summary>
        /// 获取所有未禁用的技能
        /// </summary>
        Task<List<Skill>> GetEnabledSkillsAsync();

        /// <summary>
        /// 获取技能
        /// </summary>
        Task<Skill> GetSkillAsync(string skillName);

        /// <summary>
        /// 检查用户输入是否为技能指令，并返回对应的技能（异步版本）
        /// </summary>
        /// <param name="userInput">用户输入</param>
        /// <returns>如果是指令则返回对应的技能，否则返回null</returns>
        Task<Skill> GetSkillFromCommandAsync(string userInput);

        /// <summary>
        /// 检查用户输入是否为技能指令，并返回对应的技能（同步版本）
        /// </summary>
        /// <param name="userInput">用户输入</param>
        /// <returns>如果是指令则返回对应的技能，否则返回null</returns>
        Skill GetSkillFromCommand(string userInput);
    }
}