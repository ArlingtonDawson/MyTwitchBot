using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace MyTwitchBot
{
    public class TwitchOAuth
    {
        private readonly string _redirectUri;
        private const string TokenFile = "twitch_tokens.json";

        public string ClientId { get { return _clientId; }}
        private readonly string _clientId;
        private readonly string _clientSecret;

        public TwitchOAuth(string clientId, string clientSecret, string callbackPort = "")
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            if (!string.IsNullOrEmpty(callbackPort))
                _redirectUri = $"http://localhost:{callbackPort}/";
            else
                _redirectUri = "http://localhost/";
        }

        public async Task<string> GetAccessTokenAsync()
        {
            if (File.Exists(TokenFile))
            {
                var tokens = JsonSerializer.Deserialize<TokenResponse>(
                    await File.ReadAllTextAsync(TokenFile));

                if (tokens != null && !string.IsNullOrWhiteSpace(tokens.refresh_token))
                {
                    // Refresh 5 minutes early to avoid using a token that is about to expire
                    bool tokenStillGood =
                        tokens.expires_at_utc > DateTime.UtcNow.AddMinutes(5);

                    if (tokenStillGood)
                        return tokens.access_token;

                    return await RefreshAccessTokenAsync(tokens.refresh_token);
                }
            }

            return await LoginAsync();
        }

        private async Task<string> LoginAsync()
        {
            var state = Guid.NewGuid().ToString("N");
            var scopes = HttpUtility.UrlEncode(
                    "channel:read:ads channel:manage:ads channel:moderate " +
                    "moderator:read:followers channel:read:subscriptions " +
                    "moderator:read:banned_users user:bot channel:bot user:read:chat user:write:chat");

            var authUrl =
                "https://id.twitch.tv/oauth2/authorize" +
                $"?client_id={_clientId}" +
                $"&redirect_uri={HttpUtility.UrlEncode(_redirectUri)}" +
                $"&response_type=code" +
                $"&scope={scopes}" +
                $"&state={state}";

            using var listener = new HttpListener();
            listener.Prefixes.Add(_redirectUri);
            listener.Start();

            Process.Start(new ProcessStartInfo
            {
                FileName = authUrl,
                UseShellExecute = true
            });

            var context = await listener.GetContextAsync();

            var code = context.Request.QueryString["code"];
            var returnedState = context.Request.QueryString["state"];

            var responseText = "<html><body>You can close this window.</body></html>";
            var buffer = Encoding.UTF8.GetBytes(responseText);
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer);
            context.Response.Close();

            listener.Stop();

            if (returnedState != state)
                throw new Exception("OAuth state mismatch.");

            if (string.IsNullOrWhiteSpace(code))
                throw new Exception("No OAuth code returned.");

            return await ExchangeCodeForTokenAsync(code);
        }

        private async Task<string> ExchangeCodeForTokenAsync(string code)
        {
            using var http = new HttpClient();

            var values = new Dictionary<string, string>
            {
                ["client_id"] = _clientId,
                ["client_secret"] = _clientSecret,
                ["code"] = code,
                ["grant_type"] = "authorization_code",
                ["redirect_uri"] = _redirectUri
            };

            var response = await http.PostAsync(
                "https://id.twitch.tv/oauth2/token",
                new FormUrlEncodedContent(values));

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var tokens = await SaveTokenAsync(json);
            return tokens.access_token;
        }

        private async Task<string> RefreshAccessTokenAsync(string refreshToken)
        {
            using var http = new HttpClient();

            var values = new Dictionary<string, string>
            {
                ["client_id"] = _clientId,
                ["client_secret"] = _clientSecret,
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken
            };

            var response = await http.PostAsync(
                "https://id.twitch.tv/oauth2/token",
                new FormUrlEncodedContent(values));

            if (!response.IsSuccessStatusCode)
            {
                File.Delete(TokenFile);
                return await LoginAsync();
            }

            var json = await response.Content.ReadAsStringAsync();

            var tokens = await SaveTokenAsync(json);
            return tokens.access_token;
        }

        private async Task<TokenResponse> SaveTokenAsync(string json)
        {
            var token = JsonSerializer.Deserialize<TokenResponse>(json)!;

            token.expires_at_utc = DateTime.UtcNow.AddSeconds(token.expires_in);

            var savedJson = JsonSerializer.Serialize(token, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(TokenFile, savedJson);

            return token;
        }

        private class TokenResponse
        {
            public string access_token { get; set; } = "";
            public string refresh_token { get; set; } = "";
            public int expires_in { get; set; }
            public DateTime expires_at_utc { get; set; }
            public string token_type { get; set; } = "";
        }
    }
}