using POGOProtos.Data;
using POGOProtos.Inventory;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Responses;
using PokemonGo.RocketAPI;
using PokemonGoGUI.GoManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonGoGUI.GoManager
{
    public partial class Manager
    {
        private async Task<MethodResult> IncubateEggs()
        {
            if(!UserSettings.IncubateEggs)
            {
                LogCaller(new LoggerEventArgs("Incubating disabled", LoggerTypes.Info));

                return new MethodResult
                {
                    Message = "Incubate eggs disabled",
                    Success = true
                };
            }

            MethodResult<EggIncubator> incubatorResponse = GetIncubator();

            if(!incubatorResponse.Success)
            {
                return new MethodResult
                {
                    Message = incubatorResponse.Message,
                    Success = true
                };
            }

            PokemonData egg = Eggs.FirstOrDefault(x => String.IsNullOrEmpty(x.EggIncubatorId));

            if (egg == null)
            {
                return new MethodResult
                {
                    Message = "No egg to incubate",
                    Success = true
                };
            }

            try
            {
                UseItemEggIncubatorResponse response = await _client.Inventory.UseItemEggIncubator(incubatorResponse.Data.Id, egg.Id);

                if(response.Result != UseItemEggIncubatorResponse.Types.Result.Success)
                {
                    LogCaller(new LoggerEventArgs(String.Format("Failed to incubate egg. Response: {0}", response.Result), LoggerTypes.Warning));

                    return new MethodResult();
                }

                LogCaller(new LoggerEventArgs(String.Format("Incubating egg in {0}. Pokmeon Id: {1}", response.EggIncubator.ItemId.ToString().Replace("ItemIncubator", ""), response.EggIncubator.PokemonId), LoggerTypes.Incubate));

                return new MethodResult
                {
                    Message = "Success",
                    Success = true
                };
            }
            catch(Exception ex)
            {
                LogCaller(new LoggerEventArgs("Incubate egg request failed", LoggerTypes.Exception, ex));

                return new MethodResult
                {
                    Message = "Incubate egg request failed"
                };
            }
        }

        private MethodResult<EggIncubator> GetIncubator()
        {
            if(Incubators == null)
            {
                return new MethodResult<EggIncubator>();
            }

            EggIncubator unusedUnlimitedIncubator = Incubators.FirstOrDefault(x => x.ItemId == ItemId.ItemIncubatorBasicUnlimited && x.PokemonId == 0);

            if(unusedUnlimitedIncubator != null)
            {
                return new MethodResult<EggIncubator>
                {
                    Data = unusedUnlimitedIncubator,
                    Success = true
                };
            }

            IEnumerable<EggIncubator> incubators = Incubators.Where(x => x.ItemId == ItemId.ItemIncubatorBasic && x.PokemonId == 0);

            foreach(EggIncubator incubator in incubators)
            {
                return new MethodResult<EggIncubator>
                {
                    Data = incubator,
                    Success = true
                };
            }

            return new MethodResult<EggIncubator>
            {
                Message = "No unused incubators"
            };
        }
    }
}
