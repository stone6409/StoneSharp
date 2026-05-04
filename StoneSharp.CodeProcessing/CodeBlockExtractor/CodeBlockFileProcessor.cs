using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace StoneSharp.CodeProcessing.CodeBlockExtractor
{
    /// <summary>
    /// 增强的代码块文件处理器 - 支持根据目录结构保存文件
    /// </summary>
    public class CodeBlockFileProcessor
    {
        public CodeBlockFileProcessor()
        {
        }

        private static string GetRelativePath(string relativeTo, string path)
        {
            Uri fromUri = new Uri(Path.GetFullPath(relativeTo) + Path.DirectorySeparatorChar);
            Uri toUri = new Uri(Path.GetFullPath(path));
            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            return Uri.UnescapeDataString(relativeUri.ToString()).Replace('/', Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// 从Markdown文件中提取代码块并根据目录结构保存
        /// </summary>
        /// <returns>保存的文件数量</returns>
        public int ExtractAndSaveWithDirectoryStructureFromFile(string markdownFilePath, string outputFolder, bool overwriteExisting = false, bool clearTargetFolder = false)
        {
            try
            {
                Console.WriteLine($"开始解析Markdown文件: {markdownFilePath}");

                // 读取Markdown文件内容
                string markdownContent = File.ReadAllText(markdownFilePath);

                return ExtractAndSaveWithDirectoryStructure(markdownContent, outputFolder, overwriteExisting, clearTargetFolder);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"处理过程中发生错误: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// 从Markdown文件中提取代码块并根据目录结构保存
        /// </summary>
        /// <returns>保存的文件数量</returns>
        public int ExtractAndSaveWithDirectoryStructure(string markdownContent, string outputFolder, bool overwriteExisting = false, bool clearTargetFolder = false)
        {
            try
            {
                // 查找目录结构
                List<string> directoryPaths = DirectoryStructureParser.FindDirectoryStructure(markdownContent);

                // 提取代码块
                List<CodeBlock> codeBlocks = MarkdownCodeBlockExtractor.ExtractCodeBlocks(markdownContent);

                Console.WriteLine($"找到 {codeBlocks.Count} 个代码块");

                // 根据目录结构保存文件并返回保存的文件数量
                return SaveCodeBlocksWithDirectoryStructure(codeBlocks, directoryPaths, outputFolder, overwriteExisting, clearTargetFolder);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"处理过程中发生错误: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// 根据目录结构保存代码块并返回保存的文件数量
        /// </summary>
        /// <returns>保存的文件数量</returns>
        private int SaveCodeBlocksWithDirectoryStructure(List<CodeBlock> codeBlocks, List<string> directoryPaths, string outputFolder, bool overwriteExisting, bool clearTargetFolder)
        {
            InitializeOutputFolder(outputFolder, clearTargetFolder);
            var fileNameToCodeBlock = CreateFileNameToCodeBlockMap(codeBlocks);
            var fileStats = ProcessDirectoryStructureFiles(directoryPaths, fileNameToCodeBlock, outputFolder, overwriteExisting);

            ProcessRemainingCodeBlocks(fileNameToCodeBlock, directoryPaths, outputFolder, overwriteExisting, ref fileStats);

            LogCompletionStats(fileStats, outputFolder);

            // 返回实际保存的文件数量
            return fileStats.SavedCount;
        }

        private void InitializeOutputFolder(string outputFolder, bool clearTargetFolder)
        {
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
                Console.WriteLine($"创建输出文件夹: {outputFolder}");
            }
            else if (clearTargetFolder)
            {
                ClearOutputFolder(outputFolder);
            }
            else
            {
                Console.WriteLine($"使用现有输出文件夹: {outputFolder}");
            }
        }

        private Dictionary<string, CodeBlock> CreateFileNameToCodeBlockMap(List<CodeBlock> codeBlocks)
        {
            var fileNameToCodeBlock = new Dictionary<string, CodeBlock>(StringComparer.OrdinalIgnoreCase);

            foreach (var codeBlock in codeBlocks)
            {
                if (!string.IsNullOrEmpty(codeBlock.FileName))
                {
                    string fileName = Path.GetFileName(codeBlock.FileName);
                    fileNameToCodeBlock[fileName] = codeBlock;
                }
            }

            return fileNameToCodeBlock;
        }

        private FileProcessingStats ProcessDirectoryStructureFiles(List<string> directoryPaths, Dictionary<string, CodeBlock> fileNameToCodeBlock, string outputFolder, bool overwriteExisting)
        {
            var stats = new FileProcessingStats();

            foreach (string filePath in directoryPaths)
            {
                string fileName = Path.GetFileName(filePath);

                if (fileNameToCodeBlock.TryGetValue(fileName, out CodeBlock codeBlock))
                {
                    ProcessFileWithDirectory(filePath, outputFolder, codeBlock, overwriteExisting, ref stats);
                    fileNameToCodeBlock.Remove(fileName);
                }
                else
                {
                    stats.NotFoundCount++;
                    Console.WriteLine($"警告: 未找到对应的代码块: {fileName}");
                }
            }

            return stats;
        }

        private void ProcessFileWithDirectory(string relativePath, string outputFolder, CodeBlock codeBlock, bool overwriteExisting, ref FileProcessingStats stats)
        {
            string fullOutputPath = Path.Combine(outputFolder, relativePath);
            string directory = Path.GetDirectoryName(fullOutputPath);

            EnsureDirectoryExists(directory);

            bool saved = SaveFileWithPath(fullOutputPath, codeBlock.CodeContent, overwriteExisting);

            if (saved)
            {
                stats.SavedCount++;
                Console.WriteLine($"保存文件: {relativePath} (语言: {codeBlock.CodeLanguage})");
            }
            else
            {
                stats.SkippedCount++;
                Console.WriteLine($"跳过文件: {relativePath} (已存在)");
            }
        }

        private void ProcessRemainingCodeBlocks(Dictionary<string, CodeBlock> fileNameToCodeBlock, List<string> directoryPaths, string outputFolder, bool overwriteExisting, ref FileProcessingStats stats)
        {
            bool hasSaved = stats.SavedCount > 0;

            foreach (var kvp in fileNameToCodeBlock)
            {
                string fileName = kvp.Key;
                CodeBlock codeBlock = kvp.Value;
                
                if (TryProcessXamlCsFile(fileName, codeBlock, fileNameToCodeBlock, directoryPaths, outputFolder, overwriteExisting, ref stats))
                {
                    continue;
                }

                // 如果已经有导出成功，则没有提取到名字的不导出
                if (hasSaved && codeBlock.Source != FileNameSource.Extracted)
                {
                    Console.WriteLine($"跳过文件: {fileName} (不是提取到的文件名)");
                    continue;
                }

                ProcessRemainingFile(fileName, codeBlock, outputFolder, overwriteExisting, ref stats);
            }
        }

        private bool TryProcessXamlCsFile(string fileName, CodeBlock codeBlock, Dictionary<string, CodeBlock> fileNameToCodeBlock, List<string> directoryPaths, string outputFolder, bool overwriteExisting, ref FileProcessingStats stats)
        {
            if (!fileName.EndsWith(".xaml.cs", StringComparison.OrdinalIgnoreCase))
                return false;

            string xamlFileName = Regex.Replace(fileName, "\\.xaml\\.cs", ".xaml", RegexOptions.IgnoreCase);
            string xamlDirectoryPath = FindXamlDirectoryPath(directoryPaths, xamlFileName);

            if (string.IsNullOrEmpty(xamlDirectoryPath))
            {
                stats.NotFoundCount++;
                Console.WriteLine($"警告: 未找到对应的代码块: {fileName} (也未找到对应的 .xaml 文件目录)");
                return false;
            }

            return SaveXamlCsFile(fileName, xamlDirectoryPath, fileNameToCodeBlock, outputFolder, overwriteExisting, ref stats);
        }

        private bool SaveXamlCsFile(string fileName, string xamlDirectoryPath, Dictionary<string, CodeBlock> fileNameToCodeBlock, string outputFolder, bool overwriteExisting, ref FileProcessingStats stats)
        {
            string xamlDirectory = Path.GetDirectoryName(xamlDirectoryPath);
            string fullOutputPath = Path.Combine(outputFolder, xamlDirectory, fileName);

            EnsureDirectoryExists(Path.GetDirectoryName(fullOutputPath));

            CodeBlock xamlCsCodeBlock = FindXamlCsCodeBlock(fileNameToCodeBlock, fileName, Path.GetFileName(xamlDirectoryPath));

            if (xamlCsCodeBlock == null)
            {
                stats.NotFoundCount++;
                Console.WriteLine($"警告: 未找到对应的代码块: {fileName} (即使尝试了 .xaml.cs 匹配)");
                return false;
            }

            bool saved = SaveFileWithPath(fullOutputPath, xamlCsCodeBlock.CodeContent, overwriteExisting);

            if (saved)
            {
                stats.SavedCount++;
                Console.WriteLine($"保存 .xaml.cs 文件: {Path.Combine(xamlDirectory, fileName)} (语言: {xamlCsCodeBlock.CodeLanguage})");
                return true;
            }
            else
            {
                stats.SkippedCount++;
                Console.WriteLine($"跳过 .xaml.cs 文件: {Path.Combine(xamlDirectory, fileName)} (已存在)");
                return true; // 虽然没保存成功，但表示已处理过
            }
        }

        private void ProcessRemainingFile(string fileName, CodeBlock codeBlock, string outputFolder, bool overwriteExisting, ref FileProcessingStats stats)
        {
            string filePath = Path.Combine(outputFolder, fileName);
            bool saved = SaveFileWithPath(filePath, codeBlock.CodeContent, overwriteExisting);

            if (saved)
            {
                stats.SavedCount++;
                Console.WriteLine($"保存额外文件: {fileName} (语言: {codeBlock.CodeLanguage})");
            }
            else
            {
                stats.SkippedCount++;
                Console.WriteLine($"跳过文件: {fileName} (可能已存在)");
            }
        }

        private void EnsureDirectoryExists(string directory)
        {
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                Console.WriteLine($"创建目录: {directory}");
            }
        }

        private void LogCompletionStats(FileProcessingStats stats, string outputFolder)
        {
            Console.WriteLine($"完成！保存了 {stats.SavedCount} 个文件，跳过了 {stats.SkippedCount} 个已存在的文件");
            Console.WriteLine($"未找到对应代码块: {stats.NotFoundCount} 个文件");
            Console.WriteLine($"所有文件已保存到: {outputFolder}");
        }

        private class FileProcessingStats
        {
            public int SavedCount { get; set; }
            public int SkippedCount { get; set; }
            public int NotFoundCount { get; set; }
        }

        /// <summary>
        /// 查找 .xaml 文件在目录结构中的完整路径
        /// </summary>
        private string FindXamlDirectoryPath(List<string> directoryPaths, string xamlFileName)
        {
            foreach (string filePath in directoryPaths)
            {
                string fileName = Path.GetFileName(filePath);
                if (fileName.Equals(xamlFileName, StringComparison.OrdinalIgnoreCase))
                {
                    return filePath;
                }
            }
            return null;
        }

        /// <summary>
        /// 查找 .xaml.cs 代码块，支持多种可能的文件名匹配
        /// </summary>
        private CodeBlock FindXamlCsCodeBlock(Dictionary<string, CodeBlock> fileNameToCodeBlock, string expectedFileName, string xamlFileName)
        {
            // 尝试直接匹配
            if (fileNameToCodeBlock.TryGetValue(expectedFileName, out CodeBlock codeBlock))
            {
                return codeBlock;
            }

            // 尝试匹配不带 .xaml 前缀的 .cs 文件
            // 例如：MainWindow.xaml.cs 可能被命名为 MainWindow.cs
            string shortFileName = Regex.Replace(xamlFileName, "\\.xaml", "", RegexOptions.IgnoreCase) + ".cs";
            if (fileNameToCodeBlock.TryGetValue(shortFileName, out codeBlock))
            {
                return codeBlock;
            }

            // 尝试匹配所有 .cs 文件，找出与 .xaml 文件名对应的
            foreach (var kvp in fileNameToCodeBlock)
            {
                string existingFileName = kvp.Key;

                // 检查是否为 .cs 文件
                if (existingFileName.ToLower().EndsWith(".cs"))
                {
                    // 检查是否与 .xaml 文件对应
                    string baseName = Regex.Replace(xamlFileName, "\\.xaml", "", RegexOptions.IgnoreCase);

                    if (existingFileName.ToLower().StartsWith(baseName.ToLower()))
                    {
                        return kvp.Value;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 保存文件到指定路径
        /// </summary>
        private bool SaveFileWithPath(string filePath, string content, bool overwriteExisting)
        {
            try
            {
                // 检查文件是否已存在
                if (File.Exists(filePath))
                {
                    if (overwriteExisting)
                    {
                        File.WriteAllText(filePath, content);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    File.WriteAllText(filePath, content);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存文件时出错 {filePath}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 清空输出文件夹
        /// </summary>
        private void ClearOutputFolder(string folderPath)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    return;
                }

                Console.WriteLine($"正在清空目标文件夹: {folderPath}");

                // 获取所有文件和子文件夹
                string[] files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
                string[] directories = Directory.GetDirectories(folderPath, "*", SearchOption.AllDirectories);

                // 先删除所有文件
                foreach (string file in files)
                {
                    try
                    {
                        File.Delete(file);
                        Console.WriteLine($"  删除文件: {GetRelativePath(folderPath, file)}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  删除文件失败 {GetRelativePath(folderPath, file)}: {ex.Message}");
                    }
                }

                // 从深到浅删除所有子文件夹
                Array.Sort(directories, (a, b) => b.Length.CompareTo(a.Length));
                foreach (string directory in directories)
                {
                    try
                    {
                        Directory.Delete(directory, true);
                        Console.WriteLine($"  删除文件夹: {GetRelativePath(folderPath, directory)}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  删除文件夹失败 {GetRelativePath(folderPath, directory)}: {ex.Message}");
                    }
                }

                Console.WriteLine($"清空完成：删除了 {files.Length} 个文件和 {directories.Length} 个文件夹");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清空文件夹时出错: {ex.Message}");
            }
        }
    }
}