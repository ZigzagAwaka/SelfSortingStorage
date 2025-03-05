﻿using BepInEx.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace SelfSortingStorage
{
    class Config
    {
        public bool GeneralImprovements = false;
        public readonly Dictionary<int, int> rowsOrder = new Dictionary<int, int>();
        public readonly ConfigEntry<bool> enableSaving;
        public readonly ConfigEntry<bool> allowScrapItems;
        public readonly ConfigEntry<string> cupboardColor;
        public readonly ConfigEntry<string> boxPosition;
        public readonly ConfigEntry<bool> rescaleItems;
        public readonly ConfigEntry<string> rowsOrderStr;
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
            rowsOrderStr = cfg.Bind("Storage", "Rows order", "1,2,3,4", "Specify the order of items placement in the storage. Each number represents a shelve of the storage from top to bottom and the first one to be filled will be the number '1'.\nExample: Having an order of '1,2,3,4' will fill items from top to bottom, and having '3,1,2,4' will fill the middle shelves first.\nDON'T CHANGE THIS CONFIG WHEN THE SSS IS ALREADY UNLOCKED, a fresh save is required to avoid bad things happening.");
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

            if (rowsOrderStr.Value == (string)rowsOrderStr.DefaultValue)
                return;
            int i = 0;
            foreach (string orderStr in rowsOrderStr.Value.Split(',').Select(s => s.Trim()))
            {
                if (!int.TryParse(orderStr, out var order) || order <= 0 || order > 4 || i >= 4)
                    break;
                rowsOrder[order] = i++;
            }
            if (rowsOrder.Count != 4 || rowsOrder.Keys.Distinct().Count() != 4)
            {
                Plugin.logger.LogWarning("Invalid 'Rows order' config value. Default order will be used.");
                rowsOrder.Clear();
            }
        }
    }
}
