using Discord;
using Discord.WebSocket;
using QualityEnsurance.Constants;
using QualityEnsurance.Models;
using QualityEnsurance.Extensions;
using Microsoft.EntityFrameworkCore;

namespace QualityEnsurance.DiscordEventHandlers
{
    public class PresenceHandler
    {
        private readonly DiscordSocketClient _discord;
        private readonly IDbContextFactory<QualityEnsuranceContext> _contextFactory;

        public readonly List<ActionEntry> PendingActions = new();

        public PresenceHandler(DiscordSocketClient discord, IDbContextFactory<QualityEnsuranceContext> contextFactory)
        {
            _discord = discord;
            _discord.PresenceUpdated += PresenceUpdated;
            _contextFactory = contextFactory;
        }

        private async Task PresenceUpdated(SocketUser socketUser, SocketPresence oldPresence, SocketPresence newPresence)
        {
            if (socketUser is not SocketGuildUser user) // We get notified for every individualy (If user1 is in guild1 and guild2 we get to seperate notifications for each guild) 
                return;
            var oldActivities = oldPresence.Activities?.ToArray() ?? Array.Empty<IActivity>();
            var newActivities = newPresence.Activities?.ToArray() ?? Array.Empty<IActivity>();

            if (Equals(oldActivities, newActivities))
                return;

            using var context = _contextFactory.CreateDbContext();

            long guildId = (long)user.Guild.Id;
            Guild guild = context.GetGuild(guildId);

            // Cache userentrys before any could be added
            var userEntrys = PendingActions.Where(ae => ae.UserId == (long)user.Id && ae.GuildId == guildId).ToArray();

            // Iterate over newly started activities and start Action task if required
            IEnumerable<IActivity> startedActivites = newActivities.Where(a => !oldActivities.Any(a2 => a2.CustomEquals(a)));                        
            foreach (var startedActivity in startedActivites)
            {
                GuildActivity guildActivity = null;

                switch (startedActivity)
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
                    case Game when startedActivity.GetType().Name == nameof(Game):
                        guildActivity = guild.GuildActivities.FirstOrDefault(ga => ga.Activity.Name != null && ga.Activity.Name == startedActivity.Name.ToLower());
                        break;
                } 

                if (guildActivity == null || guildActivity.Action == BotActionType.None || userEntrys.Any(ue => ue.ActivityId == guildActivity.ActivityId))
                    continue;

                GuildActivityUser gau = context.GetGuildActivityUser(guildActivity, context.GetUser((long)user.Id));
                if (gau.Blacklisted || (guildActivity.RequireWhitelist && !gau.Whitelisted))
                        continue;

                DateTime timeOfAction = DateTime.Now.Add(TimeSpan.FromSeconds(guildActivity.CountdownDurationS));

                if (guildActivity.StartMessage != null)
                    await user.SendMessageAsync(guildActivity.StartMessage);

                ActionEntry actionEntry = new()
                {
                    UserId = (long)user.Id,
                    GuildId = guild.Id,
                    ActivityId = guildActivity.Activity.Id,
                    CountdownDurationS = guildActivity.CountdownDurationS,
                    ETA = timeOfAction,
                    ActionTask = new()
                };

                PendingActions.Add(actionEntry);
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

                    ActionEntry entry = userEntrys.FirstOrDefault(ae => ae.ActivityId == guildActivity.Activity.Id);

                    if (entry == null)
                        continue;

                    PendingActions.Remove(entry);
                    entry.Dispose();
                }            
            }
        }

        private async void DoActionTask(ActionEntry entry)
        {
            TimeSpan actionDelay = entry.ETA - DateTime.Now;
            if (actionDelay < TimeSpan.Zero)
                actionDelay = TimeSpan.Zero;
            try
            {
                await Task.Delay(actionDelay, entry.ActionTask.Token);
            } 
            catch (TaskCanceledException) { return; /* canceled */ }
            catch (Exception ex) { Console.WriteLine(ex); return; }

            using var context = _contextFactory.CreateDbContext();

            GuildActivity guildActivity = context.GetGuild(entry.GuildId).GuildActivities.SingleOrDefault(ga => ga.Activity.Id == entry.ActivityId);
            if (guildActivity == null)
                return; // Activity deleted in mean time

            if (guildActivity.CountdownDurationS > entry.CountdownDurationS)
            {
                entry.ETA = DateTime.Now + TimeSpan.FromSeconds(guildActivity.CountdownDurationS - entry.CountdownDurationS);
                entry.CountdownDurationS = guildActivity.CountdownDurationS;
                DoActionTask(entry);
                return;
            }

            SocketGuild guild = _discord.GetGuild((ulong)guildActivity.Guild.Id);
            if (guild == null)
                return; // Bot removed from guild or guild deleted
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
            }

            if (guildActivity.ActionMessage != null)
                try
                {
                    await user.SendMessageAsync(guildActivity.ActionMessage);
                }
                catch { /* Ignore forbidden */ }

            PendingActions.Remove(entry);
            entry.Dispose();
        }
    }
}
