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

namespace PokemonGoGUI.GoManager
{
    public partial class Manager
    {
        public void UpdateInventory()
        {
            LogCaller(new LoggerEventArgs("Updating inventory.", LoggerTypes.Debug));

            try
            {
                UpdatePlayerStats();
                UpdatePokemon();
                UpdatePokedex();
                UpdatePokemonCandy();
                UpdateItemList();
                UpdateIncubators();

            }
            catch (Exception ex1)
            {
                AccountState = Enums.AccountState.PokemonBanAndPokestopBanTemp;
                LogCaller(new LoggerEventArgs(String.Format("Failed updating inventory."), LoggerTypes.Debug, ex1));
                Stop();
            }
        }

        public void UpdateItemList()
        {
            Items = _client.ClientSession.Player.Inventory.InventoryItems.Where(x => x.InventoryItemData?.Item != null).Select(x => x.InventoryItemData.Item).ToList();
        }

        public void UpdatePlayerStats()
        {
            InventoryItem item = _client.ClientSession.Player.Inventory.InventoryItems.FirstOrDefault(
                                     x => x.InventoryItemData?.PlayerStats != null);

            Stats = item.InventoryItemData.PlayerStats;
        }


        public void UpdatePokemon()
        {
            /*if (_client==null)
                LogCaller(new LoggerEventArgs("Invalid _client Object ", LoggerTypes.Debug));
            if (_client.ClientSession==null)
                LogCaller(new LoggerEventArgs("Invalid ClientSession Object ", LoggerTypes.Debug));
            if (_client.ClientSession.Player==null)
                LogCaller(new LoggerEventArgs("Invalid Player Object ", LoggerTypes.Debug));
            if (_client.ClientSession.Player.Inventory==null)
                LogCaller(new LoggerEventArgs("Invalid Inventory Object", LoggerTypes.Debug));
            if (_client.ClientSession.Player.Inventory.InventoryItems==null)
                LogCaller(new LoggerEventArgs("Invalid InventoryItems Object", LoggerTypes.Debug));
                */
            var pokemonDatas = _client.ClientSession.Player.Inventory.InventoryItems.Select(item => item.InventoryItemData?.PokemonData);
            Pokemon = pokemonDatas.Where(item => item != null && !item.IsEgg).ToList();
            Eggs = pokemonDatas?.Where(item => item != null && item.IsEgg).ToList();
        }

        public void UpdatePokedex()
        {
            Pokedex = _client.ClientSession.Player.Inventory.InventoryItems.Where(x => x.InventoryItemData?.PokedexEntry != null).Select(x => x.InventoryItemData.PokedexEntry).ToList();
        }

        public void UpdateIncubators()
        {
            Incubators = _client.ClientSession.Player.Inventory.InventoryItems.First(x => x.InventoryItemData?.EggIncubators != null).InventoryItemData.EggIncubators.EggIncubator.ToList();
        }

        public void UpdatePokemonCandy()
        {
            PokemonCandy = _client.ClientSession.Player.Inventory.InventoryItems.Where(x => x.InventoryItemData?.Candy != null).Select(x => x.InventoryItemData.Candy)?.ToList();
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

            return (double)(Pokemon.Count + Eggs?.Count) / PlayerData.MaxPokemonStorage * 100;
        }
    }
}
