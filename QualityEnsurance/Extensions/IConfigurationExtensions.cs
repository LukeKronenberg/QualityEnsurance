using Microsoft.Extensions.Configuration;

namespace QualityEnsurance.Extensions
{
    public static class IConfigurationExtensions
    {
        public static TElement[] GetArray<TElement>(this IConfiguration config, string key)
        {
            return config.GetSection(key).Get<TElement[]>();
        }
    }
}
