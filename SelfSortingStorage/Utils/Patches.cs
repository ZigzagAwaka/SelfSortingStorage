using HarmonyLib;
using SelfSortingStorage.Cupboard;
using Unity.Netcode;
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
                        SmartMemory.Instance.Size--;
                        cupboard.placedItems.Remove(spawnIndex);
                    }
                    spawnIndex++;
                }
            }
            if (Plugin.config.verboseLogging.Value)
                Plugin.logger.LogInfo("Smart Cupboard stored scraps were removed due to all players being dead.");
        }
    }


    [HarmonyPatch(typeof(BeltBagItem))]
    internal class BeltBagItemPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("TryAddObjectToBagServerRpc")]
        public static bool TryAddObjectToBagServerRpcPatch(BeltBagItem __instance, NetworkObjectReference netObjectRef, int playerWhoAdded)
        {
            if (StartOfRound.Instance == null || !StartOfRound.Instance.IsServer || SmartMemory.Instance == null || SmartMemory.Instance.Size == 0)
                return true;
            var cupboard = Object.FindObjectOfType<SmartCupboard>();
            if (cupboard == null || cupboard.placedItems.Count == 0)
                return true;
            if (netObjectRef.TryGet(out var networkObject))
            {
                var component = networkObject.GetComponent<GrabbableObject>();
                if (component != null && !component.isHeld && !component.heldByPlayerOnServer && !component.isHeldByEnemy)
                {
                    foreach (var (_, item) in cupboard.placedItems)
                    {
                        if (component.itemProperties.itemName == item.itemProperties.itemName && component.transform.position == item.transform.position)
                        {
                            __instance.CancelAddObjectToBagClientRpc(playerWhoAdded);
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}
