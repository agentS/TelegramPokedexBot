using System.Threading.Tasks;
using PokeApiNet;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace eu.mauerkids.pokedexbot.bot.command
{
    public interface IBotCommand
    {
        Task Handle(Message message, ITelegramBotClient botClient, PokeApiClient pokeApiClient);
    }
}