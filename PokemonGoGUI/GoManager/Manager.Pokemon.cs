using Google.Protobuf;
using POGOProtos.Data;
using POGOProtos.Enums;
using POGOProtos.Inventory;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Responses;
using POGOProtos.Settings.Master;
using PokemonGoGUI.Enums;
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
        public async Task<MethodResult> TransferPokemon(IEnumerable<PokemonData> pokemonsToTransfer)
        {
            var pokemonToTransfer = pokemonsToTransfer.Where(x => x.Favorite != 1 && !x.IsEgg && string.IsNullOrEmpty(x.DeployedFortId) && x.Id != PlayerData.BuddyPokemon?.Id);
            if (!UserSettings.TransferAtOnce)
            {
                foreach (PokemonData pokemon in pokemonToTransfer)
                {
                    var message = new ReleasePokemonMessage { PokemonId = pokemon.Id };
                    try
                    {
                        var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
                        {
                            RequestType = RequestType.ReleasePokemon,
                            RequestMessage = message.ToByteString()
                        });

                        ReleasePokemonResponse releasePokemonResponse = null;

                        releasePokemonResponse = ReleasePokemonResponse.Parser.ParseFrom(response);
                        if (releasePokemonResponse.Result == ReleasePokemonResponse.Types.Result.Success)
                        {
                            LogCaller(new LoggerEventArgs(
                                String.Format("Successully transferred {0}. Cp: {1}. IV: {2:0.00}%",
                                    pokemon.PokemonId,
                                    pokemon.Cp,
                                    CalculateIVPerfection(pokemon)),
                                LoggerTypes.Transfer));

                            await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));

                            Pokemon.Remove(pokemon);
                        }
                        else
                        {
                            LogCaller(new LoggerEventArgs(String.Format("Faill to transfer {0}. Because: {1}.",
                                pokemon.PokemonId,
                                releasePokemonResponse.Result), LoggerTypes.Warning));
                        }
                    }
                    catch (Exception ex)
                    {
                        LogCaller(new LoggerEventArgs("ReleasePokemonResponse parsing failed because response was empty", LoggerTypes.Exception, ex));

                        //if bug reload all test...
                        UpdateInventory(0);

                        return new MethodResult();
                    }
                }
                return new MethodResult
                {
                    Success = true
                };
            }
            else
            {
                var message = new ReleasePokemonMessage { PokemonIds = { pokemonToTransfer.Where(x => x != null && x.PokemonId != PokemonId.Missingno).Select(x => x.Id) } };
                try
                {
                    var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
                    {
                        RequestType = RequestType.ReleasePokemon,
                        RequestMessage = message.ToByteString()
                    });

                    ReleasePokemonResponse releasePokemonResponse = null;
                    //evites crash
                    try
                    {
                        releasePokemonResponse = ReleasePokemonResponse.Parser.ParseFrom(response);
                    }
                    catch (Exception ex)
                    {
                        LogCaller(new LoggerEventArgs("Faill release pokemon", LoggerTypes.Exception, ex));

                        //if bug reload all test...
                        UpdateInventory(0);

                        return new MethodResult
                        {
                            Message = "Faill transfert."
                        };
                    }
                    if (releasePokemonResponse.Result == ReleasePokemonResponse.Types.Result.Success)
                    {
                        LogCaller(new LoggerEventArgs(
                            String.Format("Successully candy awarded {0} of {1} Pokemons.",
                                releasePokemonResponse.CandyAwarded,
                                pokemonToTransfer.Count()),
                            LoggerTypes.Transfer));

                        await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));

                        foreach (var poktoremove in pokemonsToTransfer)
                            Pokemon.Remove(poktoremove);
                    }
                    else
                    {
                        LogCaller(new LoggerEventArgs(String.Format("Faill to transfer {0} Pokemons.",
                            pokemonToTransfer.Count()), LoggerTypes.Warning));
                    }
                }
                catch (Exception ex)
                {
                    LogCaller(new LoggerEventArgs("ReleasePokemonResponse parsing failed because response was empty", LoggerTypes.Exception, ex));
                    
                    //if bug reload all test...
                    UpdateInventory(0);

                    return new MethodResult();
                }

                return new MethodResult
                {
                    Success = true
                };
            }
        }

        private async Task<MethodResult> TransferFilteredPokemon()
        {
            MethodResult<List<PokemonData>> transferResult = GetPokemonToTransfer();

            if (!transferResult.Success || transferResult.Data.Count == 0)
            {
                return new MethodResult();
            }

            LogCaller(new LoggerEventArgs(String.Format("Transferring {0} pokemon", transferResult.Data.Count), LoggerTypes.Info));

            await TransferPokemon(transferResult.Data);

            return new MethodResult
            {
                Success = true,
                Message = "Success"
            };
        }

        public MethodResult<List<PokemonData>> GetPokemonToTransfer()
        {
            if (!UserSettings.TransferPokemon)
            {
                LogCaller(new LoggerEventArgs("Transferring disabled", LoggerTypes.Debug));

                return new MethodResult<List<PokemonData>>
                {
                    Data = new List<PokemonData>(),
                    Message = "Transferring disabled",
                    Success = true
                };
            }

            if (!Pokemon.Any()) {
                LogCaller(new LoggerEventArgs("You have no pokemon", LoggerTypes.Info));

                return new MethodResult<List<PokemonData>> {
                    Message = "You have no pokemon"
                };
            }

            var pokemonToTransfer = new List<PokemonData>();

            IEnumerable<IGrouping<PokemonId, PokemonData>> groupedPokemon = Pokemon.GroupBy(x => x.PokemonId);

            foreach (IGrouping<PokemonId, PokemonData> group in groupedPokemon)
            {
                TransferSetting settings = UserSettings.TransferSettings.FirstOrDefault(x => x.Id == group.Key);

                if (settings == null)
                {
                    LogCaller(new LoggerEventArgs(String.Format("Failed to find transfer settings for pokemon {0}", group.Key), LoggerTypes.Warning));

                    continue;
                }

                if (!settings.Transfer)
                {
                    continue;
                }

                switch (settings.Type)
                {
                    case TransferType.All:
                        pokemonToTransfer.AddRange(group.ToList());
                        break;
                    case TransferType.BelowCP:
                        pokemonToTransfer.AddRange(GetPokemonBelowCP(group, settings.MinCP));
                        break;
                    case TransferType.BelowIVPercentage:
                        pokemonToTransfer.AddRange(GetPokemonBelowIVPercent(group, settings.IVPercent));
                        break;
                    case TransferType.KeepPossibleEvolves:
                        pokemonToTransfer.AddRange(GetPokemonByPossibleEvolve(group, settings.KeepMax));
                        break;
                    case TransferType.KeepStrongestX:
                        pokemonToTransfer.AddRange(GetPokemonByStrongest(group, settings.KeepMax));
                        break;
                    case TransferType.KeepXHighestIV:
                        pokemonToTransfer.AddRange(GetPokemonByIV(group, settings.KeepMax));
                        break;
                    case TransferType.BelowCPAndIVAmount:
                        pokemonToTransfer.AddRange(GetPokemonBelowCPIVAmount(group, settings.MinCP, settings.IVPercent));
                        break;
                    case TransferType.BelowCPOrIVAmount:
                        pokemonToTransfer.AddRange(GetPokemonBelowIVPercent(group, settings.IVPercent));
                        pokemonToTransfer.AddRange(GetPokemonBelowCP(group, settings.MinCP));
                        pokemonToTransfer = pokemonToTransfer.DistinctBy(x => x.Id).ToList();
                        break;
                    case TransferType.Slashed:
                        pokemonToTransfer.AddRange(group.ToList());
                        pokemonToTransfer = pokemonToTransfer.DistinctBy(x => x.IsBad).ToList();
                        break;
                }
            }

            if (UserSettings.TransferSlashPokemons)
            {
                var slashPokemons = Pokemon.Where(x => x.IsBad);
                foreach (var slashPokemon in slashPokemons)
                {
                    var inlist = pokemonToTransfer.FirstOrDefault(x => x.Id == slashPokemon.Id);
                    if (inlist == null)
                    {
                        pokemonToTransfer.Add(slashPokemon);
                    }
                }
            }

            return new MethodResult<List<PokemonData>>
            {
                Data = pokemonToTransfer,
                Message = String.Format("Found {0} pokemon to transfer", pokemonToTransfer.Count),
                Success = true
            };
        }

        private List<PokemonData> GetPokemonBelowCPIVAmount(IGrouping<PokemonId, PokemonData> pokemon, int minCp, double percent)
        {
            var toTransfer = new List<PokemonData>();

            foreach (PokemonData pData in pokemon)
            {
                double perfectResult = CalculateIVPerfection(pData);

                if (perfectResult >= 0 && perfectResult < percent && pData.Cp < minCp)
                {
                    toTransfer.Add(pData);
                }
            }

            return toTransfer;
        }

        private List<PokemonData> GetPokemonBelowCP(IGrouping<PokemonId, PokemonData> pokemon, int minCp)
        {
            return pokemon.Where(x => x.Cp < minCp).ToList();
        }

        private List<PokemonData> GetPokemonBelowIVPercent(IGrouping<PokemonId, PokemonData> pokemon, double percent)
        {
            var toTransfer = new List<PokemonData>();

            foreach (PokemonData pData in pokemon)
            {
                double perfectResult = CalculateIVPerfection(pData);

                if (perfectResult >= 0 && perfectResult < percent)
                {
                    toTransfer.Add(pData);
                }
            }

            return toTransfer;
        }

        private List<PokemonData> GetPokemonByStrongest(IGrouping<PokemonId, PokemonData> pokemon, int amount)
        {
            return pokemon.OrderByDescending(x => x.Cp).Skip(amount).ToList();
        }

        private List<PokemonData> GetPokemonByIV(IGrouping<PokemonId, PokemonData> pokemon, int amount)
        {
            if (!pokemon.Any())
            {
                return new List<PokemonData>();
            }

            //Test out first one to make sure things are correct
            double perfectResult = CalculateIVPerfection(pokemon.First());

            return pokemon.OrderByDescending(x => CalculateIVPerfection(x)).ThenByDescending(x => x.Cp).Skip(amount).ToList();
        }

        private List<PokemonData> GetPokemonByPossibleEvolve(IGrouping<PokemonId, PokemonData> pokemon, int limit)
        {
            PokemonSettings setting = null;
            if (!PokeSettings.TryGetValue(pokemon.Key, out setting))
            {
                LogCaller(new LoggerEventArgs(String.Format("Failed to find settings for pokemon {0}", pokemon.Key), LoggerTypes.Info));

                return new List<PokemonData>();
            }

            Candy pokemonCandy = PokemonCandy.FirstOrDefault(x => x.FamilyId == setting.FamilyId);

            int candyToEvolve = setting.CandyToEvolve;
            int totalPokemon = pokemon.Count();
            int totalCandy = pokemonCandy.Candy_;

            if (candyToEvolve == 0)
            {
                return new List<PokemonData>();
            }

            int maxPokemon = totalCandy / candyToEvolve;

            if (maxPokemon > limit)
            {
                maxPokemon = limit;
            }

            return pokemon.OrderByDescending(x => x.Cp).Skip(maxPokemon).ToList();
        }

        // NOTE: this is the real IV Percent, using only Individual values.
        public static double CalculateIVPerfection(PokemonData pokemon)
        {
            // NOTE: 45 points = 15 at points + 15 def points + 15 sta points
            //  100/45 simplifying is 20/9
            return ((double)pokemon.IndividualAttack + pokemon.IndividualDefense + pokemon.IndividualStamina) * 20 / 9;
        }

        // This other Percent gives different IV % for the same IVs depending of the pokemon level.
        public MethodResult<double> CalculateIVPerfectionUsingMaxCP(PokemonData pokemon)
        {
            MethodResult<PokemonSettings> settingResult = GetPokemonSetting(pokemon.PokemonId);

            if (!settingResult.Success)
            {
                return new MethodResult<double>
                {
                    Data = -1,
                    Message = settingResult.Message
                };
            }

            /*
            if (Math.Abs(pokemon.CpMultiplier + pokemon.AdditionalCpMultiplier) <= 0)
            {
                double perfection = (double)(pokemon.IndividualAttack * 2 + pokemon.IndividualDefense + pokemon.IndividualStamina) / (4.0 * 15.0) * 100.0;

                return new MethodResult<double>
                {
                    Data = perfection,
                    Message = "Success",
                    Success = true
                };
            }*/

            double maxCp = CalculateMaxCpMultiplier(pokemon);
            double minCp = CalculateMinCpMultiplier(pokemon);
            double curCp = CalculateCpMultiplier(pokemon);

            double perfectPercent = (curCp - minCp) / (maxCp - minCp) * 100.0;

            return new MethodResult<double>
            {
                Data = perfectPercent,
                Message = "Success",
                Success = true
            };
        }

        public async Task<MethodResult> FavoritePokemon(IEnumerable<PokemonData> pokemonToFavorite, bool favorite = true)
        {
            foreach (PokemonData pokemon in pokemonToFavorite)
            {
                bool isFavorited = true;
                string message = "unfavorited";

                if (pokemon.Favorite == 0)
                {
                    isFavorited = false;
                    message = "favorited";
                }

                if (isFavorited == favorite)
                {
                    continue;
                }

                try
                {
                    var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
                    {
                        RequestType = RequestType.SetFavoritePokemon,
                        RequestMessage = new SetFavoritePokemonMessage
                        {
                            PokemonId = (long)pokemon.Id,
                            IsFavorite = favorite
                        }.ToByteString()
                    });

                    SetFavoritePokemonResponse setFavoritePokemonResponse = null;

                    setFavoritePokemonResponse = SetFavoritePokemonResponse.Parser.ParseFrom(response);
                    LogCaller(new LoggerEventArgs(
                        String.Format("Successully {3} {0}. Cp: {1}. IV: {2:0.00}%",
                            pokemon.PokemonId,
                            pokemon.Cp,
                            CalculateIVPerfection(pokemon), message),
                        LoggerTypes.Info));

                    await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));

                    return new MethodResult
                    {
                        Success = true
                    };
                }
                catch (Exception ex)
                {
                    LogCaller(new LoggerEventArgs("Favorite request failed", LoggerTypes.Exception, ex));
                    return new MethodResult();
                }
            }
            return new MethodResult();
        }

        private double CalculateMaxCpMultiplier(PokemonData poke)
        {
            PokemonSettings pokemonSettings = GetPokemonSetting(poke.PokemonId).Data;

            return (pokemonSettings.Stats.BaseAttack + 15) * Math.Sqrt(pokemonSettings.Stats.BaseDefense + 15) *
            Math.Sqrt(pokemonSettings.Stats.BaseStamina + 15);
        }

        private double CalculateCpMultiplier(PokemonData poke)
        {
            PokemonSettings pokemonSettings = GetPokemonSetting(poke.PokemonId).Data;

            return (pokemonSettings.Stats.BaseAttack + poke.IndividualAttack) *
            Math.Sqrt(pokemonSettings.Stats.BaseDefense + poke.IndividualDefense) *
            Math.Sqrt(pokemonSettings.Stats.BaseStamina + poke.IndividualStamina);
        }

        private double CalculateMinCpMultiplier(PokemonData poke)
        {
            PokemonSettings pokemonSettings = GetPokemonSetting(poke.PokemonId).Data;

            return pokemonSettings.Stats.BaseAttack * Math.Sqrt(pokemonSettings.Stats.BaseDefense) * Math.Sqrt(pokemonSettings.Stats.BaseStamina);
        }
    }
}
