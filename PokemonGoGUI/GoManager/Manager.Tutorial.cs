using POGOProtos.Data.Player;
using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGoGUI.GoManager
{
    public partial class Manager
    {
        public void test()
        {
            var b = _client.Download.GetSettings().Result;
            var d =_client.Download.GetSettings().Result;
            var a = _client.Misc.MarkTutorialComplete().Result;

            _client.Inventory.GetHatchedEgg();
            var c = _client.Player.GetPlayer().Result;
        }
    }
}
