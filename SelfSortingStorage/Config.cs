using BepInEx.Configuration;

namespace SelfSortingStorage
{
    class Config
    {
        public readonly ConfigEntry<bool> enableSaving;
        public readonly ConfigEntry<bool> allowScrapItems;
        public readonly ConfigEntry<bool> rescaleItems;
        public readonly ConfigEntry<string> cupboardColor;
        public readonly ConfigEntry<int> cupboardPrice;
        public readonly ConfigEntry<bool> verboseLogging;

        public Config(ConfigFile cfg)
        {
            cfg.SaveOnConfigSet = false;
            enableSaving = cfg.Bind("General", "Save items", true, "Allows stored items to be saved in the host player's current save file.");
            allowScrapItems = cfg.Bind("General", "Allow Scrap items", true, "Allows scrap items to be stored in the Smart Cupboard.");
            rescaleItems = cfg.Bind("General", "Rescale big items", true, "Big items will be rescaled when stored in the Smart Cupboard.");
            cupboardColor = cfg.Bind("General", "Cupboard Color", "#000E57", "Customize the color of the Smart Cupboard.");
            cupboardPrice = cfg.Bind("Shop", "Price", 20, "The price of the Smart Cupboard in the store.");
            verboseLogging = cfg.Bind("Logs", "Verbose logs", false, "Enable more explicit logs in the console (for debug reasons).");
            cfg.Save();
            cfg.SaveOnConfigSet = true;
        }

        public void SetupCustomConfigs()
        {
        }
    }
}
