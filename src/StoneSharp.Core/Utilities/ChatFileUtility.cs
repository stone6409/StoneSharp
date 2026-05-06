using StoneSharp.Core.Models;
using System.Text.RegularExpressions;

namespace StoneSharp.Core.Utilities
{
    public static class ChatFileUtility
    {
        private static string folderPath;

        public static void Initialize(string path)
        {
            folderPath = path;
            FilePathUtility.EnsureDirectoryExists(folderPath);
        }

        public static string NewId()
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString("x");
            var guid = Guid.NewGuid().ToString("N").Substring(0, 8);
            return $"{timestamp}{guid}";
        }

        public static bool CreateChatFile(string id, string name)
        {
            string filePath = Path.Combine(folderPath, GenerateFileName(id, name));
            try
            {
                File.Create(filePath).Close();
            }
            catch (Exception ex)
            {
                // 处理文件创建失败的异常
                Console.WriteLine($"Error creating chat file: {ex.Message}");

                return false;
            }

            return true;
        }

        public static bool DeleteChatFile(string id, string name)
        {
            string filePath = Path.Combine(folderPath, GenerateFileName(id, name));
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                // 处理文件删除失败的异常
                Console.WriteLine($"Error deleting chat file: {ex.Message}");

                return false;
            }

            return true;
        }

        public static List<Chat> GetAllChats()
        {
            List<Chat> chats = new List<Chat>();

            // 获取指定文件夹中所有后缀名为 ".chat" 的文件
            string[] fileEntries = Directory.GetFiles(folderPath, "*.chat");

            foreach (string filePath in fileEntries)
            {
                Chat chat = ParseChatFileName(Path.GetFileName(filePath));
                if (chat != null)
                {
                    chat.Time = File.GetLastWriteTime(filePath);
                    chats.Add(chat);
                }
            }

            chats = chats.OrderByDescending(c => c.Time).ToList();
            return chats;
        }

        public static List<Chat> SearchChats(string searchTerm)
        {
            List<Chat> allChats = GetAllChats();
            return allChats.Where(chat => chat.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // 解析 chat 文件名并返回 Chat 对象
        private static Chat ParseChatFileName(string fileName)
        {
            // 新格式: {id}_{name}.chat，其中 id 是字母数字组合（时间戳hex+guid）
            Regex regex = new Regex(@"^([a-z0-9]+)_(.+)\.chat$", RegexOptions.IgnoreCase);
            Match match = regex.Match(fileName);
            if (match.Success && match.Groups.Count == 3)
            {
                string id = match.Groups[1].Value;
                string name = match.Groups[2].Value;

                return new Chat
                {
                    Id = id,
                    Name = name
                };
            }

            return null;
        }

        private static string GenerateFileName(string id, string name)
        {
            // 移除名称中的无效字符
            foreach (char invalidChar in System.IO.Path.GetInvalidFileNameChars())
            {
                name = name.Replace(invalidChar, '_');
            }

            // 如果生成的名称名太长，则截断
            const int MaxFileNameLength = 40; // 文件名的最大长度
            if (name.Length > MaxFileNameLength)
            {
                name = name.Substring(0, MaxFileNameLength);
            }

            // 将id和name组合成一个文件名
            string fileName = $"{id}_{name}.chat";

            return fileName;
        }


        public static string GetFilePath(string id, string name)
        {
            string filePath = Path.Combine(folderPath, GenerateFileName(id, name));
            return filePath;
        }
    }
}
