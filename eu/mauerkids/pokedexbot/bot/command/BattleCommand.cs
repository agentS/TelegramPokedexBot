using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using PokeApiNet;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Type = PokeApiNet.Type;

namespace eu.mauerkids.pokedexbot.bot.command
{
    public class BattleCommand : IBotCommand
    {
        private const string POKEMON_TYPE_SEPARATOR = ", ";
        private const double DOUBLE_DAMAGE_MULTIPLIER = 2;

        private class Weakness
        {
            public Type Type { get; }
            public double Multiplier { get; set; }

            public Weakness() {}

            public Weakness(Type type, double multiplier)
            {
                Type = type;
                Multiplier = multiplier;
            }
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

                List<Type> types = await pokeApiClient.GetResourceAsync(
                    pokemon.Types.Select(type => type.Type)
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
                
                var weaknesses = new Dictionary<int, Weakness>();
                foreach (Type type in types)
                {
                    List<Type> doubleDamageTypes =  await pokeApiClient.GetResourceAsync(
                        type.DamageRelations.DoubleDamageFrom
                    );
                    foreach (Type doubleDamageType in doubleDamageTypes)
                    {
                        Weakness quadrupleWeakness;
                        if (weaknesses.TryGetValue(doubleDamageType.Id, out quadrupleWeakness))
                        {
                            quadrupleWeakness.Multiplier *= DOUBLE_DAMAGE_MULTIPLIER;
                        }
                        else
                        {
                            weaknesses.Add(
                                doubleDamageType.Id,
                                new Weakness(doubleDamageType, DOUBLE_DAMAGE_MULTIPLIER)
                            );
                        }
                    }
                }
                
                StringBuilder weaknessesText = new StringBuilder("**Weaknesses:**\n");
                foreach (Weakness weakness in weaknesses.Values)
                {
                    weaknessesText.Append(weakness.Type.Name)
                        .Append(": ")
                        .Append(weakness.Multiplier)
                        .Append("x\n");
                }
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat,
                    text: weaknessesText.ToString(),
                    parseMode: ParseMode.MarkdownV2
                );

                if (!string.IsNullOrEmpty(pokemon.Sprites.FrontDefault))
                {
                    await botClient.SendPhotoAsync(
                        chatId: message.Chat,
                        photo: pokemon.Sprites.FrontDefault,
                        caption: pokemon.Name,
                        ParseMode.Html
                    );
                }
                if (!string.IsNullOrEmpty(pokemon.Sprites.BackDefault))
                {
                    await botClient.SendPhotoAsync(
                        chatId: message.Chat,
                        photo: pokemon.Sprites.BackDefault,
                        caption: pokemon.Name,
                        ParseMode.Html
                    );
                }
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
