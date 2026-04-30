using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MyTwitchBot
{
    public class TwitchApplicationClient
    {
        private TwitchOAuth _twitchOAuth;
        private string _broadcasterId = "";
        public TwitchApplicationClient(TwitchOAuth twitchOAuth, string broadcasterId)
        { 
            _twitchOAuth = twitchOAuth;
            _broadcasterId = broadcasterId;
            
        }
        private async Task<HttpClient> CreateTwitchClient()
        {
            var http = new HttpClient();

            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await _twitchOAuth.GetAccessTokenAsync());

            http.DefaultRequestHeaders.Add("Client-Id", _twitchOAuth.ClientId);

            return http;
        }

        public async Task SubscribeToFollowsAsync(string sessionId)
        {
            await SubscribeAsync("channel.follow", "2", sessionId,
                new { broadcaster_user_id = _broadcasterId, moderator_user_id = _broadcasterId });
        }
        public async Task SubscribeToSubscriptionsAsync(string sessionId)
        {
            await SubscribeAsync("channel.subscribe", "1", sessionId,
                new { broadcaster_user_id = _broadcasterId });
        }

        public async Task SubscribeToGiftSubscriptionsAsync(string sessionId)
        {
            await SubscribeAsync("channel.subscription.gift", "1", sessionId,
                new { broadcaster_user_id = _broadcasterId });
        }

        public async Task SubscribeToBansAsync(string sessionId)
        {
            await SubscribeAsync("channel.ban", "1", sessionId,
                new { broadcaster_user_id = _broadcasterId });
        }

        private async Task SubscribeAsync(string type, string version, string sessionId, object condition)
        {
            using var http = await CreateTwitchClient();

            var body = JsonSerializer.Serialize(new
            {
                type,
                version,
                condition,
                transport = new
                {
                    method = "websocket",
                    session_id = sessionId
                }
            });

            var response = await http.PostAsync(
                "https://api.twitch.tv/helix/eventsub/subscriptions",
                new StringContent(body, Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();
            Console.WriteLine($"Subscribed to {type}");
        }

        public async Task<string> GetBroadcasterId(string username)
        {
            using var http = await CreateTwitchClient();

            var json = await http.GetStringAsync(
                $"https://api.twitch.tv/helix/users?login={username}");

            using var doc = JsonDocument.Parse(json);
            var data = doc.RootElement.GetProperty("data");

            if (data.GetArrayLength() == 0)
                throw new Exception("User not found.");

            return data[0].GetProperty("id").GetString();
        }

        public async Task<int> GetAdSnoozeCount()
        {
            using var http = await CreateTwitchClient();

            var json = await http.GetStringAsync(
                $"https://api.twitch.tv/helix/channels/ads?broadcaster_id={_broadcasterId}");

            using var doc = JsonDocument.Parse(json);
            var data = doc.RootElement.GetProperty("data");

            if (data.GetArrayLength() == 0)
                return 0;

            return ReadInt(data[0], "snooze_count");
        }

        public async Task<int> SnoozeNextAd()
        {
            using var http = await CreateTwitchClient();

            var response = await http.PostAsync($"https://api.twitch.tv/helix/channels/ads/schedule/snooze?broadcaster_id={_broadcasterId}",
                                                content: null);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var data = doc.RootElement.GetProperty("data");

            return ReadInt(data[0], "snooze_count");
        }

        public async Task<int> GetViewerCount()
        {
            using var http = await CreateTwitchClient();

            var json = await http.GetStringAsync(
                $"https://api.twitch.tv/helix/streams?user_id={_broadcasterId}");

            using var doc = JsonDocument.Parse(json);
            var data = doc.RootElement.GetProperty("data");

            if (data.GetArrayLength() == 0)
                return 0; // offline

            return data[0].GetProperty("viewer_count").GetInt32();
        }

        public async Task<DateTimeOffset?> GetNextAdTime()
        {
            using var http = await CreateTwitchClient();

            var json = await http.GetStringAsync(
                $"https://api.twitch.tv/helix/channels/ads?broadcaster_id={_broadcasterId}");

            using var doc = JsonDocument.Parse(json);
            var data = doc.RootElement.GetProperty("data");

            if (data.GetArrayLength() == 0)
                return null;

            string value = data[0].GetProperty("next_ad_at").ToString();

            if (string.IsNullOrWhiteSpace(value))
                return null;

            // Twitch docs say RFC3339, but handle Unix timestamp too.
            if (long.TryParse(value, out var unixSeconds))
                return DateTimeOffset.FromUnixTimeSeconds(unixSeconds);

            if (DateTimeOffset.TryParse(value, out var parsed))
                return parsed;

            return null;
        }

        public int ReadInt(JsonElement element, string propertyName)
        {
            var prop = element.GetProperty(propertyName);

            return prop.ValueKind switch
            {
                JsonValueKind.Number => prop.GetInt32(),
                JsonValueKind.String => int.TryParse(prop.GetString(), out var value) ? value : 0,
                _ => 0
            };
        }
    }
}
