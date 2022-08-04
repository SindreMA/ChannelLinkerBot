using System;
using System.Collections.Generic;
using System.Text;

namespace ChannelLinkerBotv2.DTO
{
    public class GuildSettingsDTO

    {
        public ulong GuildID { get; set; }
        public Modes LinkMode { get; set; }
        public enum Modes
        {
            PicturesAndFilesOnly,
            FilesOnly,
            TextOnly,
            NoneEmbedOnly,
            EmbedOnly,
            All
        };

    }
}