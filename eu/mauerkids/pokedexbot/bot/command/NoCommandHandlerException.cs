using System;
using PokeApiNet;

namespace eu.mauerkids.pokedexbot.bot.command
{
    public class NoCommandHandlerException : Exception
    {
        public NoCommandHandlerException(string commandName)
            : base($"No handler found for command {commandName}.")
        {}
    }
}