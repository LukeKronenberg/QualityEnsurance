using QualityEnsurance.Extensions;

namespace QualityEnsurance
{
    public class ActionEntry : IDisposable
    {
        public long UserId;
        public long GuildId;
        public long ActivityId;
        public int CountdownDurationS;
        public DateTime ETA;
        public CancellationTokenSource ActionTask;

        public void Dispose()
        {
            if (!ActionTask.IsCancellationRequested)
                ActionTask?.Cancel();
            ActionTask?.Dispose();
        }
    }
    public class Command
    {
        public readonly string Name;
        public readonly string Description;
        public readonly string DescriptionBasic; // Description that is shown in help menu
        public readonly string DescriptionFull;
        public readonly Syntax[] Syntaxes;

        public Command(Type c)
        {
            Name = c.GetField("Name").Value<string>();
            Description = c.GetField("Description")?.Value<string>();
            DescriptionBasic = c.GetField("DescriptionBasic")?.Value<string>();
            DescriptionFull = c.GetField("DescriptionFull")?.Value<string>();
            Syntaxes = c.GetField("Syntaxes")?.Value<Syntax[]>() ?? Array.Empty<Syntax>();
        }
    }
    public class Syntax
    {
        public string Description { get; init; }
        public Parameter[] Parameters { get; init; } = Array.Empty<Parameter>();
    }
    public class Parameter
    {
        public string Name { get; init; }
        public string Type { get; init; }
        public string Description { get; set; }
    }
}
