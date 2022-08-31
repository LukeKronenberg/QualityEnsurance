using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QualityEnsurance.Extensions
{
    public static class MarkdownFormat
    {
        public static string SanitizeCodeContent(string s) => Regex.Replace(s, "`", @"\`");
    }
}
