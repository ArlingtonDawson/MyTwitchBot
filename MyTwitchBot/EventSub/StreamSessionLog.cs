using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTwitchBot.EventSub
{
    public class StreamSessionLog
    {
        private readonly List<string> _newFollowers = new();
        private readonly List<string> _newSubscribers = new();
        private readonly Dictionary<string, int> _gifters = new();
        private readonly HashSet<string> _bannedUsers = new();
        private readonly List<string> _returningViewers = new();

        public IReadOnlyList<string> NewFollowers => _newFollowers.AsReadOnly();
        public IReadOnlyList<string> NewSubscribers => _newSubscribers.AsReadOnly();
        public IReadOnlyDictionary<string, int> Gifters => _gifters.AsReadOnly();
        public bool IsBanned(string username) =>_bannedUsers.Contains(username.ToLower());
        public IReadOnlyList<string> ReturningViewers => _returningViewers.AsReadOnly();


        public void AddFollower(string username)
        {
            if (IsBanned(username)) return;
            if (!_newFollowers.Contains(username))
                _newFollowers.Add(username);
        }

        public void AddSubscriber(string username)
        {
            if (IsBanned(username)) return;
            if (!_newSubscribers.Contains(username))
                _newSubscribers.Add(username);
        }

        public void AddGifter(string username, int giftCount)
        {
            if (IsBanned(username)) return;
            if (_gifters.ContainsKey(username))
                _gifters[username] += giftCount;
            else
                _gifters[username] = giftCount;
        }
        public void AddReturningViewer(string username)
        {
            if (IsBanned(username)) return;

            // Don't add if they're already a new follower
            if (_newFollowers.Any(f => f.ToLower() == username.ToLower())) return;

            if (!_returningViewers.Any(v => v.ToLower() == username.ToLower()))
                _returningViewers.Add(username);
        }

        public void BanUser(string username)
        {
            _bannedUsers.Add(username.ToLower());

            // Clean up any existing entries
            _newFollowers.RemoveAll(f => f.ToLower() == username.ToLower());
            _newSubscribers.RemoveAll(s => s.ToLower() == username.ToLower());
            _gifters.Remove(username);
        }
    }
}
