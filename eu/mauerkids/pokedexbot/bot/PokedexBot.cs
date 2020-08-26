using System;
using System.Collections.Generic;
using System.Threading;
using eu.mauerkids.pokedexbot.bot.command;
using PokeApiNet;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace eu.mauerkids.pokedexbot.bot
{
    public sealed class PokedexBot
    {
        private static readonly Dictionary<String, PokedexBotCommand> COMMAND_NAMES = new Dictionary<string, PokedexBotCommand>()
        {
            {"/battle", PokedexBotCommand.BattleStatistics}
        };

        private static string ExtractCommand(string message)
        {
            int commandSeparatorIndex = message.IndexOf(' ');
            if (commandSeparatorIndex == (-1))
            {
                if (message[0] == '/')
                {
                    return message;
                }
                throw new UnknownCommandException("Blank command");
            }
            else
            {
                string commandString = message.Substring(0, commandSeparatorIndex);
                return commandString;
            }
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
                    string command = ExtractCommand(message.Text);
                    IBotCommand handler = CommandSelector.MapCommandToHandler(command);
                    await handler.Handle(message, this._botClient, this._pokeApiClient);
                }
                catch (Exception exception)
                {
                    // Workaround since C# does not support multi catch as in Java
                    if (exception is UnknownCommandException || exception is NoCommandHandlerException)
                    {
                        await this._botClient.SendTextMessageAsync(
                            chatId: message.Chat,
                            text: "Unfortunately the bot does not support this command. Enter /help or /start for more options."
                        );
                    }
                    else
                    {
                        Console.WriteLine(exception.Message);
                        Console.WriteLine(exception.StackTrace);
                        await this._botClient.SendTextMessageAsync(
                            chatId: message.Chat,
                            text: "500 - Internal Server Error"
                        );
                    }
                }
            }
        }

        public void Stop()
        {
            this._botClient.OnMessage -= this.OnMessage;
            this._botClient.StopReceiving();
        }
    }
}
