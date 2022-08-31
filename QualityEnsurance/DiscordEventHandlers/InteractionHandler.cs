using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Diagnostics;
using QualityEnsurance.Extensions;

namespace QualityEnsurance.DiscordEventHandlers
{
    public class InteractionHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly IDbContextFactory<ApplicationContext> _contextFactory;
        private readonly InteractionService _handler;
        private readonly IServiceProvider _services;
        private readonly IConfiguration _config;

        public InteractionHandler(DiscordSocketClient client, IDbContextFactory<ApplicationContext> contextFactory, InteractionService handler, IConfiguration config, IServiceProvider services)
        {
            _client = client;
            _contextFactory = contextFactory;
            _handler = handler;
            _services = services;
            _config = config;
        }

        public async Task InitializeAsync()
        {
            _client.Ready += ReadyAsync;
            _client.InteractionCreated += HandleInteraction;
            _handler.SlashCommandExecuted += CommandExecuted;

            _handler.Log += LogAsync;


            await _handler.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            
        }

        private Task LogAsync(LogMessage log) { Console.WriteLine(log); return Task.CompletedTask; }

        private async Task ReadyAsync()
        {
#if DEBUG
            foreach (var guildId in _config.GetArray<ulong>("testGuilds"))
            {
                try
                {
                    await _handler.RegisterCommandsToGuildAsync(guildId, true);
                    Console.WriteLine($"Registered commands to test guild with id \"{guildId}\"");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error trying to add commands to test guild \"{guildId}\"");
                    Console.WriteLine(ex);
                }
            }
#endif
            await _handler.RegisterCommandsGloballyAsync(true);
        }

        private async Task HandleInteraction(SocketInteraction interaction)
        {
            var context = new SocketInteractionContext(_client, interaction);

            var result = await _handler.ExecuteCommandAsync(context, _services);

            if (!result.IsSuccess)
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        await interaction.RespondAsync("You are missing required permissions to execute this command!", ephemeral: true);
                        break;
                    default:
                        break;
                }
        }

        private async Task CommandExecuted(SlashCommandInfo arg1, IInteractionContext context, IResult result)
        {
            if (!result.IsSuccess)
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        await context.Interaction.RespondAsync($"Error: {result.ErrorReason}", ephemeral: true);
                        break;
                    case InteractionCommandError.Exception:
                        if (result is ExecuteResult res)
                            switch (res.Exception)
                            {
                                case TimeoutException:
                                    Console.WriteLine($"WARNING: Reply to command timeouted. System clock needs resync");
                                    break;
                            }
                        break;
                }
        } 
    }
}
