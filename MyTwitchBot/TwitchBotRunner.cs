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
        private readonly string _channelName;
        private readonly FollowerTracker _followerTracker;

        private TwitchBotRunner(
            ILoggerFactory loggerFactory,
            ChatCommandDispatcher dispatcher,
            AdMonitor adMonitor,
            TwitchEventSubClient eventSubClient,
            ChatContext context,
            FollowerTracker followerTracker,
            string channelName)
        {
            _loggerFactory = loggerFactory;
            _dispatcher = dispatcher;
            _adMonitor = adMonitor;
            _eventSubClient = eventSubClient;
            _context = context;
            _channelName = channelName;
            _followerTracker = followerTracker;

            ILogger<TwitchBotRunner> _logger = _loggerFactory.CreateLogger<TwitchBotRunner>();
        }


        public static async Task<TwitchBotRunner> CreateAsync()
        {
            var config = BuildConfig();
            var loggerFactory = BuildLoggerFactory();

            var sessionLog = new StreamSessionLog();
            var scrollGenerator = new ScrollGenerator();
            var voteManager = new AdVoteManager();
            var quoteRepository = new QuoteRepository();
            await quoteRepository.LoadAsync();

            var dispatcher = BuildDispatcher(voteManager, sessionLog, scrollGenerator, quoteRepository);

            var appClient = await BuildAppClientAsync(config, dispatcher);

            var followerTracker = new FollowerTracker(appClient, sessionLog);
            var adMonitor = new AdMonitor(appClient, voteManager);
            var context = BuildContext(config, appClient);
            var eventSubClient = BuildEventSubClient(appClient, sessionLog, dispatcher, followerTracker, context);

            return new TwitchBotRunner(
                loggerFactory,
                dispatcher,
                adMonitor,
                eventSubClient,
                context,
                followerTracker,
                config["Twitch:Channel"]);
        }

        private static IConfiguration BuildConfig()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddUserSecrets<TwitchBotRunner>()
                .Build();
        }

        private static ILoggerFactory BuildLoggerFactory()
        {
            return LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Debug)
                    .AddConsole()
                    .AddFile("logs/twitchbot-{Date}.txt");
            });
        }

        private static ChatCommandDispatcher BuildDispatcher(
            AdVoteManager voteManager,
            StreamSessionLog sessionLog,
            ScrollGenerator scrollGenerator,
            QuoteRepository quoteRepository)
        {
            var dispatcher = new ChatCommandDispatcher();

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

            return dispatcher;
        }
        private static async Task<TwitchApplicationClient> BuildAppClientAsync(
            IConfiguration config, ChatCommandDispatcher dispatcher)
        {
            var allCommands = dispatcher.GetAllCommands();
            var broadcasterScopes = ScopeBuilder.BuildBroadcasterScopes(allCommands);
            var hasBotAccount = !string.IsNullOrWhiteSpace(config["Twitch:BotID"]);

            var broadcasterOAuth = new TwitchOAuth(
                config["Twitch:AppClientID"],
                config["Twitch:AppSecret"],
                broadcasterScopes,
                "twitch_tokens_broadcaster.json");

            await broadcasterOAuth.GetAccessTokenAsync();

            var botOAuth = broadcasterOAuth;
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

            return new TwitchApplicationClient(
                broadcasterOAuth,
                botOAuth,
                config["Twitch:BroadcasterId"],
                hasBotAccount ? config["Twitch:BotID"] : config["Twitch:BroadcasterId"]);
        }

        private static ChatContext BuildContext(IConfiguration config, TwitchApplicationClient appClient)
        {
            return new ChatContext
            {
                AppClient = appClient,
                ChannelName = config["Twitch:Channel"],
                Username = config["Twitch:Username"],
                StreakKeeper = new StreakKeeper(),
                BackupStreakKeeper = new StreakKeeper()
            };
        }

        private static TwitchEventSubClient BuildEventSubClient(
            TwitchApplicationClient appClient,
            StreamSessionLog sessionLog,
            ChatCommandDispatcher dispatcher,
            FollowerTracker followerTracker,
            ChatContext context)
        {
            return new TwitchEventSubClient(
                appClient,
                sessionLog,
                async (username, isMod, message) =>
                {
                    context.Username = username;
                    context.LastMessage = message;
                    await followerTracker.CheckAndTrackAsync(username);
                    await dispatcher.DispatchAsync(message, isMod, context);
                });
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
