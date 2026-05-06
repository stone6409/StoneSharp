using StoneSharp.Core.Models.ContextItems;
using StoneSharp.Core.Stores;

namespace StoneSharp.Core.Providers
{
    public class ContextItemUsageProvider
    {
        public ContextItemUsageProvider(string fileName)
        {
            ContextItemUsageStore = new ContextItemUsageStore(fileName, true);
        }

        public ContextItemUsageStore ContextItemUsageStore { get; private set; }

        public ContextItemUsageCollection LoadContextItemUsages()
        {
            return ContextItemUsageStore.LoadContextItemUsages();
        }

        public void UpdateContextItemUsages(ContextItemUsageCollection contextItemUsages)
        {
            ContextItemUsageStore.UpdateContextItemUsages(contextItemUsages);
        }
    }
}
