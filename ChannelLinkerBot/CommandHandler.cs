using System;
using Discord;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using ChannelLinkerBot.DTO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using ChannelLinkerBotv2.DTO;

namespace TemplateBot
{
    class CommandHandler
    {
        private DiscordSocketClient _client;
        private CommandService _service;

        public static List<MessagePrefixDTO> MessagePrefixList = new List<MessagePrefixDTO>();
        public static List<ChannelLinkDTO> ChannelsLinkedList = new List<ChannelLinkDTO>();
        public static List<GuildSettingsDTO> GuildSettingsList = new List<GuildSettingsDTO>();

        public CommandHandler(DiscordSocketClient client)
        {
            _client = client;
            _service = new CommandService();
            _service.AddModulesAsync(Assembly.GetEntryAssembly());
            _client.MessageReceived += _client_MessageReceived;
            _client.SetGameAsync(".? for commands");
            try
            {

                string json = File.ReadAllText("GuildSettingsList.json");
                GuildSettingsList = JsonConvert.DeserializeObject<List<GuildSettingsDTO>>(json);
            }
            catch (Exception)
            {
            }
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
        public bool DoesMessageContainListitems(List<string> list, SocketCommandContext context)
        {
            foreach (var item in list)
            {
                if (context.Message.Content.Contains(item))
                {
                    return true;
                    break;
                }
            }
            return false;

          
        }
        public bool ContainImages(SocketCommandContext Context)
        {
            List<string> FileTypes = new List<string>();
            FileTypes.Add("png");
            FileTypes.Add("gif");
            FileTypes.Add("tif");
            FileTypes.Add("jpg");
            FileTypes.Add("jpeg");
            FileTypes.Add("jif");
            FileTypes.Add("jfif");
            FileTypes.Add("tiff");
            FileTypes.Add("jp2");
            FileTypes.Add("jpx");
            FileTypes.Add("j2k");
            FileTypes.Add("j2c");
            FileTypes.Add("fpx");
            FileTypes.Add("pcd");
            FileTypes.Add("pdf");

            if (Context.Message.Attachments.Count != 0)
            {
                return true;
            }
            else if (Context.Message.Embeds.Count != 0)
            {
                foreach (var item in Context.Message.Embeds)
                {
                    if (item.Thumbnail.HasValue)
                    {
                        return true;
                        break;
                    }
                }
                return false;
            }
            else if ((Context.Message.Content.ToLower().Contains("http") || Context.Message.Content.ToLower().Contains("www"))  && DoesMessageContainListitems(FileTypes,Context))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool IsEmbed(SocketCommandContext Context)
        {
            if (Context.Message.Embeds.Count != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckType(SocketCommandContext Context)
        {
            if (GuildSettingsList.Exists(x => x.GuildID == Context.Guild.Id))
            {
                
                var item = GuildSettingsList.Find(x => x.GuildID == Context.Guild.Id);
                if (item.LinkMode == GuildSettingsDTO.Modes.EmbedOnly)
                    
                {
                    if (Context.Message.Embeds.Count != 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (item.LinkMode == GuildSettingsDTO.Modes.NoneEmbedOnly)
                {
                    if (Context.Message.Embeds.Count == 0)
                    {
                        return true;
                    }
                    else if (isOnlyImage(Context.Message,Context))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (item.LinkMode == GuildSettingsDTO.Modes.PicturesAndFilesOnly)
                {
                    if (ContainImages(Context))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (item.LinkMode == GuildSettingsDTO.Modes.TextOnly)
                {
                    if (!ContainImages(Context) && !IsEmbed(Context))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (item.LinkMode == GuildSettingsDTO.Modes.FilesOnly)
                {
                    if (Context.Message.Attachments.Count != 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (item.LinkMode == GuildSettingsDTO.Modes.All)
                {
                   
                        return true;
                  
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }

            
        }
        public bool isOnlyImage(SocketUserMessage message , SocketCommandContext Context)
        {
            if (GuildSettingsList.Exists(x => x.GuildID == Context.Guild.Id))
            {
                var item = GuildSettingsList.Find(x => x.GuildID == Context.Guild.Id);
                if (item.LinkMode != GuildSettingsDTO.Modes.EmbedOnly)
                {
                    if  (!message.Embeds.ToArray()[0].Author.HasValue && !message.Embeds.ToArray()[0].Color.HasValue && !message.Embeds.ToArray()[0].Footer.HasValue && !message.Embeds.ToArray()[0].Image.HasValue && !message.Embeds.ToArray()[0].Provider.HasValue && message.Embeds.ToArray()[0].Title == null && !message.Embeds.ToArray()[0].Video.HasValue && message.Embeds.ToArray()[0].Color == null && message.Embeds.ToArray()[0].Description == null && message.Embeds.ToArray()[0].Footer == null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }


        }
        private async Task _client_MessageReceived(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            if (message == null) return;

            var context = new SocketCommandContext(_client, message);
            int argPost = 0;



            if (message.HasCharPrefix('.', ref argPost))
            {
                var result = _service.ExecuteAsync(context, argPost);
                if (!result.Result.IsSuccess && result.Result.Error != CommandError.UnknownCommand)
                {
                    await context.Channel.SendMessageAsync(result.Result.ErrorReason);
                }
                await Program.Log(" -  ||" + context.Guild.Id + "||   - " + "Invoked " + message + " in " + context.Channel + " with " + result.Result, ConsoleColor.Magenta);
            }
            else if (context.Guild.Id != 335029764282777600 && CheckType(context))
            {
                await Program.Log(" -  ||" + context.Guild.Id + "||   - " + context.Channel + "-" + context.User.Username + " : " + message, ConsoleColor.White);


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
                                if ((message.Embeds.Count != 0 && prefix.Prefix != null))
                                {
                                    foreach (var item in message.Embeds)
                                    {
                                        if (prefix.Prefix != null)
                                        {
                                            if (prefix.Prefix.Contains("[_EMBED_]"))
                                            {
                                                await To.SendMessageAsync("", false, item);
                                            }
                                            else
                                            {
                                                await To.SendMessageAsync("", false, SimpleEmbed(item, prefix, From, To, context));
                                            }
                                        }
                                        Console.WriteLine("Redirected embed message");
                                    }
                                }
                                else if (message.Embeds.Count != 0 && prefix.Prefix == null && isOnlyImage(message,context))
                                {
                                    await To.SendMessageAsync(message.Embeds.ToArray()[0].Url);
                                    Console.WriteLine("Picture redirected");
                                }
                                else if((message.Embeds.Count != 0 && prefix.Prefix == null))
                                {
                                    foreach (var item in message.Embeds)
                                    {
                                      
                                        await To.SendMessageAsync("", false, item);
                                        Console.WriteLine("Redirected embed message");
                                    }
                                }
                                else
                                {
                                    if (prefix.Prefix != null)
                                    {
                                        if (prefix.Prefix.Contains("[EMBED]"))
                                        {
                                            if (prefix.Prefix == "[EMBED]")
                                            {
                                                if (message.Attachments.Count != 0)
                                                {
                                                    int i = 1;
                                                    foreach (var item in context.Message.Attachments)
                                                    {
                                                        await To.SendMessageAsync("", false, SimpleEmbed(new Color(1f, 1f, 1f), "Message in " + From.Name, context.Message.Content, item.Url));
                                                        Console.WriteLine("Redirected message");
                                                        i++;
                                                    }
                                                }
                                                else
                                                {
                                                    await To.SendMessageAsync("", false, SimpleEmbed(new Color(1f, 1f, 1f), "Message in " + From.Name, context.Message.Content));
                                                    Console.WriteLine("Redirected message");
                                                }
                                            }
                                            else
                                            {
                                                if (message.Attachments.Count != 0)
                                                {
                                                    int i = 1;
                                                    foreach (var item in context.Message.Attachments)
                                                    {
                                                        await To.SendMessageAsync("", false, SimpleEmbed(new Color(1f, 1f, 1f), prefix.Prefix.Replace("[EMBED]", "").Replace("[TITLE]", "").Replace("[CHANNEL]", "**" + From.Name + "**").Replace("[USER]", context.User.Mention).Replace("[_USER_]", "**" + context.User.Username + "**"), context.Message.Content, item.Url));
                                                        Console.WriteLine("Redirected message");
                                                        i++;
                                                    }
                                                }
                                                else
                                                {
                                                    await To.SendMessageAsync("", false, SimpleEmbed(new Color(1f, 1f, 1f), prefix.Prefix.Replace("[EMBED]", "").Replace("[TITLE]", "").Replace("[CHANNEL]", "**" + From.Name + "**").Replace("[USER]", context.User.Mention).Replace("[_USER_]", "**" + context.User.Username + "**"), context.Message.Content));
                                                    Console.WriteLine("Redirected message");
                                                }
                                            }
                                        }
                                        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                                        else if (prefix.Prefix.Contains("[_EMBED_]"))
                                        {
                                            if (prefix.Prefix == "[_EMBED_]")
                                            {
                                                if (message.Attachments.Count != 0)
                                                {
                                                    int i = 1;
                                                    foreach (var item in context.Message.Attachments)
                                                    {
                                                        await To.SendMessageAsync("", false, SimpleEmbed(new Color(1f, 1f, 1f), "Message in " + From.Name, context.Message.Content, item.Url));
                                                        Console.WriteLine("Redirected message");
                                                        i++;
                                                    }
                                                }
                                                else
                                                {
                                                    await To.SendMessageAsync("", false, SimpleEmbed(new Color(1f, 1f, 1f), "Message in " + From.Name, context.Message.Content));
                                                    Console.WriteLine("Redirected message");
                                                }
                                            }
                                            else
                                            {
                                                if (message.Attachments.Count != 0)
                                                {
                                                    int i = 1;
                                                    foreach (var item in context.Message.Attachments)
                                                    {
                                                        await To.SendMessageAsync("", false, SimpleEmbed(new Color(1f, 1f, 1f), "Message in " + From.Name, prefix.Prefix.Replace("[_EMBED_]", "").Replace("[TITLE]", "").Replace("[CHANNEL]", "**" + From.Name + "**").Replace("[USER]", context.User.Mention).Replace("[_USER_]", "**" + context.User.Username + "**") + "  : " + context.Message.Content, item.Url));
                                                        Console.WriteLine("Redirected message");
                                                        i++;
                                                    }
                                                }
                                                else
                                                {
                                                    await To.SendMessageAsync("", false, SimpleEmbed(new Color(1f, 1f, 1f), "Message in " + From.Name, prefix.Prefix.Replace("[_EMBED_]", "").Replace("[TITLE]", "").Replace("[CHANNEL]", "**" + From.Name + "**").Replace("[USER]", context.User.Mention).Replace("[_USER_]", "**" + context.User.Username + "**") + "  : " + context.Message.Content));
                                                    Console.WriteLine("Redirected message");
                                                }
                                            }
                                        }
                                        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                                        else
                                        {
                                            if (message.Attachments.Count != 0)
                                            {
                                                int i = 1;
                                                foreach (var item in context.Message.Attachments)
                                                {
                                                    await To.SendFileAsync(URLToStream(item.Url), item.Filename, prefix.Prefix.Replace("[TITLE]", "").Replace("[CHANNEL]", "**" + From.Name + "**").Replace("[USER]", context.User.Mention).Replace("[_USER_]", "**" + context.User.Username + "**") + " : " + context.Message.Content);
                                                    Console.WriteLine("Redirected message");
                                                    i++;
                                                }
                                            }
                                            else
                                            {
                                                await To.SendMessageAsync("" + prefix.Prefix.Replace("[TITLE]", "").Replace("[CHANNEL]", "**" + From.Name + "**").Replace("[USER]", context.User.Mention).Replace("[_USER_]", "**" + context.User.Username + "**") + " : " + context.Message.Content);
                                                Console.WriteLine("Redirected message");
                                            }

                                        }

                                    }
                                    else
                                    {
                                        if (message.Attachments.Count != 0)
                                        {
                                            int i = 1;
                                            foreach (var item in context.Message.Attachments)
                                            {

                                                await To.SendFileAsync(URLToStream(item.Url), item.Filename, context.Message.Content);
                                                Console.WriteLine("Redirected message");
                                                i++;
                                            }
                                        }
                                        else
                                        {
                                            await To.SendMessageAsync(context.Message.Content);
                                            Console.WriteLine("Redirected message");
                                        }
                                    }

                                }
                            }

                        }
                    }

                }
                catch (Exception e)
                {

                }



                
            }
            else
            {
                await Program.Log("Message BLOCKED!", ConsoleColor.Red);
            }
        }
        public static string[] SplitOnString(string input, string Spliton)
        {
            return Regex.Split(input, Spliton);
        }
        public string DownloadFile(string file)
        {
            Random s = new Random();
            string SavePath = "File" + s.Next(1000, 9999) + ".jpg";
            using (var cli = new HttpClient())
            {
                var rslt = cli.GetAsync(file).GetAwaiter().GetResult();

                if (rslt.IsSuccessStatusCode)
                {
                    var dat = rslt.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                    File.WriteAllBytes(SavePath, dat);
                }
            }
            return SavePath;
        }
        public Stream URLToStream(string URL)
        {

            Byte[] dat = null;
            using (var cli = new HttpClient())
            {
                var rslt = cli.GetAsync(URL).GetAwaiter().GetResult();

                if (rslt.IsSuccessStatusCode)
                {
                    dat = rslt.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                }
            }
            return new MemoryStream(dat);

        }
        public static Embed SimpleEmbed(Color c, string title, string description)
        {
            EmbedBuilder eb = new EmbedBuilder();

            eb.WithColor(c);
            eb.Title = title;
            eb.WithDescription(description);


            return eb.Build();
        }
        public static Embed SimpleEmbed(Color c, string title, string description, string ImageURL)
        {
            EmbedBuilder eb = new EmbedBuilder();
            eb.WithImageUrl(ImageURL);
            eb.WithColor(c);
            eb.Title = title;
            eb.WithDescription(description);
            return eb.Build();
        }
        public static Embed SimpleEmbed(Embed item, MessagePrefixDTO prefix, SocketTextChannel From, SocketTextChannel To, SocketCommandContext context)
        {
            EmbedBuilder eb = new EmbedBuilder();
            string oldtitle = item.Title;
            if (item.Thumbnail != null)
            {
                var be = SplitOnString(item.Thumbnail.Value.ProxyUrl, "/http");
                string link = "";
                try
                {
                    if (be[2].StartsWith("s/"))
                    {
                        link = "https://" + be[2];
                    }
                    else
                    {
                        link = "http://" + be[2];
                    }
                    link = link.Replace("http://s/", "http://").Replace("https://s/", "https://");
                    eb.WithThumbnailUrl(link);
                }
                catch (Exception)
                {
                    if (be[1].StartsWith("s/"))
                    {
                        link = "https://" + be[1];
                    }
                    else
                    {
                        link = "http://" + be[1];
                    }
                    link = link.Replace("http://s/", "http://").Replace("https://s/", "https://");
                    eb.WithThumbnailUrl(link);
                }
            }
            if (item.Image != null)
            {
                var be = SplitOnString(item.Image.Value.ProxyUrl, "/http");
                string link = "";
                if (be[2].StartsWith("s/"))
                {
                    link = "https://" + be[2];
                }
                else
                {
                    link = "http://" + be[2];
                }
                link = link.Replace("http://s/", "http://").Replace("https://s/", "https://");
                eb.WithImageUrl(link);
            }
            eb.WithColor(item.Color.Value);
            eb.WithUrl(item.Url);
            if (prefix.Prefix != null)
            {
                eb.Title = prefix.Prefix.Replace("[EMBED]", "").Replace("[CHANNEL]", "**" + From.Name + "**").Replace("[USER]", "").Replace("[_USER_]", "**" + context.User.Username + "**").Replace("[TITLE]", oldtitle);
            }
            eb.WithDescription(item.Description);
            return eb.Build();
        }
    }
}
