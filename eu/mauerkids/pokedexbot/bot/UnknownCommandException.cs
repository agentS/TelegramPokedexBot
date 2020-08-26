using System;

namespace eu.mauerkids.pokedexbot.bot
{
    public class UnknownCommandException : Exception
    {
        public UnknownCommandException(string commandName)
            : base($"Command {commandName} is not supported by this bot.")
        { }
   }
}
