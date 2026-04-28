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

        public IReadOnlyList<string> NewFollowers => _newFollowers.AsReadOnly();
        public IReadOnlyList<string> NewSubscribers => _newSubscribers.AsReadOnly();
        public IReadOnlyDictionary<string, int> Gifters => _gifters.AsReadOnly();

        public void AddFollower(string username)
        {
            if (!_newFollowers.Contains(username))
                _newFollowers.Add(username);
        }

        public void AddSubscriber(string username)
        {
            if (!_newSubscribers.Contains(username))
                _newSubscribers.Add(username);
        }

        public void AddGifter(string username, int giftCount)
        {
            if (_gifters.ContainsKey(username))
                _gifters[username] += giftCount;
            else
                _gifters[username] = giftCount;
        }
    }
}
