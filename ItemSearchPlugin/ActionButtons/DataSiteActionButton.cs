using Lumina.Excel.Sheets;

namespace ItemSearchPlugin.ActionButtons
{
    class DataSiteActionButton(ItemSearchPluginConfig pluginConfig) : IActionButton
    {
        public override ActionButtonPosition ButtonPosition => ActionButtonPosition.TOP;

        public override void Dispose()
        {
        }

        public override string GetButtonText(Item selectedItem)
        {
            // 复用 GetShowButton 的逻辑确保 SelectedDataSite 不为 null
            if (pluginConfig.SelectedDataSite == null)
            {
                return string.Empty;
            }
            
            return string.Format(
                Loc.Localize("ItemSearchDataSiteViewButton", "View on {0}"),
                Loc.Localize(pluginConfig.SelectedDataSite.NameTranslationKey, pluginConfig.SelectedDataSite.Name)
            );
        }

        public override bool GetShowButton(Item selectedItem)
        {
            return pluginConfig.SelectedDataSite != null;
        }

        public override void OnButtonClicked(Item selectedItem)
        {
            pluginConfig.SelectedDataSite?.OpenItem(selectedItem);
        }
    }
}
