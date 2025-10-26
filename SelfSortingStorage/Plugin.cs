using BepInEx;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using SelfSortingStorage.Utils;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace SelfSortingStorage
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("evaisa.lethallib", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.github.teamxiaolan.dawnlib", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("ShaosilGaming.GeneralImprovements", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("mattymatty.MattyFixes", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("Toybox.LittleCompany", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        const string GUID = "zigzag.SelfSortingStorage";
        const string NAME = "SelfSortingStorage";
        const string VERSION = "1.4.1";

        public static Plugin instance;
        public static ManualLogSource logger;
        private readonly Harmony harmony = new Harmony(GUID);
        internal static Config config { get; private set; } = null!;
        internal const string VANILLA_NAME = "LethalCompanyGame";
        internal int ROWS_LENGTH { get; private set; } = 4;
        internal readonly static List<(System.Func<PlayerControllerB, bool>, string)> spTriggerValidations = new List<(System.Func<PlayerControllerB, bool>, string)>();

        void HarmonyPatchAll()
        {
            harmony.CreateClassProcessor(typeof(SavingPatchVanilla), true).Patch();

            harmony.CreateClassProcessor(typeof(StartOfRoundPatch), true).Patch();
            harmony.CreateClassProcessor(typeof(RoundManagerPatch), true).Patch();
            harmony.CreateClassProcessor(typeof(BeltBagItemPatch), true).Patch();
            harmony.CreateClassProcessor(typeof(MenuManagerPatch), true).Patch();

            if (Compatibility.LittleCompanyInstalled)
                harmony.CreateClassProcessor(typeof(ShipBuildModeManagerLittleCompanyPatch), true).Patch();
            else
                harmony.CreateClassProcessor(typeof(ShipBuildModeManagerPatch), true).Patch();
        }

        private void ReplaceTransform(GameObject prefab, string originName, string destinationName)
        {
            var originTransform = prefab.transform.Find(originName);
            var destinationTransform = prefab.transform.Find(destinationName);
            if (originTransform != null && destinationTransform != null)
            {
                originTransform.position = destinationTransform.position;
                originTransform.rotation = destinationTransform.rotation;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051")]
        void Awake()
        {
            instance = this;
            logger = Logger;

            string assetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "selfsortingstorage");
            AssetBundle bundle = AssetBundle.LoadFromFile(assetDir);

            string directory = "Assets/Data/_Misc/";
            var sssUnlockable = bundle.LoadAsset<UnlockablesList>(directory + "SSS_Module/SSSModuleUnlockableList.asset").unlockables[0];

            config = new Config(Config);
            config.SetupCustomConfigs();
            Effects.SetupNetwork();

            if (!Compatibility.LethalLibInstalled && !Compatibility.DawnLibInstalled)
            {
                throw new FileNotFoundException("SelfSortingStorage could not be loaded due to a missing dependency.");
            }

            if (config.wideVersion.Value)
            {
                var widePrefab = bundle.LoadAsset<GameObject>(directory + "SSS_Module/SSS_Module_WideVariant.prefab");
                sssUnlockable.prefabObject = widePrefab;
                ROWS_LENGTH = 7;
            }

            var sssPrefab = sssUnlockable.prefabObject;
            if (ColorUtility.TryParseHtmlString(config.cupboardColor.Value, out var customColor))
                sssPrefab.GetComponent<MeshRenderer>().materials[0].color = customColor;
            if (!config.scanNode.Value)
            {
                var scanNode = sssPrefab.transform.Find("ChutePosition/ActualPos/ScanNode");
                scanNode?.gameObject.SetActive(false);
            }
            if (!config.resetButton.Value)
            {
                var resetButton = sssPrefab.transform.Find("DeathButtonPosition");
                resetButton?.gameObject.SetActive(false);
            }
            if (!config.cozyLights.Value)
            {
                var lights = sssPrefab.transform.Find("Lights");
                lights?.gameObject.SetActive(false);
            }
            if (config.boxPosition.Value == "R")
            {
                ReplaceTransform(sssPrefab, "ChutePosition/ActualPos", "ChutePosition/Pos2");
                if (config.resetButton.Value)
                {
                    ReplaceTransform(sssPrefab, "DeathButtonPosition/ActualPos", "DeathButtonPosition/Pos2");
                }
                if (config.cozyLights.Value)
                {
                    ReplaceTransform(sssPrefab, "Lights/LightsButtonPosition/ActualPos", "Lights/LightsButtonPosition/Pos2");
                }
            }

            if (Compatibility.DawnLibInstalled)
            {
                Effects.DawnLibRegisterSSS(sssUnlockable);
            }
            else if (Compatibility.LethalLibInstalled)
            {
                Effects.LethalLibRegisterSSS(sssUnlockable);
            }

            HarmonyPatchAll();
            logger.LogInfo("SelfSortingStorage is loaded !");
        }
    }
}
