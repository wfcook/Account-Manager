using System.Collections.Generic;
using Google.Protobuf;
using POGOProtos.Data;
using POGOProtos.Inventory;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Responses;
using PokemonGoGUI.Extensions;
using PokemonGoGUI.GoManager.Models;
using PokemonGoGUI.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using POGOProtos.Data.Player;
using System.Collections.Concurrent;
using PokemonGoGUI.Enums;

namespace PokemonGoGUI.GoManager
{
    public partial class Manager
    {
        private ConcurrentDictionary<string, InventoryItem> InventoryItems = new ConcurrentDictionary<string, InventoryItem>();

        private string GetInventoryItemHashKey(InventoryItem item)
        {
            if (item == null || item.InventoryItemData == null)
                return null;

            var delta = item.InventoryItemData;

            if (delta.AppliedItems != null)
                return "AppliedItems";

            if (delta.AvatarItem != null)
                return "AvatarItem." + delta.AvatarItem.AvatarTemplateId;

            if (delta.Candy != null)
                return "Candy." + delta.Candy.FamilyId;

            if (delta.EggIncubators != null)
                return "EggIncubators";

            if (delta.InventoryUpgrades != null)
                return "InventoryUpgrades";

            if (delta.Item != null)
                return "Item." + delta.Item.ItemId;

            if (delta.PlayerCamera != null)
                return "PlayerCamera";

            if (delta.PlayerCurrency != null)
                return "PlayerCurrency";

            if (delta.PlayerStats != null)
                return "PlayerStats";

            if (delta.PokedexEntry != null)
                return "PokedexEntry." + delta.PokedexEntry.PokemonId;

            if (delta.PokemonData != null)
                return GetPokemonHashKey(delta.PokemonData.Id);

            if (delta.Quest != null)
                return "Quest." + delta.Quest.QuestType;

            if (delta.RaidTickets != null)
                return delta.RaidTickets.RaidTicket.ToString();

            throw new Exception("Unexpected inventory error. Could not generate hash code.");
        }

        private string GetPokemonHashKey(ulong id)
        {
            return "PokemonData." + id;
        }

        private IEnumerable<AppliedItem> GetAppliedItems()
        {
            return InventoryItems.Select(i => i.Value.InventoryItemData?.AppliedItems)
                .Where(aItems => aItems?.Item != null)
                .SelectMany(aItems => aItems.Item);
        }

        private IEnumerable<ItemData> GetItemsData()
        {
            return InventoryItems.Select(x => x.Value.InventoryItemData.Item)
                .Where(x => x != null);
        }

        private ItemData GetItemData(ItemId itemId)
        {
            return GetItemsData()?.FirstOrDefault(p => p.ItemId == itemId);
        }

        private int GetItemCount(ItemId itemId)
        {
            var itemData = GetItemData(itemId);
            return (itemData != null) ? itemData.Count : 0;
        }

        private int GetItemsCount()
        {
            return InventoryItems.Where(p => p.Value.InventoryItemData.Item != null)
                .Sum(p => p.Value.InventoryItemData.Item.Count);
        }

        private PlayerStats GetPlayerStats()
        {
            return InventoryItems.Select(i => i.Value.InventoryItemData?.PlayerStats)
                .Where(i => i != null).FirstOrDefault();
        }

        private IEnumerable<EggIncubator> GetIncubators()
        {
            return InventoryItems.Where(x => x.Value.InventoryItemData.EggIncubators != null)
                    .SelectMany(i => i.Value.InventoryItemData.EggIncubators.EggIncubator)
                    .Where(i => i != null);
        }

        private IEnumerable<PokemonData> GetEggs()
        {
            return InventoryItems.Select(i => i.Value.InventoryItemData?.PokemonData)
               .Where(p => p != null && p.IsEgg);
        }

        private IEnumerable<PokemonData> GetPokemons()
        {
            return InventoryItems
                .Select(kvp => kvp.Value.InventoryItemData?.PokemonData)
                .Where(p => p != null && p.PokemonId > 0);
        }

        private IEnumerable<Candy> GetCandies()
        {
            return InventoryItems
                .Select(kvp => kvp.Value.InventoryItemData?.Candy)
                .Where(p => p != null && p.FamilyId > 0);
        }

        private IEnumerable<PokedexEntry> GetPokedex()
        {
            return InventoryItems
                .Select(kvp => kvp.Value.InventoryItemData?.PokedexEntry)
                .Where(p => p != null && p.PokemonId > 0);
        }

        private PokemonData GetPokemon(ulong pokemonId)
        {
            return GetPokemons().FirstOrDefault(p => p.Id == pokemonId);
        }

        private bool RemoveInventoryItem(string key)
        {
            InventoryItem toRemove;
            try
            {
                return InventoryItems.TryRemove(key, out toRemove);
            }
            catch (ArgumentNullException)
            {
                return false;
            }
        }

        internal void RemoveInventoryItems(IEnumerable<InventoryItem> items)
        {
            foreach (var item in items)
            {
                RemoveInventoryItem(item);
            }
        }

        /*
         * used if call request
        public void MergeWith(GetHoloInventoryResponse update)
        {
            var delta = update.InventoryDelta;

            if (delta?.InventoryItems == null)
            {
                return;
            }

            foreach (var item in delta.InventoryItems)
            {
                AddRemoveOrUpdateItem(item);
            }

            //OnInventoryUpdated?.Invoke();
        }
        */

        private bool RemoveInventoryItem(InventoryItem item)
        {
            if (item == null)
                return false;

            return RemoveInventoryItem(GetInventoryItemHashKey(item));
        }

        private void AddRemoveOrUpdateItem(InventoryItem item)
        {
            if (item == null)
                return;

            if (item.DeletedItem != null)
            {
                // Items with DeletedItem have a null InventoryItemData and are not added to inventory.
                // But we still need to remove the pokemon with Id == item.DeletedItem.PokemonId from the inventory.
                var pokemonToRemoveKey = $"PokemonData.{item.DeletedItem.PokemonId}"; // Manually construct key.
                RemoveInventoryItem(pokemonToRemoveKey);
            }
            else
            {
                InventoryItems.AddOrUpdate(GetInventoryItemHashKey(item), item, (key, oldItem) =>
                {
                    // Check timestamps to make sure we update with a newer item.
                    if (oldItem.ModifiedTimestampMs < item.ModifiedTimestampMs)
                    {
                        // Copy fields over to the old item.
                        oldItem.InventoryItemData = item.InventoryItemData;
                        oldItem.ModifiedTimestampMs = item.ModifiedTimestampMs;
                    }

                    return oldItem;
                });
            }
        }

        /// <summary>
        /// Load Inventory methodes.
        /// </summary>
        /// <param name="index 0">Load All.</param>
        /// <param name="index 1">Load Items.</param>
        /// <param name="index 2">Load Pokemon.</param>
        /// <param name="index 3">Load Pokedex.</param>
        /// <param name="index 4">Load PokemonCandy.</param>
        /// <param name="index 5">Load Incubators.</param>
        /// <param name="index 6">Load Eggs.</param>
        /// <param name="index 7">Load Stats.</param>
        public void UpdateInventory(InventoryRefresh type)
        {
            if (!_client.LoggedIn)
            {
              return;
            }

            foreach (var item in _client.ClientSession.Player.Inventory.InventoryItems)
                AddRemoveOrUpdateItem(item);

            LogCaller(new LoggerEventArgs($"Updating inventory. Items to load: {type}", LoggerTypes.Debug));

            try
            {
                switch (type)
                {
                    case InventoryRefresh.All:
                        Items.Clear();
                        Pokemon.Clear();
                        Pokedex.Clear();
                        PokemonCandy.Clear();
                        Incubators.Clear();
                        Eggs.Clear();
                        Stats = GetPlayerStats();
                        Items = GetItemsData().ToList();
                        Pokedex = GetPokedex().ToList();
                        PokemonCandy = GetCandies().ToList();
                        Incubators = GetIncubators().ToList();
                        Eggs = GetEggs().ToList();
                        Pokemon = GetPokemons().ToList();
                        break;
                    case InventoryRefresh.Items:
                        Items.Clear();
                        Items = GetItemsData().ToList();
                        break;
                    case InventoryRefresh.Pokemon:
                        Pokemon.Clear();
                        Pokemon = GetPokemons().ToList();
                        break;
                    case InventoryRefresh.Pokedex:
                        Pokedex.Clear();
                        Pokedex = GetPokedex().ToList();
                        break;
                    case InventoryRefresh.PokemonCandy:
                        PokemonCandy.Clear();
                        PokemonCandy = GetCandies().ToList();
                        break;
                    case InventoryRefresh.Incubators:
                        Incubators.Clear();
                        Incubators = GetIncubators().ToList();
                        break;
                    case InventoryRefresh.Eggs:
                        Eggs.Clear();
                        Eggs = GetEggs().ToList();
                        break;
                    case InventoryRefresh.Stats:
                        Stats = GetPlayerStats();
                        break;
                }
            }
            catch (Exception ex1)
            {
                LogCaller(new LoggerEventArgs(String.Format("Failed updating inventory."), LoggerTypes.Debug, ex1));
                ++_failedInventoryReponses;
            }
        }

        public async Task<MethodResult> RecycleFilteredItems()
        {
            if (Items.Count == 0 || Items == null)
                return new MethodResult
                {
                    Message = "Not items here...."
                };

            if (!UserSettings.RecycleItems)
            {
                return new MethodResult
                {
                    Message = "Item deletion not enabled"
                };
            }

            foreach (ItemData item in Items)
            {
                InventoryItemSetting itemSetting = UserSettings.ItemSettings.FirstOrDefault(x => x.Id == item.ItemId);

                if (itemSetting == null)
                {
                    continue;
                }

                int toDelete = item.Count - itemSetting.MaxInventory;

                if (toDelete <= 0)
                {
                    continue;
                }

                await RecycleItem(itemSetting, toDelete);

                await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
            }

            UpdateInventory(InventoryRefresh.Items);

            return new MethodResult
            {
                Message = "Success",
                Success = true
            };
        }

        public async Task<MethodResult> RecycleItem(ItemData item, int toDelete)
        {
            InventoryItemSetting itemSetting = UserSettings.ItemSettings.FirstOrDefault(x => x.Id == item.ItemId);

            return itemSetting == null ? new MethodResult() : await RecycleItem(itemSetting, toDelete);
        }

        public async Task<MethodResult> RecycleItem(InventoryItemSetting itemSetting, int toDelete)
        {
            try
            {
                var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
                {
                    RequestType = RequestType.RecycleInventoryItem,
                    RequestMessage = new RecycleInventoryItemMessage
                    {
                        Count = toDelete,
                        ItemId = itemSetting.Id
                    }.ToByteString()
                });

                RecycleInventoryItemResponse recycleInventoryItemResponse = RecycleInventoryItemResponse.Parser.ParseFrom(response);

                switch (recycleInventoryItemResponse.Result)
                {
                    case RecycleInventoryItemResponse.Types.Result.ErrorCannotRecycleIncubators:
                        return new MethodResult();
                    case RecycleInventoryItemResponse.Types.Result.ErrorNotEnoughCopies:
                        return new MethodResult();
                    case RecycleInventoryItemResponse.Types.Result.Success:
                        LogCaller(new LoggerEventArgs(String.Format("Deleted {0} {1}. Remaining {2}", toDelete, itemSetting.FriendlyName, recycleInventoryItemResponse.NewCount), LoggerTypes.Recycle));

                        return new MethodResult
                        {
                            Success = true
                        };
                    case RecycleInventoryItemResponse.Types.Result.Unset:
                        return new MethodResult();
                }
                return new MethodResult();
            }
            catch (Exception ex)
            {
                LogCaller(new LoggerEventArgs(String.Format("Failed to recycle iventory item {0}", itemSetting.FriendlyName), LoggerTypes.Warning, ex));
                return new MethodResult();
            }
        }

        private async Task<MethodResult> UseIncense(ItemId item = ItemId.ItemIncenseOrdinary)
        {
            try
            {
                var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
                {
                    RequestType = RequestType.UseIncense,
                    RequestMessage = new UseIncenseMessage
                    {
                        IncenseType = item
                    }.ToByteString()
                });

                UseIncenseResponse useIncenseResponse = UseIncenseResponse.Parser.ParseFrom(response);

                switch (useIncenseResponse.Result)
                {
                    case UseIncenseResponse.Types.Result.IncenseAlreadyActive:
                        return new MethodResult();
                    case UseIncenseResponse.Types.Result.LocationUnset:
                        return new MethodResult();
                    case UseIncenseResponse.Types.Result.Success:
                        LogCaller(new LoggerEventArgs(String.Format("Used incense {0}.", item), LoggerTypes.Success));
                        return new MethodResult
                        {
                            Success = true
                        };
                    case UseIncenseResponse.Types.Result.NoneInInventory:
                        return new MethodResult();
                    case UseIncenseResponse.Types.Result.Unknown:
                        return new MethodResult();
                }
                return new MethodResult();
            }
            catch (Exception ex)
            {
                LogCaller(new LoggerEventArgs(String.Format("Failed to use incense item {0}", item), LoggerTypes.Warning, ex));
                return new MethodResult();
            }
        }

        public double FilledInventoryStorage()
        {
            if (Items == null || PlayerData == null)
            {
                return 100;
            }

            return (double)Items.Sum(x => x.Count) / PlayerData.MaxItemStorage * 100;
        }

        public double FilledPokemonStorage()
        {
            if (Pokemon == null || PlayerData == null)
            {
                return 100;
            }

            return (double)(Pokemon.Count + Eggs.Count) / PlayerData.MaxPokemonStorage * 100;
        }
    }
}
