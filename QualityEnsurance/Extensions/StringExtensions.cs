using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;

namespace QualityEnsurance.Extensions
{
    public static class StringExtensions
    {
        public static string Sanitize(this string s) => Format.Sanitize(s);

        /// <summary>
        /// Sanitizes any <code>`</code> character so that a string can't break out of markdown code formating.
        /// </summary>
        /// <param name="s"></param>
        /// <returns>The sanitzed string</returns>
        public static string SanitizeCode(this string s) => MarkdownFormat.SanitizeCodeContent(s);
    }
}
