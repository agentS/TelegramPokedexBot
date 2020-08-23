using System;

namespace eu.mauerkids.pokedexbot.bot
{
    public class UnknownPokemonException : Exception
    {
        public UnknownPokemonException(string name)
            : base($"Unfortunately, {name} is not a Pokémon.")
        {
        }
    }
}