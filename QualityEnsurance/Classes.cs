using QualityEnsurance.Extensions;

namespace QualityEnsurance
{
    public class ActionEntry : IDisposable
    {
        public long UserId;
        public long GuildId;
        public long ActivityId;
        public DateTimeOffset Start;
        public DateTimeOffset ETA;
        public CancellationTokenSource CancellationReference;

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            
            if (!CancellationReference.IsCancellationRequested)
                CancellationReference?.Cancel();
            CancellationReference?.Dispose();
        }
    }
    
    public class Command
    {
        public string Name { get; init; }
        /// <summary>
        /// Description shown in the help command
        /// </summary>
        public string DescriptionBasic { get; init; }
        public string DescriptionFull { get; init; }
        public Syntax[] Syntaxes { get; init; } = Array.Empty<Syntax>();
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
