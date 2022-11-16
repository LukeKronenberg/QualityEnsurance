using Discord;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QualityEnsurance.Models
{
    [Table("activity")]
    public class Activity : DbModel
    {
        [Column("name")]
        public string Name { get; set; }

        #region RichGame
        [Column("application_id")]
        public long? ApplicationId { get; set; }
        #endregion

        #region SpotifyGame
        [Column("spotify_id")]
        public string SpotifyId { get; set; }
        [Column("album_title")]
        public string AlbumTitle { get; set; }
        [Column("track_title")]
        public string TrackTitle { get; set; }
        #endregion

        #region CustomStatusGame
        [Column("state")]
        public string State { get; set; }
        #endregion

        public virtual List<GuildActivity> GuildActivities { get; set; } = new(0);
        public virtual List<GuildActivityUser> GuildActivityUserSettings { get; set; } = new(0);


        public Activity() { }
        public Activity(IActivity activity)
        {
            unchecked
            {
                switch (activity)
                {
                    case RichGame richGame:
                        Name = richGame.Name;
                        ApplicationId = (long)richGame.ApplicationId;
                        break;
                    case SpotifyGame spotifyGame:
                        Name = spotifyGame.Name;
                        SpotifyId = spotifyGame.TrackId;
                        AlbumTitle = spotifyGame.AlbumTitle;
                        TrackTitle = spotifyGame.TrackTitle;
                        break;
                    case Game game: // Mach "Game" last because other types are inhereting
                        Name = game.Name;
                        break;
                    default:
                        throw new NotSupportedException($"ActivityType \"{activity.GetType().FullName}\" not supported.");
                }
            }
        }

        public FilterType GetFilterType()
        {
            if (ApplicationId.HasValue)
            {
                if (Name != null)
                    return FilterType.ApplicationAndName;
                return FilterType.OnlyApplication;
            }
            if (SpotifyId != null)
                return FilterType.OnlySpotify;
            if (State != null)
                return FilterType.CustomStatus;
            return FilterType.OnlyName;
        }

        public override bool Equals(object obj)
        {
            return obj is Activity a && Equals(a);
        }

        public bool Equals(Activity activity)
        {
            if (activity == null)
                return false;
            return 
                activity.Name == Name && 
                activity.ApplicationId == ApplicationId && 
                activity.State == State &&
                activity.SpotifyId == SpotifyId &&
                activity.AlbumTitle == AlbumTitle &&
                activity.TrackTitle == TrackTitle;
        }

        public enum FilterType
        {
            CustomStatus,
            OnlyName,
            OnlyApplication,
            ApplicationAndName,
            OnlySpotify,
            SpotifyOrName,
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
