using HarmonyLib;
using SelfSortingStorage.Cupboard;
using UnityEngine;

namespace SelfSortingStorage.Utils
{
    [HarmonyPatch(typeof(GameNetworkManager))]
    internal class GameNetworkManagerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("SaveItemsInShip")]
        public static void SaveSmartCupboard()
        {
            if (!Plugin.config.enableSaving.Value || StartOfRound.Instance == null || !StartOfRound.Instance.IsServer)
                return;
            string saveFile = GameNetworkManager.Instance.currentSaveFileName;
            SavingModule.Save(saveFile);
        }
    }


    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("LoadShipGrabbableItems")]
        public static void LoadSmartCupboard()
        {
            if (!Plugin.config.enableSaving.Value || StartOfRound.Instance == null || !StartOfRound.Instance.IsServer)
                return;
            string saveFile = GameNetworkManager.Instance.currentSaveFileName;
            SavingModule.Load(saveFile);
        }

        [HarmonyPostfix]
        [HarmonyPatch("ResetShip")]
        public static void ResetSmartCupboard()
        {
            if (StartOfRound.Instance == null || !StartOfRound.Instance.IsServer || SmartMemory.Instance == null || SmartMemory.Instance.Size == 0)
                return;
            var cupboard = Object.FindObjectOfType<SmartCupboard>();
            if (cupboard == null)
                return;
            SmartMemory.Instance.ClearAll();
            cupboard.placedItems.Clear();
            if (Plugin.config.verboseLogging.Value)
                Plugin.logger.LogInfo("Smart Cupboard was reseted due to a game over.");
        }
    }


    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("DespawnPropsAtEndOfRound")]
        public static void ResetSmartCupboardIfAllDeads()
        {
            if (RoundManager.Instance == null || !RoundManager.Instance.IsServer || SmartMemory.Instance == null || SmartMemory.Instance.Size == 0)
                return;
            if (StartOfRound.Instance == null || !StartOfRound.Instance.allPlayersDead)
                return;
            int spawnIndex = 0;
            var cupboard = Object.FindObjectOfType<SmartCupboard>();
            if (cupboard == null)
                return;
            foreach (var list in SmartMemory.Instance.ItemList)
            {
                foreach (var item in list)
                {
                    if (item.IsValid() && item.Values[0] != 0)
                    {
                        item.Id = "INVALID";
                        cupboard.placedItems.Remove(spawnIndex);
                    }
                    spawnIndex++;
                }
            }
            if (Plugin.config.verboseLogging.Value)
                Plugin.logger.LogInfo("Smart Cupboard stored scraps were removed due to all players being dead.");
        }
    }
}
