using System;
using System.Collections.Generic;
using System.Text;

namespace ChannelLinkerBot.DTO
{
    public class ChannelLinkDTO
    {
        public ulong GuildID { get; set; }
        public ulong ChannelCopyFrom { get; set; }
        public ulong ChannelCopyTo { get; set; }
    }
}
