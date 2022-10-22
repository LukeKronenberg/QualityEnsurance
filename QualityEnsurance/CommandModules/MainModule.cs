using Discord;
using static QualityEnsurance.Constants.Constants;
using static QualityEnsurance.Constants.Commands;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Discord.Interactions;
using VDF.Core;
using System.Text.RegularExpressions;
using QualityEnsurance.Extensions;
using QualityEnsurance.DiscordEventHandlers;
using QualityEnsurance.Attributes;

namespace QualityEnsurance.CommandModules
{
    public class MainModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IDbContextFactory<QualityEnsuranceContext> _contextFactory;
        public MainModule(IDbContextFactory<QualityEnsuranceContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        [SlashCommand("ping", "pong!")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task PingPong() => await RespondAsync($"pong! {Context.Client.Latency}ms {new Emoji("🏓")}\nhttps://cdn.discordapp.com/attachments/943786016169398282/943786087921356800/pong.mp4", ephemeral: true);

        [SlashCommand("help", "Help")]
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task ShowTutorial(string commandName = null)
        {
            if (commandName == null)
            {
                IEnumerable<Command> unsafeCommands = GetCommands();

                EmbedBuilder titleBuilder = new EmbedBuilder().WithTitle("**__Quality Ensurance__** Bot");
                EmbedFieldBuilder titleField = new EmbedFieldBuilder()
                    .WithName("Help")
                    .WithValue($"Use `{Help.Name}` + `command name` to get more information for a command.");
                titleBuilder.AddField(titleField);
                await RespondAsync(embed: titleBuilder.Build(), ephemeral: true);


                int index = 0;
                // Separete into chunks of 25 to not overwflow maximum EmbedFields of 25
                foreach (var chunckedCommands in unsafeCommands.Chunk(25))
                {
                    EmbedBuilder builder = new();
                    if (index == 0)
                        builder.WithTitle("Commands");

                    foreach (var c in chunckedCommands)
                    {
                        index++;
                        var commandBuilder = new EmbedFieldBuilder()
                            .WithName($"> **{c.Name}**")
                            .WithValue($"{c.DescriptionBasic}");

                        builder.AddField(commandBuilder);
                    }
                    if (index == unsafeCommands.Count())
                        builder.WithCurrentTimestamp();

                    await FollowupAsync(embed: builder.Build(), ephemeral: true);
                }
            } 
            else
            {
                Command command = GetCommands().SingleOrDefault(c => c.Name.ToLower() == commandName.ToLower());
                if (command == null)
                {
                    await RespondAsync($"{Context.User.Mention} `{commandName?.SanitizeCode()}` is not a valid command!");
                    return;
                }

                var embedBuilder = new EmbedBuilder().WithTitle($"**{command.Name}**");
                var descriptionFullField = new EmbedFieldBuilder()
                    .WithName("> __Description__:")
                    .WithValue($"> {command.DescriptionFull}");
                embedBuilder.AddField(descriptionFullField);

                var parmeterCommandField = new EmbedFieldBuilder()
                    .WithName("__**Parameters**__:")
                    .WithValue($"(`name-of-param`: `type-of-param`)");
                embedBuilder.AddField(parmeterCommandField);

                foreach (var syntax in command.Syntaxes)
                {
                    var syntaxBuilder = new EmbedFieldBuilder();
                    if (syntax.Description != null)
                        syntaxBuilder.WithName($"*{syntax.Description}*");
                    else
                        syntaxBuilder.WithName("_ _");
                    if (syntax.Parameters.Any())
                    {
                        StringBuilder paramBuilder = new();
                        foreach (var param in syntax.Parameters)
                        {
                            paramBuilder.Append(
                                $"**`{param.Name}`**: `{param.Type}`{(param.Description != null? $"\n> {param.Description}":null)}\n\n"
                            );
                        }
                        syntaxBuilder.Value = paramBuilder.ToString();
                    }
                    else
                        syntaxBuilder.Value = "`no parameters required`";
                    embedBuilder.AddField(syntaxBuilder);
                }

                await RespondAsync(embed: embedBuilder.Build(), ephemeral: true);
            }
        }

        [SlashCommand("purge", "Delete a set amount of recent messages in a channel.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Purge(int amount)
        {
            await DeferAsync(ephemeral: true);

            if (Context.Channel is not ITextChannel channel)
                return;

            var messages = channel.GetMessagesAsync(amount);

            try
            {
                await channel.DeleteMessagesAsync(await messages.FlattenAsync());

                await FollowupAsync($"Succesfully deleted {amount} messages.", ephemeral: true);
            }
            catch (ArgumentOutOfRangeException)
            {
                await FollowupAsync("Can only delete messages which are newer than 2 weeks! sorry", ephemeral: true);
            }
        }
    }

    [Group("admin", "Commands used by the owner of the bot.")]
    [RequireOwnerOrConfigWhitelist]
    public class AdminModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly PresenceHandler _presenceHandler;
        private readonly IDbContextFactory<QualityEnsuranceContext> _contextFactory;

        public AdminModule(PresenceHandler presenceHandler, IDbContextFactory<QualityEnsuranceContext> contextFactory)
        {
            _presenceHandler = presenceHandler;
            _contextFactory = contextFactory;
        }

        [SlashCommand("check-duplicates", "Check for duplicate videos in a channel")]
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        public async Task CheckDuplicates(int messageAmount = 100)
        {
            if (Context.Channel is not ITextChannel channel)
                return;

            var guild = Context.Guild;

            await DeferAsync();
            await FollowupAsync($"Fetching {messageAmount} messages...");

            var messages = await channel.GetMessagesAsync(messageAmount).FlattenAsync();

            var linkParser = new Regex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            List<(IMessage Message, IAttachment Attachment, string FileUrl, string CachedFileName)> msgToVidLink = new();

            await ModifyOriginalResponseAsync(msg => msg.Content = $"Searching messages for attachments and links...");

            int attachmentCount = 0;
            foreach (var msg in messages)
            {
                foreach (var attachment in msg.Attachments)
                {
                    string fileExtension = attachment.Filename.Split('.').Last();
                    if (!VideoExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
                        continue;
                    string fileName = $"{attachment.Id}.{fileExtension}";
                    attachmentCount++;
                    msgToVidLink.Add((msg, attachment, null, fileName));
                }

                foreach (var link in linkParser.Matches(msg.Content).ToArray())
                {
                    string fileExtension = link.Value.Split('?')[0].Split('.').Last();
                    if (!VideoExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
                        continue;
                    string fileName = $"{attachmentCount}.{fileExtension}";
                    msgToVidLink.Add((msg, null, link.Value, fileName));
                    attachmentCount++;
                }
            }

            await ModifyOriginalResponseAsync(msg => msg.Content = $"Downloading links and attachments. Progress: 0/{attachmentCount} | Errors: 0");

            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), $"duplicate_download_cache", guild.Id.ToString());
            DirectoryInfo directory = new(directoryPath);
            if (directory.Exists)
                directory.Delete(true);
            directory.Create();

            int currentDownloadIndex = 0;
            int downloadErrorCount = 0;

            Task messageDelay = null;
            using HttpClient client = new();

            object parallelLocker = new();
            await Parallel.ForEachAsync(msgToVidLink.ToArray(), new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount } , async (item, cancelToken) =>
            {
                try
                {
                    if (item.Attachment == null)
                    {
                        var content = await client.GetByteArrayAsync(item.FileUrl, cancelToken);
                        File.WriteAllBytes($"{directoryPath}\\{item.CachedFileName}", content);
                    }
                    else
                    {
                        var content = await client.GetByteArrayAsync(item.Attachment.Url, cancelToken);
                        File.WriteAllBytes($"{directoryPath}\\{item.CachedFileName}", content);
                    }
                }
                catch (Exception)
                {
                    msgToVidLink.Remove(item);
                    Interlocked.Decrement(ref attachmentCount);
                    Interlocked.Increment(ref downloadErrorCount);
                }

                currentDownloadIndex = Interlocked.Increment(ref currentDownloadIndex);
                lock (parallelLocker)
                {
                    if (messageDelay?.IsCompleted ?? true)
                    {
                        ModifyOriginalResponseAsync(msg => msg.Content = $"Downloading links and attachments. Progress: {currentDownloadIndex}/{attachmentCount} | Errors: {downloadErrorCount}")
                            .GetAwaiter()
                            .GetResult();
                        messageDelay = Task.Delay(2000, cancelToken);
                    }
                }
            });

            VDF.Core.Utils.DatabaseUtils.ClearDatabase();
            ScanEngine engine = new();
            
            Settings scanSettings = engine.Settings;
            scanSettings.IncludeList.Add(directoryPath);
            scanSettings.HardwareAccelerationMode = VDF.Core.FFTools.FFHardwareAccelerationMode.auto;
            scanSettings.IncludeImages = false;
            scanSettings.MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount / 2);

            TaskCompletionSource completionSource = new();
            engine.ScanDone += (sender, e) => completionSource.SetResult();
            
            object progressEventLocker = new();
            engine.Progress += (sender, e) =>
            {
                lock (progressEventLocker)
                {
                    if (messageDelay?.IsCompleted ?? true)
                    {
                        ModifyOriginalResponseAsync(msg => {
                            msg.Content = $"Found {attachmentCount} attachments and processing them now... {e.CurrentPosition}/{e.MaxPosition}";
                        }).GetAwaiter().GetResult();
                        messageDelay = Task.Delay(5000);
                    }
                }
            };

            engine.StartSearch();

            await completionSource.Task;

            var duplicates = engine.Duplicates;

            var dupesGrouped = duplicates.GroupBy(dupe => dupe.GroupId);
            int dupesGroupedIndex = 1;

            if (!dupesGrouped.Any())
                await ModifyOriginalResponseAsync(msg => msg.Content = $"No Duplicates found!");
            else
            {
                foreach (var dupes in dupesGrouped)
                {
                    var embedBuilder = new EmbedBuilder()
                        .WithTitle($"Dupplicate group: {dupesGroupedIndex}")
                        .WithDescription($"Found {dupes.Count()} video/image matching.");

                    foreach (var dupe in dupes)
                    {
                        var fileName = Path.GetFileName(dupe.Path);

                        var info = msgToVidLink.FirstOrDefault(i => i.CachedFileName == fileName);

                        if (info != default)
                        {
                            embedBuilder.AddField(new EmbedFieldBuilder()
                                .WithName($"Similarity: {dupe.Similarity}")
                                .WithValue(
                                    $"Message Link: {info.Message.GetJumpUrl()}\n" +
                                    (info.Attachment == null ?
                                        $"Link in Question: {info.FileUrl}" :
                                        $"Attachment Name: {info.Attachment.Filename}")
                                ));
                        }
                    }

                    await FollowupAsync(embed: embedBuilder.Build());
                }

                await DeleteOriginalResponseAsync();
            }

            directory.Delete(true);
        }

        [SlashCommand("pending-action-amount", "Gets the number of actions pending for execution.")]
        public async Task PendingActionAmount()
        {
            await RespondAsync($"**Amount**: {_presenceHandler.PendingActions.Count}", ephemeral: true);
        }

        [SlashCommand("channel-settings", "Change channel configuration.")]
        public async Task ChannelSettings(
            [Summary("upload-link", "Reuploads any image or video link posted and removes the original message.")]
            bool? uploadLink = null)
        {
            if (Context.Channel is not IGuildChannel discordChannel)
                return;

            using var context = _contextFactory.CreateDbContext();

            var channel = context.GetChannel((long)discordChannel.Id);

            EmbedBuilder builder = new();
            builder.WithTitle("Channel Settings");

            string GetStatus(bool oldValue, bool? newValue)
            {
                StringBuilder sBuilder = new();
                sBuilder.Append(oldValue ? "`Enabled`" : "`Disabled`");
                if (newValue.HasValue)
                    sBuilder.Append($" => {(newValue.Value ? "`Enabled`" : "`Disabled`")}");
                return sBuilder.ToString();
            }

            builder.WithFields(new EmbedFieldBuilder()
                .WithName("Upload link")
                .WithValue(GetStatus(channel.UploadLink, uploadLink)));

            if (uploadLink.HasValue)
                channel.UploadLink = uploadLink.Value;

            context.SaveChanges();

            await RespondAsync(embed: builder.Build(), ephemeral: true);
        }
    }
}