using POGOProtos.Data;
using POGOProtos.Enums;
using POGOProtos.Inventory.Item;
using POGOProtos.Settings.Master;
using System.Collections.Generic;

namespace PokemonGoGUI.Models
{
    //**********************************//
    //EvolutionBranch first code

    public class EvolutionToPokemon
        {
            public int CandyNeed { get; set; }
            public ulong OriginPokemonId { get; set; }
            public PokemonId Pokemon { get; set; }
            public bool AllowEvolve { get; set; }
            public ItemId ItemNeed { get; set; }
        }

    public class EvoleBranch
    {
        private PokemonSettings Setting;
        private bool Allowevolve { get; set; }
        private PokemonData PokemonData { get; set; }
        public List<EvolutionToPokemon> EvolutionBranchs { get; set; }

        public EvoleBranch(PokemonData pokemon, PokemonSettings setting)
        {
            PokemonData = pokemon;
            Setting = setting;

            EvolutionBranchs = new List<EvolutionToPokemon>();

            //TODO - implement the candy count for enable evolution
            foreach (var item in setting.EvolutionBranch)
            {
                EvolutionBranchs.Add(new EvolutionToPokemon()
                {
                    CandyNeed = item.CandyCost,
                    ItemNeed = item.EvolutionItemRequirement,
                    Pokemon = item.Evolution,
                    AllowEvolve = Allowevolve,
                    OriginPokemonId = pokemon.Id
                });
            }
        }
    }
}
