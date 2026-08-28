using HarmonyLib;
using SelfSortingStorage.Cupboard;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace SelfSortingStorage.Utils
{
    [HarmonyPatch]
    internal class SavingPatchVanilla
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameNetworkManager), "SaveItemsInShip")]
        public static void SaveSmartCupboard()
        {
            if (!Plugin.config.enableSaving.Value || StartOfRound.Instance == null || !StartOfRound.Instance.IsServer)
                return;
            string saveFile = GameNetworkManager.Instance.currentSaveFileName;
            SavingModule.Save(saveFile);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(StartOfRound), "LoadShipGrabbableItems")]
        public static void LoadSmartCupboard()
        {
            if (!Plugin.config.enableSaving.Value || StartOfRound.Instance == null || !StartOfRound.Instance.IsServer)
                return;
            string saveFile = GameNetworkManager.Instance.currentSaveFileName;
            SavingModule.Load(saveFile);
        }
    }


    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void SetSmartCupboardDefaultScreen()
        {
            if (!SmartCupboard.SpawnedInShip && Compatibility.GeneralImprovementsInstalled && Plugin.config.customScreenPos.Value > 0 && Plugin.config.customScreenPos.Value <= 14)
                Effects.SetScreenText(Plugin.config.customScreenPos.Value - 1, $"<color=#ffff00>{"Smart Cupboard:\n$" + Plugin.config.cupboardPrice.Value}</color>");
        }

        [HarmonyPostfix]
        [HarmonyPatch("ResetShip")]
        public static void ResetSmartCupboard()
        {
            if (StartOfRound.Instance == null || !StartOfRound.Instance.IsServer || SmartMemory.Instance == null || SmartMemory.Instance.Size == 0)
                return;
            if (Plugin.config.preventMemoryReset.Value)
                return;
            SmartCupboard cupboard = Object.FindObjectOfType<SmartCupboard>();
            if (cupboard == null)
                return;
            SmartMemory.Instance.ClearAll();
            cupboard.placedItems.Clear();
            if (Plugin.config.verboseLogging.Value)
                Plugin.logger.LogInfo("Smart Cupboard was reseted due to a game over.");
        }

        [HarmonyPostfix]
        [HarmonyPatch("GetValueOfAllScrap")]
        public static void GetValueOfAllScrapInCupboard(ref int __result, bool onlyNewScrap)
        {
            if (StartOfRound.Instance == null || !StartOfRound.Instance.IsServer || SmartMemory.Instance == null || SmartMemory.Instance.Size == 0)
                return;
            int spawnIndex = 0;
            SmartCupboard cupboard = Object.FindObjectOfType<SmartCupboard>();
            if (cupboard == null)
                return;
            foreach (var list in SmartMemory.Instance.ItemList)
            {
                foreach (var itemData in list)
                {
                    if (itemData.IsValid() && cupboard.placedItems.TryGetValue(spawnIndex, out var itemObject) && itemObject.itemProperties.isScrap)
                    {
                        for (int i = 1; i < itemData.Quantity; i++) // starts at 1 because the first item's value in the stack is already got in vanilla!
                        {
                            if (!onlyNewScrap || !itemData.States[i])
                                __result += itemData.Values[i];
                        }
                    }
                    spawnIndex++;
                }
            }
        }
    }


    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch
    {
        /// <summary>
        /// Manages removal of scraps if all players are dead, and manages
        /// the pre-set of the persistent state of items otherwise
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch("DespawnPropsAtEndOfRound")]
        public static void EndOfRoundCupboardEffects(bool despawnAllItems)
        {
            if (RoundManager.Instance == null || !RoundManager.Instance.IsServer || StartOfRound.Instance == null || SmartMemory.Instance == null || SmartMemory.Instance.Size == 0)
                return;
            int spawnIndex = 0;
            bool shouldResetScraps = StartOfRound.Instance.allPlayersDead && !Plugin.config.preventMemoryReset.Value;
            SmartCupboard cupboard = Object.FindObjectOfType<SmartCupboard>();
            if (cupboard == null)
                return;
            foreach (var list in SmartMemory.Instance.ItemList)
            {
                foreach (var itemData in list)
                {
                    if (itemData.IsValid())
                    {
                        if (despawnAllItems || (shouldResetScraps && cupboard.placedItems.TryGetValue(spawnIndex, out var itemObject) && itemObject.itemProperties.isScrap))
                        {
                            itemData.Id = "INVALID";
                            SmartMemory.Instance.Size--;
                            cupboard.placedItems.Remove(spawnIndex);
                        }
                        else
                        {
                            itemData.States.Clear();
                            itemData.States.AddRange(Enumerable.Repeat(true, itemData.Quantity));
                        }
                    }
                    spawnIndex++;
                }
            }
            if (shouldResetScraps && Plugin.config.verboseLogging.Value)
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


    [HarmonyPatch(typeof(LittleCompany.patches.ShipBuildModeManagerPatch))]
    internal class ShipBuildModeManagerLittleCompanyPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("CreateGhostObjectAndHighlight_PostFix")]
        public static bool CreateGhostObjectAndHighlightPatch(object[] __args)
        {
            var buildModeManager = (ShipBuildModeManager)__args[0];
            if (buildModeManager.placingObject == null || buildModeManager.placingObject.parentObject.name != "SSS_Module_WideVariant(Clone)")
                return true;
            LittleCompany.modifications.ShipObjectModification.ScalingOf(buildModeManager.placingObject);
            var scale = buildModeManager.placingObject.mainMesh.transform.localScale;
            buildModeManager.ghostObjectMesh.transform.localScale = scale;
            buildModeManager.selectionOutlineMesh.transform.localScale = scale * 1.04f;
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