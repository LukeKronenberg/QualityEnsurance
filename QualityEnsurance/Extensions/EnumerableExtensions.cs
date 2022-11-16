using QualityEnsurance;
using QualityEnsurance.Models;
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

        /// <summary>
        /// Converts a Enumerable into multiple chunks based on an converter function to calculate a size for each element.
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <param name="source"></param>
        /// <param name="elementToSize"></param>
        /// <param name="maxChunkSize"></param>
        /// <param name="maxChunkLength"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Throws if an elements size is larger than <paramref name="maxChunkSize"/></exception>
        public static IEnumerable<TElement[]> Chunk<TElement>(this IEnumerable<TElement> source, Func<TElement, double> elementToSize, double maxChunkSize, int maxChunkLength = -1)
        {
            List<TElement> lastElements = new();
            double lastSizesSum = 0;
            foreach (var element in source)
            {
                double len = elementToSize(element);
                if (len > maxChunkSize)
                    throw new ArgumentException("Size of element is greater than maxSize");

                if (lastSizesSum + len > maxChunkSize || lastElements.Count == maxChunkLength)
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
