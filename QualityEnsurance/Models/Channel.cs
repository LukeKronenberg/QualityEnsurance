using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace QualityEnsurance.Models
{
    [Table("channel")]
    public class Channel : DbModel
    {
        [Column("upload_links")]
        public bool UploadLinks { get; set; } = false;
    }
}
