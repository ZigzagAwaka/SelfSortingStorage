using BepInEx.Configuration;

namespace SelfSortingStorage
{
    class Config
    {
        public readonly ConfigEntry<string> cupboardColor;

        public Config(ConfigFile cfg)
        {
            cfg.SaveOnConfigSet = false;
            cupboardColor = cfg.Bind("Visuals", "Cupboard Color", "#000E57", "Customize the color of the Smart Cupboard.");
            cfg.Save();
            cfg.SaveOnConfigSet = true;
        }

        public void SetupCustomConfigs()
        {
        }
    }
}
