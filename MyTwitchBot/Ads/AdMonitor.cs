using MyTwitchBot.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTwitchBot.Ads
{
    public class AdMonitor
    {
        private readonly TwitchApplicationClient _appClient;
        private readonly AdVoteManager _voteManager;
        private readonly int _warningSeconds;
        private readonly int _votingWindowSeconds;
        private bool _warningFired = false;
        private bool _votingHappened = false;

        public AdMonitor(TwitchApplicationClient appClient, AdVoteManager voteManager,
    int warningSeconds = 120, int votingWindowSeconds = 60)
        {
            _appClient = appClient;
            _voteManager = voteManager;
            _warningSeconds = warningSeconds;
            _votingWindowSeconds = votingWindowSeconds;
        }

        public async Task StartAsync(ChatContext context, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(30_000, ct); // poll every 30 seconds

                var nextAd = await _appClient.GetNextAdTime();
                if (nextAd == null) continue;

                var secondsUntilAd = (nextAd.Value - DateTimeOffset.UtcNow).TotalSeconds;
                if (secondsUntilAd < 0) continue;

                if (secondsUntilAd <= _warningSeconds && !_warningFired)
                {
                    _warningFired = true;
                    _votingHappened = false;

                    var snoozeCount = await _appClient.GetAdSnoozeCount();

                    if (snoozeCount > 0)
                    {
                        // Alert chat and open the vote
                        await context.IrcClient.SendMessageAsync(context.ChannelName,
                            $"⚠️ Ad in ~{(int)secondsUntilAd}s! " +
                            $"Type !skipAd to vote to snooze it! ({snoozeCount} snooze(s) remaining)");

                        _voteManager.OpenVote();

                        // Start the voting window in the background
                        _ = RunVotingWindowAsync(context, ct);
                    }
                    else
                    {
                        await context.IrcClient.SendMessageAsync(context.ChannelName,
                            $"⚠️ Ad in ~{(int)secondsUntilAd}s! No snoozes remaining.");
                    }
                }

                // Reset warning flag after ad has passed
                if (_votingHappened == true && secondsUntilAd > _warningSeconds)
                {
                    _warningFired = false;
                    _votingHappened = false;
                }
            }
        }

        private async Task RunVotingWindowAsync(ChatContext context, CancellationToken ct)
        {
            await Task.Delay(_votingWindowSeconds * 1000, ct);

            _voteManager.CloseVote();

            var viewers = await _appClient.GetViewerCount();
            var threshold = viewers / 2;

            if (threshold == 0) threshold++;

            if (_voteManager.VoteCount >= threshold)
            {
                var remaining = await _appClient.SnoozeNextAd();
                await context.IrcClient.SendMessageAsync(context.ChannelName,
                    $"✅ Vote passed ({_voteManager.VoteCount}/{viewers} viewers)! " +
                    $"Ad snoozed. Snoozes remaining: {remaining}");
            }
            else
            {
                await context.IrcClient.SendMessageAsync(context.ChannelName,
                    $"❌ Vote failed ({_voteManager.VoteCount}/{viewers} viewers voted). Ad will play.");
            }

            _votingHappened = true;
        }
    }
}
