using SelfSortingStorage.Cupboard;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

            public ItemNetworkReference(NetworkObjectReference netObjectRef, int value)
            {
                this.netObjectRef = netObjectRef;
                this.value = value;
            }
        }


        public static void Message(string title, string bottom, bool warning = false)
        {
            HUDManager.Instance.DisplayTip(title, bottom, warning);
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

        public static ItemNetworkReference Spawn(Item item, Vector3 position, Quaternion rotation, Transform? parent = null)
        {
            if (parent == null)
                parent = RoundManager.Instance.spawnedScrapContainer ?? StartOfRound.Instance.elevatorTransform;
            GameObject gameObject = Object.Instantiate(item.spawnPrefab, position, rotation, parent);
            GrabbableObject component = gameObject.GetComponent<GrabbableObject>();
            component.transform.rotation = Quaternion.Euler(component.itemProperties.restingRotation);
            component.transform.localRotation = rotation;
            component.fallTime = 1f;
            component.hasHitGround = true;
            component.reachedFloorTarget = true;
            if (item.isScrap)
                component.scrapValue = (int)(Random.Range(item.minValue, item.maxValue) * RoundManager.Instance.scrapValueMultiplier);
            component.NetworkObject.Spawn();
            return new ItemNetworkReference(gameObject.GetComponent<NetworkObject>(), component.scrapValue);
        }

        public static IEnumerator SyncItem(NetworkObjectReference itemRef, int value)
        {
            NetworkObject itemNetObject = null;
            float startTime = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - startTime < 8f && !itemRef.TryGet(out itemNetObject))
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
            if (component.itemProperties.isScrap)
                component.SetScrapValue(value);
            component.fallTime = 0f;
        }
    }
}
