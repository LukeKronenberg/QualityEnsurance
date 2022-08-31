using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QualityEnsurance.Models
{
    [Table("guild")]
    public class Guild : DbModel
    {
        [Column("max_activities")]
        public int MaxActivities { get; set; } = 100;
        [Column("guild_activity_next_id"), ConcurrencyCheck]
        public uint GuildActivityNextId { get; set; } = 1;

        public virtual List<GuildActivity> GuildActivities { get; set; } = new(0);
        public virtual List<GuildActivityUser> GuildActivityUserSettings { get; set; } = new(0);
    }
}