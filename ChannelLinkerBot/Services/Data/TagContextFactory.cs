using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace UtilityBot.Services.Data
{
    // Used for design-time tools
    public class TagContextFactory : IDbContextFactory<TagContext>
    {
        public TagContext Create(DbContextFactoryOptions options)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TagContext>();
            var config = Configuration.Config.LoadFrom(options.ContentRootPath);
            var connection = config.Database.ConnectionString;
            optionsBuilder.UseNpgsql(connection);

            return new TagContext(optionsBuilder.Options);
        }
    }
}
