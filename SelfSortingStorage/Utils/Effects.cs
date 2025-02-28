using GameNetcodeStuff;
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
            HUDManager.Instance.DisplayTip(title, bottom, warning);
        }

        public static bool IsTriggerValid(PlayerControllerB player, out string notValidText)
        {
            notValidText = "[Nothing to store]";
            if (player.isHoldingObject && !player.isGrabbingObjectAnimation && player.currentlyHeldObjectServer != null)
            {
                if (player.currentlyHeldObjectServer.itemProperties.itemName == "Body")
                {
                    notValidText = "[Bodies not allowed]";
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

        public static ItemNetworkReference SpawnItem(Item item, Vector3 position, Quaternion rotation, Transform? parent = null, int value = 0, int save = 0)
        {
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

        public static IEnumerator SyncItem(NetworkObjectReference itemRef, int value, int save, Transform? parent = null)
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
            component.isInElevator = true;
            component.isInShipRoom = true;
            if (component.itemProperties.isScrap)
                component.SetScrapValue(value);
            if (component.itemProperties.saveItemVariable)
                component.LoadItemSaveData(save);
            component.fallTime = 0f;
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
            var collider = component.GetComponent<BoxCollider>();
            if (collider == null || size == null)
                return;
            var volume = (size.x * 2) * (size.y * 2) * (size.z * 2);
            if (volume > 0.08f)
            {
                component.transform.localScale = (volume < 1f ? (0.07f * 100 / volume) : 20) * component.transform.localScale / 100;
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
    }
}
