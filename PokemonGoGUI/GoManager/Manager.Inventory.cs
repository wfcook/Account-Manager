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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonGoGUI.GoManager
{
    public partial class Manager
    {
       
        public IEnumerable<ItemData> GetItems()
        {
            return _client.ClientSession.Player.Inventory.InventoryItems.Where(x => x.InventoryItemData?.Item != null).Select(x => x.InventoryItemData.Item);
        }

        public POGOProtos.Data.Player.PlayerStats GetPlayerStats()
        {
            InventoryItem item = _client.ClientSession.Player.Inventory.InventoryItems.FirstOrDefault(
                                     x => x.InventoryItemData?.PlayerStats != null);

            return item.InventoryItemData.PlayerStats;
        }


        public IEnumerable<PokemonData> GetPokemons()
        {
            var pokemonDatas = _client.ClientSession.Player.Inventory.InventoryItems.Select(item => item.InventoryItemData?.PokemonData);
            return pokemonDatas.Where(item => item != null && !item.IsEgg);
        }
        public IEnumerable<PokemonData> GetEggs()
        {
            var pokemonDatas = _client.ClientSession.Player.Inventory.InventoryItems.Select(item => item.InventoryItemData?.PokemonData);
            return pokemonDatas?.Where(item => item != null && item.IsEgg);
        }

        public IEnumerable<PokedexEntry> GetPokedex()
        {
            return _client.ClientSession.Player.Inventory.InventoryItems.Where(x => x.InventoryItemData?.PokedexEntry != null).Select(x => x.InventoryItemData.PokedexEntry);
        }

        public IEnumerable<EggIncubator> GetIncubators()
        {
            return _client.ClientSession.Player.Inventory.InventoryItems.First(x => x.InventoryItemData?.EggIncubators != null).InventoryItemData.EggIncubators.EggIncubator;
        }

        public IEnumerable<Candy> GetPokemonCandies()
        {
            return _client.ClientSession.Player.Inventory.InventoryItems.Where(x => x.InventoryItemData?.Candy != null).Select(x => x.InventoryItemData.Candy);
        }

        public async Task<MethodResult> RecycleFilteredItems()
        {
            if (!UserSettings.RecycleItems)
            {
                return new MethodResult
                {
                    Message = "Item deletion not enabled"
                };
            }


            foreach (ItemData item in GetItems())
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

                RecycleInventoryItemResponse recycleInventoryItemResponse = null;

                recycleInventoryItemResponse = RecycleInventoryItemResponse.Parser.ParseFrom(response);
                LogCaller(new LoggerEventArgs(String.Format("Deleted {0} {1}. Remaining {2}", toDelete, itemSetting.FriendlyName, recycleInventoryItemResponse.NewCount), LoggerTypes.Recycle));

                return new MethodResult
                {
                    Success = true
                };

            }
            catch (Exception ex)
            {
                LogCaller(new LoggerEventArgs(String.Format("Failed to recycle iventory item {0}", itemSetting.FriendlyName), LoggerTypes.Warning, ex));

                return new MethodResult();
            }
        }

        public double FilledInventoryStorage()
        {
            if ( PlayerData == null)
            {
                return 100;
            }

            return (double)GetItems().Sum(x => x.Count) / PlayerData.MaxItemStorage * 100;
        }
        public double FilledPokemonStorage()
        {
            if (PlayerData == null)
            {
                return 100;
            }

            return (double)(GetPokemons().Count() + GetEggs().Count()) / PlayerData.MaxPokemonStorage * 100;
        }
    }
}
