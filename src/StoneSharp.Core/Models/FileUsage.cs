using System.Xml.Serialization;

namespace StoneSharp.Core.Models
{
    [XmlRoot("FileUsage")]
    public class FileUsage
    {
        [XmlElement("Entry")]
        public List<FileUsageEntry> Entries { get; set; } = new List<FileUsageEntry>();

        public void UpdateCount(string filePath, int count)
        {
            var entry = Entries.FirstOrDefault(e => e.FilePath == filePath);
            if (entry != null)
            {
                entry.Count = count;
            }
            else
            {
                Entries.Add(new FileUsageEntry { FilePath = filePath, Count = count });
            }
        }

        public void AddOrUpdate(string filePath, int count, bool isPinned = false)
        {
            var entry = Entries.FirstOrDefault(e => e.FilePath == filePath);
            if (entry != null)
            {
                entry.Count = count;
                entry.IsPinned = isPinned;
            }
            else
            {
                Entries.Add(new FileUsageEntry { FilePath = filePath, Count = count, IsPinned = isPinned });
            }
        }

        public void AddOrUpdateEntries(List<FileUsageEntry> fileUsageEntries)
        {
            foreach (var fileUsageEntry in fileUsageEntries)
            {
                var existingFileUsageEntry = Entries.FirstOrDefault(e => e.FilePath == fileUsageEntry.FilePath);
                if (existingFileUsageEntry != null)
                {
                    // 更新现有条目
                    existingFileUsageEntry.Count = fileUsageEntry.Count;
                    existingFileUsageEntry.IsPinned = fileUsageEntry.IsPinned;
                }
                else
                {
                    // 添加新条目
                    Entries.Add(fileUsageEntry);
                }
            }
        }

        public List<FileUsageEntry> GetTopMostUsedFiles()
        {
            // 获取所有置顶的文件
            var pinnedFiles = Entries.Where(e => e.IsPinned).ToList();

            // 如果置顶文件不足 20 个，则从剩余文件中按使用次数补充
            if (pinnedFiles.Count < 100)
            {
                // 获取剩余文件中按使用次数排序的前 (20 - pinnedFiles.Count) 个文件
                var remainingFiles = Entries
                    .Where(e => !e.IsPinned) // 排除已经置顶的文件
                    .OrderByDescending(e => e.Count)
                    .Take(100 - pinnedFiles.Count)
                    .ToList();

                // 将置顶文件和补充文件合并
                pinnedFiles.AddRange(remainingFiles);
            }

            return pinnedFiles;
        }
    }
}