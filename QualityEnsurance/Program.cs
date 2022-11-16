using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QualityEnsurance.DiscordEventHandlers;
using Discord.Interactions;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;

namespace QualityEnsurance
{
    public class Program
    {
        public static IConfigurationRoot Configuration { get; set; }
        public static IServiceProvider Services { get; set; }

        public const string UserId = "QualityEnsurance";

        public static async Task Main(string[] args)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("./appsettings.json", false, true)
                .Build();

            Services = new ServiceCollection()
                .AddSingleton<IConfiguration>(Configuration)
                .AddDbContextFactory<QualityEnsuranceContext>(options =>
                    {
                        options.UseNpgsql(Configuration.GetConnectionString("DB_Connection"));
                        options.UseLazyLoadingProxies();
                        options.EnableDetailedErrors();
                        options.EnableSensitiveDataLogging();
                    })
                .AddSingleton<DiscordSocketClient>(s => new(new DiscordSocketConfig() { 
                    GatewayIntents = 
                        GatewayIntents.GuildMessages    |
                        GatewayIntents.MessageContent   |
                        GatewayIntents.GuildPresences   | 
                        GatewayIntents.GuildMembers     |
                        GatewayIntents.Guilds
                }))
                .AddSingleton<InteractionService>(s => new(s.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<InteractionHandler>()
                .AddSingleton<PresenceHandler>()
                .AddSingleton<MessageHandler>()
                .BuildServiceProvider();

            Services.GetRequiredService<MessageHandler>().Initialize();
            await Services.GetRequiredService<PresenceHandler>().InitializeAsync();

            var client = Services.GetRequiredService<DiscordSocketClient>();
            client.Log += LogAsync;

            await Services.GetRequiredService<InteractionHandler>()
                .InitializeAsync();
            
            await client.LoginAsync(TokenType.Bot, Configuration.GetConnectionString("Bot_Secret_Token"));
            await client.StartAsync();

            await Task.Delay(Timeout.Infinite);
        }

        public static Task LogAsync(LogMessage log)
        {
            return LogAsync(log.ToString(prependTimestamp: false));
        }

        public static Task LogAsync(object obj) => LogAsync(obj.ToString());

        public static Task LogAsync(string message)
        {
            StringBuilder builder = new();
            builder.Append($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ");
            builder.Append(message);
            Console.WriteLine(builder.ToString());
            return Task.CompletedTask;
        }
    }
}