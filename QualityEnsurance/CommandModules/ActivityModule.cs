using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using QualityEnsurance.Attributes;
using QualityEnsurance.Constants;
using QualityEnsurance.Extensions;
using QualityEnsurance.Models;
using System.Text;
using QualityEnsurance.DiscordEventHandlers;
using Microsoft.Extensions.Configuration;


namespace QualityEnsurance.CommandModules
{
    [Group("activity", "All activity related commands.")]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    [RequireUserPermissionOrOwner(GuildPermission.Administrator)]
    public class ActivityModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IDbContextFactory<QualityEnsuranceContext> _contextFactory;
        private readonly PresenceHandler _presenceHandler;
        private readonly IConfiguration _config;

        public ActivityModule(IDbContextFactory<QualityEnsuranceContext> contextFactory, PresenceHandler presenceHandler, IConfiguration config)
        {
            _contextFactory = contextFactory;
            _presenceHandler = presenceHandler;
            _config = config;
        }

        [SlashCommand("list", "List all registered activities or active activities of the specified user.")]
        public async Task ListActivities(IUser user = null)
        {
            await DeferAsync(ephemeral: true);

            if (user == null)
            {
                ulong guildId = Context.Guild.Id;

                using var context = _contextFactory.CreateDbContext();
                var guild = context.Guilds.Include(g => g.GuildActivities).Get(context, (long)guildId);

                if (guild.GuildActivities.Count == 0)
                {
                    var builder = new EmbedBuilder()
                        .WithCurrentTimestamp()
                        .WithTitle($"No activities registered for {Context.Guild.Name.Sanitize()}");
                    await FollowupAsync(embed: builder.Build(), ephemeral: true);
                }
                else
                {
                    List<Embed> embeds = new()
                    {
                        new EmbedBuilder()
                        .WithTitle($"Registered activites in {Context.Guild.Name.Sanitize()}:")
                        .WithCurrentTimestamp()
                        .Build()
                    };

                    foreach (var activitiesByType in guild.GuildActivities.GroupBy(ga => ga.Activity.GetFilterType()))
                    {
                        int activitesByTypeIndex = 0;
                        foreach (var activities in activitiesByType.OrderByName().Chunk(25))
                        {
                            EmbedBuilder builder = new();
                            if (activitesByTypeIndex == 0)
                            {
                                switch (activitiesByType.Key)
                                {
                                    case Activity.FilterType.OnlyName:
                                        builder.WithTitle("Filtered by Name:");
                                        break;
                                    case Activity.FilterType.OnlyApplication:
                                        builder.WithTitle("Filtered by App-Id:");
                                        break;
                                    case Activity.FilterType.ApplicationAndName:
                                        builder.WithTitle("Filtered by App-Id and name:");
                                        break;
                                    case Activity.FilterType.OnlySpotify:
                                        builder.WithTitle("Filtered by Spotify-Id:");
                                        break;
                                    case Activity.FilterType.CustomStatus:
                                        builder.WithTitle("Filtered by Custom Status content:");
                                        break;
                                    default:
                                        throw new NotSupportedException($"\"{activitiesByType.Key}\" is not supported!");
                                }
                            }

                            foreach (var guildActivity in activities)
                            {
                                EmbedFieldBuilder fieldBuilder = new();
                                fieldBuilder.WithName($"Id: {guildActivity.IdWithinGuild}");
                                
                                OptionBuilder optionBuilder = new();
                                if (guildActivity.Activity.Name != null && guildActivity.Activity.Name != "custom status")
                                    optionBuilder.AddOption("**Name**:", guildActivity.Activity.Name);
                                if (guildActivity.Activity.State != null)
                                    optionBuilder.AddOption("**State**:", guildActivity.Activity.State);
                                if (guildActivity.Activity.ApplicationId != null)
                                    optionBuilder.AddOption("**App-Id**:", guildActivity.Activity.ApplicationId);
                                if (guildActivity.Activity.SpotifyId != null)
                                    optionBuilder.AddOption("**Spotify-Id**:", guildActivity.Activity.SpotifyId);

                                optionBuilder.AddOption("**Action**:", guildActivity.Action);
                                optionBuilder.AddOption("**Countdown-Duration**:", $"{guildActivity.CountdownDurationS}s");
                                if (guildActivity.Action == BotActionType.Timeout)
                                    optionBuilder.AddOption("**Timeout duration**:", $"{guildActivity.TimeoutDurationS}s");
                                optionBuilder.AddOption("**Start message**:", guildActivity.StartMessage ?? "No message");
                                optionBuilder.AddOption("**Action message**:", guildActivity.ActionMessage ?? "No message");

                                fieldBuilder.WithValue(optionBuilder.ToString());
                                builder.AddField(fieldBuilder);
                            }
                            embeds.Add(builder.Build());
                        }
                    }
                    await FollowupAsync(embeds: embeds.ToArray(), ephemeral: true);
                }
            }
            else
            {
                var uncheckedActivities = user.Activities;

                if (uncheckedActivities.Count == 0)
                {
                    var builder = new EmbedBuilder()
                        .WithCurrentTimestamp()
                        .WithTitle($"Activities")
                        .WithDescription($"{user.Mention} has currently no active activites.");
                    await FollowupAsync(embed: builder.Build(), ephemeral: true);
                }
                else
                {
                    List<Embed> embeds = new()
                    {
                        new EmbedBuilder()
                           .WithTitle($"{user.Username?.Sanitize()} has {uncheckedActivities.Count} active activit{(uncheckedActivities.Count == 1 ? "y" : "ies")}.")
                           .WithCurrentTimestamp()
                           .Build()
                    };

                    foreach (var activitiesByType in uncheckedActivities.GroupBy(ga => ga.GetType()))
                    {
                        List<EmbedFieldBuilder> fields = new();
                        foreach (var (activity, index) in activitiesByType.Order().Select((value, i) => (value, i)))
                        {
                            OptionBuilder optionBuilder = new();
                            switch (activitiesByType.Key.Name)
                            {
                                case nameof(Game):
                                    optionBuilder.AddOption("**Activity-Name**:", activity.Name?.SanitizeCode());
                                    if (!string.IsNullOrEmpty(activity.Details))
                                        optionBuilder.AddOption("**Activity-Details**:", activity.Details?.SanitizeCode());
                                    break;
                                case nameof(RichGame):
                                    var richgame = (RichGame)activity;
                                    optionBuilder.AddOption("**Activity-Name**:", richgame.Name?.SanitizeCode());
                                    optionBuilder.AddOption("**App-Id**:", richgame.ApplicationId);
                                    if (!string.IsNullOrEmpty(richgame.State))
                                        optionBuilder.AddOption("**Activity-State**:", richgame.State?.SanitizeCode());
                                    if (!string.IsNullOrEmpty(richgame.Details))
                                        optionBuilder.AddOption("**Activity-Details**:", richgame.Details?.SanitizeCode());
                                    break;
                                case nameof(SpotifyGame):
                                    var spotifyGame = (SpotifyGame)activity;
                                    optionBuilder.AddOption("**Activity-Name**:", spotifyGame.Name?.SanitizeCode());
                                    optionBuilder.AddOption("**Spotify-Id**:", spotifyGame.TrackId);
                                    if (spotifyGame.AlbumTitle != null)
                                        optionBuilder.AddOption("**Album-Title**:", spotifyGame.AlbumTitle?.SanitizeCode());
                                    break;
                                case nameof(CustomStatusGame):
                                    var customStatusGame = (CustomStatusGame)activity;
                                    optionBuilder.AddOption("**State**:", customStatusGame.State?.SanitizeCode());
                                    break;
                            }

                            fields.Add(new EmbedFieldBuilder().WithName($"{index + 1}.").WithValue(optionBuilder.ToString()));
                        }

                        int currentActivityIndex = 0;
                        foreach (var chuckedFields in fields.Chunk(f => f.Name.Length + f.Value.ToString().Length, 6000, 25))
                        {
                            EmbedBuilder builder = new();
                            if (currentActivityIndex == 0)
                            {
                                switch (activitiesByType.Key.Name)
                                {
                                    case nameof(Game):
                                        builder.WithTitle("Unknown Activities:");
                                        break;
                                    case nameof(RichGame):
                                        builder.WithTitle("Applications:");
                                        break;
                                    case nameof(SpotifyGame):
                                        builder.WithTitle("Spotify Songs:");
                                        break;
                                    case nameof(CustomStatusGame):
                                        builder.WithTitle("Custom Status:");
                                        break;
                                    default:
                                        continue;
                                }
                            }
                            builder.WithFields(chuckedFields);
                            embeds.Add(builder.Build());
                        }
                    }
                    await FollowupAsync(embeds: embeds.ToArray(), ephemeral: true);
                }
            }
        }

        [SlashCommand("add", "Add an activity to watch for")]
        public async Task AddActivity(
            [Summary("name", "Name of the Activity (Case insensitive). (Use \"custom status\" to match Custom statuses)")]
            [MaxLength(100)]
            string name = null,
            [Summary("app-id", "Id of an discord application (Only works when the app developer has registered it).")]
            [MaxLength(25)]
            string appIdAsString = null,
            [Summary("spotify-id", "The Spotify id of the song.")]
            [MaxLength(100)]
            string spotifyId = null,
            [Summary("state", "Filter by the custom status content or the state of an application (eg. \"In menu\")")]
            [MaxLength(100)]
            string state = null,
            [Summary("action", "What to do for punishment. Default: Nothing")]
            [Choice("Nothing", (int)BotActionType.None), 
             Choice("Only-message", (int)BotActionType.OnlyMessage), 
             Choice("Timeout", (int)BotActionType.Timeout), 
             Choice("Ban", (int)BotActionType.Ban)]
            BotActionType actionType = BotActionType.None,
            [Summary("countdown-duration", "Duration of timeout after an timeout action in seconds. Default: 30min")]
            [MinValue(0), MaxValue(86400)]
            int countdownDuration = 1800,
            [Summary("timeout-duration", "Duration of timeout after an timeout action in seconds. Default: 30min")]
            [MinValue(1), MaxValue(31536000)]
            int timeoutDuration = 1800,
            [Summary("start-message", "Set the message an user will recive when starting the activity. Leave empty to disable.")]
            [MaxLength(500)]
            string startMessage = null,
            [Summary("action-message", "Set the message an user will recive when recieving activity action. Leave empty to disable.")]
            [MaxLength(500)]
            string actionMessage = null
            )
        {
            name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
            state = string.IsNullOrWhiteSpace(state) ? null : state.Trim();
            if (name?.Length > 100)
            {
                await RespondAsync("The name can't be longer than 100 caracters!", ephemeral: true);
                return;
            }
            name = name?.ToLower();
            if (name == "custom status" && state == null)
            {
                await RespondAsync("When checking for the custom status activity, the state value must be provided.", ephemeral: true);
                return;
            }

            long? appId = null;
            if (appIdAsString != null)
            {
                if (ulong.TryParse(appIdAsString.Trim(), out ulong appIdTmp))
                    appId = (long)appIdTmp;
                else
                {
                    await RespondAsync("The provided input is not an valid App-Id. Use `/activity list @User` to get App-Ids.", ephemeral: true);
                    return;
                }
            }
            if (name == null && appId == null && spotifyId == null)
            {
                await RespondAsync("At least one of \"name\", \"app-id\" or \"spotify-id\" must be set.", ephemeral: true);
                return;
            }

            if (appId != null && spotifyId != null)
            {
                await RespondAsync("No activity can have both \"app-id\" and \"spotify-id\" set.", ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);

            using var context = _contextFactory.CreateDbContext();

            var guild = context.Get<Guild>((long)Context.Guild.Id);
            int gaCount = guild.GuildActivities.Count();
            // Check if maximum activities for this guild is reached and skip check if botowner
            if (gaCount >= guild.MaxActivities && Array.IndexOf(_config.GetBotOwners(), Context.User.Id) == -1)
            {
                await FollowupAsync($"The maximum amount of activities registered for this guild has been reached ({gaCount}/{guild.MaxActivities}).", ephemeral: true);
                return;
            }
            
            // Find possibly already existing activity
            var activity = context.Activities.SingleOrDefault(a => a.Name == name && a.ApplicationId == appId && a.SpotifyId == spotifyId && a.State == state);
            if (activity != null)
            {
                var ga = guild.GuildActivities.FirstOrDefault(ga => ga.Activity == activity);
                if (ga != null)
                {
                    await FollowupAsync("An activity with the same name, App-Id or Spotify-Id is alreay registered.", ephemeral: true);
                    return;
                }
            } 
            else
            {
                activity = new()
                {
                    Name = name,
                    ApplicationId = appId,
                    SpotifyId = spotifyId,
                    State = state
                };
                context.Activities.Add(activity);
            }

            GuildActivity guildActivity = new()
            {
                IdWithinGuild = guild.GuildActivityNextId++,
                Guild = guild,
                Activity = activity,
                User = context.Get<User>((long)Context.User.Id),
                Action = actionType,
                StartMessage = startMessage,
                ActionMessage = actionMessage,
                TimeoutDurationS = timeoutDuration,
                CountdownDurationS = countdownDuration,
            };

            context.Add(guildActivity);
            context.SaveChanges();

            EmbedBuilder embed = new();
            embed.WithTitle("Successfully added activity.");
            guildActivity.AddToEmbed(embed);
            await FollowupAsync(embed: embed.Build(), ephemeral: true);
        }

        [SlashCommand("edit", "Edit an activity with new values.")]
        public async Task EditActivity(
            [Summary("id", "The id of the activity. Can be found by using `/activity list`.")]
            uint id,
            [Summary("action", "What to do for punishment.")]
            [Choice("Nothing", (int)BotActionType.None), 
             Choice("Only-message", (int)BotActionType.OnlyMessage), 
             Choice("Timeout", (int)BotActionType.Timeout), 
             Choice("Ban", (int)BotActionType.Ban)]
            BotActionType? actionType = null,
            [Summary("countdown-duration", "Duration of timeout after an timeout action in seconds.")]
            [MinValue(0), MaxValue(86400)]
            int? countdownDuration = null,
            [Summary("timeout-duration", "Duration of timeout after an timeout action in seconds.")]
            [MinValue(1), MaxValue(31536000)]
            int? timeoutDuration = null,
            [Summary("start-message", "Set the message an user will recive when starting an activity. Set value to \"disable\" to disable.")]
            [MaxLength(500)]
            string startMessage = null,
            [Summary("action-message", "Set the message an user will recive when being punished. Set value to \"disable\" to disable.")]
            [MaxLength(500)]
            string actionMessage = null
            )
        {
            await DeferAsync(ephemeral: true);

            using var context = _contextFactory.CreateDbContext();

            GuildActivity guildActivity = context.GuildActivities.FirstOrDefault(ga => ga.GuildId == (long)Context.Guild.Id && ga.IdWithinGuild == id);
            if (guildActivity == null)
            {
                await FollowupAsync($"No activity found with id {id}.", ephemeral: true);
                return;
            }

            GuildActivity guildActivityCopy = guildActivity.ShallowCopy();

            if (actionType.HasValue)
                guildActivity.Action = actionType.Value;
            if (countdownDuration.HasValue)
                guildActivity.CountdownDurationS = countdownDuration.Value;
            if (timeoutDuration.HasValue)
                guildActivity.TimeoutDurationS = timeoutDuration.Value;
            if (startMessage != null)
            {
                if (startMessage.ToLower() == "disable")
                    guildActivity.StartMessage = null;
                else
                    guildActivity.StartMessage = startMessage;
            }
            if (actionMessage != null)
            {
                if (actionMessage.ToLower() == "disable")
                    guildActivity.ActionMessage = null;
                else
                    guildActivity.ActionMessage = actionMessage;
            }

            context.SaveChanges();
            
            EmbedBuilder embed = new();
            embed.WithTitle($"Successfully edited activity {guildActivity.IdWithinGuild}.");
            guildActivity.AddToEmbed(embed, guildActivityCopy);
            
            await FollowupAsync(embed: embed.Build(), ephemeral: true);
        }

        [SlashCommand("remove", "Remove an registered activity.")]
        public async Task RemoveActivity(
            [Summary("id", "The id of the activity. Can be found by using `/activity list`.")]
            uint id)
        {
            await DeferAsync(ephemeral: true);

            using var context = _contextFactory.CreateDbContext();

            GuildActivity guildActivity = context.GuildActivities.FirstOrDefault(ga => ga.GuildId == (long)Context.Guild.Id && ga.IdWithinGuild == id);

            if (guildActivity == null)
            {
                await FollowupAsync($"No activity with id \"{id}\" found.", ephemeral: true);
                return;
            }

            if (guildActivity.Activity.GuildActivities.Count == 1)
                context.Remove(guildActivity.Activity);
            context.RemoveRange(guildActivity.PendingActions);
            context.RemoveRange(guildActivity.GuildActivityUserSettings);
            context.Remove(guildActivity);
            
            var pendingActionEntries = _presenceHandler.Actions
                .Where(ae => ae.Value.GuildId == (long)Context.Guild.Id && ae.Value.ActivityId == guildActivity.ActivityId)
                .ToArray();
            
            context.SaveChanges();
            
            foreach (var actionEntry in pendingActionEntries)
            {
                _presenceHandler.Actions.Remove(actionEntry.Key, out _);
                actionEntry.Value.Dispose();
            }

            await FollowupAsync($"Successfully deleted activity with id \"{id}\".", ephemeral: true);
        }

        [SlashCommand("remove-all", "Remove all activities.")]
        public async Task RemoveAllActivities(
            [Summary("confirm", "Confirm that you want to remove all activities.")]
            bool confirm = false)
        {
            if (!confirm)
            {
                await RespondAsync("Please confirm this action by setting the optional paramter \"confirm\" to True.", ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);

            using var context = _contextFactory.CreateDbContext();

            IQueryable<GuildActivity> guildActivities = context.GuildActivities
                .Include(ga => ga.GuildActivityUserSettings)
                .Include(ga => ga.PendingActions)
                .Where(ga => ga.Guild.Id == (long)Context.Guild.Id);
            
            if (!guildActivities.Any())
            {
                await FollowupAsync($"No activities registered for this guild.", ephemeral: true);
                return;
            }
            
            context.RemoveRange(guildActivities.SelectMany(ga => ga.PendingActions));
            context.RemoveRange(guildActivities.SelectMany(ga => ga.GuildActivityUserSettings));
            context.RemoveRange(guildActivities);
            context.SaveChanges();

            var pendingActionsPairs = _presenceHandler.Actions
                .Where(ae => ae.Value.GuildId == (long)Context.Guild.Id);
            foreach (var actionPair in pendingActionsPairs)
            {
                _presenceHandler.Actions.Remove(actionPair.Key, out _);
                actionPair.Value.Dispose();
            }

            await FollowupAsync($"All activities successfully removed.", ephemeral: true);
        }
    }

    [Group("action", "Commands related to enqued actions")]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(ChannelPermission.SendMessages)]
    [RequireUserPermissionOrOwner(GuildPermission.Administrator)]
    public class ActionModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IDbContextFactory<QualityEnsuranceContext> _contextFactory;
        private readonly PresenceHandler _presenceHandler;

        public ActionModule(IDbContextFactory<QualityEnsuranceContext> contextFactory, PresenceHandler presenceHandler)
        {
            _contextFactory = contextFactory;
            _presenceHandler = presenceHandler;
        }

        [SlashCommand("list", "List all pending actions for this guild.")]
        public async Task GetActions()
        {
            await DeferAsync(ephemeral: true);

            using var context = _contextFactory.CreateDbContext();

            // Get existing guild or create a new instance
            Guild guild = context.Guilds
                .Include(g => g.GuildActivities)
                .Get(context, (long)Context.Guild.Id);

            (ActionEntry entry, GuildActivity activity)[] guildEntries = _presenceHandler.Actions
                .Where(ae => ae.Value.GuildId == (long)Context.Guild.Id)
                .Select(ae => (ae.Value, guild.GuildActivities.FirstOrDefault(ga => ga.ActivityId == ae.Value.ActivityId)))
                .ToArray();

            if (guildEntries.Length == 0)
            {
                await FollowupAsync("No actions pending.", ephemeral: true);
                return;
            }

            if (guildEntries.Length > 50)
            {
                await FollowupAsync($"To many pending actions ({guildEntries.Length}) to display them.", ephemeral: true);
            }
            else
            {
                List<EmbedFieldBuilder> fields = new();
                foreach (var entry in guildEntries)
                {
                    fields.Add(new EmbedFieldBuilder().WithName($"Activity-Id: {entry.activity.IdWithinGuild}")
                        .WithValue(
                            $"**ETA**: {entry.entry.ETA:H:mm:ss dd.MM.yyyy}\n" +
                            $"**Remaining**: -{entry.entry.ETA - DateTime.Now:d\\d\\ hh\\h\\ mm\\m\\ ss\\s}\n" +
                            $"**Action**: {entry.activity.Action}\n" +
                            $"**Target**: {Context.Client.GetUser((ulong)entry.entry.UserId).Mention}"));
                }

                List<Embed> embeds = new();
                foreach (var chunckedFileds in fields.Chunk(f => f.Name.Length + f.Value.ToString().Length, 6000, 25))
                {
                    EmbedBuilder embed = new();
                    embed.WithTitle("Pending actions");
                    embed.WithFields(chunckedFileds);
                    embeds.Add(embed.Build());
                }

                foreach (var chunckedEmbeds in embeds.Chunk(e => e.Title.Length + e.Fields.Sum(f => f.Name.Length + f.Value.ToString().Length), 6000, 10))
                {
                    await FollowupAsync("_ _", embeds: chunckedEmbeds, ephemeral: true);
                }
            }
        }
    }
}
