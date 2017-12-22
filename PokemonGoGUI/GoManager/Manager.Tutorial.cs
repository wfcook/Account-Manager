using Google.Protobuf;
using POGOProtos.Data.Player;
using POGOProtos.Enums;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Responses;
using PokemonGoGUI.Extensions;
using PokemonGoGUI.GoManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonGoGUI.GoManager
{
    public partial class Manager
    {
        public async Task<MethodResult> MarkStartUpTutorialsComplete(bool forceAvatarUpdate)
        {
            MethodResult reauthResult = await CheckReauthentication();

            if (!reauthResult.Success)
            {
                return reauthResult;
            }

            bool success = true;

            if (PlayerData == null)
            {
                MethodResult result = await GetPlayer();

                if (!result.Success)
                {
                    return result;
                }

                await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
            }


            List<TutorialState> startupTutorials = new List<TutorialState>
            {
                TutorialState.AccountCreation,
                TutorialState.AvatarSelection,
                TutorialState.FirstTimeExperienceComplete,
                TutorialState.LegalScreen,
                TutorialState.NameSelection,
                TutorialState.PokemonCapture
            };

            List<TutorialState> completedTutorials = PlayerData.TutorialState.ToList();

            foreach (TutorialState completed in completedTutorials)
            {
                startupTutorials.Remove(completed);
            }

            if (!completedTutorials.Contains(TutorialState.AvatarSelection) || forceAvatarUpdate)
            {
                await SetPlayerAvatar();
            }
            else
            {
                LogCaller(new LoggerEventArgs("Avatar already set", LoggerTypes.Info));
            }

            if (startupTutorials.Count != 0)
            {
                MethodResult result = await MarkTutorialsComplete(startupTutorials);

                if (!result.Success)
                {
                    success = false;
                }

                await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
            }
            else
            {
                LogCaller(new LoggerEventArgs("Startup tutorials already complete", LoggerTypes.Info));
            }

            //Complete encounter
            List<PokemonId> pokemon = new List<PokemonId>
            {
                PokemonId.Charmander,
                PokemonId.Bulbasaur,
                PokemonId.Squirtle,
                PokemonId.Pikachu
            };

            PokemonId chosenPokemon = PokemonId.Pikachu;

            lock (_rand)
            {
                chosenPokemon = pokemon[_rand.Next(0, pokemon.Count)];
            }

            if (!completedTutorials.Contains(TutorialState.PokemonCapture))
            {
                MethodResult result = await CompleteEncounterTutorial(chosenPokemon);

                if (!result.Success)
                {
                    success = false;
                }

                await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));
            }
            else
            {
                LogCaller(new LoggerEventArgs("Encounter tutorial already complete", LoggerTypes.Info));
            }

            if (success)
            {
                LogCaller(new LoggerEventArgs("Finished tutorial", LoggerTypes.Debug));
            }
            else
            {
                LogCaller(new LoggerEventArgs("Error occured when completing tutorial", LoggerTypes.Warning));
            }

            return new MethodResult()
            {
                Success = true
            };
        }

        public async Task<MethodResult> MarkTutorialsComplete(List<TutorialState> tutorials)
        {
            try
            {
                var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
                {
                    RequestType = RequestType.MarkTutorialComplete,
                    RequestMessage = new MarkTutorialCompleteMessage
                    {
                        SendMarketingEmails = false,
                        SendPushNotifications = false,
                        TutorialsCompleted = { tutorials }
                    }.ToByteString()
                });

                MarkTutorialCompleteResponse markTutorialCompleteResponse = null;

                markTutorialCompleteResponse = MarkTutorialCompleteResponse.Parser.ParseFrom(response);
                LogCaller(new LoggerEventArgs("Tutorial completion request wasn't successful", LoggerTypes.Success));

                _client.ClientSession.Player.Data = markTutorialCompleteResponse.PlayerData;
                PlayerData = _client.ClientSession.Player.Data;

                return new MethodResult
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                LogCaller(new LoggerEventArgs("Failed to mark tutorials as complete", LoggerTypes.Exception, ex));

                return new MethodResult();
            }
        }

        public async Task<MethodResult> CompleteEncounterTutorial(PokemonId pokemon)
        {
            try
            {
                var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
                {
                    RequestType = RequestType.EncounterTutorialComplete,
                    RequestMessage = new EncounterTutorialCompleteMessage
                    {
                        PokemonId = pokemon
                    }.ToByteString()
                });

                EncounterTutorialCompleteResponse encounterTutorialCompleteResponse = null;

                encounterTutorialCompleteResponse = EncounterTutorialCompleteResponse.Parser.ParseFrom(response);
                LogCaller(new LoggerEventArgs(String.Format("Caught a {0}", pokemon), LoggerTypes.Success));

                return new MethodResult
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                LogCaller(new LoggerEventArgs("EncounterTutorialCompleteResponse parsing failed because response was empty", LoggerTypes.Exception, ex));

                return new MethodResult();
            }
        }

        public async Task<MethodResult> SetPlayerAvatar()
        {
            try
            {
                PlayerAvatar avatar = new PlayerAvatar();

                var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
                {
                    RequestType = RequestType.SetAvatar,
                    RequestMessage = new SetAvatarMessage
                    {
                        PlayerAvatar = avatar
                    }.ToByteString()
                });

                SetAvatarResponse setAvatarResponse = null;

                setAvatarResponse = SetAvatarResponse.Parser.ParseFrom(response);
                LogCaller(new LoggerEventArgs("Avatar set to defaults", LoggerTypes.Success));


                return new MethodResult
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                LogCaller(new LoggerEventArgs("Failed to set avatar", LoggerTypes.Exception, ex));

                return new MethodResult();
            }
        }

        public async Task<MethodResult> SetAvatarItemAsViewed()
        {
            try
            {
                var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
                {
                    RequestType = RequestType.SetAvatarItemAsViewed,
                    RequestMessage = new SetAvatarItemAsViewedMessage
                    {
                        //TODO: get avatarids 
                        //AvatarTemplateId = { }
                    }.ToByteString()
                });

                SetAvatarItemAsViewedResponse setAvatarItemAsViewedResponse = null;

                setAvatarItemAsViewedResponse = SetAvatarItemAsViewedResponse.Parser.ParseFrom(response);
                LogCaller(new LoggerEventArgs("Set avatar item as viewed", LoggerTypes.Success));

                return new MethodResult
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                LogCaller(new LoggerEventArgs("SetAvatarItemAsViewedResponse parsing failed because response was empty", LoggerTypes.Exception, ex));

                return new MethodResult();
            }
        }
    }
}
