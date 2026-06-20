using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyTwitchBot.Ads;
using MyTwitchBot.Commands;
using MyTwitchBot.EventSub;
using MyTwitchBot.Quotes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace MyTwitchBot
{
    public class TwitchBotRunner : IAsyncDisposable
    {
        private readonly ILogger<TwitchBotRunner> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ChatCommandDispatcher _dispatcher;
        private readonly AdMonitor _adMonitor;
        private readonly TwitchEventSubClient _eventSubClient;
        private readonly ChatContext _context;
        private readonly FollowerTracker _followerTracker;
        private readonly string _channelName;
        private readonly string _oauthToken;

        private TwitchBotRunner(
            ILoggerFactory loggerFactory,
            ChatCommandDispatcher dispatcher,
            AdMonitor adMonitor,
            TwitchEventSubClient eventSubClient,
            ChatContext context,
            FollowerTracker followerTracker,
            string channelName,
            string oauthToken)
        {
            _loggerFactory = loggerFactory;
            _dispatcher = dispatcher;
            _adMonitor = adMonitor;
            _eventSubClient = eventSubClient;
            _context = context;
            _channelName = channelName;
            _oauthToken = oauthToken;
            _followerTracker = followerTracker;

            ILogger<TwitchBotRunner> _logger = _loggerFactory.CreateLogger<TwitchBotRunner>();
        }


        public static async Task<TwitchBotRunner> CreateAsync()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddUserSecrets<TwitchBotRunner>()
                .Build();

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Debug)
                    .AddConsole()
                    .AddFile("logs/twitchbot-{Date}.txt");
            });


           
            var voteManager = new AdVoteManager();

            var sessionLog = new StreamSessionLog();
            var scrollGenerator = new ScrollGenerator();

            var dispatcher = new ChatCommandDispatcher();

            var quoteRepository = new QuoteRepository();
            await quoteRepository.LoadAsync();

            dispatcher.Register(new WinCommand());
            dispatcher.Register(new LoseCommand());
            dispatcher.Register(new UndoWinCommand());
            dispatcher.Register(new UndoLoseCommand());
            dispatcher.Register(new GameCommand());
            dispatcher.Register(new TitleCommand());
            dispatcher.Register(new SkipAdCommand(voteManager));
            dispatcher.Register(new EndStreamCommand(sessionLog, scrollGenerator));
            dispatcher.Register(new QuoteAddCommand(quoteRepository));
            dispatcher.Register(new QuoteCommand(quoteRepository));
            dispatcher.Register(new QuoteSearchCommand(quoteRepository));


            var allCommands = dispatcher.GetAllCommands();
            var broadcasterScopes = ScopeBuilder.BuildBroadcasterScopes(allCommands);

            var hasBotAccount = !string.IsNullOrWhiteSpace(config["Twitch:BotID"]);

            var broadcasterOAuth = new TwitchOAuth(
                config["Twitch:AppClientID"],
                config["Twitch:AppSecret"],
                broadcasterScopes,
                "twitch_tokens_broadcaster.json");

            await broadcasterOAuth.GetAccessTokenAsync();

            TwitchOAuth botOAuth;
            if (hasBotAccount)
            {
                var botScopes = ScopeBuilder.BuildBotScopes(allCommands);
                botOAuth = new TwitchOAuth(
                    config["Twitch:AppClientID"],
                    config["Twitch:AppSecret"],
                    botScopes,
                    "twitch_tokens_bot.json");

                await botOAuth.GetAccessTokenAsync();
            }
            else
            {
                botOAuth = broadcasterOAuth;
            }

            var appClient = new TwitchApplicationClient(
                broadcasterOAuth, hasBotAccount?botOAuth:broadcasterOAuth,
                config["Twitch:BroadcasterId"], hasBotAccount?config["Twitch:BotID"]: config["Twitch:BroadcasterId"]);

            var followerTracker = new FollowerTracker(appClient, sessionLog);
            var adMonitor = new AdMonitor(appClient, voteManager);

            var context = new ChatContext
            {
                AppClient = appClient,
                ChannelName = config["Twitch:Channel"],
                Username = config["Twitch:Username"],
                StreakKeeper = new StreakKeeper(),
                BackupStreakKeeper = new StreakKeeper()
            };

            var eventSubClient = new TwitchEventSubClient(
                appClient,
                sessionLog,
                async (username, isMod, message) =>
                {
                    Console.WriteLine($"DEBUG: user={username} mod={isMod} msg={message}");
                    context.Username = username;
                    context.LastMessage = message;
                    await followerTracker.CheckAndTrackAsync(username);
                    await dispatcher.DispatchAsync(message, isMod, context);
                });

            return new TwitchBotRunner(
                loggerFactory,
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
            using var cts = new CancellationTokenSource();

            await Task.WhenAll(
                _adMonitor.StartAsync(_context, cts.Token),
                _eventSubClient.StartAsync(cts.Token));
        }

        public ValueTask DisposeAsync()
        {

            if (_loggerFactory != null)
                _loggerFactory.Dispose();
            return ValueTask.CompletedTask;

        }
    }
}
