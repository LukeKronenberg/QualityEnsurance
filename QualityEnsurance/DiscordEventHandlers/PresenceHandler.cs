using Discord;
using Discord.WebSocket;
using QualityEnsurance.Constants;
using QualityEnsurance.Models;
using QualityEnsurance.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace QualityEnsurance.DiscordEventHandlers
{
    public class PresenceHandler
    {
        private readonly DiscordSocketClient _discord;
        private readonly IDbContextFactory<QualityEnsuranceContext> _contextFactory;

        /// <summary>
        /// A collection to keep track of all countdown tasks from users which triggered an activity
        /// </summary>
        public readonly ConcurrentDictionary<(long guildId, long userId, long activityId), ActionEntry> Actions = new();

        public PresenceHandler(DiscordSocketClient discord, IDbContextFactory<QualityEnsuranceContext> contextFactory)
        {
            _discord = discord;
            _contextFactory = contextFactory;
        }

        public async Task InitializeAsync()
        {
            _discord.PresenceUpdated += PresenceUpdated;
            using var context = _contextFactory.CreateDbContext();

            var pendingActions = await context.PendingActions.ToArrayAsync();

            foreach (var qa in pendingActions)
            {
                var entry = new ActionEntry
                {
                    GuildId = qa.GuildId,
                    UserId = qa.UserId,
                    ActivityId = qa.ActivityId,
                    Start = qa.Start,
                    ETA = qa.ETA,
                    CancellationReference = new CancellationTokenSource()
                };
                DoActionTask(entry);
                Actions.Add(entry);
            }
        }

        private async Task PresenceUpdated(SocketUser socketUser, SocketPresence oldPresence, SocketPresence newPresence)
        {
            if (socketUser is not SocketGuildUser discordUser) // We get notified for every guild individualy (If user1 is in guild1 and guild2 we get to seperate notifications for each guild) 
                return;
            var oldActivities = oldPresence.Activities?.ToArray() ?? Array.Empty<IActivity>();
            var newActivities = newPresence.Activities?.ToArray() ?? Array.Empty<IActivity>();

            if (Equals(oldActivities, newActivities))
                return;

            using var context = _contextFactory.CreateDbContext();

            long guildId = (long)discordUser.Guild.Id;
            Guild guild = context.Guilds
                .Include(g => g.GuildActivities)
                .FirstOrDefault(g => g.Id == guildId);

            if (guild == null)
                return;

            // Save original state
            var userEntrys = Actions.Where(ae => ae.Value.UserId == (long)discordUser.Id && ae.Value.GuildId == guildId).ToArray();

            // Iterate over newly started activities and start Action task if required
            IEnumerable<IActivity> startedActivites = newActivities.Where(a => !oldActivities.Any(a2 => a2.CustomEquals(a)));      
            foreach (var startedActivity in startedActivites)
            {
                GuildActivity guildActivity = null;
                DateTimeOffset? start = null;

                // Try to find an registered activity
                switch (startedActivity)
                {
                    case RichGame richGame:
                        guildActivity = guild.GuildActivities.FirstOrDefault(ga =>
                            (ga.Activity.ApplicationId != null && ga.Activity.ApplicationId == (long)richGame.ApplicationId) || 
                            (ga.Activity.Name != null && ga.Activity.Name == richGame.Name.ToLower())
                        );
                        start = richGame.Timestamps?.Start;
                        break;
                    case SpotifyGame spotifyGame:
                        guildActivity = guild.GuildActivities.FirstOrDefault(ga => 
                            ga.Activity.SpotifyId != null && ga.Activity.SpotifyId == spotifyGame.TrackId
                        );
                        start = spotifyGame.StartedAt;
                        break;
                    case CustomStatusGame customStatusGame:
                        guildActivity = guild.GuildActivities.FirstOrDefault(ga => 
                            ga.Activity.Name == "custom status" && ga.Activity.State == customStatusGame.State?.ToLower()
                        );
                        break;
                    case Game game:
                        guildActivity = guild.GuildActivities.FirstOrDefault(ga => 
                            ga.Activity.Name != null && ga.Activity.Name == game.Name.ToLower()
                        );
                        break;
                } 

                if (guildActivity == null || 
                    guildActivity.Action == BotActionType.None || 
                    userEntrys.Any(ue => ue.Value.ActivityId == guildActivity.ActivityId))
                    continue;

                GuildActivityUser gau = context.GetGuildActivityUser(guildActivity, context.Get<User>((long)discordUser.Id));
                if (gau.Blacklisted || (guildActivity.RequireWhitelist && !gau.Whitelisted))
                        continue;

                DateTimeOffset eta = DateTimeOffset.Now.Add(TimeSpan.FromSeconds(guildActivity.CountdownDurationS));

                if (guildActivity.StartMessage != null)
                    try
                    {
                        await discordUser.SendMessageAsync(guildActivity.StartMessage);
                    }
                    catch { /* Catch not allowed */ }

                ActionEntry actionEntry = new()
                {
                    UserId = (long)discordUser.Id,
                    GuildId = guild.Id,
                    ActivityId = guildActivity.Activity.Id,
                    Start = start ?? DateTimeOffset.UtcNow,
                    ETA = eta,
                    CancellationReference = new()
                };

                PendingAction pendingAction = new()
                {
                    Guild = guild,
                    User = gau.User,
                    Activity = guildActivity.Activity,
                    Start = actionEntry.Start,
                    ETA = actionEntry.ETA
                };

                context.PendingActions.Add(pendingAction);

                try
                {
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    await Program.LogAsync(ex);
                    continue;
                }
                Actions.Add(actionEntry);
                DoActionTask(actionEntry);
            }

            // Iterate over just stopped activities and cancel action tasks if required
            if (userEntrys.Any())
            {
                IEnumerable<IActivity> stoppedActivites = oldActivities.Where(a => !newActivities.Any(a2 => a2.CustomEquals(a)));
                foreach (var stoppedActivity in stoppedActivites)
                {
                    GuildActivity guildActivity = null;

                    switch (stoppedActivity)
                    {
                        case RichGame richGame:
                            guildActivity = guild.GuildActivities.FirstOrDefault((ga) =>
                            {
                                return (ga.Activity.ApplicationId != null || ga.Activity.Name != null) &&
                                    (ga.Activity.ApplicationId == null || ga.Activity.ApplicationId == (long)richGame.ApplicationId) &&
                                    (ga.Activity.Name == null || ga.Activity.Name == richGame.Name.ToLower()) &&
                                    (ga.Activity.State == null || ga.Activity.State == richGame.State?.ToLower());

                            });
                            break;
                        case SpotifyGame spotifyGame:
                            guildActivity = guild.GuildActivities.FirstOrDefault(ga => ga.Activity.SpotifyId != null && ga.Activity.SpotifyId == spotifyGame.TrackId);
                            break;
                        case CustomStatusGame customStatusGame:
                            guildActivity = guild.GuildActivities.FirstOrDefault(ga => ga.Activity.Name == "custom status" && ga.Activity.State == customStatusGame.State?.ToLower());
                            break;
                        case Game when stoppedActivity.GetType().Name == nameof(Game):
                            guildActivity = guild.GuildActivities.FirstOrDefault(ga => ga.Activity.Name != null && ga.Activity.Name == stoppedActivity.Name.ToLower());
                            break;
                    }

                    if (guildActivity == null)
                        continue;

                    ActionEntry entry = userEntrys.FirstOrDefault(ae => ae.Value.ActivityId == guildActivity.Activity.Id).Value;
                    if (entry == null)
                        continue;

                    PendingAction pendingAction = context.PendingActions.FirstOrDefault(entry);
                    if (pendingAction != null)
                        context.Remove(pendingAction);

                    Actions.Remove(entry);
                    entry.Dispose();
                    try
                    {
                        await context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        await Program.LogAsync(ex);
                        continue;
                    }
                }
            }
        }

        private async void DoActionTask(ActionEntry entry)
        {
            TimeSpan actionDelay = entry.ETA - DateTimeOffset.Now;
            if (actionDelay > TimeSpan.Zero)
            {
                try
                {
                    await Task.Delay(actionDelay, entry.CancellationReference.Token);
                } 
                catch (TaskCanceledException) { return; /* canceled */ }
                catch (Exception ex) { Console.WriteLine(ex); return; }
            }
            else if (actionDelay < TimeSpan.FromSeconds(-10))
            {
                // Action is too late, can happen when bot is shut down for a while
                return;
            }

            using var context = _contextFactory.CreateDbContext();

            PendingAction pendingAction = context.PendingActions.FirstOrDefault(entry);
            if (pendingAction == null)
            {
                // Race condition check
                Actions.Remove(entry);
                entry.Dispose();
                return;
            }

            GuildActivity guildActivity = context.GuildActivities
                .Include(ga => ga.Guild)
                .SingleOrDefault(ga => ga.ActivityId == entry.ActivityId && ga.GuildId == entry.GuildId);
            
            if (guildActivity == null)
                return; // Activity deleted while waiting

            int entryCountdownDurationS = (entry.ETA - entry.Start).Seconds;
            if (guildActivity.CountdownDurationS > entryCountdownDurationS)
            {
                entry.ETA = DateTime.Now + TimeSpan.FromSeconds(guildActivity.CountdownDurationS - entryCountdownDurationS);
                entry.Start = entry.ETA - TimeSpan.FromSeconds(guildActivity.CountdownDurationS); // Recalcualte start value to remove inaccuracys

                pendingAction.ETA = entry.ETA;
                pendingAction.Start = entry.Start;

                await context.SaveChangesAsync();

                DoActionTask(entry);
                return;
            }

            SocketGuild guild = _discord.GetGuild((ulong)guildActivity.Guild.Id);
            if (guild == null)
                return; // Bot removed from guild or guild deleted
            await guild.DownloadUsersAsync();
            SocketGuildUser user = guild.GetUser((ulong)entry.UserId);
            if (user == null)
                return; // User no longer in guild

            switch (guildActivity.Action)
            {
                case BotActionType.Ban:
                    try
                    {
                        await user.BanAsync();
                    }
                    catch { /* Ignore forbidden */ }
                    break;
                case BotActionType.Timeout:
                    try
                    {
                        await user.SetTimeOutAsync(TimeSpan.FromSeconds(guildActivity.TimeoutDurationS));
                    }
                    catch { /* Ignore forbidden */ }
                    break;
                case BotActionType.OnlyMessage:
                    if (guildActivity.ActionMessage != null)
                        try
                        {
                            await user.SendMessageAsync(guildActivity.ActionMessage);
                        }
                        catch { /* Ignore forbidden */ }
                    break;
                case BotActionType.None:
                    break;
            }

            context.PendingActions.Remove(pendingAction);
            Actions.Remove(entry);
            entry.Dispose();

            await context.SaveChangesAsync();
        }
    }
    
    static class PrescenceHandlerExtensions
    {
        public static void Add(this ConcurrentDictionary<(long, long, long), ActionEntry> dict, ActionEntry entry)
        {
            dict[(entry.GuildId, entry.UserId, entry.ActivityId)] = entry;
        }

        public static bool Remove(this ConcurrentDictionary<(long, long, long), ActionEntry> dict, ActionEntry entry)
        {
            return dict.TryRemove((entry.GuildId, entry.UserId, entry.ActivityId), out _);
        }

        public static PendingAction FirstOrDefault(this DbSet<PendingAction> pendingAction, ActionEntry entry)
        {
            return pendingAction.FirstOrDefault(qa => qa.GuildId == entry.GuildId && qa.UserId == entry.UserId && qa.ActivityId == entry.ActivityId);
        }
    }
}
