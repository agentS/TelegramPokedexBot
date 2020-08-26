namespace eu.mauerkids.pokedexbot.bot.command
{
    public sealed class CommandSelector
    {
        private CommandSelector() {}

        public static IBotCommand MapCommandToHandler(string command)
        {
            switch (command)
            {
                case "/start":
                    return new StartCommand();
                case "/help":
                    return new HelpCommand();
                case "/battle":
                    return new BattleCommand();
                default:
                    throw new NoCommandHandlerException(command);
            }
        }
    }
}