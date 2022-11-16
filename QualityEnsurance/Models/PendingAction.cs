using Discord;
using System.ComponentModel.DataAnnotations.Schema;
namespace QualityEnsurance.Models
{
    [Table("pending_actions")]
    public class PendingAction
    {

        [Column("guild_activity_user_guild_id")]
        public long GuildId { get; set; } 
        public virtual Guild Guild { get; set; }
        
        [Column("guild_activity_user_activity_id")]
        public long ActivityId { get; set; }
        public virtual Activity Activity { get; set; }

        public virtual GuildActivity GuildActivity { get; set; }


        [Column("guild_activity_user_user_id")]
        public long UserId { get; set; }
        public virtual User User { get; set; }

        public virtual GuildActivityUser GuildActivityUser { get; set; }

        /// <summary>
        /// The time the action was enqueued. Can be used with <see cref="RichGame.Timestamps"/> to determin if the action is still valid.
        /// </summary>
        [Column("start")]
        public DateTimeOffset Start { get; set; }

        [Column("eta")]
        public DateTimeOffset ETA { get; set; }
    }
}
