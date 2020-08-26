using System.Threading.Tasks;
using PokeApiNet;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace eu.mauerkids.pokedexbot.bot.command
{
    public class StartCommand : IBotCommand
    {
        public async Task Handle(Message message, ITelegramBotClient botClient, PokeApiClient pokeApiClient)
        {
            await botClient.SendTextMessageAsync(
                chatId: message.Chat,
                text: $"Hello, {message.From.FirstName}! I'm the Pokédex Bot. Supported commands are /start, /help, and /battle <name or ID>.",
                parseMode: ParseMode.Default
            );
        }
    }
}