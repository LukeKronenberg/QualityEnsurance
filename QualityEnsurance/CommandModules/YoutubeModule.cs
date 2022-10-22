using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using Google.Apis.YouTube.v3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using QualityEnsurance.Attributes;
using QualityEnsurance.Models;
using QualityEnsurance.Extensions;

namespace QualityEnsurance.CommandModules
{
    [Group("youtube", "Youtube related commands. Only for bot developers.")]
    [RequireContext(ContextType.Guild)]
    [RequireOwnerOrConfigWhitelist]
    public class YoutubeModule : InteractionModuleBase<SocketInteractionContext>
    {

        private readonly IDbContextFactory<QualityEnsuranceContext> _contextFactory;
        private readonly IConfiguration _config;

        public YoutubeModule(IDbContextFactory<QualityEnsuranceContext> contextFactory, IConfiguration config)
        {
            _contextFactory = contextFactory;
            _config = config;
        }

        [SlashCommand("identifiers", "List all saved identifiers.")]
        public async Task Identifiers()
        {
            await DeferAsync(ephemeral: true);
            
            using var context = _contextFactory.CreateDbContext();
            var youtubeUsers = await context.YoutubeUsers.ToListAsync();
            var embed = new EmbedBuilder()
                .WithTitle("Youtube Identifiers")
                .WithDescription(string.Join("\n", youtubeUsers.Select(user => $"`{user.Identifier.SanitizeCode()}` {user.ChannelUrl}")));
            
            await FollowupAsync(embed: embed.Build(), ephemeral: true);
        }

        [SlashCommand("register", "Permit the bot to access a youtube account. (Bot developer only).")]
        [RequireOwner]
        public async Task Register(
            [Summary("identifier", "A unique name for the account too be linked")]
            string identifier,
            [Summary("channel-url", "Optional channel url. Does not update by itself.")]
            string channelUrl = null,
            [Summary("ignore-existing", "Override an already existing identifier.")]
            bool ignoreExisting = false)
        {
            if (string.IsNullOrWhiteSpace(identifier) || identifier.Length > 100)
            {
                await RespondAsync("Identifier invalid. (Empty or longer than 100 caracters)", ephemeral: true);
                return;
            }
            
            await DeferAsync(ephemeral: true);

            identifier = identifier.ToLower().Trim();

            using var context = _contextFactory.CreateDbContext();

            YoutubeUser user = context.YoutubeUsers.SingleOrDefault(yu => yu.Identifier == identifier);

            if (user != null && !ignoreExisting)
            {
                await FollowupAsync("User with identifier already exists.", ephemeral: true);
                return;
            }

            UserCredential creds =
                await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets
                    {
                        ClientId = _config["Youtube:ClientId"],
                        ClientSecret = _config["Youtube:ClientSecret"]
                    },
                    new[]
                    {
                        YouTubeService.Scope.YoutubeUpload,
                        YouTubeService.Scope.YoutubeForceSsl,
                        YouTubeService.Scope.Youtubepartner
                    },
                    "QualityEnsurance",
                    CancellationToken.None,
                    dataStore: new Google.Apis.Util.Store.NullDataStore());

            bool isNew = user == null;

            user ??= new();
            user.Identifier = identifier;
            user.ChannelUrl = channelUrl;
            user.AccessToken = creds.Token.AccessToken;
            user.RefreshToken = creds.Token.RefreshToken;
            user.ExpiresInSeconds = creds.Token.ExpiresInSeconds ?? 0;
            user.Issued = creds.Token.IssuedUtc;

            if (isNew)
                context.YoutubeUsers.Add(user);
            context.SaveChanges();

            await FollowupAsync("Done!", ephemeral: true);
        }
        
        [SlashCommand("upload", "Upload an video")]
        public async Task UploadVideo(
            [Summary("identifier", "The identifier of the channel to use.")]
            string identifier,
            [Summary("title", "Title of the video (required).")]
            string title,
            [Summary("publicity", "The publicity of the video when it is uploaded")]
            YoutubePublicityType publicity,
            [Summary("video-url", "The video to upload")]
            string video_url,
            [Summary("publish-at", "Date and time when the video should be published. (YYYY-MM-DD hh:mm:ss)")]
            DateTime? publishAt = null,
            [Summary("permiere", "If the video should premiere.")]
            bool premiere = false,
            [Summary("description", "Description of the video.")]
            string description = null,
            [Summary("made-for-kids", "If the video is specifically made for youtube kids.")]
            bool madeForKids = false)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                await RespondAsync("Identifier can't be empty!", ephemeral: true);
                return;
            }

            if (publishAt < DateTime.Now)
            {
                await RespondAsync("Publish date can't be in the past!", ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);

            identifier = identifier.ToLower().Trim();

            using var context = _contextFactory.CreateDbContext();

            YoutubeUser user = context.YoutubeUsers.SingleOrDefault(yu => yu.Identifier == identifier);

            if (user == null)
            {
                await FollowupAsync($"`{identifier.SanitizeCode()}` not found.", ephemeral: true);
                return;
            }


            using HttpClient client = new();

            Stream stream;
            try
            {
                stream = await client.GetStreamAsync(video_url);
                Console.WriteLine($"Download video from {video_url} successfull");
            }
            catch (Exception ex)
            {
                await FollowupAsync($"Failed to download video. {ex.Message}", ephemeral: true);
                return;
            }

            await FollowupAsync($"Trying to log into youtube api for `{identifier.SanitizeCode()}`...", ephemeral: true);

            GoogleAuthorizationCodeFlow flow = new(new GoogleAuthorizationCodeFlow.Initializer()
            {
                ClientSecrets = new ClientSecrets()
                {
                    ClientId = _config["Youtube:ClientId"],
                    ClientSecret = _config["Youtube:ClientSecret"]
                },
                DataStore = new Google.Apis.Util.Store.NullDataStore(),
                Scopes = new[]
                {
                    YouTubeService.Scope.YoutubeUpload,
                    YouTubeService.Scope.YoutubeForceSsl,
                    YouTubeService.Scope.Youtubepartner
                }
            });

            UserCredential creds = new(flow, "Quality Ensurance", new() { AccessToken = user.AccessToken, RefreshToken = user.RefreshToken });

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = creds,
                ApplicationName = "QualityEnsurance"
            });

            Video video = new()
            {
                Snippet = new()
                {
                    Title = title,
                    Description = description,
                    LiveBroadcastContent = premiere? "upcoming" : "none",
                    PublishedAt = publishAt
                },
                Status = new()
                {
                    PrivacyStatus = publicity.ToString().ToLower(),
                    PublishAt = publishAt,
                    MadeForKids = madeForKids
                },
            };
            
            var videoInsertRequest = youtubeService.Videos.Insert(video, "snippet,status", stream, "video/*");

            videoInsertRequest.ProgressChanged += async (Google.Apis.Upload.IUploadProgress progress) => {
                switch (progress.Status)
                {
                    case Google.Apis.Upload.UploadStatus.NotStarted:
                        break;
                    case Google.Apis.Upload.UploadStatus.Starting:
                        break;
                    case Google.Apis.Upload.UploadStatus.Uploading:
                        await ModifyOriginalResponseAsync(msg => msg.Content = $"Uploading ... {progress.BytesSent}/{stream.Length} ({progress.BytesSent / stream.Length * 100}%)");
                        break;
                    case Google.Apis.Upload.UploadStatus.Completed:
                        break;
                    case Google.Apis.Upload.UploadStatus.Failed:
                        await ModifyOriginalResponseAsync(msg => msg.Content = $"Upload failed. {progress.Exception}");
                        break;
                    default:
                        break;
                }
            };

            videoInsertRequest.ResponseReceived += async (Video videoResponse) => {
                video = videoResponse;
                await ModifyOriginalResponseAsync(msg => msg.Content = $"Upload completed. https://youtu.be/{videoResponse.Id}");
            };

            await videoInsertRequest.UploadAsync();

            stream.Dispose();
        }

        [SlashCommand("list", "List queued videos for a channel.")]
        public async Task List(
            [Summary("identifier", "Identifier of the channel.")]
            string identifier,
            [Summary("only-upcoming", "If only videos that will premier in the future will be returned. (default true).")]
            bool onlyUpcoming = true)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                await RespondAsync("Identifier can't be empty!", ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);

            identifier = identifier.ToLower().Trim();

            using var context = _contextFactory.CreateDbContext();

            YoutubeUser user = context.YoutubeUsers.SingleOrDefault(yu => yu.Identifier == identifier);

            if (user == null)
            {
                await FollowupAsync($"`{identifier.SanitizeCode()}` not found.", ephemeral: true);
                return;
            }

            using GoogleAuthorizationCodeFlow flow = new(new GoogleAuthorizationCodeFlow.Initializer()
            {
                ClientSecrets = new ClientSecrets()
                {
                    ClientId = _config["Youtube:ClientId"],
                    ClientSecret = _config["Youtube:ClientSecret"]
                },
                DataStore = new Google.Apis.Util.Store.NullDataStore(),
                Scopes = new[]
                {
                    YouTubeService.Scope.YoutubeUpload,
                    YouTubeService.Scope.YoutubeForceSsl,
                    YouTubeService.Scope.Youtubepartner
                }
            });

            UserCredential creds = new(flow, "Quality Ensurance", new() { AccessToken = user.AccessToken, RefreshToken = user.RefreshToken });

            using var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = creds,
                ApplicationName = "QualityEnsurance"
            });

            var channelsListRequest = youtubeService.Channels.List("contentDetails");
            channelsListRequest.Mine = true;
            var channelsListResponse = await channelsListRequest.ExecuteAsync();

            var channelUploadListId = channelsListResponse.Items.First().ContentDetails.RelatedPlaylists.Uploads;
            List<string> videoIds = new();

            var nextPageToken = "";
            while (nextPageToken != null)
            {
                var playlistItemsListRequest = youtubeService.PlaylistItems.List("ContentDetails");
                playlistItemsListRequest.PlaylistId = channelUploadListId;
                playlistItemsListRequest.MaxResults = 50;
                playlistItemsListRequest.PageToken = nextPageToken;

                var playlistItemsListResponse = await playlistItemsListRequest.ExecuteAsync();

                videoIds.AddRange(playlistItemsListResponse.Items.Select(r => r.ContentDetails.VideoId));

                nextPageToken = playlistItemsListResponse.NextPageToken;
            }

            List<Video> videos = new();
            foreach (var chunkedVideoIds in videoIds.Chunk(50))
            {
                var youtubeVideosRequest = youtubeService.Videos.List("snippet");
                youtubeVideosRequest.Id = chunkedVideoIds;

                var listVideosResponse = await youtubeVideosRequest.ExecuteAsync();

                if (onlyUpcoming)
                    videos.AddRange(listVideosResponse.Items.Where(vid => vid.Snippet.LiveBroadcastContent == "upcoming"));
                else
                    videos.AddRange(listVideosResponse.Items);
            }


            var titleBuilder = new EmbedBuilder()
                .WithTitle($"Found {videos.Count} video/s.")
                .WithDescription(string.Join('\n', 
                    $"**Identifier:** `{identifier.SanitizeCode()}`",
                    $"**Only upcoming:** `{onlyUpcoming}`"));

            List<Embed> embeds = new() { titleBuilder.Build() };
            int videoNumber = 1;
            foreach (var videoChunk in videos.Chunk(25))
            {
                EmbedBuilder builder = new EmbedBuilder()
                    .WithTitle($"Upcoming videos {videoNumber}-{videoNumber + videoChunk.Length - 1}");

                foreach (var video in videoChunk)
                {
                    EmbedFieldBuilder fieldBuilder = new();
                    fieldBuilder.WithName($"Title: {Format.Sanitize(video.Snippet.Title)}");
                    fieldBuilder.WithValue(
                        $"Link: https://youtu.be/{video.Id}\n" +
                        $"Published at: {video.Snippet.PublishedAt:yyyy-MM-dd hh:mm:ss}\n" +
                        $"Premiere: {video.Snippet.LiveBroadcastContent == "upcoming"}");

                    builder.AddField(fieldBuilder);
                }

                videoNumber += videoChunk.Length;
                embeds.Add(builder.Build());
            }

            foreach (var embedChunk in embeds.Chunk(e => e.Title.Length + e.Fields.Sum(f => f.Name.Length + f.Value.Length), 6000))
                await FollowupAsync(embeds: embedChunk, ephemeral: true);
        }

        public enum YoutubePublicityType
        {
            Public,
            Unlisted,
            Private
        }
    }
}
