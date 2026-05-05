using Microsoft.Extensions.Configuration;
using MyTwitchBot.Ads;
using MyTwitchBot.Commands;
using MyTwitchBot.EventSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTwitchBot
{
    public class TwitchBotRunner
    {
        private readonly TwitchIrcClient _ircClient;
        private readonly ChatCommandDispatcher _dispatcher;
        private readonly AdMonitor _adMonitor;
        private readonly TwitchEventSubClient _eventSubClient;
        private readonly ChatContext _context;
        private readonly FollowerTracker _followerTracker;
        private readonly string _channelName;
        private readonly string _oauthToken;

        private TwitchBotRunner(
            TwitchIrcClient ircClient,
            ChatCommandDispatcher dispatcher,
            AdMonitor adMonitor,
            TwitchEventSubClient eventSubClient,
            ChatContext context,
            FollowerTracker followerTracker,
            string channelName,
            string oauthToken)
        {
            _ircClient = ircClient;
            _dispatcher = dispatcher;
            _adMonitor = adMonitor;
            _eventSubClient = eventSubClient;
            _context = context;
            _channelName = channelName;
            _oauthToken = oauthToken;
            _followerTracker = followerTracker;
        }


        public static TwitchBotRunner Create()
        {
            var config = new ConfigurationBuilder()
                .AddUserSecrets<TwitchBotRunner>()
                .Build();

            var twitchOAuth = new TwitchOAuth(
                config["Twitch:AppClientID"],
                config["Twitch:AppSecret"]);

            var appClient = new TwitchApplicationClient(twitchOAuth, config["Twitch:BroadcasterId"]);

            var voteManager = new AdVoteManager();
            var adMonitor = new AdMonitor(appClient, voteManager);

            var sessionLog = new StreamSessionLog();
            var scrollGenerator = new ScrollGenerator();
            var eventSubClient = new TwitchEventSubClient(appClient, sessionLog);
            var followerTracker = new FollowerTracker(appClient, sessionLog);

            var dispatcher = new ChatCommandDispatcher();
            dispatcher.Register(new WinCommand());
            dispatcher.Register(new LoseCommand());
            dispatcher.Register(new UndoWinCommand());
            dispatcher.Register(new UndoLoseCommand());
            dispatcher.Register(new SkipAdCommand(voteManager));
            dispatcher.Register(new EndStreamCommand(sessionLog, scrollGenerator));

            var ircClient = new TwitchIrcClient();

            var context = new ChatContext
            {
                IrcClient = ircClient,
                ChannelName = config["Twitch:Channel"],
                Username = config["Twitch:Username"],
                StreakKeeper = new StreakKeeper(),
                BackupStreakKeeper = new StreakKeeper()
            };

            return new TwitchBotRunner(
                ircClient,
                dispatcher,
                adMonitor,
                eventSubClient,
                context,
                followerTracker,
                config["Twitch:Channel"],
                config["Twitch:OAuthToken"]);
        }

        public async Task RunAsync()
        {
            await _ircClient.ConnectAsync(
                _context.Username,
                _oauthToken,
                _channelName);

            using var cts = new CancellationTokenSource();

            await Task.WhenAll(
                RunChatLoopAsync(cts.Token),
                _adMonitor.StartAsync(_context, cts.Token),
                _eventSubClient.StartAsync(cts.Token));
        }

        private async Task RunChatLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                string message = await _ircClient.ReadMessageAsync();
                if (message == null) break;

                Console.WriteLine(message);

                if (message.Contains("PING"))
                    await _ircClient.PongAsync();

                if (message.Contains("PRIVMSG"))
                {
                    string messageBody = message.Substring(message.IndexOf(':'));
                    _context.Username = TwitchIrcMessageParser.GetUsername(messageBody);
                    string chatMessage = TwitchIrcMessageParser.GetMessageText(messageBody);
                    bool isMod = TwitchIrcMessageParser.IsModerator(message);

                    await _followerTracker.CheckAndTrackAsync(_context.Username);

                    await _dispatcher.DispatchAsync(chatMessage, isMod, _context);
                }
            }
        }
    }
}
