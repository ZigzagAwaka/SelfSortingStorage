﻿using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SelfSortingStorage.Cupboard
{
    internal class SavingModule
    {
        public const string SaveKey = "SmartCupboardItems";

        public static void Save(string saveFile)
        {
            var shouldSave = GetItems(out var items);
            if (!shouldSave)
            {
                ES3.DeleteKey(SaveKey, saveFile);
                return;
            }
            ES3.Save(SaveKey, JsonConvert.SerializeObject(items), saveFile);
            Plugin.logger.LogInfo("SmartCupboard items saved.");
        }

        public static void Load(string saveFile)
        {
            if (!ES3.KeyExists(SaveKey, saveFile) || SmartMemory.Instance == null)
                return;
            var cupboard = Object.FindObjectOfType<SmartCupboard>();
            var loadedData = ES3.Load<string>(SaveKey, saveFile);
            var itemsRaw = JsonConvert.DeserializeObject<IEnumerable<SmartMemory.Data>>(loadedData);
            if (itemsRaw == null || cupboard == null)
            {
                Plugin.logger.LogError("Items from SmartCupboard could not be loaded.");
                return;
            }
            var items = itemsRaw.ToArray();
            foreach (var item in items)
            {
                SmartMemory.Instance.StoreData(item, out _, true);
            }
            cupboard.StartCoroutine(cupboard.ReloadPlacedItems());
            Plugin.logger.LogInfo("SmartCupboard items loaded.");
        }

        private static bool GetItems(out SmartMemory.Data[]? items)
        {
            items = null;
            if (SmartMemory.Instance == null || SmartMemory.Instance.Size == 0)
                return false;
            var length = SmartMemory.Instance.ItemList.Count;
            items = new SmartMemory.Data[SmartMemory.Instance.Size];
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    var item = SmartMemory.Instance.ItemList[i][j];
                    if (item.IsValid())
                        items[j + i * length] = item;
                }
            }
            return true;
        }
    }
}
