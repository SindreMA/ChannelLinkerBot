using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UtilityBot.Services.Data
{
    public class TagContext : DbContext
    {
        public TagContext(DbContextOptions<TagContext> options) : base(options) { }

        public DbSet<TagInfo> Tags { get; set; }
    }

    // Named 'TagInfo' to prevent conflicts with Discord.Tag
    public class TagInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public long OwnerId { get; set; }
        public string Name { get; set; }
        public List<AliasInfo> Aliases { get; set; }
        public string Content { get; set; }
        public int Uses { get; set; }
    }

    public class AliasInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Trigger { get; set; }

        public int TagId { get; set; }
        public TagInfo Tag { get; set; }
    }
}
