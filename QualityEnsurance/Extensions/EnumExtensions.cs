using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace QualityEnsurance.Extensions
{
    public static class ActivityTypeExtensions
    {
        public static bool IsGame(this IActivity activity, out RichGame richGame)
        {
            richGame = activity as RichGame;
            return activity.Type == ActivityType.Playing || activity.Type == ActivityType.Competing;
        }

        public static bool IsSong(this IActivity activity, out SpotifyGame spotifyGame)
        {
            spotifyGame = activity as SpotifyGame;
            return activity.Type == ActivityType.Listening;
        }
        public static bool IsStreaming(this IActivity activity, out StreamingGame streamingGame)
        {
            streamingGame = activity as StreamingGame;
            return activity.Type == ActivityType.Streaming;
        }
    }
}
