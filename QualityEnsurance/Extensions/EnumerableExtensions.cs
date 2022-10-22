using QualityEnsurance.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace QualityEnsurance.Extensions
{
    public static class EnumerableExtensions
    {
        public static IOrderedEnumerable<IActivity> Order(this IEnumerable<IActivity> activities) =>
            activities.OrderBy(a => a switch
            {
                CustomStatusGame customStatusGame => customStatusGame.State,
                SpotifyGame spotifyGame => spotifyGame.TrackTitle,
                RichGame => a.Name,
                Game when a.GetType().Name == "Game" => a.Name,
                _ => throw new NotSupportedException($"\"{a.GetType().Name}\" is not supported.")
            });
        public static IOrderedEnumerable<Activity> OrderByName(this IEnumerable<Activity> guildActivities) =>
            guildActivities.OrderBy(a => a.ApplicationId?.ToString() ?? a.SpotifyId ?? a.Name);
        public static IOrderedEnumerable<GuildActivity> OrderByName(this IEnumerable<GuildActivity> guildActivities) =>
            guildActivities.OrderBy(a => a.Activity.ApplicationId?.ToString() ?? a.Activity.SpotifyId ?? a.Activity.Name );

        public static IEnumerable<T[]> Chunk<T>(this IEnumerable<T> source, Func<T, int> sizeConverter, int maxSize)
        {
            List<T> lastElements = new();
            int lastSizesSum = 0;
            foreach (var element in source)
            {
                int len = sizeConverter(element);
                if (len > maxSize)
                    throw new ArgumentException("Size of element is greater than maxSize");

                if (lastSizesSum + len > maxSize)
                {
                    yield return lastElements.ToArray();
                    lastElements.Clear();
                    lastSizesSum = 0;
                }
                lastSizesSum += len;
                lastElements.Add(element);
            }

            yield return lastElements.ToArray();
        }
    }
}
