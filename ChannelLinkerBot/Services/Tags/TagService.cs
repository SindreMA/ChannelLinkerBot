using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace UtilityBot.Services.Tags
{
    public class TagService
    {
        public TagDb Database { get; }

        private readonly CommandService _commands;

        private ModuleInfo _tagModule;

        public TagService(CommandService commands)
        {
            _commands = commands;
            Database = TagDb.Load();
            BuildCommands().GetAwaiter().GetResult();
        }

        public async Task BuildCommands()
        {
            if (_tagModule != null)
                await _commands.RemoveModuleAsync(_tagModule);

            _tagModule = await _commands.CreateModuleAsync("", module =>
            {
                module.Name = "Tags";
                
                foreach (var tag in Database.Tags)
                {
                    module.AddCommand(tag.Name, async (context, args, map) =>
                    {
                        var builder = new EmbedBuilder()
                            .WithTitle(tag.Name)
                            .WithDescription(tag.Content);
                        var user = await context.Channel.GetUserAsync(tag.OwnerId);
                        if (user != null)
                            builder.Author = new EmbedAuthorBuilder()
                                .WithIconUrl(user.GetAvatarUrl())
                                .WithName(user.Username);

                        await context.Channel.SendMessageAsync("", embed: builder.Build());
                    }, builder =>
                    {
                        builder.AddAliases(tag.Aliases.ToArray());
                    });
                }
            });
        }

        public async Task AddTag(Tag tag)
        {
            Database.Tags.Add(tag);
            Database.Save();
            await BuildCommands();
        }
        public async Task RemoveTag(Tag tag)
        {
            Database.Tags.Remove(tag);
            Database.Save();
            await BuildCommands();
        }
    }
}
