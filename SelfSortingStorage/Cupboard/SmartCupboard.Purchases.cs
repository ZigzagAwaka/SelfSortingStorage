using System.Linq;
using Unity.Netcode;

namespace SelfSortingStorage.Cupboard
{
    public partial class SmartCupboard
    {
        public bool TryStoreItemFromPurchase(Item item, int value = 0, int save = 0, bool playSound = false)
        {
            if (!IsServer || item == null || item.spawnPrefab == null || item.spawnPrefab.GetComponent<GrabbableObject>() == null) // processed from server/host, not null, can spawn, is grabbable
                return false;

            string id = SmartMemory.Data.ConvertID(item);

            if (memory.IsFull() && !memory.ItemList.Any(list => list.Any(data => data.IsValid() && data.Id == id)))
            {
                Plugin.logger.LogInfo($"[AutoStoreTerminalPurchases] Cupboard is full and no existing stack for {id}, rejecting.");

                return false;
            }

            var itemData = new SmartMemory.Data{Id = id, Quantity = 1};

            itemData.Values.Add(item.isScrap ? value : 0);
            itemData.Saves.Add(item.saveItemVariable ? save : 0);
            itemData.States.Add(true);

            bool shouldSpawn = memory.StoreData(itemData, out int spawnIndex);

            if (memory.IsFull())
                SetSizeClientRpc(memory.Size);

            int place = spawnIndex / Plugin.instance.ROWS_LENGTH;
            int diff = spawnIndex % Plugin.instance.ROWS_LENGTH;

            if (place < 0 || place >= memory.ItemList.Count || diff < 0 || diff >= memory.ItemList[place].Count)
            {
                Plugin.logger.LogWarning($"[AutoStoreTerminalPurchases] Invalid spawn index {spawnIndex}.");

                return false;
            }

            var storedData = memory.ItemList[place][diff];

            if (!storedData.IsValid())
                return false;

            int actualQuantity = storedData.Quantity;

            if (shouldSpawn)
            {
                SpawnItem(storedData, spawnIndex, false);
            }
            else if (playSound && placedItems.TryGetValue(spawnIndex, out var component))
            {
                PlayDropSFXClientRpc(component.gameObject.GetComponent<NetworkObject>());
            }

            if (Plugin.config.quantityDisplayActive.Value)
                UpdateDisplayedQuantityClientRpc(spawnIndex, actualQuantity);

            if (Plugin.config.quantityCursortipActive.Value && placedItems.TryGetValue(spawnIndex, out var currentComponent))
                UpdateNetworkQuantityClientRpc(currentComponent.NetworkObjectId);

            return true;
        }
    }
}