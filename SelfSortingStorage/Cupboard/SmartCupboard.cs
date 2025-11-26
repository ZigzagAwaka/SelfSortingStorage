using GameNetcodeStuff;
using SelfSortingStorage.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        private GrabbableObject? awaitingObject = null;
        private bool responseOnAwaiting = false;
        private bool isRespawningFromSave = false;

        public static bool SpawnedInShip { get; private set; } = false;
        public static Dictionary<ulong, int> StoredInstanceQuantities { get; } = new Dictionary<ulong, int>();

        public SmartCupboard() { }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            memory.SetupInitialState();
            if (IsServer)
            {
                memory.InitializeCache();
                if (Compatibility.GeneralImprovementsInstalled && Plugin.config.customScreenPos.Value > 0 && Plugin.config.customScreenPos.Value <= 14)
                    StartCoroutine(DisplayOnScreen());
                var hangarShip = GameObject.Find("/Environment/HangarShip");
                if (hangarShip != null)
                    parentObject.transform.SetParent(hangarShip.transform, worldPositionStays: true);
            }
            SpawnedInShip = true;
            PreparePlacePositions();
            if (IsServer && Plugin.config.permanentItems.Count != 0)
                StartCoroutine(ForceStorePermanentItems());
            if (!Compatibility.CompatibilityModsAreValid)
                Compatibility.CheckCompatibilitySSS(displayTip: true);
            if (!IsServer)
                StartCoroutine(SyncCupboard());
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            SpawnedInShip = false;
            StoredInstanceQuantities.Clear();
        }

        private void PreparePlacePositions()
        {
            if (Plugin.config.rowsOrder.Count == 4)
            {
                var posTmp = new Transform[placePositions.Length];
                placePositions.CopyTo(posTmp, 0);
                foreach (var (order, actual) in Plugin.config.rowsOrder)
                {
                    int id = actual * Plugin.instance.ROWS_LENGTH;
                    int place = (order - 1) * Plugin.instance.ROWS_LENGTH;
                    for (int i = place; i < place + Plugin.instance.ROWS_LENGTH; i++)
                    {
                        placePositions[i] = posTmp[id++];
                    }
                }
            }
        }

        public void Update()
        {
            if (GameNetworkManager.Instance != null && GameNetworkManager.Instance.localPlayerController != null)
            {
                if (CheckIsFull(GameNetworkManager.Instance.localPlayerController))
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
                if (!IsServer && awaitingObject != null)
                {
                    if (GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer == null ||
                        GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer != awaitingObject)
                    {
                        responseOnAwaiting = false;
                        awaitingObject = null;
                    }
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
                        if (Plugin.config.quantityCursortipActive.Value)
                            UpdateNetworkQuantityClientRpc(item.NetworkObjectId, shouldRemove: true);
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
            isRespawningFromSave = true;
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
                                Effects.ReParentItemToCupboard(array[i], this, spawnIndex);
                                if (Plugin.config.rescaleItems.Value)
                                    Effects.RescaleItemIfTooBig(array[i]);
                                if (Plugin.config.quantityCursortipActive.Value)
                                    StoredInstanceQuantities.Add(array[i].NetworkObjectId, item.Quantity);
                                placedItems[spawnIndex] = array[i];
                            }
                        }
                    }
                    spawnIndex++;
                }
            }
        }

        private IEnumerator ForceStorePermanentItems()
        {
            yield return new WaitForSeconds(1f);  // wait for ReloadPlacedItems()
            if (isRespawningFromSave)
            {
                yield break;
            }
            foreach (var permanent in Plugin.config.permanentItems)
            {
                var item = Effects.GetItem(permanent.Item1);
                if (item == null || item.spawnPrefab == null)
                {
                    Plugin.logger.LogError("The " + permanent.Item1 + " is not a valid item. Have you entered the item name correctly ?");
                    continue;
                }
                var grabbableObject = item.spawnPrefab.GetComponent<GrabbableObject>();
                if (grabbableObject == null)
                    continue;
                var itemData = new SmartMemory.Data(grabbableObject) { Quantity = permanent.Item2 };
                itemData.Values.AddRange(Enumerable.Repeat(itemData.Values[0], itemData.Quantity - 1));
                itemData.Saves.AddRange(Enumerable.Repeat(itemData.Saves[0], itemData.Quantity - 1));
                if (memory.StoreData(itemData, out int spawnIndex))
                    SpawnItem(itemData, spawnIndex);
                if (Plugin.config.verboseLogging.Value)
                    Plugin.logger.LogWarning(memory.ToString());
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

        public void GetPlacePosition(int spawnIndex, out Transform parent, out Vector3 position, out Quaternion rotation)
        {
            parent = placePositions[spawnIndex];
            position = parent.position;
            rotation = parent.rotation * Quaternion.Euler(0, 0, 180);
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
                {
                    PlayDropSFXClientRpc(component.gameObject.GetComponent<NetworkObject>());
                    if (Plugin.config.quantityCursortipActive.Value)
                        UpdateNetworkQuantityClientRpc(component.NetworkObjectId);
                }
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
                var itemRef = Effects.SpawnItem(item, this, spawnIndex, itemData.Values[0], itemData.Saves[0]);
                SyncItemClientRpc(itemRef.netObjectRef, itemRef.value, itemRef.save, itemData.Quantity, spawnIndex, isStacked);
            }
        }

        [ClientRpc]
        private void SyncItemClientRpc(NetworkObjectReference itemRef, int value, int save, int quantity, int spawnIndex, bool isStacked)
        {
            StartCoroutine(SyncItem(itemRef, value, save, quantity, spawnIndex, isStacked));
        }

        private IEnumerator SyncItem(NetworkObjectReference itemRef, int value, int save, int quantity, int spawnIndex, bool isStacked)
        {
            yield return Effects.SyncItem(itemRef, this, spawnIndex, value, save);
            if (itemRef.TryGet(out var itemNetObject))
            {
                var component = itemNetObject.GetComponent<GrabbableObject>();
                if (!isStacked)
                    component.PlayDropSFX();
                if (Plugin.config.quantityCursortipActive.Value)
                    StoredInstanceQuantities.Add(component.NetworkObjectId, quantity);
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
        private void ScaleItemClientRpc(NetworkObjectReference itemRef, bool scaleMode, Vector3 size = default)
        {
            if (Plugin.config.rescaleItems.Value)
                StartCoroutine(ScaleItem(itemRef, scaleMode, size));
        }

        private IEnumerator ScaleItem(NetworkObjectReference itemRef, bool scaleMode, Vector3 size, bool syncedFromHost = false, int spawnIndex = -1, Vector3 syncedPos = default, Quaternion syncedRot = default)
        {
            if (scaleMode)
            {
                NetworkObject? itemNetObject;
                while (!itemRef.TryGet(out itemNetObject))
                    yield return new WaitForSeconds(0.03f);
                var component = itemNetObject.GetComponent<GrabbableObject>();
                while (component.originalScale == Vector3.zero)  // wait for item to start (set originalScale)
                    yield return new WaitForSeconds(0.03f);
                if (!syncedFromHost)
                    Effects.RescaleItemIfTooBig(component, size);
                else
                {
                    if (Plugin.config.rescaleItems.Value)
                        Effects.OverrideOriginalScale(component, size);
                    Effects.ReParentItemToCupboard(component, this, spawnIndex, true, syncedPos, syncedRot);
                }
            }
            else
            {
                if (itemRef.TryGet(out var itemNetObject))
                {
                    Effects.ScaleBackItem(itemNetObject.GetComponent<GrabbableObject>());
                }
            }
        }

        [ClientRpc]
        private void UpdateNetworkQuantityClientRpc(ulong networkIdToRemove, bool shouldRemove = false)
        {
            if (Plugin.config.quantityCursortipActive.Value && StoredInstanceQuantities.ContainsKey(networkIdToRemove))
            {
                if (shouldRemove)
                    StoredInstanceQuantities.Remove(networkIdToRemove);
                else
                    StoredInstanceQuantities[networkIdToRemove]++;
            }
        }

        private IEnumerator SyncNetworkQuantity(NetworkObjectReference itemRef, int quantity)
        {
            NetworkObject? itemNetObject;
            while (!itemRef.TryGet(out itemNetObject))
                yield return new WaitForSeconds(0.03f);
            var component = itemNetObject.GetComponent<GrabbableObject>();
            StoredInstanceQuantities.Add(component.NetworkObjectId, quantity);
        }

        private bool CheckIsFull(PlayerControllerB player)
        {
            if (!memory.IsFull())
                return false;
            if (!player.isHoldingObject || player.isGrabbingObjectAnimation || player.currentlyHeldObjectServer == null)
                return true;
            if (IsServer)
                return ServerCheckingIsFull(player);
            else
            {
                if (responseOnAwaiting)
                    return false;
                if (awaitingObject != null)
                    return true;
                awaitingObject = player.currentlyHeldObjectServer;
                AskServerCheckIsFullServerRpc(player.playerClientId, player.OwnerClientId);
                return true;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void AskServerCheckIsFullServerRpc(ulong playerId, ulong clientId)
        {
            bool response;
            var player = StartOfRound.Instance.allPlayerScripts[playerId];
            if (player == null || player.currentlyHeldObjectServer == null)
                response = true;
            else
                response = ServerCheckingIsFull(player);
            var clientRpcParams = new ClientRpcParams() { Send = new ClientRpcSendParams() { TargetClientIds = new[] { clientId } } };
            ServerResponseCheckIsFullClientRpc(response, clientRpcParams);
        }

        private bool ServerCheckingIsFull(PlayerControllerB player)
        {
            foreach (var (_, item) in placedItems)
            {
                if (player.currentlyHeldObjectServer.itemProperties.itemName == item.itemProperties.itemName &&
                    player.currentlyHeldObjectServer.itemProperties.name == item.itemProperties.name)
                    return false;
            }
            return true;
        }

        [ClientRpc]
        private void ServerResponseCheckIsFullClientRpc(bool response, ClientRpcParams clientRpcParams = default)
        {
            if (response)
                return;
            responseOnAwaiting = true;
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
            if (memory.Size == 0)
                SetScreenTextServerRpc("No items in Smart Cupboard");
            if (placedItems.Count == 0)
                return;
            var clientRpcParams = new ClientRpcParams() { Send = new ClientRpcSendParams() { TargetClientIds = new[] { clientId } } };
            foreach (var (spawnIndex, item) in placedItems)
            {
                if (!item.isHeld && !item.isHeldByEnemy)
                {
                    int quantity = 1;
                    if (Plugin.config.quantityCursortipActive.Value && StoredInstanceQuantities.TryGetValue(item.NetworkObjectId, out var storedQuantity))
                    {
                        quantity = storedQuantity;
                    }
                    Vector3 syncedPosition = item.targetFloorPosition;
                    Quaternion syncedRotation = item.transform.localRotation;
                    SyncClientRpc(item.gameObject.GetComponent<NetworkObject>(), quantity, spawnIndex, item.originalScale, syncedPosition, syncedRotation, clientRpcParams);
                }
            }
        }

        [ClientRpc]
        private void SyncClientRpc(NetworkObjectReference itemRef, int quantity, int spawnIndex, Vector3 originalScale, Vector3 syncedPos, Quaternion syncedRot, ClientRpcParams clientRpcParams = default)
        {
            StartCoroutine(ScaleItem(itemRef, true, originalScale, true, spawnIndex, syncedPos, syncedRot));
            if (Plugin.config.quantityCursortipActive.Value)
            {
                StartCoroutine(SyncNetworkQuantity(itemRef, quantity));
            }
        }

        [ClientRpc]
        private void SetSizeClientRpc(int size)
        {
            memory.Size = size;
        }

        private IEnumerator DisplayOnScreen()
        {
            int lastId = 0;
            while (SmartMemory.Instance != null)
            {
                if (memory.Size != 0)
                {
                    var it = 0;
                    var builder = new System.Text.StringBuilder();
                    for (int i = lastId; i < memory.Capacity; i++)
                    {
                        var item = memory.RetrieveData(i, false);
                        if (item != null)
                        {
                            builder.Append(GetItemScreenText(item));
                            it++;
                        }
                        var verifReset = it == memory.Size || i + 1 >= memory.Capacity;
                        if (it == 4 || verifReset)
                        {
                            lastId = verifReset ? 0 : i + 1;
                            break;
                        }
                    }
                    if (it == 0)
                        continue;
                    SetScreenTextServerRpc(builder.ToString());
                    yield return new WaitForSeconds(10);
                }
                else
                {
                    SetScreenTextServerRpc("No items in Smart Cupboard");
                    yield return new WaitUntil(() => memory.Size != 0);
                }
            }
        }

        private string GetItemScreenText(SmartMemory.Data item)
        {
            var name = item.Id.Split('/')[1];
            var quantity = item.Quantity >= 100 ? 99 : item.Quantity;
            var offset = quantity < 10 ? 6 : 5;
            var cutName = name.Length >= offset + 1 ? name[..offset] : name;
            return cutName + " x" + quantity + "\n";
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetScreenTextServerRpc(string text)
        {
            SetScreenTextClientRpc(text);
        }

        [ClientRpc]
        private void SetScreenTextClientRpc(string text)
        {
            if (Compatibility.GeneralImprovementsInstalled && Plugin.config.customScreenPos.Value > 0 && Plugin.config.customScreenPos.Value <= 14)
                Effects.SetScreenText(Plugin.config.customScreenPos.Value - 1, text);
        }
    }
}
