using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace QualityEnsurance.Models
{
    [Table("channel")]
    public class Channel : DbModel
    {
        [Column("upload_link")]
        public bool UploadLink { get; set; } = false;
    }
}
