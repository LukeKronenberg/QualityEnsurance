using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QualityEnsurance.Models
{
    [Table("guild_activity_user")]
    public class GuildActivityUser
    {
        [Column("guild_activity_guild_id")]
        public long GuildId { get; set; }
        public virtual Guild Guild { get; set; }

        [Column("guild_activity_activity_id")]
        public long ActivityId { get; set; }
        public virtual Activity Activity { get; set; }

        public virtual GuildActivity GuildActivity { get; set; }
        public virtual PendingAction PendingActions { get; set; }

        [Column("user_id")]
        public long UserId { get; set; }
        public virtual User User { get; set; }

        [Column("whitelisted")]
        public bool Whitelisted { get; set; }
        [Column("blacklisted")]
        public bool Blacklisted { get; set; } = false;
    }
}
