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


        public void AddToEmbed(EmbedBuilder embed)
        {
            embed.AddField("Id:", $"`{IdWithinGuild}`", true);
            if (Activity.Name != null)
                embed.AddField("Name:", $"`{Activity.Name.SanitizeCode()}`", true);
            if (Activity.State != null)
                embed.AddField("State:", $"`{Activity.State.SanitizeCode()}`", true);
            if (Activity.ApplicationId.HasValue)
                embed.AddField("App-Id:", $"`{Activity.ApplicationId}`", true);
            if (Activity.SpotifyId != null)
                embed.AddField("Spotify-Id:", $"`{Activity.SpotifyId.SanitizeCode()}`", true);
            embed.AddField("Action:", $"`{Action}`", true);
            embed.AddField("Countdown-Duration:", $"{CountdownDurationS}s", true);
            if (Action == BotActionType.Timeout)
                embed.AddField("Timeout duration:", $"{TimeoutDurationS}s", true);
            embed.AddField("Start msg.:", $"{StartMessage ?? "No message"}");
            embed.AddField("Action msg.:", $"{ActionMessage ?? "No message"}");
        }
    }
}
