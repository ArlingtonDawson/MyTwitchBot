using MyTwitchBot.EventSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTwitchBot
{
    public class FollowerTracker
    {
        private readonly TwitchApplicationClient _appClient;
        private readonly StreamSessionLog _sessionLog;
        private readonly HashSet<string> _checkedUsers = new();

        public FollowerTracker(TwitchApplicationClient appClient,
            StreamSessionLog sessionLog)
        {
            _appClient = appClient;
            _sessionLog = sessionLog;
        }

        public async Task CheckAndTrackAsync(string username)
        {
            // Only check each user once per stream
            if (_checkedUsers.Contains(username.ToLower())) return;
            if (_sessionLog.IsBanned(username)) return;

            _checkedUsers.Add(username.ToLower());

            var isFollower = await _appClient.IsFollowerAsync(username);
            if (isFollower)
                _sessionLog.AddReturningViewer(username);
        }
    }

}
