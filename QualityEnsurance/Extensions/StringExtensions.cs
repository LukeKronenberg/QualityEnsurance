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

        public static string SanitizeCode(this string s) => MarkdownFormat.SanitizeCodeContent(s);
    }
}
