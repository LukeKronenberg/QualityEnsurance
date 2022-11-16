using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using QualityEnsurance.Models;

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
            context.Get<Guild>((long)joinedGuild.Id);
            context.SaveChanges();
            return Task.CompletedTask;
        }
    }
}
