using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QualityEnsurance.Models
{
    public abstract class DbModel
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }
    }
}
