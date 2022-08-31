using Discord;
using Discord.WebSocket;
using QualityEnsurance.Models;
using QualityEnsurance.Services;
using Microsoft.EntityFrameworkCore;

namespace QualityEnsurance.DiscordEventHandlers
{
    public class GuildHandler
    {
        private readonly DiscordSocketClient _discord;
        private readonly IDbContextFactory<ApplicationContext> _contextFactory;

        public GuildHandler(DiscordSocketClient discord, IDbContextFactory<ApplicationContext> contextFactory)
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
