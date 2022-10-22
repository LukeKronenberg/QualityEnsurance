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
        
        [Column("channel_url")]
        public string ChannelUrl { get; set; }

        [Column("access_token")]
        public string AccessToken { get; set; }
        
        [Column("refresh_token")]
        public string RefreshToken { get; set; }
        
        [Column("expires_in_seconds")]
        public long ExpiresInSeconds { get; set; }

        [Column("issued")]
        public DateTime Issued { get; set; }
    }
}
