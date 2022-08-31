using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QualityEnsurance.Models
{
    [Table("user")]
    public class User : DbModel
    {
        public virtual List<GuildActivityUser> GuildActivityUserSettings { get; set; } = new(0);
    }
}