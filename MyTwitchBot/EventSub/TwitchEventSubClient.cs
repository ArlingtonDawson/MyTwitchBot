using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyTwitchBot.EventSub
{
    public class TwitchEventSubClient
    {
        private const string EventSubUrl = "wss://eventsub.wss.twitch.tv/ws";

        private readonly TwitchApplicationClient _appClient;
        private readonly StreamSessionLog _sessionLog;
        private readonly ClientWebSocket _webSocket = new();

        public TwitchEventSubClient(TwitchApplicationClient appClient, StreamSessionLog sessionLog)
        {
            _appClient = appClient;
            _sessionLog = sessionLog;
        }

        public async Task StartAsync(CancellationToken ct)
        {
            await _webSocket.ConnectAsync(new Uri(EventSubUrl), ct);
            Console.WriteLine("Connected to Twitch EventSub.");

            await ReceiveLoopAsync(ct);
        }

        private async Task ReceiveLoopAsync(CancellationToken ct)
        {
            var buffer = new byte[4096];

            while (!ct.IsCancellationRequested)
            {
                var result = await _webSocket.ReceiveAsync(buffer, ct);
                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

                using var doc = JsonDocument.Parse(json);
                var messageType = doc.RootElement
                    .GetProperty("metadata")
                    .GetProperty("message_type")
                    .GetString();

                switch (messageType)
                {
                    case "session_welcome":
                        await HandleWelcomeAsync(doc);
                        break;

                    case "session_keepalive":
                        Console.WriteLine("EventSub keepalive received.");
                        break;

                    case "notification":
                        HandleNotification(doc);
                        break;
                }
            }
        }

        private async Task HandleWelcomeAsync(JsonDocument doc)
        {
            var sessionId = doc.RootElement
                .GetProperty("payload")
                .GetProperty("session")
                .GetProperty("id")
                .GetString();

            Console.WriteLine($"EventSub session established: {sessionId}");

            // Now subscribe to the events we care about
            await _appClient.SubscribeToFollowsAsync(sessionId);
            await _appClient.SubscribeToSubscriptionsAsync(sessionId);
            await _appClient.SubscribeToGiftSubscriptionsAsync(sessionId);
        }

        private void HandleNotification(JsonDocument doc)
        {
            var subscriptionType = doc.RootElement
                .GetProperty("metadata")
                .GetProperty("subscription_type")
                .GetString();

            var eventData = doc.RootElement.GetProperty("payload").GetProperty("event");

            switch (subscriptionType)
            {
                case "channel.follow":
                    var follower = eventData.GetProperty("user_name").GetString();
                    _sessionLog.AddFollower(follower);
                    Console.WriteLine($"New follower: {follower}");
                    break;

                case "channel.subscribe":
                    var subscriber = eventData.GetProperty("user_name").GetString();
                    _sessionLog.AddSubscriber(subscriber);
                    Console.WriteLine($"New subscriber: {subscriber}");
                    break;

                case "channel.subscription.gift":
                    var gifter = eventData.GetProperty("user_name").GetString();
                    var giftCount = eventData.GetProperty("total").GetInt32();
                    _sessionLog.AddGifter(gifter, giftCount);
                    Console.WriteLine($"Gift sub: {gifter} x{giftCount}");
                    break;
            }
        }
    }
}
