using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using QualityEnsurance.Attributes;
using QualityEnsurance.Constants;
using QualityEnsurance.Extensions;
using QualityEnsurance.Models;
using System.Text;
using QualityEnsurance.DiscordEventHandlers;


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

        public ActivityModule(IDbContextFactory<QualityEnsuranceContext> contextFactory, PresenceHandler presenceHandler)
        {
            _contextFactory = contextFactory;
            _presenceHandler = presenceHandler;
        }

        [SlashCommand("list", "List all registered activities or active activities of the specified user.")]
        public async Task ListActivities(IUser user = null)
        {
            await DeferAsync(ephemeral: true);

            if (user == null)
            {
                ulong guildId = Context.Guild.Id;

                using var context = _contextFactory.CreateDbContext();
                var guild = context.GetGuild((long)guildId);

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
                                StringBuilder valueBuilder = new();
                                if (guildActivity.Activity.Name != null && guildActivity.Activity.Name != "custom status")
                                    valueBuilder.Append($"**Name**: `{guildActivity.Activity.Name.SanitizeCode()}`\n");
                                if (guildActivity.Activity.State != null)
                                    valueBuilder.Append($"**State**: `{guildActivity.Activity.State.SanitizeCode()}`\n");
                                if (guildActivity.Activity.ApplicationId != null)
                                    valueBuilder.Append($"**App-Id**: `{guildActivity.Activity.ApplicationId}`\n");
                                if (guildActivity.Activity.SpotifyId != null)
                                    valueBuilder.Append($"**Spotify-Id**: `{guildActivity.Activity.SpotifyId.SanitizeCode()}`\n");

                                valueBuilder.Append($"**Action**: {guildActivity.Action}\n");
                                valueBuilder.Append($"**Countdown-Duration**: {guildActivity.CountdownDurationS}s\n");
                                if (guildActivity.Action == BotActionType.Timeout)
                                    valueBuilder.Append($"**Timeout duration**: {guildActivity.TimeoutDurationS}\n");
                                valueBuilder.Append($"**Start message**: {guildActivity.StartMessage ?? "No message"}\n");
                                valueBuilder.Append($"**Action message**: {guildActivity.ActionMessage ?? "No message"}");

                                fieldBuilder.WithValue(valueBuilder.ToString());
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
                        int activitesByTypeIndex = 1;
                        foreach (var activities in activitiesByType.Order().Chunk(25))
                        {
                            EmbedBuilder builder = new();
                            if (activitesByTypeIndex == 1)
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

                            foreach (var activity in activities)
                            {
                                EmbedFieldBuilder fieldBuilder = new();
                                fieldBuilder.WithName($"{activitesByTypeIndex}.");
                                switch (activitiesByType.Key.Name)
                                {
                                    case nameof(Game):
                                        fieldBuilder.Value =
                                            $"**Activity-Name**: `{activity.Name?.SanitizeCode()}`";
                                        if (!string.IsNullOrEmpty(activity.Details))
                                            fieldBuilder.Value += $"\n**Activity-Details: `{activity.Details.SanitizeCode()}`";
                                        break;
                                    case nameof(RichGame):
                                        var richgame = (RichGame)activity;
                                        fieldBuilder.Value =
                                            $"**Activity-Name**: `{activity.Name?.SanitizeCode()}`\n" +
                                            $"**Application-Id**: `{richgame.ApplicationId}`";
                                        if (!string.IsNullOrEmpty(richgame.State))
                                            fieldBuilder.Value += $"\n**Activity-State**: `{richgame.State.SanitizeCode()}`";
                                        if (!string.IsNullOrEmpty(richgame.Details))
                                            fieldBuilder.Value += $"\n**Activity-Details**: `{activity.Details.SanitizeCode()}`";
                                        break;
                                    case nameof(SpotifyGame):
                                        var spotifyGame = (SpotifyGame)activity;
                                        fieldBuilder.Value =
                                            $"**Activity-Name**: `{activity.Name?.SanitizeCode()}`\n" +
                                            $"**Spotify-Id**: `{spotifyGame.TrackId}`\n";
                                        if (spotifyGame.AlbumTitle != null)
                                            fieldBuilder.Value += $"**Album**: `{spotifyGame.AlbumTitle.SanitizeCode()}`\n";
                                        break;
                                    case nameof(CustomStatusGame):
                                        var customStatusGame = (CustomStatusGame)activity;
                                        fieldBuilder.Value = 
                                            $"**State**: `{customStatusGame.State?.SanitizeCode()}`";
                                        break;
                                }

                                builder.AddField(fieldBuilder);
                                activitesByTypeIndex++;
                            }
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
            string name = null,
            [Summary("app-id", "Id of an discord application (Only works when the app developer has registered it).")]
            string appIdAsString = null,
            [Summary("spotify-id", "The Spotify id of the song.")]
            string spotifyId = null,
            [Summary("state", "Filter by the custom status content or the state of an application (eg. \"In menu\")")]
            string state = null,
            [Summary("action", "What to do for punishment. Default: Nothing")]
            [Choice("Nothing", (int)BotActionType.None), Choice("Only-message", (int)BotActionType.OnlyMessage), Choice("Timeout", (int)BotActionType.Timeout), Choice("Ban", (int)BotActionType.Ban)]
            BotActionType actionType = BotActionType.None,
            [Summary("countdown-duration", "Duration of timeout after an timeout action in seconds. Default: 30min")]
            int countdownDuration = 1800,
            [Summary("timeout-duration", "Duration of timeout after an timeout action in seconds. Default: 30min")]
            int timeoutDuration = 1800,
            [Summary("start-message", "Set the message an user will recive when starting the activity. Leave empty to disable.")]
            string startMessage = null,
            [Summary("action-message", "Set the message an user will recive when recieving activity action. Leave empty to disable.")]
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

            var guild = context.GetGuild((long)Context.Guild.Id);
            ulong ownerId = (await Context.Client.GetApplicationInfoAsync()).Owner.Id;
            int gaCount = guild.GuildActivities.Where(ga => ga.UserId != (long)ownerId).Count();
            if (gaCount >= guild.MaxActivities && ownerId != Context.User.Id)
            {
                await FollowupAsync($"The maximum amount of activities registered for this guild has been reached ({gaCount}/{guild.MaxActivities}).", ephemeral: true);
                return;
            }
            
            var activity = context.Activities.SingleOrDefault(a => a.Name == name && a.ApplicationId == appId && a.SpotifyId == spotifyId && a.State == state);
            if (activity != null)
            {
                var ga = guild.GuildActivities.FirstOrDefault(ga => ga.Activity == activity);
                if (ga != null)
                {
                    await FollowupAsync("An activity with the same \"name\", \"app-id\" or \"track-id\" is alreay registered.", ephemeral: true);
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
                User = context.GetUser((long)Context.User.Id),
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
            [Choice("Nothing", (int)BotActionType.None), Choice("Only-message", (int)BotActionType.OnlyMessage), Choice("Timeout", (int)BotActionType.Timeout), Choice("Ban", (int)BotActionType.Ban)]
            BotActionType? actionType = null,
            [Summary("countdown-duration", "Duration of timeout after an timeout action in seconds.")]
            int? countdownDuration = null,
            [Summary("timeout-duration", "Duration of timeout after an timeout action in seconds.")]
            int? timeoutDuration = null,
            [Summary("start-message", "Set the message an user will recive when starting an activity. Set value to \"disable\" to disable.")]
            string startMessage = null,
            [Summary("action-message", "Set the message an user will recive when being punished. Set value to \"disable\" to disable.")]
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
            embed.WithTitle("Successfully edited activity.");
            guildActivity.AddToEmbed(embed);
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
            context.RemoveRange(guildActivity.GuildActivityUserSettings);
            context.Remove(guildActivity);
            context.SaveChanges();
            
            var pendingActions = _presenceHandler.PendingActions.Where(ae => ae.GuildId == (long)Context.Guild.Id && ae.ActivityId == guildActivity.ActivityId).ToArray();
            foreach (var action in pendingActions)
            {
                _presenceHandler.PendingActions.Remove(action);
                action.Dispose();
            }

            await FollowupAsync($"Successfully deleted activity with id {id}.", ephemeral: true);
        }

        [SlashCommand("remove-all", "Remove all activities.")]
        public async Task RemoveAllActivities()
        {
            await DeferAsync(ephemeral: true);

            using var context = _contextFactory.CreateDbContext();

            IQueryable<GuildActivity> guildActivities = context.GuildActivities.Where(ga => ga.Guild.Id == (long)Context.Guild.Id);
            if (!guildActivities.Any())
            {
                await FollowupAsync($"No activities registered for this guild.", ephemeral: true);
                return;
            }
            context.GuildActivities.RemoveRange(guildActivities);
            context.SaveChanges();

            var pendingActions = _presenceHandler.PendingActions.Where(ae => ae.GuildId == (long)Context.Guild.Id && guildActivities.Any(ga => ga.ActivityId == ae.ActivityId));
            foreach (var action in pendingActions)
            {
                _presenceHandler.PendingActions.Remove(action);
                action.Dispose();
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

        [SlashCommand("list", "List all enqued actions for this guild.")]
        public async Task GetActions()
        {
            using var context = _contextFactory.CreateDbContext();

            (ActionEntry entry, GuildActivity activity)[] guildEntries = _presenceHandler.PendingActions
                .Where(ae => ae.GuildId == (long)Context.Guild.Id)
                .Select(ae => (ae, context.GuildActivities
                    .FirstOrDefault(ga => 
                        ga.GuildId == (long)Context.Guild.Id && 
                            ga.ActivityId == ae.ActivityId)))
                .ToArray();

            if (guildEntries.Length == 0)
            {
                await RespondAsync("No actions enqued.", ephemeral: true);
                return;
            }

            uint embedIndex = 0;
            uint chunkedEmbedIndex = 0;
            Embed[] embeds = new Embed[(25+guildEntries.Length)/25];
            foreach (var chunkedEntries in guildEntries.Chunk(25))
            {
                EmbedBuilder embedBuilder = new();
                if (chunkedEmbedIndex++ == 0)
                    embedBuilder.WithTitle($"Enqued actions:");
                foreach (var entry in chunkedEntries)
                {
                    embedBuilder.AddField($"Activity-Id: {entry.activity.IdWithinGuild}", 
                        $"**ETA**: {entry.entry.ETA:H:mm:ss dd.MM.yyyy}\n" +
                        $"**Remaining**: -{entry.entry.ETA - DateTime.Now:d\\d\\ hh\\h\\ mm\\m\\ ss\\s}\n" +
                        $"**Action**: {entry.activity.Action}\n" +
                        $"**Target**: {Context.Client.GetUser((ulong)entry.entry.UserId).Mention}");
                }
                embeds[embedIndex++] = embedBuilder.Build();
            }
            await RespondAsync(embeds: embeds, ephemeral: true);
        }
    }
}
