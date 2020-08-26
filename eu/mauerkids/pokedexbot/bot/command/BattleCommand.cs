using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using PokeApiNet;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace eu.mauerkids.pokedexbot.bot.command
{
    public class BattleCommand : IBotCommand
    {
        private const string POKEMON_TYPE_SEPARATOR = ", ";

        private static string ExtractPokemonNameOrId(string message)
        {
            int commandSeparatorIndex = message.IndexOf(' ');
            if (commandSeparatorIndex == (-1))
            {
                throw new UnknownPokemonException("no Pokémon name");
            }

            return message.Substring(commandSeparatorIndex).Trim();
        }

        public async Task Handle(Message message, ITelegramBotClient botClient, PokeApiClient pokeApiClient)
        {
            try
            {
                string pokemonNameOrId = ExtractPokemonNameOrId(message.Text);
                Pokemon pokemon = await this.LookupPokemonByNameOrId(pokemonNameOrId, pokeApiClient);
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat,
                    text: $"#{pokemon.Id} -- {pokemon.Name}"
                );

                StringBuilder pokemonTypes = new StringBuilder("Types: ");
                foreach (PokemonType type in pokemon.Types)
                {
                    pokemonTypes.Append(type.Type.Name)
                        .Append(POKEMON_TYPE_SEPARATOR);
                }

                pokemonTypes.Remove(
                    pokemonTypes.Length - POKEMON_TYPE_SEPARATOR.Length,
                    POKEMON_TYPE_SEPARATOR.Length
                );
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat,
                    text: pokemonTypes.ToString()
                );

                await botClient.SendPhotoAsync(
                    chatId: message.Chat,
                    photo: pokemon.Sprites.FrontDefault,
                    caption: pokemon.Name,
                    ParseMode.Html
                );
                await botClient.SendPhotoAsync(
                    chatId: message.Chat,
                    photo: pokemon.Sprites.BackDefault,
                    caption: pokemon.Name,
                    ParseMode.Html
                );
            }
            catch (UnknownPokemonException)
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat,
                    text: "Unfortunately there is no such Pokémon."
                );
            }
        }

        private async Task<Pokemon> LookupPokemonByNameOrId(string pokemonNameOrId, PokeApiClient pokeApiClient)
        {
            try
            {
                return await pokeApiClient.GetResourceAsync<Pokemon>(pokemonNameOrId);
            }
            catch (HttpRequestException)
            {
                throw new UnknownPokemonException(pokemonNameOrId);
            }
        }
    }
}
