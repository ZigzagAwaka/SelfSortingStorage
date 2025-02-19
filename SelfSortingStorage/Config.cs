using BepInEx.Configuration;

namespace SelfSortingStorage
{
    class Config
    {
        public Config(ConfigFile cfg)
        {
            cfg.SaveOnConfigSet = false;
            cfg.Save();
            cfg.SaveOnConfigSet = true;
        }

        public void SetupCustomConfigs()
        {
        }
    }
}
