using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QualityEnsurance.Models
{
    [Table("youtube_user")]
    public class YoutubeUser
    {
        [Key]
        [Column("identifier")]
        public string Identifier { get; set; }

        [Column("user_id")]
        public string UserId { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("access_token")]
        public string AccessToken { get; set; }
        
        [Column("refresh_token")]
        public string RefreshToken { get; set; }
        
        [Column("expires_in_seconds")]
        public long ExpiresInSeconds { get; set; }

        [Column("issued")]
        public DateTime IssuedUtc { get; set; }
    }
}
