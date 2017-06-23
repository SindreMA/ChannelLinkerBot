using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace UtilityBot.Services.Logging
{
    public class LogAdaptor
    {
        private readonly ILogger _logger;

        public static Logger CreateLogger()
        {
           return new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Verbose()
#else
                .MinimumLevel.Debug()
#endif
                .WriteTo.LiterateConsole()
#if RELEASE
                .WriteTo.RollingFile("log-{Date}.log")
#endif
                .CreateLogger();
        }

        public LogAdaptor(Logger logger, DiscordSocketClient client)
        {
            _logger = logger.ForContext<DiscordSocketClient>();
            client.Log += LogAsync;
        }

        private Task LogAsync(LogMessage message)
        {
            if (message.Exception == null)
                _logger.Write(
                    GetEventLevel(message.Severity),
                    $"{message.Message}");
            else
                _logger.Write(
                        GetEventLevel(message.Severity),
                        message.Exception,
                        $"{message.Message}"
                    );

            return Task.CompletedTask;
        }

        internal async Task LogCommand(LogMessage message)
        {
            if (message.Exception != null && message.Exception is CommandException cmd)
            {
                await cmd.Context.Channel.SendMessageAsync(cmd.ToString());
            }
            await LogAsync(message);
        }

        private static LogEventLevel GetEventLevel(LogSeverity severity)
        {
            return (LogEventLevel) Math.Abs((int) (severity - 5));
        }
    }
}
