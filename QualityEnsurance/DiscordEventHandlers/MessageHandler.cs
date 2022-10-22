using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using static QualityEnsurance.Constants.Constants;
using System.Collections.Concurrent;

namespace QualityEnsurance.DiscordEventHandlers
{
    public class MessageHandler
    {
        private readonly DiscordSocketClient _discord;
        private readonly IDbContextFactory<QualityEnsuranceContext> _contextFactory;

        public MessageHandler(DiscordSocketClient discord, IDbContextFactory<QualityEnsuranceContext> contextFactory)
        {
            _discord = discord;
            _discord.MessageReceived += MessageRecieved;
            _contextFactory = contextFactory;
        }

        public async Task MessageRecieved(SocketMessage socketMessage)
        {
            if (socketMessage is not SocketUserMessage discordMessage)
                return;

            QualityEnsuranceContext context = null;
            
            if (discordMessage.Content.Contains("http"))
            {
                if (discordMessage.Channel is not SocketTextChannel discordChannel)
                    return;
                
                context ??= _contextFactory.CreateDbContext();

                var channel = context.Channels.FirstOrDefault(c => c.Id == (long)discordChannel.Id);
                if (channel != null && channel.UploadLink)
                {
                    string content = discordMessage.Content;
                    var links = LinkParser.Matches(content).Where(link => {
                        var fileExtension = link.Value.Split('?')[0].Split('.').Last();
                        return VideoExtensions.Contains(fileExtension) || ImageExtensions.Contains(fileExtension); 
                    }).ToArray();

                    if (links.Any())
                    {
                        using HttpClient client = new();
                        List<Discord.FileAttachment> files = new();
                        
                        foreach (var link in links)
                        {
                            var response = await client.GetAsync(link.Value);
                            if (response.IsSuccessStatusCode)
                            {
                                var stream = await response.Content.ReadAsStreamAsync();
                                var fileName = link.Value.Split('?')[0].Split('/').Last();
                                files.Add(new(stream, fileName));
                            }
                        }

                        try
                        {
                            foreach (var chunckedFiles in files.Chunk(10))
                            {
                                await discordChannel.SendFilesAsync(chunckedFiles, "");
                            }

                            await discordMessage.DeleteAsync();
                        }
                        catch { return; }
                    }
                }
            }
        }
    }
}
