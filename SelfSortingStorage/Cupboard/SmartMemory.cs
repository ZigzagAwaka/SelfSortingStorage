using LethalLib.Modules;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;

namespace SelfSortingStorage.Cupboard
{
    public class SmartMemory
    {
        [Serializable]
        public class Data
        {
            public string Id = "INVALID";
            public List<int> Values = new List<int>();
            public List<int> Saves = new List<int>();
            public int Quantity = 1;

            public Data() { }

            public Data(GrabbableObject item)
            {
                Id = ConvertID(item.itemProperties);
                Values.Add(item.itemProperties.isScrap ? item.scrapValue : 0);
                Saves.Add(item.itemProperties.saveItemVariable ? item.GetItemDataToSave() : 0);
            }

            public bool IsValid()
            {
                return Id != "INVALID";
            }

            public void Update(Data data)
            {
                Id = data.Id;
                Values.Clear();
                Values.AddRange(data.Values);
                Saves.Clear();
                Saves.AddRange(data.Saves);
                Quantity = data.Quantity;
            }

            private string ConvertID(Item sourceItem)
            {
                foreach (var (id, item) in CacheItems)
                {
                    if (item == sourceItem)
                        return id;
                }
                return Plugin.VANILLA_NAME + "/" + sourceItem.itemName;
            }

            public void NetworkerSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref Id);
                if (serializer.IsWriter)
                {
                    var writter = serializer.GetFastBufferWriter();
                    writter.WriteValueSafe(Values.Count);
                    foreach (var value in Values)
                        writter.WriteValueSafe(value);
                    writter.WriteValueSafe(Saves.Count);
                    foreach (var save in Saves)
                        writter.WriteValueSafe(save);
                }
                if (serializer.IsReader)
                {
                    var reader = serializer.GetFastBufferReader();
                    reader.ReadValueSafe(out int count);
                    for (var i = 0; i < count; i++)
                    {
                        reader.ReadValueSafe(out int value);
                        Values.Add(value);
                    }
                    reader.ReadValueSafe(out count);
                    for (var i = 0; i < count; i++)
                    {
                        reader.ReadValueSafe(out int save);
                        Saves.Add(save);
                    }
                }
                serializer.SerializeValue(ref Quantity);
            }
        }


        public static SmartMemory? Instance;
        public readonly int Capacity = 16;
        public int Size = 0;
        public readonly List<int> IgnoreSpaces = new List<int>();
        public readonly List<List<Data>> ItemList = new List<List<Data>>(4);
        public readonly static Dictionary<string, Item> CacheItems = new Dictionary<string, Item>();

        public SmartMemory()
        {
            ClearAll(false);
        }

        public bool IsFull()
        {
            return Size == Capacity;
        }

        public void ClearAll(bool resetData = true)
        {
            if (resetData)
            {
                ItemList.ForEach(x => x.Clear());
                ItemList.Clear();
                Size = 0;
            }
            for (int i = 0; i < 4; i++)
            {
                var list = new List<Data>(4);
                for (int j = 0; j < 4; j++)
                    list.Add(new Data());
                ItemList.Add(list);
            }
        }

        public void Initialize()
        {
            Instance = this;
            foreach (var item in Items.scrapItems)
                CacheItems.TryAdd(item.modName + "/" + item.item.itemName, item.item);
            foreach (var item in Items.shopItems)
                CacheItems.TryAdd(item.modName + "/" + item.item.itemName, item.item);
            foreach (var item in Items.plainItems)
                CacheItems.TryAdd(item.modName + "/" + item.item.itemName, item.item);
        }

        public bool StoreData(Data data, out int spawnIndex, bool freeSpaceOnly = false)
        {
            if (Plugin.config.verboseLogging.Value)
            {
                Plugin.logger.LogWarning(ToString());
                Plugin.logger.LogWarning("Storing: " + data.Id);
            }
            spawnIndex = 0;
            var lastFreeSpaceId = -1;
            foreach (var list in ItemList)
            {
                foreach (var item in list)
                {
                    if (!item.IsValid() && lastFreeSpaceId == -1)
                    {
                        lastFreeSpaceId = spawnIndex;
                        if (IgnoreSpaces.Count > 0 && IgnoreSpaces.Exists(e => e == lastFreeSpaceId))
                            lastFreeSpaceId = -1;
                        else if (freeSpaceOnly)
                            break;
                    }
                    else if (!freeSpaceOnly && item.IsValid() && item.Id == data.Id)
                    {
                        if (Plugin.config.verboseLogging.Value)
                            Plugin.logger.LogWarning("Found a similar item");
                        item.Values.Add(data.Values[0]);
                        item.Saves.Add(data.Saves[0]);
                        item.Quantity++;
                        return false;
                    }
                    spawnIndex++;
                }
                if (freeSpaceOnly && lastFreeSpaceId != -1)
                    break;
            }
            if (lastFreeSpaceId != -1)
            {
                if (Plugin.config.verboseLogging.Value)
                    Plugin.logger.LogWarning("Found 1 free space");
                spawnIndex = lastFreeSpaceId;
                int place = (int)(lastFreeSpaceId / 4.0f);
                int diff = lastFreeSpaceId - (place * 4);
                ItemList[place][diff].Update(data);
                Size++;
                return true;
            }
            Plugin.logger.LogError("SmartCupboard was full when " + data.Id + " tried to be stored.");
            return false;
        }

        public Data? RetrieveData(int spawnIndex, bool updateQuantity = true)
        {
            if (Plugin.config.verboseLogging.Value)
            {
                Plugin.logger.LogWarning(ToString());
                Plugin.logger.LogWarning("Retrieving item at position: " + spawnIndex);
            }
            int place = (int)(spawnIndex / 4.0f);
            int diff = spawnIndex - (place * 4);
            if (place >= 4 || diff >= 4)
                return null;
            var result = ItemList[place][diff];
            if (!result.IsValid())
                return null;
            if (updateQuantity)
            {
                result.Values.RemoveAt(0);
                result.Saves.RemoveAt(0);
                result.Quantity--;
                if (result.Quantity <= 0)
                {
                    Size--;
                    ItemList[place][diff] = new Data();
                }
            }
            if (Plugin.config.verboseLogging.Value)
                Plugin.logger.LogWarning(ToString());
            return result;
        }

        public override string ToString()
        {
            int i = 0;
            var builder = new StringBuilder().Append("Stored items:\n");
            if (Size == 0)
                return builder.Append("None\n").ToString();
            foreach (var list in ItemList)
            {
                foreach (var item in list)
                {
                    if (item.IsValid())
                    {
                        builder.Append(i + ": " + item.Id + " x" + item.Quantity + "\n");
                    }
                    i++;
                }
            }
            return builder.ToString();
        }
    }
}
