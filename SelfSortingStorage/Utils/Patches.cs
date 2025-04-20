using HarmonyLib;
using SelfSortingStorage.Cupboard;
using TMPro;
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
        [HarmonyPatch("Start")]
        public static void SetSmartCupboardDefaultScreen()
        {
            if (!SmartCupboard.SpawnedInShip && Plugin.config.GeneralImprovementsInstalled && Plugin.config.customScreenPos.Value > 0 && Plugin.config.customScreenPos.Value <= 14)
                Effects.SetScreenText(Plugin.config.customScreenPos.Value - 1, $"<color=#ffff00>{"Smart Cupboard:\n$" + Plugin.config.cupboardPrice.Value}</color>");
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


    [HarmonyPatch(typeof(ShipBuildModeManager))]
    internal class ShipBuildModeManagerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("CreateGhostObjectAndHighlight")]
        public static bool CreateGhostObjectAndHighlightPatch(ShipBuildModeManager __instance)
        {
            if (__instance.placingObject == null || __instance.placingObject.parentObject.name != "SSS_Module_WideVariant(Clone)")
                return true;
            ((Behaviour)(object)HUDManager.Instance.buildModeControlTip).enabled = true;
            if (StartOfRound.Instance.localPlayerUsingController)
                HUDManager.Instance.buildModeControlTip.text = "Confirm: [Y]   |   Rotate: [L-shoulder]   |   Store: [B]";
            else
                HUDManager.Instance.buildModeControlTip.text = "Confirm: [B]   |   Rotate: [R]   |   Store: [X]";
            HUDManager.Instance.UIAudio.PlayOneShot(__instance.beginPlacementSFX);
            __instance.ghostObject.transform.eulerAngles = __instance.placingObject.mainMesh.transform.eulerAngles;
            __instance.ghostObjectMesh.mesh = __instance.placingObject.mainMesh.mesh;
            __instance.ghostObjectMesh.transform.localScale = __instance.placingObject.mainMesh.transform.localScale;
            __instance.ghostObjectMesh.transform.position = __instance.ghostObject.position + (__instance.placingObject.mainMesh.transform.position - __instance.placingObject.placeObjectCollider.transform.position);
            __instance.ghostObjectMesh.transform.localEulerAngles = Vector3.zero;
            __instance.ghostObjectRenderer.enabled = true;
            __instance.selectionOutlineMesh.mesh = __instance.placingObject.mainMesh.mesh;
            __instance.selectionOutlineMesh.transform.localScale = __instance.placingObject.mainMesh.transform.localScale;
            __instance.selectionOutlineMesh.transform.localScale = __instance.selectionOutlineMesh.transform.localScale * 1.04f;
            __instance.selectionOutlineMesh.transform.position = __instance.placingObject.mainMesh.transform.position;
            __instance.selectionOutlineMesh.transform.eulerAngles = __instance.placingObject.mainMesh.transform.eulerAngles;
            __instance.selectionOutlineRenderer.enabled = true;
            return false;
        }
    }


    [HarmonyPatch(typeof(MenuManager))]
    internal class MenuManagerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("Awake")]
        public static void AwakePatch()
        {
            foreach (var message in Effects.MenuPopupMessages)
            {
                var menuContainer = GameObject.Find("/Canvas/MenuContainer/");
                var lanPopup = GameObject.Find("Canvas/MenuContainer/LANWarning/");
                if (lanPopup == null)
                    return;
                var newPopup = Object.Instantiate(lanPopup, menuContainer.transform);
                newPopup.name = "SSS_ModsIncompatibility";
                newPopup.SetActive(true);
                var textHolder = newPopup.transform.Find("Panel/NotificationText");
                var textMesh = textHolder.GetComponent<TextMeshProUGUI>();
                textMesh.text = message;
            }
        }
    }
}