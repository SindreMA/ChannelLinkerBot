using Discord;
using Discord.Addons.EmojiTools;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UtilityBot.Services.Configuration;
using UtilityBot.Services.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;
using System.Threading;
using ChannelLinkerBot.DTO;

namespace UtilityBot
{
    public class CommandHandler
    {

        private readonly IServiceProvider _provider;
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _client;
        private readonly Config _config;
        private readonly ILogger _logger;

        public static List<MessagePrefixDTO> MessagePrefixList = new List<MessagePrefixDTO>();
        public static List<ChannelLinkDTO> ChannelsLinkedList = new List<ChannelLinkDTO>();

        public CommandHandler(IServiceProvider provider)
        {
            _provider = provider;
            _client = _provider.GetService<DiscordSocketClient>();
            _client.MessageReceived += _client_MessageReceived;
            _commands = _provider.GetService<CommandService>();
            var log = _provider.GetService<LogAdaptor>();
            _commands.Log += log.LogCommand;
            _config = _provider.GetService<Config>();
            _logger = _provider.GetService<Logger>().ForContext<CommandService>();
            _client.SetGameAsync(".? for commands");
            try
            {

                string json = File.ReadAllText("ChannelsLinked.json");
                ChannelsLinkedList = JsonConvert.DeserializeObject<List<ChannelLinkDTO>>(json);
            }
            catch (Exception)
            {
            }
            try
            {

                string json = File.ReadAllText("MessagePrefix.json");
                MessagePrefixList = JsonConvert.DeserializeObject<List<MessagePrefixDTO>>(json);
            }
            catch (Exception)
            {
            }

        }

        private async Task _client_MessageReceived(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            if (message == null) return;
            if (message.Content.StartsWith("##")) return;
            var context = new SocketCommandContext(_client, message);
            int argPos = 0;
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            try
            {

                var d = ChannelsLinkedList.Where(x => x.GuildID == context.Guild.Id);
                foreach (var s in d)
                {
                    if (s != null)
                    {
                        if (s.ChannelCopyFrom == context.Channel.Id)
                        {
                            var From = context.Guild.TextChannels.Single(x => x.Id == s.ChannelCopyFrom);

                            var To = context.Guild.TextChannels.Single(x => x.Id == s.ChannelCopyTo);
                            MessagePrefixDTO prefix = new MessagePrefixDTO();
                            try
                            {
                                prefix = MessagePrefixList.Single(x => x.GuildID == context.Guild.Id);
                            }
                            catch (Exception)
                            {

                            }

                            if (prefix.Prefix != null)
                            {
                                if (prefix.Prefix.Contains("[EMBED]"))
                                {
                                    if (prefix.Prefix == "[EMBED]")
                                    {
                                        await To.SendMessageAsync("", false, SimpleEmbed(new Color(1f, 1f, 1f), "Message in " + From.Name, context.Message.Content));
                                    }
                                    else
                                    {
                                        await To.SendMessageAsync("", false, SimpleEmbed(new Color(1f, 1f, 1f), "Message in " + From.Name, prefix.Prefix.Replace("[EMBED]", "").Replace("[CHANNEL]", "**" + From.Name + "**").Replace("[USER]", context.User.Mention).Replace("[_USER_]", "**" + context.User.Username + "**") + "  :  " + context.Message.Content));
                                    }

                                }
                                else
                                {
                                    await To.SendMessageAsync("*" + prefix.Prefix.Replace("[CHANNEL]", "**" + From.Name + "**").Replace("[USER]", context.User.Mention).Replace("[_USER_]", "**" + context.User.Username + "**") + "* : " + context.Message.Content);
                                }

                            }
                            else
                            {
                                await To.SendMessageAsync(context.Message.Content);
                            }


                        }

                    }
                }

            }
            catch (Exception)
            {

            }

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////


            if (!ParseTriggers(message, ref argPos)) return;


            var result = await _commands.ExecuteAsync(context, argPos, _provider);





            if (result is SearchResult search && !search.IsSuccess)
            {
                // await message.AddReactionAsync(EmojiExtensions.FromText(":mag_right:"));
            }
            else if (result is PreconditionResult precondition && !precondition.IsSuccess)
                await message.Channel.SendMessageAsync("Invoked {" + message + "} in {" + context.Channel + "} with {" + result + "}");
            else if (result is ParseResult parse && !parse.IsSuccess)
                await message.Channel.SendMessageAsync("Invoked {" + message + "} in {" + context.Channel + "} with {" + result + "}");
            else if (result is TypeReaderResult reader && !reader.IsSuccess)
                await message.Channel.SendMessageAsync("Invoked {" + message + "} in {" + context.Channel + "} with {" + result + "}");
            else if (!result.IsSuccess)
                await message.Channel.SendMessageAsync("Invoked {" + message + "} in {" + context.Channel + "} with {" + result + "}");

            _logger.Debug("Invoked {Command} in {Context} with {Result}", message, context.Channel, result);

        }
        private bool ParseTriggers(SocketUserMessage message, ref int argPos)
        {
            bool flag = false;
            if (message.HasMentionPrefix(_client.CurrentUser, ref argPos)) flag = true;
            else
            {
                foreach (var prefix in _config.CommandStrings)
                {
                    if (message.HasStringPrefix(prefix, ref argPos))
                    {
                        flag = true;
                        break;
                    }
                }
            }
            return flag;
        }
        public async Task ConfigureAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }
        public static Embed SimpleEmbed(Color c, string title, string description)
        {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithColor(c);
            eb.Title = title;
            eb.WithDescription(description);


            return eb.Build();
        }
    }
}
