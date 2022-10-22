using Discord;

namespace QualityEnsurance.Extensions
{
    public static class IActivityExtensions
    {
        public static bool CustomEquals(this IActivity activity1, IActivity activity2)
        {
            if (activity1 == null)
                throw new ArgumentNullException(nameof(activity1));
            if (activity2 == null)
                throw new ArgumentNullException(nameof(activity2));

            if (activity1 == activity2)
                return true;
            if (activity1.GetType() != activity2.GetType())
                return false;

            return activity1 switch
            {
                RichGame richGame1 => richGame1.ApplicationId == (activity2 as RichGame).ApplicationId,
                SpotifyGame spotifyGame1 => spotifyGame1.TrackId == (activity2 as SpotifyGame).TrackId,
                CustomStatusGame customStatusGame1 => customStatusGame1.State == (activity2 as CustomStatusGame).State,
                _ => activity1.Name == activity2.Name,
            };
        }
    }
}
