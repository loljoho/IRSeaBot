using IRSeaBot.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace IRSeaBot.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<SeenUser> SeenUsers { get; set; }
    }
}
