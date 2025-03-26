﻿using BepInEx.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace SelfSortingStorage
{
    class Config
    {
        public bool GeneralImprovements = false;
        public readonly Dictionary<int, int> rowsOrder = new Dictionary<int, int>();
        public readonly List<string> itemsBlacklist = new List<string>();
        public readonly ConfigEntry<bool> enableSaving;
        public readonly ConfigEntry<bool> allowScrapItems;
        public readonly ConfigEntry<bool> scanNode;
        public readonly ConfigEntry<string> cupboardColor;
        public readonly ConfigEntry<string> boxPosition;
        public readonly ConfigEntry<bool> rescaleItems;
        public readonly ConfigEntry<bool> perfectRescale;
        public readonly ConfigEntry<string> rowsOrderStr;
        public readonly ConfigEntry<string> blacklistStr;
        public readonly ConfigEntry<int> cupboardPrice;
        public readonly ConfigEntry<bool> wideVersion;
        public readonly ConfigEntry<bool> cozyLights;
        public readonly ConfigEntry<bool> resetButton;
        public readonly ConfigEntry<int> customScreenPos;
        public readonly ConfigEntry<bool> verboseLogging;

        public Config(ConfigFile cfg)
        {
            cfg.SaveOnConfigSet = false;
            enableSaving = cfg.Bind("General", "Save items", true, "Allows stored items to be saved in the host player's current save file.");
            allowScrapItems = cfg.Bind("General", "Allow Scrap items", true, "Allows scrap items to be stored in the Smart Cupboard.");
            scanNode = cfg.Bind("General", "Scan Node", true, "Adds a scan node on the storage box.");
            cupboardColor = cfg.Bind("General", "Cupboard Color", "#000E57", "Customize the color of the storage, default color is dark blue.");
            boxPosition = cfg.Bind("Storage", "Box position", "R", new ConfigDescription("Adjust the position of the storage box, this can be 'L' for left or 'R' for right.", new AcceptableValueList<string>("L", "R")));
            rescaleItems = cfg.Bind("Storage", "Rescale big items", true, "Big items will be rescaled when stored in the Smart Cupboard (based on their collider volume).");
            perfectRescale = cfg.Bind("Storage", "Perfect rescale", true, "Change the rescale algorithm to have items perfectly rescaled when stored (based on their collider max size).");
            rowsOrderStr = cfg.Bind("Storage", "Rows order", "1,2,3,4", "Specify the order of items placement in the storage. Each number represents a shelve of the storage from top to bottom and the first one to be filled will be the number '1'.\nExample: Having an order of '1,2,3,4' will fill items from top to bottom, and having '3,1,2,4' will fill the middle shelves first.\nDON'T CHANGE THIS CONFIG WHEN THE SSS IS ALREADY UNLOCKED, a fresh save is required to avoid bad things happening (or press the Reset Button).");
            blacklistStr = cfg.Bind("Storage", "Items Blacklist", "clipboard,Sticky note,Utility Belt", "Comma separated list of items names that will be rejected from the storage.");
            cupboardPrice = cfg.Bind("Shop", "Price", 20, "The price of the Smart Cupboard in the store.");
            wideVersion = cfg.Bind("Upgrades", "Wide Cupboard", false, "Activate the S4 upgrade, turns the Smart Cupboard into a wider version.\nDON'T CHANGE THIS CONFIG WHEN THE SSS IS ALREADY UNLOCKED, a fresh save is required to avoid bad things happening (or press the Reset Button).");
            cozyLights = cfg.Bind("Upgrades", "Cozy Lights", true, "Activate the cozy lights upgrade, adds some lights on the shelves and a button to turn them on and off.");
            resetButton = cfg.Bind("Upgrades", "Reset Button", false, "Activate the soft reset upgrade, adds the 'button of death' on the side of the storage that you can press to clear every stored items in the SSS.\nPressing this button will effectively destroy everything so you shouldn't have the need to activate this unless you really want a way to reset the storage without being fired.");
            customScreenPos = cfg.Bind("Upgrades", "SSS Screen position", 0, "If GeneralImprovements is installed and this value is not 0, then this mod will display SSS items on the desired screen. In GI config you need to set 'UseBetterMonitors = true', then set to 'None' the screen that will display the SSS items, then provide the screen position here to make it work.");
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

            if (!string.IsNullOrEmpty(blacklistStr.Value))
            {
                foreach (string itemStr in blacklistStr.Value.Split(',').Select(s => s.Trim()))
                {
                    itemsBlacklist.Add(itemStr);
                }
                if (itemsBlacklist.Count != 0)
                    RegisterBlacklist();
            }

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

        private void RegisterBlacklist()
        {
            Cupboard.SmartCupboard.AddTriggerValidation(BlacklistValidation, "[Item blacklisted]");
        }

        private bool BlacklistValidation(GameNetcodeStuff.PlayerControllerB player)
        {
            var item = player.currentlyHeldObjectServer;
            if (itemsBlacklist.Exists((i) => i == item.itemProperties.itemName))
                return false;
            return true;
        }
    }
}
