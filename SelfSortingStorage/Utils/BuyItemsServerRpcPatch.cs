using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using SelfSortingStorage.Cupboard;
using UnityEngine;

namespace SelfSortingStorage.Utils
{
    [HarmonyPatch(typeof(Terminal))]
    internal static class BuyItemsServerRpcPatch
    {

        private static Terminal cachedTerminal;

        private static Terminal GetTerminal()
        {
            if (cachedTerminal == null)
                cachedTerminal = UnityEngine.Object.FindObjectOfType<Terminal>();
            return cachedTerminal;
        }

        [HarmonyPatch(typeof(Terminal), nameof(Terminal.BuyItemsServerRpc))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            MethodInfo addRangeMethod = AccessTools.Method(typeof(List<int>), nameof(List<int>.AddRange));

            MethodInfo handlePurchasedItemsMethod = AccessTools.Method(typeof(BuyItemsServerRpcPatch), nameof(HandlePurchasedItems));

            if (addRangeMethod == null)
            {
                Plugin.logger.LogError("[AutoStoreTerminalPurchases] Could not resolve List<int>.AddRange().");

                return codes;
            }

            if (handlePurchasedItemsMethod == null)
            {
                Plugin.logger.LogError("[AutoStoreTerminalPurchases] Could not resolve HandlePurchasedItems().");

                return codes;
            }

            bool replaced = false;

            for (int i = 0; i < codes.Count; i++)
            {
                if (!codes[i].Calls(addRangeMethod))
                    continue;

                if (Plugin.config.verboseLogging.Value)
                    Plugin.logger.LogInfo("[AutoStoreTerminalPurchases] Found orderedItemsFromTerminal.AddRange() inside BuyItemsServerRpc. Replacing it.");

                /*
                 * The original instruction sequence is effectively:
                 *
                 *     orderedItemsFromTerminal
                 *     boughtItems.ToList()
                 *     List<int>.AddRange(...)
                 *
                 * The replacement method accepts the same two values:
                 *
                 *     HandlePurchasedItems(
                 *         List<int> orderedItems,
                 *         List<int> boughtItems)
                 *
                 * Therefore the existing evaluation stack can be reused.
                 */

                codes[i] = new CodeInstruction(OpCodes.Call, handlePurchasedItemsMethod);

                replaced = true;
                break;
            }

            if (Plugin.config.verboseLogging.Value)
            {
                if (replaced)
                {
                    Plugin.logger.LogInfo("[AutoStoreTerminalPurchases] Successfully replaced AddRange() with HandlePurchasedItems().");
                }
                else
                {
                    Plugin.logger.LogWarning("[AutoStoreTerminalPurchases] Transpiler did not replace AddRange(). No purchase behavior was changed.");
                }
            }

            return codes;
        }


        [HarmonyPatch(typeof(Terminal), "LoadNewNodeIfAffordable")]
        [HarmonyPrefix]
        private static void LoadNewNodeIfAffordablePrefix(Terminal __instance, TerminalNode node, out int __state)
        {
            if (__instance.IsServer && node.buyItemIndex != -1)
            {
                __state = __instance.orderedItemsFromTerminal.Count;
            }
            else
            {
                __state = -1;
            }
        }

        [HarmonyPatch(typeof(Terminal), "LoadNewNodeIfAffordable")]
        [HarmonyPostfix]
        private static void LoadNewNodeIfAffordablePostfix(Terminal __instance, TerminalNode node, int __state)
        {
            if (__state < 0)
                return;

            int addedCount = __instance.orderedItemsFromTerminal.Count - __state;

            if (addedCount <= 0)
                return;

            if (Plugin.config.verboseLogging.Value)
                Plugin.logger.LogInfo($"[AutoStoreTerminalPurchases] Host bought {addedCount} item(s) directly, bypassing BuyItemsServerRpc. Routing through HandlePurchasedItems.");

            List<int> newItems = __instance.orderedItemsFromTerminal.GetRange(__state, addedCount);
            __instance.orderedItemsFromTerminal.RemoveRange(__state, addedCount);

            HandlePurchasedItems(__instance.orderedItemsFromTerminal, newItems);

            int divertedCount = addedCount - (__instance.orderedItemsFromTerminal.Count - __state);

            if (divertedCount > 0)
            {
                __instance.numberOfItemsInDropship -= divertedCount;

                __instance.SyncGroupCreditsClientRpc(__instance.groupCredits, __instance.numberOfItemsInDropship);
            }
        }


        private static void HandlePurchasedItems(List<int> orderedItems, List<int> boughtItems)
        {
            if (boughtItems == null || boughtItems.Count == 0)
                return;

            SmartCupboard cupboard = null;
            Terminal terminal = null;

            try
            {
                if (!Plugin.config.autoStoreTerminalPurchases.Value || !SmartCupboard.SpawnedInShip)
                {
                    orderedItems.AddRange(boughtItems);

                    return;
                }

                cupboard = SmartCupboard.Instance;
                terminal = GetTerminal();

                if (cupboard == null || terminal == null)
                {
                    if (Plugin.config.verboseLogging.Value)
                        Plugin.logger.LogWarning("[AutoStoreTerminalPurchases] SmartCupboard or Terminal reference unavailable. Using normal dropship behavior.");

                    orderedItems.AddRange(boughtItems);

                    return;
                }
            }
            catch (Exception ex)
            {
                Plugin.logger.LogError($"[AutoStoreTerminalPurchases] Exception before processing purchases: {ex}");

                orderedItems.AddRange(boughtItems);
                return;
            }

            bool playSound = true;

            foreach (int index in boughtItems)
            {
                try
                {
                    if (index < 0 || index >= terminal.buyableItemsList.Length)
                    {
                        orderedItems.Add(index);

                        continue;
                    }

                    Item item = terminal.buyableItemsList[index];

                    if (item == null || item.spawnPrefab == null || item.spawnPrefab.GetComponent<GrabbableObject>() == null)
                    {
                        orderedItems.Add(index);

                        continue;
                    }

                    int value = 0;

                    if (item.isScrap)
                    {
                        int minValue = Mathf.Min(item.minValue, item.maxValue);
                        int maxValue = Mathf.Max(item.minValue, item.maxValue);
                        int rolled = UnityEngine.Random.Range(minValue, maxValue + 1);

                        if (RoundManager.Instance != null)
                        {
                            value = Mathf.RoundToInt(rolled * RoundManager.Instance.scrapValueMultiplier);
                        }
                        else
                        {
                            value = rolled;
                        }
                    }

                    bool stored = cupboard.TryStoreItemFromPurchase(item, value: value, playSound: playSound);

                    if (stored)
                    {
                        playSound = false;
                    }
                    else
                    {
                        orderedItems.Add(index);
                    }

                    if (Plugin.config.verboseLogging.Value)
                        Plugin.logger.LogInfo($"[AutoStoreTerminalPurchases] {item.itemName}: stored={stored}");
                }
                catch (Exception ex)
                {
                    Plugin.logger.LogError($"[AutoStoreTerminalPurchases] Exception handling purchased item index {index}: {ex}");

                    orderedItems.Add(index);
                }
            }
        }
    }
}