using System.Diagnostics;
using Lumina.Excel.Sheets;

namespace ItemSearchPlugin.DataSites
{
    public abstract class DataSite
    {
        public abstract string GetItemUrl(Item item);

        public abstract string Name { get; }

        public abstract string NameTranslationKey { get; }

        public virtual string Note { get; } = string.Empty;

        public virtual void OpenItem(Item item)
        {
            Process.Start(new ProcessStartInfo() { UseShellExecute = true, FileName = GetItemUrl(item) });
        }
    }
}