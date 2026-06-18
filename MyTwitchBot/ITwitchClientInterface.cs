using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTwitchBot
{
    public interface ITwitchApplicationClient
    {
        Task SendChatMessageAsync(string message);
        Task<string?> GetGameIdAsync(string gameName);
        Task<bool> UpdateChannelGameAsync(string gameId);
        Task<int> GetViewerCount();
        Task<DateTimeOffset?> GetNextAdTime();
        Task<int> GetAdSnoozeCount();
        Task<int> SnoozeNextAd();
        Task<bool> IsFollowerAsync(string username);
        Task SubscribeToFollowsAsync(string sessionId);
        Task SubscribeToSubscriptionsAsync(string sessionId);
        Task SubscribeToGiftSubscriptionsAsync(string sessionId);
        Task SubscribeToBansAsync(string sessionId);
        Task SubscribeToChatMessagesAsync(string sessionId);
    }
}
