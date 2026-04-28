using MyTwitchBot.Ads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTwitchBot.Commands
{
    public class SkipAdCommand : IChatCommand
    {
        public string CommandText => "!skipAd";
        public bool RequiresMod => false; // any viewer can vote

        private readonly AdVoteManager _voteManager;

        public SkipAdCommand(AdVoteManager voteManager)
        {
            _voteManager = voteManager;
        }

        public Task ExecuteAsync(ChatContext context)
        {
            _voteManager.CastVote(context.Username);
            return Task.CompletedTask;
        }
    }
}
