using POGOProtos.Networking.Responses;
using PokemonGo.RocketAPI;
using PokemonGoGUI.GoManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGoGUI.GoManager
{
    public partial class Manager
    {
        public async Task<MethodResult> UpdateDetails()
        {
            MethodResult echoResult = await SendEcho();

            if(!echoResult.Success)
            {
                _client.Logout();
            }

            if(!_client.LoggedIn)
            {
                MethodResult loginResult = await Login();

                if(!loginResult.Success)
                {
                    return loginResult;
                }
            }

            await GetProfile(); //Don't care if it fails

            await Task.Delay(500);

            await GetItemTemplates(); //Don't care if it fails

            await Task.Delay(500);

            await GetInventory();

            return new MethodResult
            {
                Success = true
            };
        }

        public async Task<MethodResult> GetProfile()
        {
            try
            {
                if (!_client.LoggedIn)
                {
                    MethodResult result = await Login();

                    if (!result.Success)
                    {
                        return result;
                    }
                }

                GetPlayerResponse response = await _client.Player.GetPlayer();

                if(response.Success)
                {
                    PlayerData = response.PlayerData;
                }

                return new MethodResult
                {
                    Success = response.Success
                };

            }
            catch(Exception ex)
            {
                LogCaller(new LoggerEventArgs("Failed to get player stats", LoggerTypes.Exception, ex));

                return new MethodResult
                {
                    Message = "Failed to get player stats"
                };
            }
        }
    }
}
