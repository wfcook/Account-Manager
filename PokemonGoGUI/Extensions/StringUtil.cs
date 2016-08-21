using POGOProtos.Inventory.Item;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonGoGUI.Extensions
{
    public static class StringUtil
    {
        public static string GetSummedFriendlyNameOfItemAwardList(IEnumerable<ItemAward> items)
        {
            var enumerable = items as IList<ItemAward> ?? items.ToList();

            if (!enumerable.Any())
                return string.Empty;

            return
                enumerable.GroupBy(i => i.ItemId)
                    .Select(kvp => new { ItemName = kvp.Key.ToString(), Amount = kvp.Sum(x => x.ItemCount) })
                    .Select(y => String.Format("{0} x {1}", y.Amount, y.ItemName.Replace("Item", "")))
                    .Aggregate((a, b) => String.Format("{0}, {1}", a, b));
        }
    }
}
