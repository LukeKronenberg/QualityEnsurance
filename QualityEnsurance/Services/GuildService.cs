using QualityEnsurance.Models;

namespace QualityEnsurance.Services
{
    public static class GuildService
    {
        public static Guild GetGuild(this ApplicationContext context, long guildId, bool createNew = true)
        {
            Guild guild = context.Guilds.Find(guildId);
            if (guild == null && createNew)
            {
                guild = new Guild() { Id = guildId };
                context.Guilds.Add(guild);
            }
            return guild;
        }
    }
}
