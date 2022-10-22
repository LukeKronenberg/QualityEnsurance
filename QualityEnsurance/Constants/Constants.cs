using System.Text.RegularExpressions;

namespace QualityEnsurance.Constants
{
    public static class Constants
    {
        public static Regex LinkParser = new(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
        public static string[] VideoExtensions = new[] { 
            "mp4",
            "wmv",
            "avi",
            "mkv",
            "flv",
            "mov",
            "mpg",
            "mpeg",
            "m4v",
            "asf",
            "f4v",
            "webm",
            "divx",
            "m2t",
            "m2ts",
            "vob",
            "ts"
        };

        public static readonly string[] ImageExtensions = {
            "jpg",
            "jpeg",
            "png",
            "gif",
            "bmp",
            "tiff"
        };
    }
}
