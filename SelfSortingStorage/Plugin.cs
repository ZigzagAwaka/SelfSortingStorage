using BepInEx;
using BepInEx.Logging;
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
        const string VERSION = "0.1.0";

        public static Plugin instance;
        public ManualLogSource logger;
        internal static Config config { get; private set; } = null!;

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051")]
        void Awake()
        {
            instance = this;
            logger = Logger;

            string assetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "selfsortingstorage");
            AssetBundle bundle = AssetBundle.LoadFromFile(assetDir);

            string directory = "Assets/Data/_Misc/";
            var sssUnlockable = bundle.LoadAsset<UnlockableItemDef>(directory + "SSS_Module/SSSModuleUnlockable.asset");

            config = new Config(base.Config);
            config.SetupCustomConfigs();
            SetupNetwork();

            NetworkPrefabs.RegisterNetworkPrefab(sssUnlockable.unlockable.prefabObject);
            Utilities.FixMixerGroups(sssUnlockable.unlockable.prefabObject);
            Unlockables.RegisterUnlockable(sssUnlockable, 20, StoreType.ShipUpgrade);

            logger.LogInfo("SelfSortingStorage is loaded !");
        }
    }
}
