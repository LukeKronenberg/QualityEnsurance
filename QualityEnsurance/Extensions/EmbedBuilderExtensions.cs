using Discord;
using System.Text;

namespace QualityEnsurance.Extensions
{
    public static class EmbedBuilderExtensions
    {

        public static void AddOption(this EmbedBuilder embedBuilder, string title, bool oldValue, bool? newValue = null, bool inline = true)
        {
            StringBuilder builder = new();
            builder.Append(oldValue ? "`Enabled`" : "`Disabled`");
            if (newValue.HasValue)
                builder.Append($" => {(newValue.Value ? "`Enabled`" : "`Disabled`")}");
            embedBuilder.AddField(title, builder.ToString(), inline);
        }

        public static void AddOption(this EmbedBuilder embedBuilder, string title, object oldValue, object newValue = null, bool inline = true, bool doCodeFormatting = true)
        {
            StringBuilder builder = new();

            if (doCodeFormatting)
                builder.Append($"`{oldValue.ToString().SanitizeCode()}`");
            else
                builder.Append(oldValue);

            if (newValue != null)
                if (doCodeFormatting)
                    builder.Append($" => `{newValue.ToString().SanitizeCode()}`");
                else
                    builder.Append($" => {newValue}");
            
            embedBuilder.AddField(title, builder.ToString(), inline);
        }
    }

    public static class EmbedFieldBuilderExtensions
    {
        public static void AddOption(this EmbedFieldBuilder embedFieldBuilder, string title, bool oldValue, bool? newValue = null, bool breakTitle = false)
        {
            StringBuilder builder = new();
            builder.Append(embedFieldBuilder.Value);
            builder.AppendLine();
            builder.Append(title);
            if (breakTitle)
                builder.AppendLine();
            builder.Append(oldValue ? "`Enabled`" : "`Disabled`");
            if (newValue.HasValue)
                builder.Append($" => {(newValue.Value ? "`Enabled`" : "`Disabled`")}");
            embedFieldBuilder.Value = builder.ToString();
        }

        public static void AddOption(this EmbedFieldBuilder embedFieldBuilder, string title, object oldValue, object newValue = null, bool doCodeFormatting = true, bool breakTitle = true)
        {
            StringBuilder builder = new();
            builder.Append(embedFieldBuilder.Value);
            builder.AppendLine();
            builder.Append(title + " ");
            if (breakTitle)
                builder.AppendLine();

            if (doCodeFormatting)
                builder.Append($"`{oldValue.ToString().SanitizeCode()}`");
            else
                builder.Append(oldValue);

            if (newValue != null)
                if (doCodeFormatting)
                    builder.Append($" => `{newValue.ToString().SanitizeCode()}`");
                else
                    builder.Append($" => {newValue}");
            
            embedFieldBuilder.Value = builder.ToString();
        }
    }

    public class OptionBuilder
    {
        private readonly StringBuilder _builder = new();

        public void AddOption(string title, bool oldValue, bool? newValue = null, bool breakTitle = false)
        {
            _builder.Append(title);
            if (breakTitle)
                _builder.AppendLine();
            _builder.Append(oldValue ? "`Enabled`" : "`Disabled`");
            if (newValue.HasValue)
                _builder.Append($" => {(newValue.Value ? "`Enabled`" : "`Disabled`")}");
            _builder.AppendLine();
        }

        public void AddOption(string title, object oldValue, object newValue = null, bool doCodeFormatting = true, bool breakTitle = true)
        {
            _builder.Append(title + " ");
            if (breakTitle)
                _builder.AppendLine();

            if (doCodeFormatting)
                _builder.Append($"`{oldValue.ToString().SanitizeCode()}`");
            else
                _builder.Append(oldValue);

            if (newValue != null)
                if (doCodeFormatting)
                    _builder.Append($" => `{newValue.ToString().SanitizeCode()}`");
                else
                    _builder.Append($" => {newValue}");

            _builder.AppendLine();
        }

        public override string ToString() => _builder.ToString();
    }
}
