using POGOProtos.Data;
using POGOProtos.Inventory;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Responses;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Exceptions;
using PokemonGo.RocketAPI.Helpers;
using PokemonGoGUI.GoManager.Models;
using PokemonGoGUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGoGUI.GoManager
{
    public partial class Manager
    {
        public async Task<MethodResult<List<InventoryItem>>> GetInventory()
        {
            try
            {
                if(!_client.LoggedIn)
                {
                    MethodResult result = await Login();

                    if(!result.Success)
                    {
                        return new MethodResult<List<InventoryItem>>
                        {
                            Message = result.Message
                        };
                    }
                }

                GetInventoryResponse inventory = await _client.Inventory.GetInventory();

                if (inventory == null || inventory.InventoryDelta == null)
                {
                    LogCaller(new LoggerEventArgs("Inventory request returned invalid data", LoggerTypes.Warning));

                    return new MethodResult<List<InventoryItem>>
                    {
                        Message = "Failed to get inventory."
                    };
                }

                List<InventoryItem> items = inventory.InventoryDelta.InventoryItems.ToList();
                AllItems = items;

                await UpdatePlayerStats(false);
                await UpdatePokemon(false);
                await UpdatePokedex(false);
                await UpdatePokemonCandy(false);
                await UpdateItemList(false);
                await UpdateIncubators(false);

                InventoryUpdateCaller(EventArgs.Empty);

                return new MethodResult<List<InventoryItem>>
                {
                    Data = items,
                    Message = "Successfully grabbed inventory items",
                    Success = true
                };
            }
            catch(InvalidResponseException ex)
            {
                LogCaller(new LoggerEventArgs("Inventory request has returned an invalid response", LoggerTypes.Warning, ex));

                return new MethodResult<List<InventoryItem>>
                {
                    Message = "Failed to get inventory."
                };
            }
            catch (Exception ex)
            {
                LogCaller(new LoggerEventArgs("Failed to get inventory", LoggerTypes.Exception, ex));

                return new MethodResult<List<InventoryItem>>
                {
                    Message = "Failed to get inventory"
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
                await GetInventory();
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
                await GetInventory();
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
                await GetInventory();
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
                await GetInventory();
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
                await GetInventory();
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
                await GetInventory();
            }
        }

        public async Task<MethodResult> RecycleItems()
        {
            if (!UserSettings.RecycleItems)
            {
                return new MethodResult
                {
                    Message = "Item deletion not enabled"
                };
            }

            MethodResult<List<InventoryItem>> inventoryResponse = await GetInventory();

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

                try
                {
                    RecycleInventoryItemResponse response = await _client.Inventory.RecycleItem(itemSetting.Id, toDelete);

                    if (response.Result == RecycleInventoryItemResponse.Types.Result.Success)
                    {
                        LogCaller(new LoggerEventArgs(String.Format("Deleted {0} {1}. Remaining {2}", toDelete, itemSetting.FriendlyName, response.NewCount), LoggerTypes.Recycle));
                    }
                    else
                    {
                        LogCaller(new LoggerEventArgs(String.Format("Failed to delete {0}. Message: {1}", itemSetting.FriendlyName, response.Result), LoggerTypes.Warning));
                    }

                    await Task.Delay(UserSettings.DelayBetweenPlayerActions);
                }
                catch (Exception ex)
                {
                    LogCaller(new LoggerEventArgs(String.Format("Failed to recycle iventory item {0}", itemSetting.FriendlyName), LoggerTypes.Warning, ex));
                }
            }


            return new MethodResult
            {
                Message = "Success",
                Success = true
            };
        }
    }
}
