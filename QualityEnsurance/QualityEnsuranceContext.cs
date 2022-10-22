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

        public QualityEnsuranceContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GuildActivity>().HasKey(ga => new { ga.GuildId, ga.ActivityId });
            var gauEntity = modelBuilder.Entity<GuildActivityUser>();
            gauEntity.HasKey(gau => new { gau.GuildId, gau.ActivityId, gau.UserId });
            gauEntity.HasOne(gau => gau.GuildActivity).WithMany(ga => ga.GuildActivityUserSettings).HasForeignKey(gau => new { gau.GuildId, gau.ActivityId });
        }

        public Channel GetChannel(long channelId)
        {
            Channel channel = Channels.Find(channelId);
            if (channel == null)
            {
                channel = new Channel() { Id = channelId };
                Channels.Add(channel);
            }
            return channel;
        }

        public User GetUser(long userId)
        {
            User user = Users.Find(userId);
            if (user == null)
            {
                user = new User() { Id = userId };
                Users.Add(user);
            }
            return user;
        }

        public Guild GetGuild(long guildId, bool createNew = true)
        {
            Guild guild = Guilds.Find(guildId);
            if (guild == null && createNew)
            {
                guild = new Guild() { Id = guildId };
                Guilds.Add(guild);
            }
            return guild;
        }
        public GuildActivityUser GetGuildActivityUser(GuildActivity guildActivity, User user, bool createNew = true)
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
                GuildActivityUserSettings.Add(gau);
            }
            return gau;
        }

        public GuildActivityUser GetGuildActivityUser(GuildActivity guildActivity, long userId, bool createNew = true)
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
                GuildActivityUserSettings.Add(gau);
            }
            return gau;
        }

        public GuildActivityUser GetGuildActivityUser(long guildId, long activityId, long userId, bool createNew = true)
        {
            GuildActivityUser gau = GuildActivityUserSettings.Find(guildId, activityId, userId);
            if (gau == null && createNew)
            {
                gau = new GuildActivityUser() { GuildId = guildId, ActivityId = activityId, UserId = userId };
                GuildActivityUserSettings.Add(gau);
            }
            return gau;
        }
    }
}