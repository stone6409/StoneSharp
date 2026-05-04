using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using System.IO;
using System.Text.RegularExpressions;

namespace StoneSharp.CodeProcessing.CodeBlockExtractor
{
    /// <summary>
    /// 代码路径提取器 - 专门用于从代码内容中提取文件路径信息
    /// </summary>
    public static class CodePathExtractor
    {
        private static readonly Regex[] PathPatterns = new[]
        {
            new Regex(@"using\s+([\w\.]+)\s*;", RegexOptions.Compiled),  // C# using语句
            new Regex(@"namespace\s+([\w\.]+)", RegexOptions.Compiled),  // C# namespace
            new Regex(@"class\s+(\w+)", RegexOptions.Compiled),          // C# class
            new Regex(@"public\s+class\s+(\w+)", RegexOptions.Compiled), // C# public class
            new Regex(@"<Application\s+x:Class=""([\w\.]+)""", RegexOptions.Compiled), // Application
            new Regex(@"<Window\s+x:Class=""([\w\.]+)""", RegexOptions.Compiled), // WPF Window
            new Regex(@"<UserControl\s+x:Class=""([\w\.]+)""", RegexOptions.Compiled), // WPF UserControl
            new Regex(@"<Page\s+x:Class=""([\w\.]+)""", RegexOptions.Compiled), // WPF Page
        };

        /// <summary>
        /// 从代码块列表中提取可能的文件路径
        /// </summary>
        public static List<string> ExtractPathsFromCodeBlocks(List<CodeBlock> codeBlocks, IEnumerable<string> projectFolders)
        {
            var directoryPaths = new List<string>();

            foreach (var codeBlock in codeBlocks)
            {
                string extractedPath = ExtractPathFromCodeContent(codeBlock.CodeContent, codeBlock.CodeLanguage, projectFolders);
                if (!string.IsNullOrEmpty(extractedPath))
                {
                    string fileName = Path.GetFileName(extractedPath);
                    codeBlock.FileName = fileName;
                    directoryPaths.Add(extractedPath);
                }
            }

            return directoryPaths;
        }

        /// <summary>
        /// 从代码内容中提取路径信息
        /// </summary>
        public static string ExtractPathFromCodeContent(string codeContent, string codeLanguage, IEnumerable<string> projectFolders = null)
        {
            if (string.IsNullOrEmpty(codeContent))
                return null;

            string result = null;
            switch (codeLanguage?.ToLower())
            {
                case "csharp":
                case "cs":
                case "c#":
                    result = ExtractPathFromCSharpCode(codeContent, projectFolders);
                    break;
                case "xml":
                    result = ExtractPathFromXamlCode(codeContent, projectFolders);
                    break;
                default:
                    break;
            }

            if (string.IsNullOrEmpty(result))
            {
                result = ExtractPathFromGenericCode(codeContent, projectFolders);
            }

            return result;
        }

        /// <summary>
        /// 构建文件路径（共享方法）
        /// </summary>
        private static string BuildFilePath(string codeNamespace, string codeType, string extension, IEnumerable<string> projectFolders)
        {
            if (string.IsNullOrEmpty(codeType))
                return null;

            string projectName = null;

            // 如果提供了工程文件夹，尝试解析包含工程名称的命名空间
            if (!string.IsNullOrEmpty(codeNamespace) && projectFolders != null && projectFolders.Any())
            {
                projectName = MapNamespaceToProjectName(codeNamespace, projectFolders);
                if (!string.IsNullOrEmpty(projectName))
                {
                    if (codeNamespace.StartsWith(projectName))
                    {
                        // 移除 projectName 部分
                        string remaining = codeNamespace.Substring(projectName.Length);

                        // 如果剩余部分以点号开头，移除点号
                        if (remaining.StartsWith("."))
                        {
                            codeNamespace = remaining.Substring(1);
                        }
                        else
                        {
                            // 如果 codeNamespace 和 projectName 完全相同，清空 codeNamespace
                            codeNamespace = string.Empty;
                        }
                    }
                }
            }

            string relativePath;

            // 构建相对路径
            if (!string.IsNullOrEmpty(codeNamespace))
            {
                relativePath = $"{codeNamespace.Replace('.', Path.DirectorySeparatorChar)}{Path.DirectorySeparatorChar}{codeType}{extension}";
            }
            else
            {
                relativePath = $"{codeType}{extension}";
            }

            return string.IsNullOrEmpty(projectName) ? relativePath : Path.Combine(projectName, relativePath);
        }

        /// <summary>
        /// 从C#代码中提取路径信息
        /// </summary>
        private static string ExtractPathFromCSharpCode(string codeContent, IEnumerable<string> projectFolders)
        {
            try
            {
                var codeSyntaxTree = CSharpSyntaxTree.ParseText(codeContent);
                var codeRoot = codeSyntaxTree.GetRoot();

                // 获取命名空间，支持两种格式
                string codeNamespace = codeRoot.DescendantNodes()
                    .OfType<BaseNamespaceDeclarationSyntax>()  // 使用基类
                    .FirstOrDefault()?.Name.ToString();

                // 获取类型声明和基类信息
                var (codeType, baseType) = ExtractCSharpTypeNameAndBaseType(codeRoot);

                if (string.IsNullOrEmpty(codeType))
                    return null;

                // 根据基类类型决定文件扩展名
                string extension = DetermineFileExtension(codeType, baseType);

                return BuildFilePath(codeNamespace, codeType, extension, projectFolders);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 根据基类类型决定文件扩展名
        /// </summary>
        private static string DetermineFileExtension(string typeName, string baseType)
        {
            if (string.IsNullOrEmpty(baseType))
                return ".cs";

            // 定义WPF相关基类列表
            var wpfBaseTypes = new HashSet<string>
            {
                "Window",
                "UserControl",
                "Page",
                "Application",
                "ResourceDictionary",
                "ContentControl",
                "Control",
                "FrameworkElement",
                "FrameworkContentElement"
            };

            // 检查是否继承自WPF相关基类
            if (wpfBaseTypes.Contains(baseType) ||
                baseType.EndsWith("Window") ||
                baseType.EndsWith("UserControl") ||
                baseType.EndsWith("Page") ||
                baseType.EndsWith("Application"))
            {
                return ".xaml.cs";
            }

            return ".cs";
        }

        /// <summary>
        /// 提取C#类型名称和基类信息
        /// </summary>
        private static (string typeName, string baseType) ExtractCSharpTypeNameAndBaseType(SyntaxNode root)
        {
            var typeNode = root.DescendantNodes()
                .FirstOrDefault(node => node is ClassDeclarationSyntax || node is StructDeclarationSyntax ||
                    node is EnumDeclarationSyntax || node is DelegateDeclarationSyntax ||
                    node is InterfaceDeclarationSyntax || node is RecordDeclarationSyntax);

            if (typeNode == null)
                return (null, null);

            string typeName = null;
            string baseType = null;

            switch (typeNode)
            {
                case ClassDeclarationSyntax classDecl:
                    typeName = classDecl.Identifier.Text;
                    // 获取基类
                    baseType = classDecl.BaseList?.Types.FirstOrDefault()?.Type.ToString();
                    break;

                case StructDeclarationSyntax structDecl:
                    typeName = structDecl.Identifier.Text;
                    break;

                case EnumDeclarationSyntax enumDecl:
                    typeName = enumDecl.Identifier.Text;
                    break;

                case DelegateDeclarationSyntax delegateDecl:
                    typeName = delegateDecl.Identifier.Text;
                    break;

                case InterfaceDeclarationSyntax interfaceDecl:
                    typeName = interfaceDecl.Identifier.Text;
                    break;

                case RecordDeclarationSyntax recordDecl:
                    typeName = recordDecl.Identifier.Text;
                    // 获取基类（record也可以有基类）
                    baseType = recordDecl.BaseList?.Types.FirstOrDefault()?.Type.ToString();
                    break;
            }

            return (typeName, baseType);
        }

        /// <summary>
        /// 从XAML代码中提取路径信息
        /// </summary>
        private static string ExtractPathFromXamlCode(string codeContent, IEnumerable<string> projectFolders)
        {
            try
            {
                var xamlDoc = XDocument.Parse(codeContent);
                var rootElement = xamlDoc.Root;

                if (rootElement == null)
                    return null;

                XNamespace xNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
                string xClass = rootElement.Attributes()
                    .FirstOrDefault(attr => attr.Name.Namespace == xNamespace && attr.Name.LocalName == "Class")?.Value;

                if (string.IsNullOrEmpty(xClass))
                    return null;

                // 分离命名空间和类名
                int lastDotIndex = xClass.LastIndexOf('.');
                if (lastDotIndex > 0)
                {
                    string codeNamespace = xClass.Substring(0, lastDotIndex);
                    string codeClass = xClass.Substring(lastDotIndex + 1);
                    return BuildFilePath(codeNamespace, codeClass, ".xaml", projectFolders);
                }
                else
                {
                    // 如果没有点号，只有类名
                    return $"{xClass}.xaml";
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 从通用代码中提取路径信息
        /// </summary>
        private static string ExtractPathFromGenericCode(string codeContent, IEnumerable<string> projectFolders)
        {
            foreach (var pattern in PathPatterns)
            {
                var match = pattern.Match(codeContent);
                if (!match.Success || match.Groups.Count <= 1)
                    continue;

                string matchedValue = match.Groups[1].Value;

                // 根据匹配的内容决定文件扩展名
                if (pattern.ToString().Contains("x:Class"))
                {
                    // 分离命名空间和类名
                    int lastDotIndex = matchedValue.LastIndexOf('.');
                    if (lastDotIndex > 0)
                    {
                        string codeNamespace = matchedValue.Substring(0, lastDotIndex);
                        string codeClass = matchedValue.Substring(lastDotIndex + 1);
                        return BuildFilePath(codeNamespace, codeClass, ".xaml", projectFolders);
                    }
                    else
                    {
                        // 如果没有点号，只有类名
                        return $"{matchedValue}.xaml";
                    }
                }
                else if (pattern.ToString().Contains("class") || pattern.ToString().Contains("namespace"))
                {
                    // 分离命名空间和类名
                    int lastDotIndex = matchedValue.LastIndexOf('.');
                    if (lastDotIndex > 0)
                    {
                        string codeNamespace = matchedValue.Substring(0, lastDotIndex);
                        string codeClass = matchedValue.Substring(lastDotIndex + 1);
                        return BuildFilePath(codeNamespace, codeClass, ".cs", projectFolders);
                    }
                    else
                    {
                        // 如果没有点号，只有类名
                        return $"{matchedValue}.cs";
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 将命名空间映射到工程相对路径
        /// </summary>
        private static string MapNamespaceToProjectName(string codeNamespace, IEnumerable<string> projectFolders)
        {
            var normalizedProjectFolders = projectFolders
                .Where(f => !string.IsNullOrEmpty(f))
                .Select(f => NormalizePath(f))
                .ToList();

            string[] namespaceSegments = codeNamespace.Split('.');
            string bestMatch = string.Empty;
            int bestMatchLength = 0;

            // 尽量匹配长的名字空间
            for (int i = namespaceSegments.Length - 1; i >= 0; i--)
            {
                string possibleProjectName = string.Join(".", namespaceSegments.Take(i + 1));

                foreach (var projectFolder in normalizedProjectFolders)
                {
                    string projectName = Path.GetFileName(projectFolder);

                    // 检查是否匹配
                    if (possibleProjectName.Contains(projectName))
                    {
                        // 选择匹配最长的项目名称
                        if (projectName.Length > bestMatchLength)
                        {
                            bestMatch = projectName;
                            bestMatchLength = projectName.Length;
                        }
                    }
                }
            }

            return bestMatch;
        }

        /// <summary>
        /// 规范化路径
        /// </summary>
        private static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            try
            {
                string fullPath = Path.GetFullPath(path);
                return fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
            catch
            {
                return path;
            }
        }

        /// <summary>
        /// 提取C#类型名称
        /// </summary>
        private static string ExtractCSharpTypeName(SyntaxNode root)
        {
            var typeNode = root.DescendantNodes()
                .FirstOrDefault(node => node is ClassDeclarationSyntax || node is StructDeclarationSyntax || 
                    node is EnumDeclarationSyntax || node is DelegateDeclarationSyntax || 
                    node is InterfaceDeclarationSyntax || node is RecordDeclarationSyntax);

            if (typeNode is ClassDeclarationSyntax classDecl)
                return classDecl.Identifier.Text;
            else if (typeNode is StructDeclarationSyntax structDecl)
                return structDecl.Identifier.Text;
            else if (typeNode is EnumDeclarationSyntax enumDecl)
                return enumDecl.Identifier.Text;
            else if (typeNode is DelegateDeclarationSyntax delegateDecl)
                return delegateDecl.Identifier.Text;
            else if (typeNode is InterfaceDeclarationSyntax interfaceDecl)
                return interfaceDecl.Identifier.Text;
            else if (typeNode is RecordDeclarationSyntax recordDecl)
                return recordDecl.Identifier.Text;
            else
                return null;
        }
    }
}