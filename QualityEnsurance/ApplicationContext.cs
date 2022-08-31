using QualityEnsurance.Models;
using Microsoft.EntityFrameworkCore;

namespace QualityEnsurance
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Guild> Guilds { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<GuildActivity> GuildActivities { get; set; }
        public DbSet<GuildActivityUser> GuildActivityUserSettings { get; set; }

        public ApplicationContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GuildActivity>().HasKey(ga => new { ga.GuildId, ga.ActivityId });
            var gauEntity = modelBuilder.Entity<GuildActivityUser>();
            gauEntity.HasKey(gau => new { gau.GuildId, gau.ActivityId, gau.UserId });
            gauEntity.HasOne(gau => gau.GuildActivity).WithMany(ga => ga.GuildActivityUserSettings).HasForeignKey(gau => new { gau.GuildId, gau.ActivityId });
        }
    }
}