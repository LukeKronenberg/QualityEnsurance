using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace QualityEnsurance.DiscordEventHandlers
{
    public class GuildHandler
    {
        private readonly DiscordSocketClient _discord;
        private readonly IDbContextFactory<QualityEnsuranceContext> _contextFactory;

        public GuildHandler(DiscordSocketClient discord, IDbContextFactory<QualityEnsuranceContext> contextFactory)
        {
            _discord = discord;
            _contextFactory = contextFactory;

            _discord.JoinedGuild += JoinedGuild;
        }

        private Task JoinedGuild(SocketGuild joinedGuild)
        {
            using var context = _contextFactory.CreateDbContext();
            context.GetGuild((long)joinedGuild.Id, true);
            context.SaveChanges();
            return Task.CompletedTask;
        }
    }
}
