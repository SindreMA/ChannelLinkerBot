using Discord;
using Discord.Addons.InteractiveCommands;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using System;
using System.Threading.Tasks;
using UtilityBot.Services.Configuration;
using UtilityBot.Services.Data;
using UtilityBot.Services.Logging;
using UtilityBot.Services.Tags;

namespace UtilityBot
{
    internal class Program
    {
        private static void Main(string[] args) =>
            new Program().RunAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private Config _config;
        private CommandHandler _handler;

        private async Task RunAsync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
#if DEBUG
                LogLevel = LogSeverity.Debug,
#else
                LogLevel = LogSeverity.Verbose,
#endif
            });
            _config = Config.Load();

            var serviceProvider = ConfigureServices();

            await _client.LoginAsync(TokenType.Bot, _config.Token);
            await _client.StartAsync();

            _handler = new CommandHandler(serviceProvider);
            await _handler.ConfigureAsync();

            await Task.Delay(-1);
        }

        private IServiceProvider ConfigureServices()
        {
            // Configure logging
            var logger = LogAdaptor.CreateLogger();
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new SerilogLoggerProvider(logger));
            // Configure services
            var services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_config)
                .AddSingleton(new CommandService(new CommandServiceConfig { CaseSensitiveCommands = false, ThrowOnError = false}))
                .AddSingleton(logger)
                .AddSingleton<LogAdaptor>()
                .AddSingleton<InteractiveService>()
                .AddSingleton<TagService>()
                /*
                .AddDbContext<TagContext>(options =>
                {
                    options
                        .UseNpgsql(_config.Database.ConnectionString)
                        .UseLoggerFactory(loggerFactory);
                })*/;
            var provider = services.BuildServiceProvider();
            // Autowire and create these dependencies now
            provider.GetService<LogAdaptor>();
            provider.GetService<TagService>();
            return provider;
        }
    }
}