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
        const string VERSION = "1.0.3";

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
            if (config.boxPosition.Value == "R")
            {
                var boxTransform = sssPrefab.transform.Find("ChutePosition/ActualPos");
                var pos2Transform = sssPrefab.transform.Find("ChutePosition/Pos2");
                if (boxTransform != null && pos2Transform != null)
                {
                    boxTransform.position = pos2Transform.position;
                    boxTransform.rotation = pos2Transform.rotation;
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
