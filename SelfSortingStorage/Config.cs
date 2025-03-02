using BepInEx.Configuration;

namespace SelfSortingStorage
{
    class Config
    {
        public bool GeneralImprovements = false;
        public readonly ConfigEntry<bool> enableSaving;
        public readonly ConfigEntry<bool> allowScrapItems;
        public readonly ConfigEntry<string> cupboardColor;
        public readonly ConfigEntry<string> boxPosition;
        public readonly ConfigEntry<bool> rescaleItems;
        public readonly ConfigEntry<int> cupboardPrice;
        public readonly ConfigEntry<bool> verboseLogging;

        public Config(ConfigFile cfg)
        {
            cfg.SaveOnConfigSet = false;
            enableSaving = cfg.Bind("General", "Save items", true, "Allows stored items to be saved in the host player's current save file.");
            allowScrapItems = cfg.Bind("General", "Allow Scrap items", true, "Allows scrap items to be stored in the Smart Cupboard.");
            cupboardColor = cfg.Bind("General", "Cupboard Color", "#000E57", "Customize the color of the storage, default color is dark blue.");
            boxPosition = cfg.Bind("Storage", "Box position", "R", new ConfigDescription("Adjust the position of the storage box, this can be 'L' for left or 'R' for right.", new AcceptableValueList<string>("L", "R")));
            rescaleItems = cfg.Bind("Storage", "Rescale big items", true, "Big items will be rescaled when stored in the Smart Cupboard.");
            cupboardPrice = cfg.Bind("Shop", "Price", 20, "The price of the Smart Cupboard in the store.");
            verboseLogging = cfg.Bind("Logs", "Verbose logs", false, "Enable more explicit logs in the console (for debug reasons).");
            cfg.Save();
            cfg.SaveOnConfigSet = true;
        }

        public void SetupCustomConfigs()
        {
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("ShaosilGaming.GeneralImprovements"))
            {
                GeneralImprovements = true;
            }
            if (!GeneralImprovements)
                Plugin.logger.LogError("GeneralImprovements is not installed! The mod will still work but you may notice some item rotation issues.");
        }
    }
}
