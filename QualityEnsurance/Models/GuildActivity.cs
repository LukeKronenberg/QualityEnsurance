using Discord;
using QualityEnsurance.Constants;
using QualityEnsurance.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QualityEnsurance.Models
{
    [Table("guild_activity")]
    public class GuildActivity
    {
        [Column("guild_id")]
        public long GuildId { get; set; }
        public virtual Guild Guild { get; set; }

        [Column("activity_id")]
        public long ActivityId { get; set; }
        public virtual Activity Activity { get; set; }

        [Column("require_whitelist")]
        public bool RequireWhitelist { get; set; }

        /// <summary>
        /// An unique id within GuildActivities from the same guild
        /// </summary>
        [Column("id_within_guild")]
        public uint IdWithinGuild { get; set; }

        /// <summary>
        /// The Id of the most recent user who registered/changed this activity
        /// </summary>
        [Column("user_id")]
        public long UserId { get; set; }
        /// <summary>
        /// The most recent user who registered/changed this activity
        /// </summary>
        public virtual User User { get; set; }

        // Do Actions?
        [Column("action")]
        public BotActionType Action { get; set; } = BotActionType.Timeout;
        [Column("countdown_duration")]
        public int CountdownDurationS { get; set; } = 1800;
        [Column("start_message")]
        public string StartMessage { get; set; } = string.Empty;
        [Column("action_message")]
        public string ActionMessage { get; set; } = string.Empty;

        // Action Configurations
        [Column("timeout_duration")]
        public int TimeoutDurationS { get; set; } = 1800;

        public virtual List<GuildActivityUser> GuildActivityUserSettings { get; set; } = new(0);
        public virtual List<PendingAction> PendingActions { get; set; } = new(0);


        public void AddToEmbed(EmbedBuilder embed, GuildActivity oldValues = null)
        {
            embed.AddOption("Id:", IdWithinGuild, oldValues?.IdWithinGuild, doCodeFormatting: false);
            if (Activity.Name != null)
                embed.AddOption("Name:", Activity.Name);
            if (Activity.State != null)
                embed.AddOption("State:", Activity.State);
            if (Activity.ApplicationId.HasValue)
                embed.AddOption("App-Idd:", Activity.ApplicationId.Value);
            if (Activity.SpotifyId != null)
                embed.AddOption("Spotify-Id:", Activity.SpotifyId);
            embed.AddOption("Action:", Action, oldValues?.Action);
            embed.AddOption("Countdown-Duration:", $"{CountdownDurationS}s", oldValues != null? $"{oldValues.CountdownDurationS}s" : null, doCodeFormatting: false);
            if (Action == BotActionType.Timeout)
                embed.AddOption("Timeout-Duration:", $"{TimeoutDurationS}s", oldValues != null ? $"{oldValues.TimeoutDurationS}s" : null, doCodeFormatting: false);
            embed.AddOption("Start msg.:", StartMessage ?? "No message", oldValues != null ? (oldValues.StartMessage ?? "No message") : null, doCodeFormatting: false);
            embed.AddOption("Action msg.:", ActionMessage ?? "No message", oldValues != null ? (oldValues.ActionMessage ?? "No message") : null, doCodeFormatting: false);
            embed.AddOption("Require Whitelist:", RequireWhitelist);
        }

        public GuildActivity ShallowCopy()
        {
            return (GuildActivity) MemberwiseClone();
        }
    }
}