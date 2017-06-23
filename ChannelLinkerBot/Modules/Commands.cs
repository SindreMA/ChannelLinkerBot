using Discord.Commands;
using ChannelLinkerBot.DTO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UtilityBot;
using System.IO;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace ChannelLinkerBot.Modules
{
    public class Main : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        [Alias("?")]
        public async Task help()
        {
            await Context.Channel.SendMessageAsync 
                (
                "*(A link is a connection between 1 channal to another, this means that all messages posted in channel A goes also to channel B)*" + Environment.NewLine + Environment.NewLine +
                "**help** (Shows this message)" + Environment.NewLine +
                "**.ShowLinks** (Returns all the current linked channels in the server)" + Environment.NewLine +
                "**.DeleteLink [FromChannelID] [ToChannelID]** (Deletes the specified link)" + Environment.NewLine +
                "**.CreateLink [FromChannelID] [ToChannelID]** (Creates a link from the first channel to the second channel)" + Environment.NewLine +
                "**.ResetLinks** (Removes all links and Removes all prefixes)" + Environment.NewLine+
                "**.Prefix [prefix message here]** (Lets you make the bot say something before it repeats the message)" + Environment.NewLine +
                "*(You can use [USER] and [CHANNEL] to make it specifi who and where it came from)*" 
                );
        }
        [Command("resetlinks")]
        public async Task Reset()
        {
            try
            {

                var s = CommandHandler.MessagePrefixList.Remove(CommandHandler.MessagePrefixList.Find(x => x.GuildID == Context.Guild.Id));
                File.WriteAllText("ChannelsLinked.json", JsonConvert.SerializeObject(CommandHandler.ChannelsLinkedList));

                
                foreach (var item in CommandHandler.ChannelsLinkedList.FindAll(x => x.GuildID == Context.Guild.Id))
                {
                    CommandHandler.ChannelsLinkedList.Remove(item);
                }
                File.WriteAllText("ChannelsLinked.json", JsonConvert.SerializeObject(CommandHandler.ChannelsLinkedList));
                await Context.Channel.SendMessageAsync("All links and prefix have been deleted!");
            }
            catch (Exception)
            {

            }


        }
        [Command("showlinks")]
        public async Task showlinks()
        {

            string msg = "";
            try
            {
                
                foreach (var item in CommandHandler.ChannelsLinkedList.FindAll(x => x.GuildID == Context.Guild.Id))
                {
                    msg = msg + " From =  **" + Context.Guild.GetChannel(item.ChannelCopyFrom).Name + "**(*" + item.ChannelCopyFrom + "*)" + " To = **" + Context.Guild.GetChannel(item.ChannelCopyTo).Name + "**(*" + item.ChannelCopyTo + "*)" + Environment.NewLine;
                }
            }
            catch (Exception)
            {
                msg = "There a no links for this guild!";
            }
            if (msg == null || msg == "")
            {
                msg = "There a no links for this guild!";
            }
            await Context.Channel.SendMessageAsync(msg);

        }
        [Command("DeleteLink")]
        [Alias("Removelink")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task Removelink(ulong from, ulong to)
        {
            var s = CommandHandler.ChannelsLinkedList.Remove(CommandHandler.ChannelsLinkedList.Find(x => x.GuildID == Context.Guild.Id && x.ChannelCopyFrom == from && x.ChannelCopyTo == to));

            if (s)
            {
                await Context.Channel.SendMessageAsync("Link have been removed!");
                File.WriteAllText("ChannelsLinked.json", JsonConvert.SerializeObject(CommandHandler.ChannelsLinkedList));

            }
            else
            {
                await Context.Channel.SendMessageAsync("Link was not found!");
            }

            

        }
        [Command("CreateLink")]
        public async Task link(ulong from , ulong to)
        {
            if (CommandHandler.ChannelsLinkedList.FindAll(x => x.ChannelCopyFrom == to).Count != 0)
            {
                await Context.Channel.SendMessageAsync("Link cant create link cause it will cause a loop! Please remove the othe link before adding this.");
            }
            else
            {

                CommandHandler.ChannelsLinkedList.Remove(CommandHandler.ChannelsLinkedList.Find(x => x.GuildID == Context.Guild.Id && x.ChannelCopyFrom == from && x.ChannelCopyTo == to));
                var Channellink = new ChannelLinkDTO();
                Channellink.GuildID = Context.Guild.Id;
                Channellink.ChannelCopyFrom = from;
                Channellink.ChannelCopyTo = to;
                CommandHandler.ChannelsLinkedList.Add(Channellink);
                await Context.Channel.SendMessageAsync("Link have been saved!");

                File.WriteAllText("ChannelsLinked.json", JsonConvert.SerializeObject(CommandHandler.ChannelsLinkedList));
            }
           
        }
        [Command("prefix")]
        public async Task prefix([Optional][Remainder]string Userprefix)
        {
            CommandHandler.MessagePrefixList.Remove(CommandHandler.MessagePrefixList.Find(x => x.GuildID == Context.Guild.Id));

            if (Userprefix != null)
            {
                MessagePrefixDTO Prefix = new MessagePrefixDTO();
                Prefix.GuildID = Context.Guild.Id;
                Prefix.Prefix = Userprefix.Replace("[USER]", Context.User.Mention );
                CommandHandler.MessagePrefixList.Add(Prefix);
                await Context.Channel.SendMessageAsync("Changes have been saved!");
            }
            else
            {
                await Context.Channel.SendMessageAsync("Prefix deleted!");
            }
            File.WriteAllText("MessagePrefix.json", JsonConvert.SerializeObject(CommandHandler.MessagePrefixList));
            
        }
    }
}
