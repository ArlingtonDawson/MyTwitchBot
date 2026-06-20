using System.Text.Json;

public class TwitchOAuth
{
    private readonly string _tokenFile;
    public string ClientId { get { return _clientId; } }
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _scopes;

    public TwitchOAuth(string clientId, string clientSecret, string scopes, string tokenFile)
    {
        _clientId = clientId;
        _clientSecret = clientSecret;
        _scopes = scopes;
        _tokenFile = tokenFile;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        if (File.Exists(_tokenFile))
        {
            var tokens = JsonSerializer.Deserialize<TokenResponse>(
                await File.ReadAllTextAsync(_tokenFile));

            if (tokens != null && !string.IsNullOrWhiteSpace(tokens.refresh_token))
            {
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
        using var http = new HttpClient();

        var deviceCodeValues = new Dictionary<string, string>
        {
            ["client_id"] = _clientId,
            ["scopes"] = _scopes
        };

        var deviceResponse = await http.PostAsync(
            "https://id.twitch.tv/oauth2/device",
            new FormUrlEncodedContent(deviceCodeValues));

        deviceResponse.EnsureSuccessStatusCode();

        var deviceJson = await deviceResponse.Content.ReadAsStringAsync();
        var deviceCode = JsonSerializer.Deserialize<DeviceCodeResponse>(deviceJson)!;

        Console.WriteLine();
        Console.WriteLine($"=== Twitch Authorization Required ({_tokenFile}) ===");
        Console.WriteLine($"Go to: {deviceCode.verification_uri}");
        Console.WriteLine("Log in as the account that should own this token, then authorize.");
        Console.WriteLine("======================================");
        Console.WriteLine();

        var interval = deviceCode.interval > 0 ? deviceCode.interval : 5;
        var deadline = DateTime.UtcNow.AddSeconds(deviceCode.expires_in);

        while (DateTime.UtcNow < deadline)
        {
            await Task.Delay(interval * 1000);

            var tokenValues = new Dictionary<string, string>
            {
                ["client_id"] = _clientId,
                ["device_code"] = deviceCode.device_code,
                ["grant_type"] = "urn:ietf:params:oauth:grant-type:device_code"
            };

            var tokenResponse = await http.PostAsync(
                "https://id.twitch.tv/oauth2/token",
                new FormUrlEncodedContent(tokenValues));

            if (tokenResponse.IsSuccessStatusCode)
            {
                var json = await tokenResponse.Content.ReadAsStringAsync();
                var tokens = await SaveTokenAsync(json);
                Console.WriteLine($"Authorization successful for {_tokenFile}!");

                await Task.Delay(2000);

                return tokens.access_token;
            }

            var errorJson = await tokenResponse.Content.ReadAsStringAsync();
            if (!errorJson.Contains("authorization_pending"))
            {
                throw new Exception($"OAuth device flow failed: {errorJson}");
            }
        }

        throw new Exception("Device code authorization timed out.");
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
            File.Delete(_tokenFile);
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

        await File.WriteAllTextAsync(_tokenFile, savedJson);
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

    private class DeviceCodeResponse
    {
        public string device_code { get; set; } = "";
        public string user_code { get; set; } = "";
        public string verification_uri { get; set; } = "";
        public int expires_in { get; set; }
        public int interval { get; set; }
    }
}