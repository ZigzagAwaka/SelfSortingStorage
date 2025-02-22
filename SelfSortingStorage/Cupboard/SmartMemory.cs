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
            public int Value = 0;
            public int Quantity = 1;

            public Data() { }

            public Data(GrabbableObject item)
            {
                Id = ConvertID(item.itemProperties);
                if (item.itemProperties.isScrap)
                    Value = item.scrapValue;
            }

            public bool IsValid()
            {
                return Id != "INVALID";
            }

            public void Update(Data data)
            {
                Id = data.Id;
                Value = data.Value;
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
                serializer.SerializeValue(ref Value);
                serializer.SerializeValue(ref Quantity);
            }
        }


        public static SmartMemory? Instance;
        public readonly int Capacity = 16;
        public int Size = 0;
        public readonly List<List<Data>> ItemList = new List<List<Data>>(4);
        public readonly static Dictionary<string, Item> CacheItems = new Dictionary<string, Item>();

        public SmartMemory()
        {
            Clear(false);
        }

        public bool IsFull()
        {
            return Size == Capacity;
        }

        public void Clear(bool clearAll = true)
        {
            if (clearAll)
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

        public bool StoreData(Data data, out int spawnIndex)
        {
            spawnIndex = 0;
            Plugin.logger.LogError(ToString());
            foreach (var list in ItemList)
            {
                foreach (var item in list)
                {
                    if (!item.IsValid())
                    {
                        Plugin.logger.LogError("invalid");
                        item.Update(data);
                        Size++;
                        return true;
                    }
                    else if (item.IsValid() && item.Id == data.Id)
                    {
                        Plugin.logger.LogError("same: " + item.Id);
                        item.Quantity++;
                        return false;
                    }
                    spawnIndex++;
                }
            }
            Plugin.logger.LogError("SmartCupboard was full when " + data.Id + " tried to be stored.");
            return false;
        }

        public Data? RetrieveData(int spawnIndex, bool updateQuantity = true)
        {
            int place = (int)(spawnIndex / 4.0f);
            int diff = spawnIndex - (place * 4);
            if (spawnIndex > Size || place >= 4 || diff >= 4)
                return null;
            var result = ItemList[place][diff];
            if (updateQuantity)
            {
                result.Quantity--;
                if (result.Quantity <= 0)
                {
                    Size--;
                    ItemList[place][diff] = new Data();
                }
            }
            return result;
        }

        public override string ToString()
        {
            int i = 0;
            var builder = new StringBuilder().Append("Item list:\n");
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
