using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PokemonGoGUI.Models
{
    public class PokeSniperResult
    {
        public int id { get; set; }
        public string name { get; set; }
        public string coords { get; set; }
        public string until { get; set; }
        public int iv { get; set; }
        public List<object> attacks { get; set; }
        public string icon { get; set; }

        public double Latitude
        {
            get
            {
                string tLat = coords.Split(',').First();
                double lat = 0;

                try
                {
                    lat = Double.Parse(tLat, CultureInfo.InvariantCulture);
                }
                catch
                {
                    //meh
                }

                return lat;
            }
        }

        public double Longitude
        {
            get
            {
                string tLong = coords.Split(',').Last();
                double lon = 0;

                try
                {
                    lon = Double.Parse(tLong, CultureInfo.InvariantCulture);
                }
                catch
                {
                    //meh
                }

                return lon;
            }
        }

        public PokemonId PokemonId
        {
            get
            {
                name = name.Replace(" M", "Male").Replace(" F", "Female").Replace("Farfetch'd", "Farfetchd").Replace("Mr.Maleime", "MrMime");

                return (PokemonId)Enum.Parse(typeof(PokemonId), name);
            }
        }

        public DateTime DespawnTime
        {
            get
            {
                DateTime despawn = new DateTime();

                DateTime.TryParse(until, out despawn);

                return despawn;
            }
        }
    }

    public class PokeSniperObject
    {
        public List<PokeSniperResult> results { get; set; }
    }
}
