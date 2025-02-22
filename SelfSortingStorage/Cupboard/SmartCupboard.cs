using GameNetcodeStuff;
using SelfSortingStorage.Utils;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace SelfSortingStorage.Cupboard
{
    public class SmartCupboard : NetworkBehaviour
    {
        public NetworkObject parentObject;
        public InteractTrigger triggerScript;
        public Transform[] placePositions;
        public SmartMemory memory = new SmartMemory();
        private readonly Dictionary<int, GrabbableObject> placedItems = new Dictionary<int, GrabbableObject>();
        private readonly List<int> indexToRemove = new List<int>();

        public SmartCupboard() { }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
                memory.Initialize();
            else
                MemoryFullServerRpc();
        }

        public void Update()
        {
            if (GameNetworkManager.Instance != null && GameNetworkManager.Instance.localPlayerController != null)
            {
                if (memory.IsFull())
                {
                    triggerScript.interactable = false;
                    triggerScript.disabledHoverTip = "[Full!]";
                }
                else
                {
                    var player = GameNetworkManager.Instance.localPlayerController;
                    triggerScript.interactable = player.isHoldingObject && !player.isGrabbingObjectAnimation && player.currentlyHeldObjectServer != null;
                    if (!triggerScript.interactable)
                        triggerScript.disabledHoverTip = "[Nothing to store]";
                }
            }
            if (IsServer)
            {
                foreach (var (spawnIndex, item) in placedItems)
                {
                    if (item.isHeld || item.isHeldByEnemy)
                    {
                        var itemData = memory.RetrieveData(spawnIndex);
                        indexToRemove.Add(spawnIndex);
                        if (itemData != null && itemData.Quantity >= 1)
                        {
                            SpawnItem(itemData, spawnIndex, true);
                        }
                    }
                }
                if (indexToRemove.Count != 0)
                {
                    foreach (var index in indexToRemove)
                        placedItems.Remove(index);
                    indexToRemove.Clear();
                }
            }
        }

        public IEnumerator ReloadPlacedItems()
        {
            yield return new WaitForSeconds(1);  // wait for items to spawn
            int spawnIndex = 0;
            float distanceSearch = 1f;
            var array = FindObjectsByType<GrabbableObject>(FindObjectsSortMode.None);
            foreach (var list in memory.ItemList)
            {
                foreach (var item in list)
                {
                    if (item.IsValid())
                    {
                        for (int i = 0; i < array.Length; i++)
                        {
                            if (array[i].itemProperties.itemName == item.Id.Split('/')[1] &&
                                Vector3.Distance(placePositions[spawnIndex].position, array[i].transform.position) <= distanceSearch)
                            {
                                placedItems[spawnIndex] = array[i];
                            }
                        }
                    }
                    spawnIndex++;
                }
            }
        }

        public void StoreObject(PlayerControllerB player)
        {
            if (player == null || !player.isHoldingObject || player.isGrabbingObjectAnimation || player.currentlyHeldObjectServer == null)
            {
                return;
            }
            StoreDataServerRpc(player.playerClientId);
            player.DestroyItemInSlotAndSync(player.currentItemSlot);
        }

        private void GetPlacePosition(int spawnIndex, out Vector3 position, out Quaternion rotation)
        {
            position = placePositions[spawnIndex].position;
            rotation = placePositions[spawnIndex].rotation * Quaternion.Euler(0, 0, 180);
        }

        [ServerRpc(RequireOwnership = false)]
        private void StoreDataServerRpc(ulong playerId)
        {
            var player = StartOfRound.Instance.allPlayerScripts[playerId];
            if (player == null)
                return;
            var item = player.currentlyHeldObjectServer;
            if (item == null)
                return;
            var itemData = new SmartMemory.Data(item);
            var shouldSpawn = memory.StoreData(itemData, out int spawnIndex);
            if (memory.IsFull())
                MemoryFullClientRpc(true);
            if (shouldSpawn)
                SpawnItem(itemData, spawnIndex);
            else
            {
                if (placedItems.TryGetValue(spawnIndex, out var component))
                    PlayDropSFXClientRpc(component.gameObject.GetComponent<NetworkObject>());
            }
            Plugin.logger.LogWarning(memory.ToString());
        }

        private void SpawnItem(SmartMemory.Data itemData, int spawnIndex, bool isStacked = false)
        {
            if (!itemData.IsValid())
                return;
            var item = Effects.GetItem(itemData.Id);
            if (item != null)
            {
                GetPlacePosition(spawnIndex, out var position, out var rotation);
                var itemRef = Effects.Spawn(item, position, rotation, parentObject.transform);
                SyncItemClientRpc(itemRef.netObjectRef, itemRef.value, spawnIndex, isStacked);
            }
        }

        [ClientRpc]
        private void SyncItemClientRpc(NetworkObjectReference itemRef, int value, int spawnIndex, bool isStacked)
        {
            StartCoroutine(SyncItem(itemRef, value, spawnIndex, isStacked));
        }

        private IEnumerator SyncItem(NetworkObjectReference itemRef, int value, int spawnIndex, bool isStacked)
        {
            yield return Effects.SyncItem(itemRef, value);
            if (itemRef.TryGet(out var itemNetObject))
            {
                var component = itemNetObject.GetComponent<GrabbableObject>();
                if (!isStacked)
                    component.PlayDropSFX();
                if (IsServer)
                {
                    placedItems[spawnIndex] = component;
                }
            }
        }

        [ClientRpc]
        private void PlayDropSFXClientRpc(NetworkObjectReference itemRef)
        {
            if (itemRef.TryGet(out var itemNetObject))
            {
                itemNetObject.GetComponent<GrabbableObject>().PlayDropSFX();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void MemoryFullServerRpc()
        {
            MemoryFullClientRpc(memory.IsFull());
        }

        [ClientRpc]
        private void MemoryFullClientRpc(bool full)
        {
            memory.Size = full ? memory.Capacity : 0;
        }
    }
}
