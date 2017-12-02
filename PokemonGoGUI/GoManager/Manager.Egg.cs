using Google.Protobuf;
using POGOProtos.Data;
using POGOProtos.Inventory;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Responses;
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

            var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.UseItemEggIncubator,
                RequestMessage = new UseItemEggIncubatorMessage
                {
                    ItemId = incubatorResponse.Data.Id,
                    PokemonId = egg.Id
                }.ToByteString()
            });

            UseItemEggIncubatorResponse useItemEggIncubatorResponse = null;

            try
            {
                useItemEggIncubatorResponse = UseItemEggIncubatorResponse.Parser.ParseFrom(response);
                LogCaller(new LoggerEventArgs(String.Format("Incubating egg in {0}. Pokmeon Id: {1}", useItemEggIncubatorResponse.EggIncubator.ItemId.ToString().Replace("ItemIncubator", ""), useItemEggIncubatorResponse.EggIncubator.PokemonId), LoggerTypes.Incubate));

                return new MethodResult
                {
                    Message = "Success",
                    Success = true
                };
            }
            catch (Exception ex)
            {
                if (response.IsEmpty)
                    LogCaller(new LoggerEventArgs("UseItemEggIncubatorResponse parsing failed because response was empty", LoggerTypes.Exception, ex));

                return new MethodResult();
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
