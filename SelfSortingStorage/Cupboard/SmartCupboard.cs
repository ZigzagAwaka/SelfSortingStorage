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
        public readonly Dictionary<int, GrabbableObject> placedItems = new Dictionary<int, GrabbableObject>();
        private readonly List<int> indexToRemove = new List<int>();

        public SmartCupboard() { }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
                memory.Initialize();
            else
                StartCoroutine(SyncCupboard());
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
                    triggerScript.interactable = Effects.IsTriggerValid(GameNetworkManager.Instance.localPlayerController, out var notValidText);
                    if (!triggerScript.interactable)
                        triggerScript.disabledHoverTip = notValidText;
                }
            }
            if (IsServer)
            {
                foreach (var (spawnIndex, item) in placedItems)
                {
                    if (item.isHeld || item.isHeldByEnemy)
                    {
                        if (Plugin.config.rescaleItems.Value)
                            ScaleItemClientRpc(item.gameObject.GetComponent<NetworkObject>(), false);
                        var itemData = memory.RetrieveData(spawnIndex);
                        if (!memory.IsFull())
                            SetSizeClientRpc(memory.Size);
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

        public static void AddTriggerValidation(System.Func<PlayerControllerB, bool> func, string notValidText)
        {
            Plugin.spTriggerValidations.Add((func, notValidText));
        }

        public IEnumerator ReloadPlacedItems()  // used by SavingModule
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
                                if (Plugin.config.rescaleItems.Value)
                                    Effects.RescaleItemIfTooBig(array[i]);
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
                SetSizeClientRpc(memory.Size);
            if (shouldSpawn)
                SpawnItem(itemData, spawnIndex);
            else
            {
                if (placedItems.TryGetValue(spawnIndex, out var component))
                    PlayDropSFXClientRpc(component.gameObject.GetComponent<NetworkObject>());
            }
            if (Plugin.config.verboseLogging.Value)
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
                var itemRef = Effects.SpawnItem(item, position, rotation, parentObject.transform, itemData.Values[0], itemData.Saves[0]);
                SyncItemClientRpc(itemRef.netObjectRef, itemRef.value, itemRef.save, spawnIndex, isStacked);
            }
        }

        [ClientRpc]
        private void SyncItemClientRpc(NetworkObjectReference itemRef, int value, int save, int spawnIndex, bool isStacked)
        {
            StartCoroutine(SyncItem(itemRef, value, save, spawnIndex, isStacked));
        }

        private IEnumerator SyncItem(NetworkObjectReference itemRef, int value, int save, int spawnIndex, bool isStacked)
        {
            yield return Effects.SyncItem(itemRef, value, save);
            if (itemRef.TryGet(out var itemNetObject))
            {
                var component = itemNetObject.GetComponent<GrabbableObject>();
                if (!isStacked)
                    component.PlayDropSFX();
                if (IsServer)
                {
                    placedItems[spawnIndex] = component;
                    if (Plugin.config.rescaleItems.Value)
                    {
                        var collider = component.GetComponent<BoxCollider>();
                        if (collider == null)
                            yield break;
                        ScaleItemClientRpc(itemRef, true, collider.bounds.extents);
                    }
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

        [ClientRpc]
        private void ScaleItemClientRpc(NetworkObjectReference itemRef, bool scaleMode, Vector3 size = default, bool overrideOriginalScale = false, ClientRpcParams clientRpcParams = default)
        {
            if (Plugin.config.rescaleItems.Value)
                StartCoroutine(ScaleItem(itemRef, scaleMode, size, overrideOriginalScale));
        }

        private IEnumerator ScaleItem(NetworkObjectReference itemRef, bool scaleMode, Vector3 size, bool overrideOriginalScale)
        {
            if (scaleMode)
            {
                NetworkObject? itemNetObject;
                while (!itemRef.TryGet(out itemNetObject))
                    yield return new WaitForSeconds(0.03f);
                var component = itemNetObject.GetComponent<GrabbableObject>();
                while (component.originalScale == Vector3.zero)  // wait for item to start (set originalScale)
                    yield return new WaitForSeconds(0.03f);
                if (!overrideOriginalScale)
                    Effects.RescaleItemIfTooBig(component, size);
                else
                    Effects.OverrideOriginalScale(component, size);
            }
            else
            {
                if (itemRef.TryGet(out var itemNetObject))
                {
                    Effects.ScaleBackItem(itemNetObject.GetComponent<GrabbableObject>());
                }
            }
        }

        private IEnumerator SyncCupboard()
        {
            if (Plugin.config.verboseLogging.Value)
                Plugin.logger.LogInfo("Syncing Cupboard from host player");
            while (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null)
                yield return new WaitForSeconds(0.03f);
            SyncServerRpc(GameNetworkManager.Instance.localPlayerController.OwnerClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SyncServerRpc(ulong clientId)
        {
            if (memory.IsFull())
                SetSizeClientRpc(memory.Size);
            if (placedItems.Count == 0 || !Plugin.config.rescaleItems.Value)
                return;
            var clientRpcParams = new ClientRpcParams() { Send = new ClientRpcSendParams() { TargetClientIds = new[] { clientId } } };
            foreach (var (_, item) in placedItems)
            {
                if (!item.isHeld && !item.isHeldByEnemy)
                {
                    ScaleItemClientRpc(item.gameObject.GetComponent<NetworkObject>(), true, item.originalScale, true, clientRpcParams);
                }
            }
        }

        [ClientRpc]
        private void SetSizeClientRpc(int size)
        {
            memory.Size = size;
        }
    }
}
