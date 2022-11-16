using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using static QualityEnsurance.Constants.Constants;
using Discord.Rest;
using QualityEnsurance.Extensions;

namespace QualityEnsurance.DiscordEventHandlers
{
    public class MessageHandler
    {
        private readonly DiscordSocketClient _discord;
        private readonly IDbContextFactory<QualityEnsuranceContext> _contextFactory;

        public MessageHandler(DiscordSocketClient discord, IDbContextFactory<QualityEnsuranceContext> contextFactory)
        {
            _discord = discord;
            _contextFactory = contextFactory;
        }

        public void Initialize()
        {
            _discord.MessageReceived += MessageRecieved;
        }

        public async Task MessageRecieved(SocketMessage socketMessage)
        {
            if (socketMessage is not SocketUserMessage discordMessage)
                return;

            if (discordMessage.Channel is not SocketTextChannel discordChannel)
                return;
            
            QualityEnsuranceContext context = null;
            
            if (discordMessage.Content.Contains("http"))
            {
                context ??= _contextFactory.CreateDbContext();

                var channel = context.Channels.FirstOrDefault(c => c.Id == (long)discordChannel.Id);
                if (channel != null && channel.UploadLinks)
                {
                    string content = discordMessage.Content;
                    // Search message content for video or image links. Will not work for links which don't end with a file name
                    // and don't care because it's supposed to be used for copied discord attachement links
                    var links = LinkParser.Matches(content)
                        .Where(link => {
                            var fileExtension = link.Value.Split('?')[0].Split('.').Last();
                            return VideoExtensions.Contains(fileExtension) || ImageExtensions.Contains(fileExtension); 
                        })
                        .Select(match => match.Value)
                        .ToList();

                    if (links.Any())
                    {
                        // Don't forget attachements
                        links.AddRange(
                            discordMessage.Attachments
                                .Select(a => a.Url)
                                .Where(link =>
                                {
                                    var fileExtension = link.Split('?')[0].Split('.').Last();
                                    return VideoExtensions.Contains(fileExtension) || ImageExtensions.Contains(fileExtension);
                                }
                        ));

                        using HttpClient client = new();
                        List<Discord.FileAttachment> files = new();
                        
                        try
                        {
                            foreach (var link in links)
                            {
                                var response = await client.GetAsync(link);
                                if (response.IsSuccessStatusCode)
                                {
                                    var stream = await response.Content.ReadAsStreamAsync();
                                    var fileName = link.Split('?')[0].Split('/').Last();
                                    files.Add(new(stream, fileName));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            return;
                        }

                        List<RestUserMessage> sentMessages = new();

                        try
                        {
                            foreach (var chunckedFiles in files.Chunk(file => file.Stream.Length, 8_388_600, 10))
                            {
                                sentMessages.Add(await discordChannel.SendFilesAsync(chunckedFiles, ""));
                            }

                            await discordChannel.SendMessageAsync($"Uploaded links posted by {discordMessage.Author.Mention}");
                            
                            await discordMessage.DeleteAsync();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            try
                            {
                                if (sentMessages.Any())
                                {
                                    foreach (var message in sentMessages)
                                    {
                                        await message.DeleteAsync();
                                    }
                                }
                            }
                            catch { }
                            return; 
                        }
                    }
                }
            }
        }
    }
}
