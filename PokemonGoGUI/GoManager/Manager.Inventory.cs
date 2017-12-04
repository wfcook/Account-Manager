using Google.Protobuf;
using POGOProtos.Data;
using POGOProtos.Inventory;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Responses;
using PokemonGoGUI.Exceptions;
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
        public async Task<MethodResult<List<InventoryItem>>> UpdateInventory()
        {
            try
            {
                if (!_client.LoggedIn)
                {
                    MethodResult result = await Login();

                    if (!result.Success)
                    {
                        return new MethodResult<List<InventoryItem>>
                        {
                            Message = result.Message,
                            Success = result.Success
                        };
                    }
                }

                try
                {
                    AllItems = _client.ClientSession.Player.Inventory.InventoryItems.ToList();

                    await UpdatePlayerStats(false);
                    await UpdatePokemon(false);
                    await UpdatePokedex(false);
                    await UpdatePokemonCandy(false);
                    await UpdateItemList(false);
                    await UpdateIncubators(false);

                    InventoryUpdateCaller(EventArgs.Empty);

                    return new MethodResult<List<InventoryItem>>
                    {
                        Data = AllItems,
                        Message = "Successfully grabbed inventory items",
                        Success = true
                    };
                }
                catch (Exception)
                {
                    return new MethodResult<List<InventoryItem>>
                    {
                        Message = "Failed to get inventory",
                        Success = false
                    };
                }
            }
            catch (Exception)
            {
                return new MethodResult<List<InventoryItem>>
                {
                    Message = "Failed to get inventory",
                    Success = false
                };
            }
        }

        public async Task UpdateItemList(bool updateInventory)
        {
            if (!updateInventory && AllItems != null)
            {
                List<ItemData> items = AllItems.Where(x => x.InventoryItemData.Item != null).Select(x => x.InventoryItemData.Item).ToList();

                Items = items;
            }
            else
            {
                await UpdateInventory();
            }
        }

        public async Task UpdatePlayerStats(bool updateInventory)
        {
            if (!updateInventory && AllItems != null)
            {
                InventoryItem item = AllItems.FirstOrDefault(
                    x => x.InventoryItemData.PlayerStats != null);

                //Saves for viewing
                Stats = item.InventoryItemData.PlayerStats;
            }
            else
            {
                await UpdateInventory();
            }
        }

        public async Task UpdatePokemon(bool updateInventory)
        {
            if (!updateInventory && AllItems != null)
            {
                List<PokemonData> pokemon = AllItems.Where(x => x.InventoryItemData.PokemonData != null && !x.InventoryItemData.PokemonData.IsEgg).Select(x => x.InventoryItemData.PokemonData).ToList();
                List<PokemonData> eggs = AllItems.Where(x => x.InventoryItemData.PokemonData != null && x.InventoryItemData.PokemonData.IsEgg).Select(x => x.InventoryItemData.PokemonData).ToList();

                Pokemon = pokemon;
                Eggs = eggs;

            }
            else
            {
                await UpdateInventory();
            }
        }

        public async Task UpdatePokedex(bool updateInventory)
        {
            if (!updateInventory && AllItems != null)
            {
                List<PokedexEntry> pokedex = AllItems.Where(x => x.InventoryItemData.PokedexEntry != null).Select(x => x.InventoryItemData.PokedexEntry).ToList();

                Pokedex = pokedex;
            }
            else
            {
                await UpdateInventory();
            }
        }

        public async Task UpdateIncubators(bool updateInventory)
        {
            if (!updateInventory && AllItems != null)
            {
                List<EggIncubator> incubators = AllItems.First(x => x.InventoryItemData.EggIncubators != null).InventoryItemData.EggIncubators.EggIncubator.ToList();

                Incubators = incubators;
            }
            else
            {
                await UpdateInventory();
            }
        }

        public async Task UpdatePokemonCandy(bool updateInventory)
        {
            if (!updateInventory && AllItems != null)
            {
                List<Candy> pokemonCandy = AllItems.Where(x => x.InventoryItemData.Candy != null).Select(x => x.InventoryItemData.Candy).ToList();

                PokemonCandy = pokemonCandy;
            }
            else
            {
                await UpdateInventory();
            }
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

            MethodResult<List<InventoryItem>> inventoryResponse = await UpdateInventory();

            if (!inventoryResponse.Success)
            {
                return inventoryResponse;
            }


            await UpdateItemList(false);

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

            if(itemSetting == null)
            {
                return new MethodResult();
            }

            return await RecycleItem(itemSetting, toDelete);
        }

        public async Task<MethodResult> RecycleItem(InventoryItemSetting itemSetting, int toDelete)
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

            try
            {
                recycleInventoryItemResponse = RecycleInventoryItemResponse.Parser.ParseFrom(response);
                LogCaller(new LoggerEventArgs(String.Format("Deleted {0} {1}. Remaining {2}", toDelete, itemSetting.FriendlyName, recycleInventoryItemResponse.NewCount), LoggerTypes.Recycle));

                return new MethodResult
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                if (response.IsEmpty)
                    LogCaller(new LoggerEventArgs(String.Format("Failed to recycle iventory item {0}", itemSetting.FriendlyName), LoggerTypes.Warning, ex));

                return new MethodResult();
            }
        }

        public double FilledInventorySpace()
        {
            if(Items == null || PlayerData == null)
            {
                return 100;
            }

            return (double)Items.Sum(x => x.Count) / PlayerData.MaxItemStorage * 100;
        }
    }
}
