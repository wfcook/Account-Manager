using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POGOProtos.Enums;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Responses;

namespace PokemonGo.RocketAPI.Rpc
{
    public class Inventory : BaseRpc
    {
        public Inventory(Client client) : base(client)
        {
        }

        public async Task<ReleasePokemonResponse> TransferPokemon(ulong pokemonId)
        {
            var message = new ReleasePokemonMessage
            {
                PokemonId = pokemonId
            };

            return await PostProtoPayload<Request, ReleasePokemonResponse>(RequestType.ReleasePokemon, message).ConfigureAwait(false);
        }

        public async Task<EvolvePokemonResponse> EvolvePokemon(ulong pokemonId)
        {
            var message = new EvolvePokemonMessage
            {
                PokemonId = pokemonId
            };

            return await PostProtoPayload<Request, EvolvePokemonResponse>(RequestType.EvolvePokemon, message).ConfigureAwait(false);
        }

        public async Task<UpgradePokemonResponse> UpgradePokemon(ulong pokemonId)
        {
            var message = new UpgradePokemonMessage()
            {
                PokemonId = pokemonId
            };

            return await PostProtoPayload<Request, UpgradePokemonResponse>(RequestType.UpgradePokemon, message).ConfigureAwait(false);
        }

        public async Task<GetInventoryResponse> GetInventory()
        {
            return await PostProtoPayload<Request, GetInventoryResponse>(RequestType.GetInventory, new GetInventoryMessage()).ConfigureAwait(false);
        }

        public async Task<RecycleInventoryItemResponse> RecycleItem(ItemId itemId, int amount)
        {
            var message = new RecycleInventoryItemMessage
            {
                ItemId = itemId,
                Count = amount
            };

            return await PostProtoPayload<Request, RecycleInventoryItemResponse>(RequestType.RecycleInventoryItem, message).ConfigureAwait(false);
        }

        public async Task<UseItemXpBoostResponse> UseItemXpBoost()
        {
            var message = new UseItemXpBoostMessage()
            {
                ItemId = ItemId.ItemLuckyEgg
            };

            return await PostProtoPayload<Request, UseItemXpBoostResponse>(RequestType.UseItemXpBoost, message).ConfigureAwait(false);
        }

        public async Task<UseItemEggIncubatorResponse> UseItemEggIncubator(string itemId, ulong pokemonId)
        {
            var message = new UseItemEggIncubatorMessage()
            {
                ItemId = itemId,
                PokemonId = pokemonId
            };

            return await PostProtoPayload<Request, UseItemEggIncubatorResponse>(RequestType.UseItemEggIncubator, message).ConfigureAwait(false);
        }

        public async Task<GetHatchedEggsResponse> GetHatchedEgg()
        {
            return await PostProtoPayload<Request, GetHatchedEggsResponse>(RequestType.GetHatchedEggs, new GetHatchedEggsMessage()).ConfigureAwait(false);
        }

        public async Task<UseItemPotionResponse> UseItemPotion(ItemId itemId, ulong pokemonId)
        {
            var message = new UseItemPotionMessage()
            {
                ItemId = itemId,
                PokemonId = pokemonId
            };

            return await PostProtoPayload<Request, UseItemPotionResponse>(RequestType.UseItemPotion, message).ConfigureAwait(false);
        }

        public async Task<UseItemEggIncubatorResponse> UseItemRevive(ItemId itemId, ulong pokemonId)
        {
            var message = new UseItemReviveMessage()
            {
                ItemId = itemId,
                PokemonId = pokemonId
            };

            return await PostProtoPayload<Request, UseItemEggIncubatorResponse>(RequestType.UseItemEggIncubator, message).ConfigureAwait(false);
        }

        public async Task<UseIncenseResponse> UseIncense(ItemId incenseType)
        {
            var message = new UseIncenseMessage()
            {
                IncenseType = incenseType
            };

            return await PostProtoPayload<Request, UseIncenseResponse>(RequestType.UseIncense, message).ConfigureAwait(false);
        }

        public async Task<UseItemGymResponse> UseItemInGym(string gymId, ItemId itemId)
        {
            var message = new UseItemGymMessage()
            {
                ItemId = itemId,
                GymId = gymId,
                PlayerLatitude = _client.CurrentLatitude,
                PlayerLongitude = _client.CurrentLongitude
            };

            return await PostProtoPayload<Request, UseItemGymResponse>(RequestType.UseItemGym, message).ConfigureAwait(false);
        }

        public async Task<NicknamePokemonResponse> NicknamePokemon(ulong pokemonId, string nickName)
        {
            var message = new NicknamePokemonMessage()
            {
                PokemonId = pokemonId,
                Nickname = nickName
            };

            return await PostProtoPayload<Request, NicknamePokemonResponse>(RequestType.NicknamePokemon, message).ConfigureAwait(false);
        }

        public async Task<SetFavoritePokemonResponse> SetFavoritePokemon(ulong pokemonId, bool isFavorite)
        {
            var message = new SetFavoritePokemonMessage()
            {
                PokemonId = pokemonId,
                IsFavorite = isFavorite
            };

            return await PostProtoPayload<Request, SetFavoritePokemonResponse>(RequestType.SetFavoritePokemon, message).ConfigureAwait(false);
        }
    }
}