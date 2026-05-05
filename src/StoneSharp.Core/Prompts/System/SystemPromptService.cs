using StoneSharp.Core.Helpers;
using System.Text;

namespace StoneSharp.Core.Prompts.System
{
    public class SystemPromptService : ISystemPromptService
    {
        private string _defaultSystemPrompt;

        public string DefaultSystemPrompt
        {
            get => _defaultSystemPrompt ?? GetDefaultSystemPrompt();
            set => _defaultSystemPrompt = value;
        }

        public SystemPromptService()
        {
        }

        /// <summary>
        /// 获取默认系统提示语
        /// </summary>
        private string GetDefaultSystemPrompt()
        {
            return $"""
                # 系统提示语
                
                你是一个AI助手。
                
                ## 基本规则
                1. 提供准确、有用的信息
                2. 保持专业和礼貌
                3. 如果不知道答案，请诚实说明
                4. 遵循用户的具体要求
                """;
        }

        /// <summary>
        /// 异步获取系统提示语
        /// </summary>
        public async Task<string> GetSystemPromptAsync()
        {
            try
            {
                // 使用AgentFileFinder查找AGENT.md文件
                string agentFilePath = AgentFileFinder.FindAgentFile();

                if (string.IsNullOrEmpty(agentFilePath))
                {
                    return DefaultSystemPrompt;
                }

                // 读取文件内容
                return await ReadAgentFileAsync(agentFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取系统提示语时发生错误: {ex.Message}");
                return DefaultSystemPrompt;
            }
        }

        /// <summary>
        /// 同步获取系统提示语
        /// </summary>
        public string GetSystemPrompt()
        {
            try
            {
                // 使用AgentFileFinder查找AGENT.md文件
                string agentFilePath = AgentFileFinder.FindAgentFile();

                if (string.IsNullOrEmpty(agentFilePath))
                {
                    return DefaultSystemPrompt;
                }

                // 读取文件内容
                return ReadAgentFile(agentFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取系统提示语时发生错误: {ex.Message}");
                return DefaultSystemPrompt;
            }
        }

        /// <summary>
        /// 异步获取增强的系统提示语（根据用户输入动态包含技能提示语）
        /// </summary>
        public async Task<string> GetEnhancedSystemPromptAsync(string subjectPrompt, string skillPrompt, bool isPlanMode = false)
        {
            try
            {
                // 获取基础系统提示语
                var basePrompt = await GetSystemPromptAsync();

                // 组合提示语
                return CombinePrompts(basePrompt, subjectPrompt, skillPrompt, isPlanMode);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取增强系统提示语时发生错误: {ex.Message}");
                return await GetFallbackPromptAsync();
            }
        }

        /// <summary>
        /// 同步获取增强的系统提示语（根据用户输入动态包含技能提示语）
        /// </summary>
        public string GetEnhancedSystemPrompt(string subjectPrompt, string skillPrompt, bool isPlanMode = false)
        {
            // 简洁方案：重用异步方法，使用 Task.Run 避免死锁
            return Task.Run(async () => await GetEnhancedSystemPromptAsync(subjectPrompt, skillPrompt, isPlanMode)).Result;
        }

        /// <summary>
        /// 组合基础提示语和技能提示语
        /// </summary>
        private string CombinePrompts(string basePrompt, string subjectPrompt, string skillPrompt, bool isPlanMode)
        {
            var combinedPrompt = new StringBuilder();

            // 添加基础提示语
            if (!string.IsNullOrWhiteSpace(basePrompt))
            {
                combinedPrompt.AppendLine(basePrompt);
            }

            combinedPrompt.AppendLine();
            combinedPrompt.AppendLine("# 用户指令");

            // 如果启用了规划模式，添加规划模式相关的指令
            if (isPlanMode)
            {
                combinedPrompt.AppendLine("请使用“先规划后编码”的方式，按照以下要求执行：");
                combinedPrompt.AppendLine("1. 先给用户一个详细的实现计划，根据需要可以包括算法思路、数据结构和关键步骤等");
                combinedPrompt.AppendLine("2. 等用户确认计划后，再编写实际代码");
                //combinedPrompt.AppendLine("4. 考虑可能的风险和应对措施");
                //combinedPrompt.AppendLine("5. 提供时间估算和资源需求");

            }
            else
            {
                combinedPrompt.AppendLine("请按照以下要求执行：");
                //combinedPrompt.AppendLine("1. 根据需要可以调用用户提供的工具，如查找文件，读取文件等");
                //combinedPrompt.AppendLine("1. 如果上一轮会话有制定计划，接收到**执行上述计划**指令，通常是要求**调用工具**进行真实的文件写入和编辑操作等");
                combinedPrompt.AppendLine("1. 当用户说'执行计划'或类似指令时，表示需要执行之前制定好的计划，通常是要求**调用工具**进行真实的文件写入和编辑等操作");
            }

            // 添加主题提示语（如果存在）
            if (!string.IsNullOrWhiteSpace(subjectPrompt))
            {
                combinedPrompt.AppendLine();
                combinedPrompt.AppendLine(subjectPrompt);
            }

            // 添加技能提示语（如果存在）
            if (!string.IsNullOrWhiteSpace(skillPrompt))
            {
                //combinedPrompt.AppendLine("## 你拥有以下已激活的技能");
                combinedPrompt.AppendLine();
                combinedPrompt.AppendLine(skillPrompt);
            }

            return combinedPrompt.ToString();
        }

        /// <summary>
        /// 获取备用提示语（异步）
        /// </summary>
        private async Task<string> GetFallbackPromptAsync()
        {
            try
            {
                var basePrompt = await GetSystemPromptAsync();
                return basePrompt;
            }
            catch
            {
                return DefaultSystemPrompt;
            }
        }

        /// <summary>
        /// 异步读取AGENT.md文件内容
        /// </summary>
        private async Task<string> ReadAgentFileAsync(string filePath)
        {
            try
            {
                using (var reader = new StreamReader(filePath, Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"读取AGENT.md文件时发生错误: {ex.Message}");
                return DefaultSystemPrompt;
            }
        }

        /// <summary>
        /// 同步读取AGENT.md文件内容
        /// </summary>
        private string ReadAgentFile(string filePath)
        {
            try
            {
                using (var reader = new StreamReader(filePath, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"读取AGENT.md文件时发生错误: {ex.Message}");
                return DefaultSystemPrompt;
            }
        }
    }
}