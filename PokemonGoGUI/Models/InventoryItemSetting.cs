using POGOProtos.Inventory.Item;

namespace PokemonGoGUI.Models
{
    public class InventoryItemSetting
    {
        public ItemId Id { get; set; }
        public int MaxInventory { get; set; }

        public InventoryItemSetting()
        {
            Id = ItemId.ItemUnknown;
            MaxInventory = 100;
        }

        public string FriendlyName
        {
            get
            {
                return Id.ToString().Replace("Item", "");
            }
        }
    }
}
