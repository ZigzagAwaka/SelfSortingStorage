using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LethalLib.Extras;
using LethalLib.Modules;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SelfSortingStorage
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        const string GUID = "zigzag.SelfSortingStorage";
        const string NAME = "SelfSortingStorage";
        const string VERSION = "1.0.0";

        public static Plugin instance;
        public static ManualLogSource logger;
        private readonly Harmony harmony = new Harmony(GUID);
        internal static Config config { get; private set; } = null!;
        public const string VANILLA_NAME = "LethalCompanyGame";

        public static void SetupNetwork()
        {
            IEnumerable<System.Type> types;
            try
            {
                types = Assembly.GetExecutingAssembly().GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types.Where(t => t != null);
            }
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }

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
            SetupNetwork();

            ColorUtility.TryParseHtmlString(config.cupboardColor.Value, out var customColor);
            sssUnlockable.unlockable.prefabObject.GetComponent<MeshRenderer>().materials[0].color = customColor;
            NetworkPrefabs.RegisterNetworkPrefab(sssUnlockable.unlockable.prefabObject);
            Utilities.FixMixerGroups(sssUnlockable.unlockable.prefabObject);
            Unlockables.RegisterUnlockable(sssUnlockable, config.cupboardPrice.Value, StoreType.ShipUpgrade);

            HarmonyPatchAll();
            logger.LogInfo("SelfSortingStorage is loaded !");
        }
    }
}
