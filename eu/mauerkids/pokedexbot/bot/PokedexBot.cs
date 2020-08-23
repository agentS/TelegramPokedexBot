using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using PokeApiNet;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace eu.mauerkids.pokedexbot.bot
{
    public sealed class PokedexBot
    {
        private const string POKEMON_TYPE_SEPARATOR = ", ";
        
        private static readonly Dictionary<String, PokedexBotCommand> COMMAND_NAMES = new Dictionary<string, PokedexBotCommand>()
        {
            {"/battle", PokedexBotCommand.BattleStatistics}
        };

        private static PokedexBotCommand ExtractCommand(string message)
        {
            int commandSeparatorIndex = message.IndexOf(' ');
            if (commandSeparatorIndex == (-1))
            {
                throw new UnknownCommandException("Blank command");
            }

            string commandString = message.Substring(0, commandSeparatorIndex);
            PokedexBotCommand command;
            if (COMMAND_NAMES.TryGetValue(commandString, out command))
            {
                return command;
            }
            throw new UnknownCommandException(commandString);
        }

        private static string ExtractPokemonNameOrId(string message)
        {
            int commandSeparatorIndex = message.IndexOf(' ');
            if (commandSeparatorIndex == (-1))
            {
                throw new UnknownPokemonException("no Pokémon name");
            }

            return message.Substring(commandSeparatorIndex).Trim();
        }
        
        private ITelegramBotClient _botClient;
        private readonly string _accessToken;
        private readonly PokeApiClient _pokeApiClient;

        public PokedexBot(string accessToken)
        {
            this._accessToken = accessToken;
            this._pokeApiClient = new PokeApiClient();
        }

        public void Start()
        {
            this._botClient = new TelegramBotClient(this._accessToken);
            this._botClient.OnMessage += this.OnMessage;
            this._botClient.StartReceiving();
        }

        private async void OnMessage(object sender, MessageEventArgs arguments)
        {
            var message = arguments.Message;
            if (message.Text != null)
            {
                try
                {
                    PokedexBotCommand command = ExtractCommand(message.Text);

                    switch (command)
                    {
                        case PokedexBotCommand.BattleStatistics:
                            try
                            {
                                string pokemonNameOrId = ExtractPokemonNameOrId(message.Text);
                                Pokemon pokemon = await this.LookupPokemonByNameOrId(pokemonNameOrId);
                                await this._botClient.SendTextMessageAsync(
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
                                await this._botClient.SendTextMessageAsync(
                                    chatId: message.Chat,
                                    text: pokemonTypes.ToString()
                                );
                            }
                            catch (UnknownPokemonException)
                            {
                                await this._botClient.SendTextMessageAsync(
                                    chatId: message.Chat,
                                    text: "Unfortunately there is no such Pokémon."
                                );
                            }
                            break;
                    }
                }
                catch (UnknownCommandException)
                {
                    await this._botClient.SendTextMessageAsync(
                        chatId: message.Chat,
                        text: "Unfortunately the bot does not support this command."
                    );
                }
            }
        }

        private async Task<Pokemon> LookupPokemonByNameOrId(string pokemonNameOrId)
        {
            try
            {
                return await this._pokeApiClient.GetResourceAsync<Pokemon>(pokemonNameOrId);
            }
            catch (HttpRequestException)
            {
                throw new UnknownPokemonException(pokemonNameOrId);
            }
        }

        public void Stop()
        {
            this._botClient.OnMessage -= this.OnMessage;
            this._botClient.StopReceiving();
        }
    }
}
