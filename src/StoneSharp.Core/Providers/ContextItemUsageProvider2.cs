using StoneSharp.Core.Models.ContextItems;

namespace StoneSharp.Core.Providers
{
    public class ContextItemUsageProvider2
    {
        public List<ContextItemUsage> Entries { get; set; } = new List<ContextItemUsage>();

        public void UpdateCount(ContextItem contextItem, int count)
        {
            var entry = Entries.FirstOrDefault(e => e.ContextItem.Equals(contextItem));
            if (entry != null)
            {
                entry.Count = count;
            }
            else
            {
                Entries.Add(new ContextItemUsage { ContextItem = contextItem, Count = count });
            }
        }

        public void AddOrUpdate(ContextItem contextItem, int count, bool isPinned = false)
        {
            var entry = Entries.FirstOrDefault(e => e.ContextItem.Equals(contextItem));
            if (entry != null)
            {
                entry.Count = count;
                entry.IsPinned = isPinned;
            }
            else
            {
                Entries.Add(new ContextItemUsage { ContextItem = contextItem, Count = count, IsPinned = isPinned });
            }
        }

        public void AddOrUpdateEntries(List<ContextItemUsage> contextItemUsageEntries)
        {
            foreach (var contextItemUsageEntry in contextItemUsageEntries)
            {
                var existingEntry = Entries.FirstOrDefault(e => e.ContextItem.Equals(contextItemUsageEntry.ContextItem));
                if (existingEntry != null)
                {
                    // 更新现有条目
                    existingEntry.Count = contextItemUsageEntry.Count;
                    existingEntry.IsPinned = contextItemUsageEntry.IsPinned;
                }
                else
                {
                    // 添加新条目
                    Entries.Add(contextItemUsageEntry);
                }
            }
        }

        public List<ContextItemUsage> GetTopMostUsedItems()
        {
            // 获取所有置顶的条目
            var pinnedItems = Entries.Where(e => e.IsPinned).ToList();

            // 如果置顶条目不足 20 个，则从剩余条目中按使用次数补充
            if (pinnedItems.Count < 100)
            {
                // 获取剩余条目中按使用次数排序的前 (20 - pinnedItems.Count) 个条目
                var remainingItems = Entries
                    .Where(e => !e.IsPinned) // 排除已经置顶的条目
                    .OrderByDescending(e => e.Count)
                    .Take(100 - pinnedItems.Count)
                    .ToList();

                // 将置顶条目和补充条目合并
                pinnedItems.AddRange(remainingItems);
            }

            return pinnedItems;
        }
    }
}