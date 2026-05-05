using StoneSharp.Core.Models.ContextItems;
using StoneSharp.Core.RAG;
using System.Text;

namespace StoneSharp.Core.Prompts.User
{
    /// <summary>
    /// 提示语构建器服务实现
    /// </summary>
    public class UserPromptService : IUserPromptService
    {
        private readonly IRagServiceClient _ragServiceClient;

        /// <summary>
        /// 构造函数
        /// </summary>
        public UserPromptService()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public UserPromptService(IRagServiceClient ragServiceClient)
        {
            _ragServiceClient = ragServiceClient;
        }

        /// <summary>
        /// 从上下文项构建组合提示语
        /// </summary>
        public async Task<string> BuildCombinedPromptFromContextItemsAsync(string input, IEnumerable<ContextItem> contextItems, bool includeFileContent = true)
        {
            var relevantKnowledgeBases = GetRelevantKnowledgeBasesFromContextItems(contextItems);
            var attachedFiles = GetAttachedFilesFromContextItems(contextItems);
            var attachedFolders = GetAttachedFoldersFromContextItems(contextItems);
            var appliedRuleFiles = GetAppliedRuleFilesFromContextItems(contextItems);

            return await BuildCombinedPromptAsync(input, relevantKnowledgeBases, attachedFiles, attachedFolders, appliedRuleFiles, null, includeFileContent);
        }

        /// <summary>
        /// 构建组合提示语
        /// </summary>
        public async Task<string> BuildCombinedPromptAsync(string input,
            IEnumerable<RelevantKnowledgeBase> knowledgeBases,
            IEnumerable<AttachedFile> attachedFiles,
            IEnumerable<AttachedFolder> attachedFolders,
            IEnumerable<AppliedRuleFile> appliedRuleFiles,
            string baseFolder = null,
            bool includeFileContent = true)
        {
            var sb = new StringBuilder();

            sb.AppendLine("# 用户指令：");

            // 添加用户输入
            sb.AppendLine(input);
            sb.AppendLine();

            if (_ragServiceClient != null && knowledgeBases.Any())
            {
                foreach (var knowledgeBase in knowledgeBases)
                {
                    try
                    {
                        var response = await _ragServiceClient.SearchAsync(input, knowledgeBase.Name).ConfigureAwait(false);
                        if (response.Length > 0)
                        {
                            sb.AppendLine(response);
                            sb.AppendLine();
                        }
                    }
                    catch (Exception ex)
                    {
                        // 处理异常，例如记录日志或显示错误信息
                        Console.WriteLine($"Failed to load knowledge bases: {ex.Message}");
                    }
                }
            }

            // 如果有文件列表，添加当前文件部分
            if (attachedFiles.Any())
            {
                sb.AppendLine("# 相关的文件：");
                sb.AppendLine();

                foreach (var attachedFile in attachedFiles)
                {
                    string filePath = attachedFile.FilePath;
                    if (!string.IsNullOrEmpty(baseFolder))
                    {
                        filePath = Path.GetRelativePath(baseFolder, filePath);
                    }

                    sb.AppendLine($"文件路径：{filePath}");
                    if (attachedFile.EndLine > attachedFile.StartLine)
                    {
                        sb.AppendLine($"行号范围: 第{attachedFile.StartLine}行-第{attachedFile.EndLine}行");
                    }

                    if (includeFileContent || attachedFile.EndLine > attachedFile.StartLine)
                    {
                        sb.AppendLine($"内容:");
                        sb.AppendLine($"```{attachedFile.CodeLanguage}");
                        sb.AppendLine(attachedFile.FileContent);
                        sb.AppendLine("```");
                    }

                    sb.AppendLine();
                }
            }

            // 添加文件夹信息
            if (attachedFolders.Any())
            {
                sb.AppendLine("# 相关的文件夹：");
                sb.AppendLine();

                foreach (var folder in attachedFolders)
                {
                    string folderPath = folder.FolderPath;
                    if (!string.IsNullOrEmpty(baseFolder))
                    {
                        folderPath = Path.GetRelativePath(baseFolder, folderPath);
                    }

                    sb.AppendLine($"文件夹路径: {folderPath}");
                    if (includeFileContent && !string.IsNullOrEmpty(folder.FolderSummary))
                    {
                        sb.AppendLine($"摘要:");
                        sb.AppendLine($"```");
                        sb.AppendLine(folder.FolderSummary);
                        sb.AppendLine("```");
                    }

                    sb.AppendLine();
                }
            }

            // 如果有规则文件列表
            if (appliedRuleFiles.Any())
            {
                sb.AppendLine("# 回复内容遵循下面文件中的规则：");
                sb.AppendLine();

                foreach (var appliedRuleFile in appliedRuleFiles)
                {
                    sb.AppendLine();

                    string filePath = appliedRuleFile.FilePath;
                    if (!string.IsNullOrEmpty(baseFolder))
                    {
                        filePath = Path.GetRelativePath(baseFolder, filePath);
                    }

                    sb.AppendLine($"{filePath}:");
                    sb.AppendLine($"内容:");
                    sb.AppendLine(appliedRuleFile.FileContent);

                    sb.AppendLine();
                }
            }

            string result = sb.ToString();
            return result;
        }

        // 同步方法（向后兼容）
        public string BuildCombinedPromptFromContextItems(string input, IEnumerable<ContextItem> contextItems, bool includeFileContent = true)
        {
            return BuildCombinedPromptFromContextItemsAsync(input, contextItems, includeFileContent).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 构建独立的上下文项描述
        /// </summary>
        public async Task<IEnumerable<string>> BuildIndividualContextItemsAsync(string input, IEnumerable<ContextItem> contextItems, bool includeFileContent = true)
        {
            var result = new List<string>();

            foreach (var item in contextItems)
            {
                var sb = new StringBuilder();

                switch (item)
                {
                    case ContextKnowledgeBase knowledgeBase:
                        sb.AppendLine($"类型: 知识库");
                        sb.AppendLine($"名称: {knowledgeBase.Name}");

                        // 添加 RAG 搜索内容
                        if (_ragServiceClient != null)
                        {
                            try
                            {
                                var ragResponse = await _ragServiceClient.SearchAsync(input, knowledgeBase.Name).ConfigureAwait(false);
                                if (!string.IsNullOrWhiteSpace(ragResponse))
                                {
                                    sb.AppendLine($"RAG 搜索结果:");
                                    sb.AppendLine($"```");
                                    sb.AppendLine(ragResponse);
                                    sb.AppendLine($"```");
                                }
                            }
                            catch (Exception ex)
                            {
                                sb.AppendLine($"RAG 搜索异常: {ex.Message}");
                            }
                        }
                        break;

                    case ContextFile contextFile:
                        sb.AppendLine($"类型: 文件");
                        sb.AppendLine($"路径: {contextFile.FilePath}");
                        if (contextFile.StartLine > 0 && contextFile.EndLine > contextFile.StartLine)
                        {
                            sb.AppendLine($"行号范围: 第{contextFile.StartLine}行-第{contextFile.EndLine}行");
                        }

                        if (includeFileContent)
                        {
                            sb.AppendLine($"内容:");
                            sb.AppendLine($"```{contextFile.CodeLanguage}");
                            sb.AppendLine(contextFile.FileContent);
                            sb.AppendLine("```");
                        }

                        break;

                    case ContextFileSnippet contextFileSnippet:
                        sb.AppendLine($"类型: 文件片段");
                        sb.AppendLine($"路径: {contextFileSnippet.FilePath}");
                        if (contextFileSnippet.StartLine > 0 && contextFileSnippet.EndLine > contextFileSnippet.StartLine)
                        {
                            sb.AppendLine($"行号范围: 第{contextFileSnippet.StartLine}行-第{contextFileSnippet.EndLine}行");
                        }

                        if (true/*includeFileContent*/)
                        {
                            sb.AppendLine($"内容:");
                            sb.AppendLine($"```{contextFileSnippet.CodeLanguage}");
                            sb.AppendLine(contextFileSnippet.SnippetContent);
                            sb.AppendLine("```");
                        }

                        break;

                    case ContextFolder contextFolder:
                        sb.AppendLine($"类型: 文件夹");
                        sb.AppendLine($"路径: {contextFolder.FolderPath}");
                        if (includeFileContent && !string.IsNullOrEmpty(contextFolder.FolderSummary))
                        {
                            sb.AppendLine($"摘要:");
                            sb.AppendLine(contextFolder.FolderSummary);
                        }
                        break;

                    case ContextRuleFile contextRuleFile:
                        sb.AppendLine($"类型: 规则文件");
                        sb.AppendLine($"路径: {contextRuleFile.FilePath}");
                        sb.AppendLine($"内容:");
                        sb.AppendLine(contextRuleFile.FileContent);
                        break;

                    default:
                        sb.AppendLine($"类型: 未知");
                        sb.AppendLine($"原始类型: {item.GetType().Name}");
                        break;
                }

                result.Add(sb.ToString());
            }

            return result;
        }

        // 同步版本（向后兼容）
        public IEnumerable<string> BuildIndividualContextItems(string input, IEnumerable<ContextItem> contextItems, bool includeFileContent = true)
        {
            return BuildIndividualContextItemsAsync(input, contextItems, includeFileContent).GetAwaiter().GetResult();
        }

        private IEnumerable<RelevantKnowledgeBase> GetRelevantKnowledgeBasesFromContextItems(IEnumerable<ContextItem> contextItems)
        {
            return contextItems
                .OfType<ContextKnowledgeBase>()
                .Select(ckb => new RelevantKnowledgeBase
                {
                    Name = ckb.Name,
                });
        }

        private IEnumerable<AttachedFile> GetAttachedFilesFromContextItems(IEnumerable<ContextItem> contextItems)
        {
            // 保持原始顺序，按顺序处理每个ContextItem
            return contextItems
                .Where(item => item is ContextFile || item is ContextFileSnippet)
                .Select(item =>
                {
                    switch (item)
                    {
                        case ContextFile contextFile:
                            return new AttachedFile
                            {
                                FilePath = contextFile.FilePath,
                                StartLine = contextFile.StartLine,
                                EndLine = contextFile.EndLine,
                                FileContent = contextFile.FileContent,
                                CodeLanguage = contextFile.CodeLanguage
                            };
                        case ContextFileSnippet contextFileSnippet:
                            return new AttachedFile
                            {
                                FilePath = contextFileSnippet.FilePath,
                                StartLine = contextFileSnippet.StartLine,
                                EndLine = contextFileSnippet.EndLine,
                                FileContent = contextFileSnippet.SnippetContent,
                                CodeLanguage = contextFileSnippet.CodeLanguage
                            };
                        default:
                            return null;
                    }
                })
                .Where(attachedFile => attachedFile != null); // 过滤掉null值
        }

        private IEnumerable<AttachedFolder> GetAttachedFoldersFromContextItems(IEnumerable<ContextItem> contextItems)
        {
            return contextItems
                .OfType<ContextFolder>()
                .Select(af => new AttachedFolder
                {
                    FolderPath = af.FolderPath,
                    FolderSummary = af.FolderSummary,
                });
        }

        private IEnumerable<AppliedRuleFile> GetAppliedRuleFilesFromContextItems(IEnumerable<ContextItem> contextItems)
        {
            return contextItems
                .OfType<ContextRuleFile>()
                .Select(cr => new AppliedRuleFile
                {
                    FilePath = cr.FilePath,
                    FileContent = cr.FileContent,
                });
        }
    }
}