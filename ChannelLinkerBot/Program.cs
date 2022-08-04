
using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
namespace TemplateBot
{
    class Program
    {
        static void Main(string[] args)
            => new Program().StartAsync().GetAwaiter().GetResult();
        private CommandHandler _handler;
        private DiscordSocketClient _client;
        public async Task StartAsync()
        {
            await Log("Setting up the bot", ConsoleColor.Green);
            _client = new DiscordSocketClient();
            new CommandHandler(_client);
            await Log("Logging in...", ConsoleColor.Green);
            await _client.LoginAsync(TokenType.Bot, "YOUR TOKEN KEY HERE");
            await Log("Connecting...", ConsoleColor.Green);
            await _client.StartAsync();
            _client.GuildAvailable += _client_GuildAvailable;
            await Task.Delay(-1);
            _handler = new CommandHandler(_client);

        }

        private async Task _client_GuildAvailable(SocketGuild arg)
        {

            await Log(arg.Name + " Connected!", ConsoleColor.Green);
        }
        public static async Task Log(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(DateTime.Now + " : " + message, color);
            Console.ResetColor();
        }

    }
}