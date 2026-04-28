using MyTwitchBot.EventSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTwitchBot.Commands
{
    public class EndStreamCommand : IChatCommand
    {
        public string CommandText => "!endstream";
        public bool RequiresMod => true;

        private readonly StreamSessionLog _sessionLog;
        private readonly ScrollGenerator _scrollGenerator;

        public EndStreamCommand(StreamSessionLog sessionLog, ScrollGenerator scrollGenerator)
        {
            _sessionLog = sessionLog;
            _scrollGenerator = scrollGenerator;
        }

        public async Task ExecuteAsync(ChatContext context)
        {
            await context.IrcClient.SendMessageAsync(
                context.ChannelName,
                "Thanks for watching! Generating end stream credits...");

            await _scrollGenerator.GenerateAsync(_sessionLog);

            await context.IrcClient.SendMessageAsync(
                context.ChannelName,
                $"Credits generated! {_sessionLog.NewFollowers.Count} followers, " +
                $"{_sessionLog.NewSubscribers.Count} subscribers, " +
                $"{_sessionLog.Gifters.Count} gifters. See you next time! 👋");
        }
    }
}
