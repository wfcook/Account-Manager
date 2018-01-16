using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using POGOLib.Official.Net;
using POGOProtos.Inventory;
using POGOProtos.Inventory.Item;

namespace POGOLib.Official.Pokemon
{
    /// <summary>
    ///     A wrapper class for <see cref="Inventory" />.
    /// </summary>
    public class Inventory
    {
        private readonly Session _session;

        internal long LastInventoryTimestampMs;

        public Inventory(Session session)
        {
            _session = session;
        }

        /// <summary>
        ///     Gets the last received <see cref="RepeatedField{T}" /> from PokémonGo.<br />
        ///     Only use this if you know what you are doing.
        /// </summary>
        public RepeatedField<InventoryItem> InventoryItems { get; } = new RepeatedField<InventoryItem>();

        internal void RemoveInventoryItems(IEnumerable<InventoryItem> items)
        {
            foreach (var item in items)
            {
                InventoryItems.Remove(item);
            }

            _session.OnInventoryUpdate();
        }

        internal void UpdateInventoryItems(InventoryDelta delta)
        {
            if (delta?.InventoryItems == null || delta.InventoryItems.All(i => i == null))
            {
                return;
            }
            InventoryItems.AddRange(delta.InventoryItems.Where(i => i != null));
            // Only keep the newest ones
            foreach (var deltaItem in delta.InventoryItems.Where(d => d?.InventoryItemData != null))
            {
                var oldItems = new List<InventoryItem>();
                if (deltaItem.InventoryItemData.PlayerStats != null)
                {
                    oldItems.AddRange(
                        InventoryItems.Where(i => i.InventoryItemData?.PlayerStats != null)
                            .OrderByDescending(i => i.ModifiedTimestampMs)
                            .Skip(1));
                }
                if (deltaItem.InventoryItemData.PlayerCurrency != null)
                {
                    oldItems.AddRange(
                        InventoryItems.Where(i => i.InventoryItemData?.PlayerCurrency != null)
                            .OrderByDescending(i => i.ModifiedTimestampMs)
                            .Skip(1));
                }
                if (deltaItem.InventoryItemData.PlayerCamera != null)
                {
                    oldItems.AddRange(
                        InventoryItems.Where(i => i.InventoryItemData?.PlayerCamera != null)
                            .OrderByDescending(i => i.ModifiedTimestampMs)
                            .Skip(1));
                }
                if (deltaItem.InventoryItemData.InventoryUpgrades != null)
                {
                    oldItems.AddRange(
                        InventoryItems.Where(i => i.InventoryItemData?.InventoryUpgrades != null)
                            .OrderByDescending(i => i.ModifiedTimestampMs)
                            .Skip(1));
                }
                if (deltaItem.InventoryItemData.PokedexEntry != null)
                {
                    oldItems.AddRange(
                        InventoryItems.Where(
                            i =>
                                i.InventoryItemData?.PokedexEntry != null &&
                                i.InventoryItemData.PokedexEntry.PokemonId ==
                                deltaItem.InventoryItemData.PokedexEntry.PokemonId)
                            .OrderByDescending(i => i.ModifiedTimestampMs)
                            .Skip(1));
                }
                if (deltaItem.InventoryItemData.Candy != null)
                {
                    oldItems.AddRange(
                        InventoryItems.Where(
                            i =>
                                i.InventoryItemData?.Candy != null &&
                                i.InventoryItemData.Candy.FamilyId ==
                                deltaItem.InventoryItemData.Candy.FamilyId)
                            .OrderByDescending(i => i.ModifiedTimestampMs)
                            .Skip(1));
                }
                if (deltaItem.InventoryItemData.Item != null)
                {
                    oldItems.AddRange(
                        InventoryItems.Where(
                            i =>
                                i.InventoryItemData?.Item != null &&
                                i.InventoryItemData.Item.ItemId == deltaItem.InventoryItemData.Item.ItemId)
                            .OrderByDescending(i => i.ModifiedTimestampMs)
                            .Skip(1));
                }
                if (deltaItem.InventoryItemData.PokemonData != null)
                {
                    oldItems.AddRange(
                        InventoryItems.Where(
                            i =>
                                i.InventoryItemData?.PokemonData != null &&
                                i.InventoryItemData.PokemonData.Id == deltaItem.InventoryItemData.PokemonData.Id)
                            .OrderByDescending(i => i.ModifiedTimestampMs)
                            .Skip(1));
                }
                if (deltaItem.InventoryItemData.AppliedItems != null)
                {
                    oldItems.AddRange(
                        InventoryItems.Where(i => i.InventoryItemData?.AppliedItems != null)
                            .OrderByDescending(i => i.ModifiedTimestampMs)
                            .Skip(1));
                }
                if (deltaItem.InventoryItemData.EggIncubators != null)
                {
                    oldItems.AddRange(
                        InventoryItems.Where(i => i.InventoryItemData?.EggIncubators != null)
                            .OrderByDescending(i => i.ModifiedTimestampMs)
                            .Skip(1));
                }
                foreach (var oldItem in oldItems)
                {
                    InventoryItems.Remove(oldItem);
                }
            }

            var appliedItems = InventoryItems.Select(i => i.InventoryItemData?.AppliedItems)
                .Where(aItems => aItems?.Item != null)
                .SelectMany(aItems => aItems.Item).ToDictionary(item => item.ItemId, item => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(item.ExpireMs));
            DateTime expires = new DateTime(0);
            ItemId _item = ItemId.ItemUnknown;

            foreach (var item in InventoryItems.Select(i => i.InventoryItemData?.Item).Where(item => item != null))
            {
                if (appliedItems.ContainsKey(item.ItemId))
                {
                    expires = appliedItems[item.ItemId];
                    _item = item.ItemId;
                }
            }

            var time = expires - DateTime.UtcNow;
            if (expires.Ticks == 0 || time.TotalSeconds < 0)
            {
                _session.IncenseUsed = false;
            }
            else
            {
                _session.IncenseUsed = true;
                _session.Logger.Debug($"Session {_item} active {time.Minutes}m {Math.Abs(time.Seconds)}s.");
            }

            _session.OnInventoryUpdate();
        }
    }
}