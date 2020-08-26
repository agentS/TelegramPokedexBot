using System;
using dotenv.net;

namespace eu.mauerkids.pokedexbot.bot
{
    public class BotRunner
    {
        private const string ENVIRONMENT_KEY_API_KEY = "TELEGRAM_BOT_KEY";

        private static volatile bool stopBot = false;

        private static object _pokedexBotLockObject = new object();
        private static PokedexBot _pokedexBot = null;
        
        static void Main()
        {
            DotEnv.Config();

            // TODO: replace with secure string: https://docs.microsoft.com/en-us/dotnet/api/system.security.securestring?redirectedfrom=MSDN&view=netcore-3.1
            String apiKey = Environment.GetEnvironmentVariable(ENVIRONMENT_KEY_API_KEY);
            lock (_pokedexBotLockObject)
            {
                _pokedexBot = new PokedexBot(apiKey);
                _pokedexBot.Start();
            }

            AppDomain.CurrentDomain.ProcessExit += StopApplicationBySystem;
            Console.CancelKeyPress += StopApplicationManually;
            
            while (!stopBot) {}
            
            AppDomain.CurrentDomain.ProcessExit -= StopApplicationBySystem;
            Console.CancelKeyPress -= StopApplicationManually;

        }

        private static void StopApplicationBySystem(object sender, EventArgs eventArguments)
        {
            StopPokedexBot();
        }

        private static void StopApplicationManually(object sender, ConsoleCancelEventArgs eventArguments)
        {
            StopPokedexBot();
        }

        private static void StopPokedexBot()
        {
            stopBot = true;
            lock (_pokedexBotLockObject)
            {
                _pokedexBot.Stop();
            }
        }
    }
}
