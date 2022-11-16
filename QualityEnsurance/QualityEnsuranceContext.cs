using QualityEnsurance.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace QualityEnsurance
{
    public class QualityEnsuranceContext : DbContext
    {
        public DbSet<Guild> Guilds { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<GuildActivity> GuildActivities { get; set; }
        public DbSet<GuildActivityUser> GuildActivityUserSettings { get; set; }
        public DbSet<YoutubeUser> YoutubeUsers { get; set; }
        public DbSet<Channel> Channels { get; set; }

        public DbSet<PendingAction> PendingActions { get; set; }

        public QualityEnsuranceContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<GuildActivity>()
                .HasKey(ga => new { ga.GuildId, ga.ActivityId });
            
            var gauEntity = modelBuilder.Entity<GuildActivityUser>();
            gauEntity.HasKey(gau => new { gau.GuildId, gau.ActivityId, gau.UserId });
            gauEntity
                .HasOne(gau => gau.GuildActivity)
                .WithMany(ga => ga.GuildActivityUserSettings)
                .HasForeignKey(gau => new { gau.GuildId, gau.ActivityId });

            var qaEntity = modelBuilder
                .Entity<PendingAction>();
            qaEntity.HasKey(qa => new { qa.GuildId, qa.UserId, qa.ActivityId });
            qaEntity
                .HasOne(qa => qa.GuildActivityUser)
                .WithOne(gau => gau.PendingActions)
                .HasForeignKey<PendingAction>(qa => new { qa.GuildId, qa.UserId, qa.ActivityId });
            qaEntity
                .HasOne(qa => qa.GuildActivity)
                .WithMany(ga => ga.PendingActions)
                .HasForeignKey(qa => new { qa.GuildId, qa.ActivityId });
        }
        
        public TElement Get<TElement>(long id) where TElement : DbModel, new()
        {
            TElement element = Set<TElement>().FirstOrDefault(e => e.Id == id);
            if (element == null)
            {
                element = new TElement() { Id = id };
                Add(element);
            }
            return element;
        }
    }

    public static class QualityEnsuranceContextExtensions
    {
        public static TElement Get<TElement>(this IQueryable<TElement> elements, DbContext context, long id, bool createNew = true) where TElement : DbModel, new()
        {
            TElement element = elements.FirstOrDefault(e => e.Id == id);
            if (element == null && createNew)
            {
                element = new TElement() { Id = id };
                context.Add(element);
            }
            return element;
        }

        public static GuildActivityUser GetGuildActivityUser(this QualityEnsuranceContext context, GuildActivity guildActivity, User user, bool createNew = true)
        {
            GuildActivityUser gau = guildActivity.GuildActivityUserSettings.FirstOrDefault(gau => gau.User == user);
            if (gau == null && createNew)
            {
                gau = new GuildActivityUser()
                {
                    Guild = guildActivity.Guild,
                    GuildId = guildActivity.GuildId,
                    Activity = guildActivity.Activity,
                    ActivityId = guildActivity.ActivityId,
                    User = user,
                    UserId = user.Id,
                };
                guildActivity.GuildActivityUserSettings.Add(gau);
                context.GuildActivityUserSettings.Add(gau);
            }
            return gau;
        }

        public static GuildActivityUser GetGuildActivityUser(this QualityEnsuranceContext context, GuildActivity guildActivity, long userId, bool createNew = true)
        {
            GuildActivityUser gau = guildActivity.GuildActivityUserSettings.FirstOrDefault(gau => gau.UserId == userId);
            if (gau == null && createNew)
            {
                gau = new GuildActivityUser()
                {
                    Guild = guildActivity.Guild,
                    GuildId = guildActivity.GuildId,
                    Activity = guildActivity.Activity,
                    ActivityId = guildActivity.ActivityId,
                    UserId = userId
                };
                guildActivity.GuildActivityUserSettings.Add(gau);
                context.GuildActivityUserSettings.Add(gau);
            }
            return gau;
        }

        public static GuildActivityUser GetGuildActivityUser(this QualityEnsuranceContext context, long guildId, long activityId, long userId, bool createNew = true)
        {
            GuildActivityUser gau = context.GuildActivityUserSettings.Find(guildId, activityId, userId);
            if (gau == null && createNew)
            {
                gau = new GuildActivityUser() { GuildId = guildId, ActivityId = activityId, UserId = userId };
                context.GuildActivityUserSettings.Add(gau);
            }
            return gau;
        }
    }
}