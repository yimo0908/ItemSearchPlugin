using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Lumina.Excel.Sheets;

namespace ItemSearchPlugin.DataSites
{
    public class TeamcraftDataSite(ItemSearchPluginConfig config) : DataSite
    {
        public override string Name => "Teamcraft";

        public override string NameTranslationKey => "TeamcraftDataSite";

        public override string GetItemUrl(Item item) =>
            $"https://ffxivteamcraft.com/db/en/item/{item.RowId}/{item.Name.ToString().Replace(' ', '-')}";

        private static bool _teamcraftLocalFailed;
        private static readonly HttpClient HttpClient = new();

        public override void OpenItem(Item item)
        {
            if (!(_teamcraftLocalFailed || config.TeamcraftForceBrowser))
            {
                Task.Run(async () =>
                {
                    try
                    {
                        HttpClient.Timeout = TimeSpan.FromMilliseconds(500);
                        var response = await HttpClient.GetAsync($"http://localhost:14500/db/en/item/{item.RowId}");
                        response.EnsureSuccessStatusCode();
                    }
                    catch
                    {
                        try
                        {
                            if (System.IO.Directory.Exists(System.IO.Path.Combine(
                                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                    "ffxiv-teamcraft")))
                            {
                                Process.Start($"teamcraft://db/en/item/{item.RowId}");
                            }
                            else
                            {
                                _teamcraftLocalFailed = true;
                                Process.Start($"https://ffxivteamcraft.com/db/en/item/{item.RowId}");
                            }
                        }
                        catch
                        {
                            _teamcraftLocalFailed = true;
                            Process.Start($"https://ffxivteamcraft.com/db/en/item/{item.RowId}");
                        }
                    }
                });
                return;
            }

            base.OpenItem(item);
        }
    }
}
