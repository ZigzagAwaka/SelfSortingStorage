using BepInEx;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using LethalLib.Extras;
using LethalLib.Modules;
using SelfSortingStorage.Utils;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace SelfSortingStorage
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("ShaosilGaming.GeneralImprovements", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        const string GUID = "zigzag.SelfSortingStorage";
        const string NAME = "SelfSortingStorage";
        const string VERSION = "1.0.5";

        public static Plugin instance;
        public static ManualLogSource logger;
        private readonly Harmony harmony = new Harmony(GUID);
        internal static Config config { get; private set; } = null!;
        internal const string VANILLA_NAME = "LethalCompanyGame";
        internal int ROWS_LENGTH { get; private set; } = 4;
        internal readonly static List<(System.Func<PlayerControllerB, bool>, string)> spTriggerValidations = new List<(System.Func<PlayerControllerB, bool>, string)>();

        void HarmonyPatchAll()
        {
            harmony.PatchAll();
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
            var sssUnlockable = bundle.LoadAsset<UnlockableItemDef>(directory + "SSS_Module/SSSModuleUnlockable.asset");

            config = new Config(Config);
            config.SetupCustomConfigs();
            Effects.SetupNetwork();

            if (config.wideVersion.Value)
            {
                var widePrefab = bundle.LoadAsset<GameObject>(directory + "SSS_Module/SSS_Module_WideVariant.prefab");
                sssUnlockable.unlockable.prefabObject = widePrefab;
                ROWS_LENGTH = 7;
            }

            var sssPrefab = sssUnlockable.unlockable.prefabObject;
            ColorUtility.TryParseHtmlString(config.cupboardColor.Value, out var customColor);
            sssPrefab.GetComponent<MeshRenderer>().materials[0].color = customColor;
            if (!config.resetButton.Value)
            {
                var resetButton = sssPrefab.transform.Find("DeathButtonPosition");
                resetButton?.gameObject.SetActive(false);
            }
            if (config.boxPosition.Value == "R")
            {
                ReplaceTransform(sssPrefab, "ChutePosition/ActualPos", "ChutePosition/Pos2");
                if (config.resetButton.Value)
                {
                    ReplaceTransform(sssPrefab, "DeathButtonPosition/ActualPos", "DeathButtonPosition/Pos2");
                }
            }

            NetworkPrefabs.RegisterNetworkPrefab(sssPrefab);
            Utilities.FixMixerGroups(sssPrefab);
            Unlockables.RegisterUnlockable(sssUnlockable, config.cupboardPrice.Value, StoreType.ShipUpgrade);

            HarmonyPatchAll();
            logger.LogInfo("SelfSortingStorage is loaded !");
        }
    }
}
