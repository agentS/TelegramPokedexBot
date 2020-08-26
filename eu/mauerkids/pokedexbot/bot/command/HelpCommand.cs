using System.Threading.Tasks;
using PokeApiNet;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace eu.mauerkids.pokedexbot.bot.command
{
    public class HelpCommand : IBotCommand
    {
        public async Task Handle(Message message, ITelegramBotClient botClient, PokeApiClient pokeApiClient)
        {
            await botClient.SendTextMessageAsync(
                chatId: message.Chat,
                text: $"Hello, {message.From.FirstName}\\! I'm the Pokédex Bot\\. You can ask me about Pokémon names and I'll show you their Pokédex information\\. To get this information please enter /battle followed by the name or ID of the Pokémon, e\\.g\\. `/battle Charizard`\\.",
                parseMode: ParseMode.MarkdownV2
            );
        }
    }
}