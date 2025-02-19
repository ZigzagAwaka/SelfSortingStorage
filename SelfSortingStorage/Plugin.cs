using BepInEx;
using BepInEx.Logging;
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

            string assetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "premiumscraps");
            AssetBundle bundle = AssetBundle.LoadFromFile(assetDir);

            string directory = "Assets/Data/";

            var prefabs = new string[] { "Controller/ControlledAntena.prefab", "Controller/ControlledUI.prefab" };

            var audios = new string[] { "AirHorn1.ogg", "friendship_ends_here.wav", "scroll_tp.wav", "ShovelReelUp.ogg",
                "ShovelSwing.ogg", "wooden-staff-hit.wav", "MineTrigger.ogg", "book_page.wav", "CVuse1.wav", "CVuse2.wav",
                "CVuse3.wav", "CVuse4.wav", "TerminalAlarm.ogg", "Breathing.wav", "huh.wav", "book_use_redesign.wav",
                "uwu.wav", "uwu-rot.wav", "drink.wav", "spanishsound.wav", "arthas.wav", "glass-grab.wav", "glass-drop.wav",
                "beam.wav", "ControlModeStart.wav", "ControlModeStop.wav", "FlashlightOutOfBatteries.ogg", "ControlledOn.wav",
                "ControlledOff.wav", "controller-alert.wav", "LightningStrike2.ogg", "KingAAAAAAHHH.wav", "card-evil.wav",
                "card-evil2.wav", "card-boo.wav", "card-common-fr.wav", "card-evil-fr.wav", "card-evil2-fr.wav",
                "book_open_square_steel-fr.wav", "book_close_eco_friendly-fr.wav", "book_use_redesign-fr.wav", "KingBoyFR.wav",
                "KingHmmFR.wav", "KingDinnerFR.wav", "KingHelpFR.wav", "KingPeaceFR.wav", "KingSaveFR.wav", "KingShipFR.wav"
            };

            foreach (string prefab in prefabs)
            {
                gameObjects.Add(bundle.LoadAsset<GameObject>(directory + prefab));
            }

            foreach (string sfx in audios)
            {
                audioClips.Add(bundle.LoadAsset<AudioClip>(directory + "_audio/" + sfx));
            }

            var scraps = new List<Scrap> {
                new Scrap("Frieren/FrierenItem.asset", 10),
                new Scrap("Chocobo/ChocoboItem.asset", 10),
                new Scrap("AinzOoalGown/AinzOoalGownItem.asset", 5),
                new Scrap("HelmDomination/HelmDominationItem.asset", 11, 11),
                new Scrap("TheKing/TheKingItem.asset", 13, 14),
                new Scrap("HarryMason/HarryMasonItem.asset", 10, 9),
                new Scrap("Cristal/CristalItem.asset", 10),
                new Scrap("PuppyShark/PuppySharkItem.asset", 10),
                new Scrap("Rupee/RupeeItem.asset", 15),
                new Scrap("EaNasir/EaNasirItem.asset", 9),
                new Scrap("HScard/HSCardItem.asset", 9, 16),
                new Scrap("SODA/SODAItem.asset", 8),
                new Scrap("Spoon/SpoonItem.asset", 13),
                new Scrap("Crouton/CroutonItem.asset", 6),
                new Scrap("AirHornCustom/AirHornCustomItem.asset", 11, 1),
                new Scrap("Balan/BalanItem.asset", 10),
                new Scrap("CustomFace/CustomFaceItem.asset", 8, 2),
                new Scrap("Scroll/ScrollItem.asset", 7, 3),
                new Scrap("Stick/StickItem.asset", 9, 4),
                new Scrap("BookCustom/BookCustomItem.asset", 11, 5),
                new Scrap("SquareSteel/SquareSteelItem.asset", 7, 13),
                new Scrap("DarkJobApplication/JobApplicationItem.asset", 8, 6),
                new Scrap("Moogle/MoogleItem.asset", 10),
                new Scrap("Gazpacho/GazpachoItem.asset", 9, 7),
                new Scrap("Abi/AbiItem.asset", 4, 8),
                new Scrap("Bomb/BombItem.asset", 12, 10),
                new Scrap("Controller/ControllerItem.asset", 8, 12),
                new Scrap("Ronka/RonkaItem.asset", 10, 15)
            };

            int i = 0; config = new Config(base.Config, scraps);
            config.SetupCustomConfigs();
            Lang.Load(Logger, Application.systemLanguage, config.languageMode.Value);
            SetupNetwork();

            foreach (Scrap scrap in scraps)
            {
                Item item = bundle.LoadAsset<Item>(directory + scrap.asset);
                if (config.scrapValues[i].Item1 != -1) { item.minValue = config.scrapValues[i].Item1; item.maxValue = config.scrapValues[i].Item2; }
                if (scrap.behaviourId != 0) LoadItemBehaviour(item, scrap.behaviourId);
                SpecialEvent.LoadSpecialEvent(item.spawnPrefab);
                NetworkPrefabs.RegisterNetworkPrefab(item.spawnPrefab);
                Utilities.FixMixerGroups(item.spawnPrefab);
                Items.RegisterScrap(item, config.entries[i++].Value, Levels.LevelTypes.All);
            }

            HarmonyPatchAll();
            logger.LogInfo("SelfSortingStorage is loaded !");
        }
    }
}
