using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTwitchBot.Ads
{
    public class AdVoteManager
    {
        private readonly HashSet<string> _voters = new();
        public bool VoteOpen { get; private set; } = false;
        public int VoteCount => _voters.Count;

        public void OpenVote()
        {
            _voters.Clear();
            VoteOpen = true;
        }

        public void CloseVote() => VoteOpen = false;

        public void CastVote(string username)
        {
            if (VoteOpen) _voters.Add(username); // HashSet dedupes naturally
        }


    }
}
