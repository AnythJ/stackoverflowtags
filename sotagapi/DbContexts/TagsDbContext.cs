using sotagapi.Models;
using Microsoft.EntityFrameworkCore;

namespace sotagapi.DbContexts
{
    public class TagsDbContext : DbContext
    {
        public DbSet<Tag> Tags { get; set; }

        public TagsDbContext(DbContextOptions<TagsDbContext> options) : base(options) { }
    }

}
