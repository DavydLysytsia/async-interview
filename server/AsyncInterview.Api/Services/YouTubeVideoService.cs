using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.YouTube.v3.Data;
using YTService = Google.Apis.YouTube.v3.YouTubeService;

namespace AsyncInterview.Api.Services;

public class AppOptions
{
    public string? GoogleClientId { get; set; }
    public string? GoogleClientSecret { get; set; }
    public string AppBaseUrl { get; set; } = "http://localhost:5240";
    public bool DevFakeAuth { get; set; }

    public bool GoogleConfigured =>
        !string.IsNullOrWhiteSpace(GoogleClientId) && !string.IsNullOrWhiteSpace(GoogleClientSecret);
}

public class YouTubeStatus
{
    public bool Configured { get; set; }
    public bool Connected { get; set; }
    public bool NeedsReconnect { get; set; }
    public bool HasChannel { get; set; }
    public string? ChannelTitle { get; set; }
}

public class YouTubeUploadResult
{
    public string VideoId { get; set; } = "";
    public string PrivacyStatus { get; set; } = "";
}

// Thrown for the failure cases the requirements ask us to handle explicitly;
// Message is safe to show to the user.
public class YouTubeFriendlyException : Exception
{
    public string Reason { get; }
    public YouTubeFriendlyException(string reason, string message) : base(message) => Reason = reason;
}

// Handles the "Connect YouTube" OAuth flow (upload + readonly scopes, separate
// from sign-in) and the actual video upload through the YouTube Data API.
public class YouTubeVideoService
{
    private readonly AppOptions _options;
    private readonly EfDataStore _dataStore;

    public YouTubeVideoService(AppOptions options, EfDataStore dataStore)
    {
        _options = options;
        _dataStore = dataStore;
    }

    private string RedirectUri => $"{_options.AppBaseUrl}/api/youtube/callback";

    private GoogleAuthorizationCodeFlow CreateFlow()
    {
        if (!_options.GoogleConfigured)
            throw new YouTubeFriendlyException("not_configured",
                "Google API credentials are not configured on the server yet.");

        return new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = _options.GoogleClientId,
                ClientSecret = _options.GoogleClientSecret
            },
            Scopes = new[] { YTService.Scope.YoutubeUpload, YTService.Scope.YoutubeReadonly },
            DataStore = _dataStore
        });
    }

    public string BuildAuthorizationUrl(string state)
    {
        var flow = CreateFlow();
        var request = flow.CreateAuthorizationCodeRequest(RedirectUri);
        if (request is GoogleAuthorizationCodeRequestUrl googleRequest)
        {
            googleRequest.AccessType = "offline";
            // Force the consent screen so Google always returns a refresh token.
            googleRequest.Prompt = "consent";
        }
        request.State = state;
        return request.Build().AbsoluteUri;
    }

    public async Task ExchangeCodeAsync(int userId, string code, CancellationToken ct)
    {
        var flow = CreateFlow();
        await flow.ExchangeCodeForTokenAsync(userId.ToString(), code, RedirectUri, ct);
    }

    public async Task DisconnectAsync(int userId, CancellationToken ct)
    {
        var flow = CreateFlow();
        await flow.DeleteTokenAsync(userId.ToString(), ct);
    }

    private async Task<UserCredential?> LoadCredentialAsync(int userId, CancellationToken ct)
    {
        var flow = CreateFlow();
        var token = await flow.LoadTokenAsync(userId.ToString(), ct);
        if (token == null) return null;
        return new UserCredential(flow, userId.ToString(), token);
    }

    public async Task<YouTubeStatus> GetStatusAsync(int userId, CancellationToken ct)
    {
        var status = new YouTubeStatus { Configured = _options.GoogleConfigured };
        if (!status.Configured) return status;

        var credential = await LoadCredentialAsync(userId, ct);
        if (credential == null) return status;

        try
        {
            using var yt = CreateYouTubeClient(credential);
            var request = yt.Channels.List("snippet");
            request.Mine = true;
            var response = await request.ExecuteAsync(ct);

            status.Connected = true;
            status.HasChannel = response.Items is { Count: > 0 };
            status.ChannelTitle = response.Items?.FirstOrDefault()?.Snippet?.Title;
        }
        catch (TokenResponseException)
        {
            // Refresh token expired/revoked (testing-mode tokens die after ~7 days).
            status.NeedsReconnect = true;
        }
        return status;
    }

    public async Task<YouTubeUploadResult> UploadAsync(
        int userId, Stream content, string contentType, string title, string description, CancellationToken ct)
    {
        var credential = await LoadCredentialAsync(userId, ct)
            ?? throw new YouTubeFriendlyException("youtube_not_connected",
                "Connect your YouTube account before uploading a response.");

        var video = new Video
        {
            Snippet = new VideoSnippet
            {
                // YouTube titles max out at 100 characters.
                Title = title.Length > 100 ? title[..97] + "..." : title,
                Description = description,
                CategoryId = "22" // People & Blogs
            },
            Status = new VideoStatus
            {
                // We request unlisted; unverified API projects get forced to
                // private by YouTube. We save whatever actually came back.
                PrivacyStatus = "unlisted",
                SelfDeclaredMadeForKids = false
            }
        };

        try
        {
            using var yt = CreateYouTubeClient(credential);
            var insert = yt.Videos.Insert(video, "snippet,status", content, contentType);
            insert.ChunkSize = ResumableUpload.MinimumChunkSize * 4;

            var progress = await insert.UploadAsync(ct);
            if (progress.Status != UploadStatus.Completed)
                throw MapUploadError(progress.Exception);

            return new YouTubeUploadResult
            {
                VideoId = insert.ResponseBody.Id,
                PrivacyStatus = insert.ResponseBody.Status?.PrivacyStatus ?? "unknown"
            };
        }
        catch (TokenResponseException)
        {
            throw new YouTubeFriendlyException("reconnect_needed",
                "Your YouTube authorization expired. Please reconnect and try again.");
        }
    }

    private static Exception MapUploadError(Exception? ex)
    {
        if (ex is GoogleApiException api)
        {
            var reason = api.Error?.Errors?.FirstOrDefault()?.Reason ?? "";
            return reason switch
            {
                "youtubeSignupRequired" => new YouTubeFriendlyException("no_channel",
                    "This Google account has no YouTube channel. Create one on youtube.com, then try again."),
                "quotaExceeded" or "uploadLimitExceeded" => new YouTubeFriendlyException("quota",
                    "The daily YouTube upload quota was reached. Try again tomorrow."),
                _ => new YouTubeFriendlyException("upload_failed",
                    "YouTube rejected the upload. Check the file and try again.")
            };
        }
        return ex ?? new YouTubeFriendlyException("upload_failed", "The upload did not complete.");
    }

    private static YTService CreateYouTubeClient(UserCredential credential) => new(
        new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "async-interview"
        });
}
