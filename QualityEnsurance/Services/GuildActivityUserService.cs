using QualityEnsurance.Models;

namespace QualityEnsurance.Services
{
    public static class GuildActivityUserService
    {

        public static GuildActivityUser GetGuildActivityUser(this ApplicationContext context, GuildActivity guildActivity, User user, bool createNew = true)
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

        public static GuildActivityUser GetGuildActivityUser(this ApplicationContext context, GuildActivity guildActivity, long userId, bool createNew = true)
        {
            GuildActivityUser gau = guildActivity.GuildActivityUserSettings.FirstOrDefault(gau => gau.UserId == userId);
            if (gau == null && createNew)
            {
                gau = new GuildActivityUser() { 
                    Guild = guildActivity.Guild, 
                    GuildId = guildActivity.GuildId,
                    Activity = guildActivity.Activity, 
                    ActivityId = guildActivity.ActivityId,
                    UserId = userId };
                guildActivity.GuildActivityUserSettings.Add(gau);
                context.GuildActivityUserSettings.Add(gau);
            }
            return gau;
        }

        public static GuildActivityUser GetGuildActivityUser(this ApplicationContext context, long guildId, long activityId, long userId, bool createNew = true)
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
