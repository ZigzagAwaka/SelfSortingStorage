using HarmonyLib;
using SelfSortingStorage.Cupboard;

namespace SelfSortingStorage.Utils
{
    [HarmonyPatch(typeof(GameNetworkManager))]
    internal class GameNetworkManagerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("SaveItemsInShip")]
        public static void SaveSmartCupboard()
        {
            if (StartOfRound.Instance != null && !StartOfRound.Instance.IsServer)
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
            if (StartOfRound.Instance != null && !StartOfRound.Instance.IsServer)
                return;
            string saveFile = GameNetworkManager.Instance.currentSaveFileName;
            SavingModule.Load(saveFile);
        }

        [HarmonyPostfix]
        [HarmonyPatch("ResetShip")]
        public static void ResetSmartCupboard()
        {
            if ((StartOfRound.Instance != null && !StartOfRound.Instance.IsServer) || (SmartMemory.Instance == null || SmartMemory.Instance.Size == 0))
                return;
            SmartMemory.Instance.Clear();
        }
    }


    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("DespawnPropsAtEndOfRound")]
        public static void ResetSmartCupboardIfAllDeads()
        {
            if ((RoundManager.Instance != null && !RoundManager.Instance.IsServer) || (SmartMemory.Instance == null || SmartMemory.Instance.Size == 0))
                return;
            if (!StartOfRound.Instance.allPlayersDead)
                return;
            SmartMemory.Instance.Clear();
        }
    }
}
