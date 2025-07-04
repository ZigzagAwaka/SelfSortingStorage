﻿using GameNetcodeStuff;
using SelfSortingStorage.Cupboard;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;

namespace SelfSortingStorage.Utils
{
    internal class Effects
    {
        public class ItemNetworkReference
        {
            public NetworkObjectReference netObjectRef;
            public int value;
            public int save;

            public ItemNetworkReference(NetworkObjectReference netObjectRef, int value, int save)
            {
                this.netObjectRef = netObjectRef;
                this.value = value;
                this.save = save;
            }
        }

        public static readonly List<string> MenuPopupMessages = new List<string>();

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

        public static void Message(string title, string bottom, bool warning = false)
        {
            HUDManager.Instance?.DisplayTip(title, bottom, warning);
        }

        public static bool GetConfigGI(int wantedConfigID)
        {
            return wantedConfigID switch
            {
                0 => GeneralImprovements.Plugin.FixItemsLoadingSameRotation.Value,
                1 => GeneralImprovements.Plugin.FixItemsFallingThrough.Value,
                2 => GeneralImprovements.Plugin.ShipPlaceablesCollide.Value,
                _ => false,
            };
        }

        public static bool GetConfigMF(int wantedConfigID)
        {
            return wantedConfigID switch
            {
                0 => MattyFixes.MattyFixes.PluginConfig.OutOfBounds.Enabled.Value,
                _ => false,
            };
        }

        public static void SetScreenText(int screenID, string text)
        {
            GeneralImprovements.API.MonitorsAPI.GetMonitorAtIndex(screenID).QueueRender(text);
        }

        public static IEnumerator FadeOutAudio(AudioSource source, float time, bool specialStop = false)
        {
            yield return new WaitForEndOfFrame();
            var volume = source.volume;
            while (source.volume > 0)
            {
                source.volume -= volume * Time.deltaTime / time;
                if (specialStop && source.volume <= 0.04f)
                    break;
                yield return null;
            }
            source.Stop();
            source.volume = volume;
        }

        public static bool IsTriggerValid(PlayerControllerB player, out string notValidText)
        {
            notValidText = "[Nothing to store]";
            if (player.isHoldingObject && !player.isGrabbingObjectAnimation && player.currentlyHeldObjectServer != null)
            {
                if (!StartOfRound.Instance.inShipPhase && (StartOfRound.Instance.shipIsLeaving || !StartOfRound.Instance.shipHasLanded))
                {
                    notValidText = "[Wait for ship to " + (StartOfRound.Instance.shipIsLeaving ? "leave" : "land") + "]";
                    return false;
                }
                if (player.currentlyHeldObjectServer.itemProperties.itemName == "Body")
                {
                    notValidText = "[Bodies not allowed]";
                    return false;
                }
                if (player.currentlyHeldObjectServer.itemProperties.itemName == "Maneater")
                {
                    notValidText = "[Good try, but nope]";
                    return false;
                }
                if (player.currentlyHeldObjectServer.itemProperties.itemName == "Stun grenade" && player.currentlyHeldObjectServer is StunGrenadeItem grenade && grenade.hasExploded)
                {
                    notValidText = "[Exploded grenades not allowed]";
                    return false;
                }
                if (player.currentlyHeldObjectServer.itemProperties.itemName == "Belt bag")
                {
                    notValidText = "[Belt bags not compatible]";
                    return false;
                }
                if (!Plugin.config.allowScrapItems.Value && player.currentlyHeldObjectServer.itemProperties.isScrap)
                {
                    notValidText = "[Scraps not allowed]";
                    return false;
                }
                foreach (var triggerValidation in Plugin.spTriggerValidations)
                {
                    if (!triggerValidation.Item1.Invoke(player))
                    {
                        notValidText = triggerValidation.Item2;
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public static Item? GetItem(string id)
        {
            var idParts = id.Split('/');
            if (idParts == null || idParts.Length <= 1)
                return null;
            if (idParts[0] == Plugin.VANILLA_NAME)
                return StartOfRound.Instance.allItemsList.itemsList.FirstOrDefault(i => i.itemName.Equals(idParts[1]));
            else
                return SmartMemory.CacheItems.GetValueOrDefault(id);
        }

        public static ItemNetworkReference SpawnItem(Item item, SmartCupboard cupboard, int spawnIndex, int value = 0, int save = 0)
        {
            cupboard.GetPlacePosition(spawnIndex, out var parent, out var position, out var rotation);
            GameObject gameObject = Object.Instantiate(item.spawnPrefab, position, rotation, parent ?? StartOfRound.Instance.elevatorTransform);
            GrabbableObject component = gameObject.GetComponent<GrabbableObject>();
            component.transform.rotation = Quaternion.Euler(component.itemProperties.restingRotation);
            component.transform.localRotation = rotation;
            component.fallTime = 1f;
            component.hasHitGround = true;
            component.reachedFloorTarget = true;
            component.isInElevator = true;
            component.isInShipRoom = true;
            if (component.itemProperties.isScrap)
                component.SetScrapValue(value);
            if (component.itemProperties.saveItemVariable)
                component.LoadItemSaveData(save);
            component.NetworkObject.Spawn();
            return new ItemNetworkReference(gameObject.GetComponent<NetworkObject>(), value, component.itemProperties.saveItemVariable ? component.GetItemDataToSave() : save);
        }

        public static IEnumerator SyncItem(NetworkObjectReference itemRef, SmartCupboard cupboard, int spawnIndex, int value, int save)
        {
            NetworkObject? itemNetObject = null;
            float startTime = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - startTime < 8f && !itemRef.TryGet(out itemNetObject))  // wait for item to spawn
            {
                yield return new WaitForSeconds(0.03f);
            }
            if (itemNetObject == null)
            {
                Plugin.logger.LogError("Error while trying to sync the item.");
                yield break;
            }
            yield return new WaitForEndOfFrame();
            GrabbableObject component = itemNetObject.GetComponent<GrabbableObject>();
            if (component.IsServer)
            {
                var targetPosition = Vector3.zero + component.itemProperties.verticalOffset * new Vector3(0, 0, 1);
                component.transform.localPosition = targetPosition;
                component.targetFloorPosition = targetPosition;
            }
            else
            {
                cupboard.GetPlacePosition(spawnIndex, out var parent, out _, out _);
                var targetPosition = Vector3.zero + component.itemProperties.verticalOffset * new Vector3(0, 0, 1);
                component.parentObject = null;
                component.transform.SetParent(parent ?? StartOfRound.Instance.elevatorTransform, worldPositionStays: true);
                component.startFallingPosition = targetPosition;
                component.transform.localPosition = targetPosition;
                component.targetFloorPosition = targetPosition;
                component.reachedFloorTarget = true;
                component.hasHitGround = true;
            }
            component.isInElevator = true;
            component.isInShipRoom = true;
            component.fallTime = 0f;
            if (component.itemProperties.isScrap)
                component.SetScrapValue(value);
            if (component.itemProperties.saveItemVariable)
                component.LoadItemSaveData(save);
        }

        private static bool ShouldRescale(Vector3 size, out float factor)
        {
            factor = 100f;
            if (Plugin.config.perfectRescale.Value)
            {
                var max = size.x > size.y && size.x > size.z ? size.x : (size.y > size.x && size.y > size.z ? size.y : size.z);
                if (max > 0.25f)
                {
                    factor = 0.21f * 100 / max;  // factor% of the original max size
                    return true;
                }
            }
            else
            {
                var volume = (size.x * 2) * (size.y * 2) * (size.z * 2);
                if (volume > 0.08f)
                {
                    factor = volume < 0.5f ? (0.07f * 100 / volume) : 20;  // factor% of the original volume
                    return true;
                }
            }
            return false;
        }

        public static void RescaleItemIfTooBig(GrabbableObject component)
        {
            var collider = component.GetComponent<BoxCollider>();
            if (collider == null)
                return;
            RescaleItemIfTooBig(component, collider.bounds.extents);
        }

        public static void RescaleItemIfTooBig(GrabbableObject component, Vector3 size)
        {
            if (size == null)
                return;
            if (ShouldRescale(size, out var factor))
            {
                component.transform.localScale = factor * component.transform.localScale / 100;
                var targetScaledPosition = Vector3.zero + (factor * component.itemProperties.verticalOffset / 100) * new Vector3(0, 0, 1);
                component.transform.localPosition = targetScaledPosition;
                component.targetFloorPosition = targetScaledPosition;
                if (Plugin.config.verboseLogging.Value)
                    Plugin.logger.LogInfo("Item was rescaled");
            }
        }

        public static void ScaleBackItem(GrabbableObject component)
        {
            component.transform.localScale = component.originalScale;
        }

        public static void OverrideOriginalScale(GrabbableObject component, Vector3 value)
        {
            component.originalScale = value;
        }

        public static void ReParentItemToCupboard(GrabbableObject component, SmartCupboard cupboard, int spawnIndex, bool overridePosRot = false, Vector3 syncedPos = default, Quaternion syncedRot = default)
        {
            if (spawnIndex < 0)
                return;
            cupboard.GetPlacePosition(spawnIndex, out var parent, out _, out _);
            Vector3 targetPosition = overridePosRot ? syncedPos : Vector3.zero + component.itemProperties.verticalOffset * new Vector3(0, 0, 1);
            component.parentObject = null;
            component.transform.SetParent(parent ?? StartOfRound.Instance.elevatorTransform, worldPositionStays: true);
            component.transform.localPosition = targetPosition;
            component.targetFloorPosition = targetPosition;
            if (overridePosRot)
            {
                component.transform.localRotation = syncedRot;
            }
            component.reachedFloorTarget = true;
            component.hasHitGround = true;
        }
    }
}
