namespace StoneSharp.Core.Providers
{
    public class ChatGroupProvider : IChatGroupProvider
    {
        private readonly HashSet<string> _ignoredFolderNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".temp",
            ".work",
            ".history",
        };

        public ChatGroupProvider(string folderPath)
        {
            // 构造函数可以用于初始化，但当前不需要特殊处理
        }

        public void RenameFolder(string oldPath, string newPath)
        {
            if (string.IsNullOrWhiteSpace(oldPath))
                throw new ArgumentException("原路径不能为空", nameof(oldPath));

            if (string.IsNullOrWhiteSpace(newPath))
                throw new ArgumentException("新路径不能为空", nameof(newPath));

            if (!Directory.Exists(oldPath))
                throw new DirectoryNotFoundException($"文件夹不存在: {oldPath}");

            if (Directory.Exists(newPath))
                throw new IOException($"目标文件夹已存在: {newPath}");

            Directory.Move(oldPath, newPath);
        }

        public IEnumerable<string> GetSubFolders(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                throw new ArgumentException("文件夹路径不能为空", nameof(folderPath));

            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException($"文件夹不存在: {folderPath}");

            return Directory.GetDirectories(folderPath)
               .Select(Path.GetFileName)
               .Where(folderName => !_ignoredFolderNames.Contains(folderName));
        }

        public void CreateFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                throw new ArgumentException("文件夹路径不能为空", nameof(folderPath));

            Directory.CreateDirectory(folderPath);
        }

        public void DeleteFolder(string folderPath, bool recursive)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                throw new ArgumentException("文件夹路径不能为空", nameof(folderPath));

            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException($"文件夹不存在: {folderPath}");

            Directory.Delete(folderPath, recursive);
        }

        public bool FolderExists(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                return false;

            return Directory.Exists(folderPath);
        }

        public string GetUniqueFolderName(string parentPath, string baseName)
        {
            if (string.IsNullOrWhiteSpace(parentPath))
                throw new ArgumentException("父路径不能为空", nameof(parentPath));

            if (string.IsNullOrWhiteSpace(baseName))
                throw new ArgumentException("基础名称不能为空", nameof(baseName));

            string newFolderName = baseName;
            string newFolderPath = Path.Combine(parentPath, newFolderName);

            int counter = 1;
            while (Directory.Exists(newFolderPath))
            {
                newFolderName = $"{baseName}{counter}";
                newFolderPath = Path.Combine(parentPath, newFolderName);
                counter++;
            }

            return newFolderName;
        }
    }
}