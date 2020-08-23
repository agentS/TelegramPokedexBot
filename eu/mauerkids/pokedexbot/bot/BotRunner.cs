using System;
using dotenv.net;

namespace eu.mauerkids.pokedexbot.bot
{
    public class BotRunner
    {
        private const string ENVIRONMENT_KEY_API_KEY = "TELEGRAM_BOT_KEY";
        
        static void Main()
        {
            DotEnv.Config();

            // TODO: replace with secure string: https://docs.microsoft.com/en-us/dotnet/api/system.security.securestring?redirectedfrom=MSDN&view=netcore-3.1
            String apiKey = Environment.GetEnvironmentVariable(ENVIRONMENT_KEY_API_KEY);
            PokedexBot pokedexBot = new PokedexBot(apiKey);
            pokedexBot.Start();
            
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
            
            pokedexBot.Stop();
        }
    }
}
