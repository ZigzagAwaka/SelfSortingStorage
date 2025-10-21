using BepInEx.Bootstrap;

namespace SelfSortingStorage.Utils
{
    internal class Compatibility
    {
        public static bool CompatibilityModsAreValid = false;

        public static bool LethalLibInstalled = false;
        public static bool DawnLibInstalled = false;
        public static bool GeneralImprovementsInstalled = false;
        public static bool MattyFixesInstalled = false;
        public static bool LittleCompanyInstalled = false;

        public static void CheckInstalledPlugins()
        {
            LethalLibInstalled = IsPluginInstalled("evaisa.lethallib");
            DawnLibInstalled = IsPluginInstalled("com.github.teamxiaolan.dawnlib");
            GeneralImprovementsInstalled = IsPluginInstalled("ShaosilGaming.GeneralImprovements");
            MattyFixesInstalled = IsPluginInstalled("mattymatty.MattyFixes");
            LittleCompanyInstalled = IsPluginInstalled("Toybox.LittleCompany");
        }

        private static bool IsPluginInstalled(string pluginGUID, string? pluginVersion = null)
        {
            return Chainloader.PluginInfos.ContainsKey(pluginGUID) &&
                (pluginVersion == null || new System.Version(pluginVersion).CompareTo(Chainloader.PluginInfos[pluginGUID].Metadata.Version) <= 0);
        }

        public static bool CheckCompatibilitySSS(bool registerPopup = false, bool displayTip = false)
        {
            int errorID = 0;
            bool compatibilityValid = true;
            if (!LethalLibInstalled && !DawnLibInstalled)
            {
                Plugin.logger.LogError("This mod requires a library to register it's content to the game : please install DawnLib or LethalLib.");
                return false;  // no need to do more because it will throw an error in Plugin.Awake, resulting in an expected crash
            }
            if (!GeneralImprovementsInstalled && !MattyFixesInstalled)
            {
                if (DawnLibInstalled && Effects.GetConfigDL(0))
                {
                    Plugin.logger.LogError("'Item Saving' config in DawnLib needs to stay false ! Or else it will disable the item saving system.");
                    compatibilityValid = false;
                    errorID = 6000;
                }
                else if (!DawnLibInstalled)
                {
                    Plugin.logger.LogError("An additional mod is required for this mod to work properly : GeneralImprovements OR MattyFixes, please install one of these two before playing ! Without it the mod will still work but you may notice some item rotation issues.");
                    compatibilityValid = false;
                    errorID = 6001;
                }
            }
            else if (GeneralImprovementsInstalled && !Effects.GetConfigGI(2))
            {
                Plugin.logger.LogError("ShipPlaceablesCollide config in GeneralImprovements needs to be enabled !");
                compatibilityValid = false;
                errorID = 6002;
            }
            else if (!GeneralImprovementsInstalled && MattyFixesInstalled && !Effects.GetConfigMF(0))
            {
                Plugin.logger.LogError("OutOfBounds.Enabled config in MattyFixes needs to be enabled !");
                compatibilityValid = false;
                errorID = 6003;
            }
            else if (GeneralImprovementsInstalled && MattyFixesInstalled && Effects.GetConfigGI(1) == Effects.GetConfigMF(0))
            {
                Plugin.logger.LogError("FixItemsFallingThrough config in GeneralImprovements and OutOfBounds.Enabled config in MattyFixes are both " + (Effects.GetConfigMF(0) ? "enabled" : "disabled") + " ! You need one of the two to be enabled and the other to be disabled (I suggest to keep the one in GeneralImprovements).");
                compatibilityValid = false;
                errorID = 6004;
            }
            else if (GeneralImprovementsInstalled && !MattyFixesInstalled && (!Effects.GetConfigGI(0) || !Effects.GetConfigGI(1)) && (!DawnLibInstalled || Effects.GetConfigDL(0)))
            {
                Plugin.logger.LogError("FixItemsLoadingSameRotation and FixItemsFallingThrough configs in GeneralImprovements both needs to be enabled !");
                compatibilityValid = false;
                errorID = 6005;
            }
            if (!compatibilityValid)
            {
                if (registerPopup)
                    switch (errorID)
                    {
                        case 6000:
                            Effects.MenuPopupMessages.Add("Configs not valid. 'Item Saving' in DawnLib needs to stay false! (so the system will be enabled)\nError code: " + errorID);
                            break;
                        case 6001:
                            Effects.MenuPopupMessages.Add("An additional mod is required. Please install either GeneralImprovements OR MattyFixes before playing!\nError code: " + errorID);
                            break;
                        case 6002:
                            Effects.MenuPopupMessages.Add("Configs not valid. ShipPlaceablesCollide in GeneralImprovements needs to be enabled!\nError code: " + errorID);
                            break;
                        case 6003:
                            Effects.MenuPopupMessages.Add("Configs not valid. OutOfBounds.Enabled in MattyFixes needs to be enabled!\nError code: " + errorID);
                            break;
                        case 6004:
                            Effects.MenuPopupMessages.Add("Configs missmatch. FixItemsFallingThrough in GeneralImprovements and OutOfBounds.Enabled in MattyFixes can't be both enabled at the same time!\nError code: " + errorID);
                            break;
                        case 6005:
                            Effects.MenuPopupMessages.Add("Configs not valid. FixItemsLoadingSameRotation and FixItemsFallingThrough in GeneralImprovements both needs to be enabled!\nError code: " + errorID);
                            break;
                        default:
                            break;
                    }
                if (displayTip)
                    Effects.Message("SelfSortingStorage", "Mods conflicts detected ! Please check your logs. Error code: " + errorID, true);
            }
            return compatibilityValid;
        }
    }
}
