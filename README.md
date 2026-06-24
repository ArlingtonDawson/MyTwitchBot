# MyTwitchBot 🤖

A feature-rich Twitch chat bot built in C# that helps streamers manage their channel
with win/loss tracking, ad management, quotes, and end-of-stream credits.

---

## Features

- 🎮 **Win/Loss Tracker** — Track your game results live with an OBS overlay
- 🔥 **Streak Detection** — Automatically detects and displays win/loss streaks
- 📺 **Ad Management** — Alerts chat before ads and lets viewers vote to snooze them
- ❤️ **Follower & Subscriber Tracking** — Tracks new followers, subs, and gifters during your stream
- 👀 **Returning Viewer Credits** — Followers who chat during the stream are included in end credits
- 🎬 **End of Stream Credits** — Generates a cinematic scrolling credits page for OBS
- 💬 **Quote System** — Add, retrieve, and search quotes from chatA
- 🎮 **Channel Management** — Change game and stream title directly from chat
- 🔐 **Dual OAuth Support** — Separate broadcaster and bot account tokens via Device Code Flow
- 🔌 **Plugin-Ready Architecture** — Commands are self-describing with scopes and token types built in
- 📝 **Logging** — Rolling daily log files for debugging

---

## Requirements

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- [Visual Studio 2022+](https://visualstudio.microsoft.com/) or any C# IDE
- A [Twitch Developer Account](https://dev.twitch.tv/)
- A Twitch account for your bot (can be your own account or a dedicated bot account)

---

## Getting Started

### 1. Clone the repository
```bash
git clone https://github.com/ArlingtonDawson/MyTwitchBot.git
cd MyTwitchBot
```

### 2. Create your Twitch Application
1. Go to the [Twitch Developer Console](https://dev.twitch.tv/console)
2. Click **Register Your Application**
3. Set **OAuth Redirect URL** to `http://localhost:3000/`
4. Set **Category** to `Chat Bot`
5. Copy your **Client ID** and generate a **Client Secret**

### 3. Set up configuration
Copy the example config file and fill in your values:
```bash
cp appsettings.example.json appsettings.json
```

Open `appsettings.json` and fill in each value:
```json
{
  "Twitch": {
    "AppClientID": "your_app_client_id_here",
    "AppSecret": "your_app_secret_here",
    "Username": "your_bot_username_here",
    "Channel": "your_channel_name_here",
    "BroadcasterId": "your_broadcaster_id_here",
    "BotID": "your_bot_user_id_here"
  },
  "QuotesFile": "quotes.json"
}
```

> 💡 **No bot account?** Remove or leave `BotID` empty — the bot will use your broadcaster account for everything automatically.

### 4. Finding your config values

| Config Key | Where to find it |
|---|---|
| `AppClientID` | Twitch Developer Console → Your Application → Client ID |
| `AppSecret` | Twitch Developer Console → Your Application → New Secret |
| `Username` | The Twitch username of your bot account |
| `Channel` | Your channel name — the part after twitch.tv/ (no # needed) |
| `BroadcasterId` | See Finding Your Broadcaster ID below |
| `BotID` | See Finding Your Bot User ID below (optional) |

#### Finding your Broadcaster ID / Bot User ID
1. Go to [https://www.streamweasels.com/tools/convert-twitch-username-to-user-id/](https://www.streamweasels.com/tools/convert-twitch-username-to-user-id/)
2. Enter your channel name (for broadcaster) or bot account username (for bot)
3. Copy the numeric ID

### 5. Grant your bot account permissions
If using a separate bot account, make it a moderator in your Twitch chat:
```
/mod YourBotUsername
```
This gives it the permissions needed for EventSub subscriptions.

### 6. Run the bot
```bash
dotnet run --project MyTwitchBot
```

On first run the bot uses **Device Code Flow** — no browser redirects or port conflicts. You'll see:

```
=== Twitch Authorization Required (twitch_tokens_broadcaster.json) ===
Go to: https://www.twitch.tv/activate?device-code=XXXXXXXX
Log in as the account that should own this token, then authorize.
======================================

Authorization successful for twitch_tokens_broadcaster.json!

=== Twitch Authorization Required (twitch_tokens_bot.json) ===
Go to: https://www.twitch.tv/activate?device-code=XXXXXXXX
Log in as the account that should own this token, then authorize.
======================================

Authorization successful for twitch_tokens_bot.json!
```

Visit the URL on **any device or browser** — your phone, a different browser profile, anywhere — and log in as the appropriate account. No localhost ports, no browser session conflicts.

After first authorization, tokens are saved and refreshed automatically on future runs.

---

## Chat Commands

> 💡 All commands are case insensitive — `!Win`, `!WIN` and `!win` all work.

### Mod Only Commands

| Command | Description |
|---|---|
| `!win` | Records a win and updates the overlay |
| `!lose` | Records a loss and updates the overlay |
| `!-win` | Undoes the last win |
| `!-lose` | Undoes the last loss |
| `!game <name>` | Updates the channel's current game |
| `!title <text>` | Updates the stream title |
| `!endstream` | Generates end of stream credits and posts summary to chat |
| `!quoteadd "quote" - person [date]` | Adds a new quote. Date defaults to today if omitted |

### Viewer Commands

| Command | Description |
|---|---|
| `!skipAd` | Vote to snooze an upcoming ad (only active during voting window) |
| `!quote` | Displays a random quote |
| `!quote 5` | Displays quote number 5 |
| `!quotesearch keyword` | Searches quotes by text or person name |

### Command Examples
```
!game Overwatch                              → ✅ Game updated to Overwatch!
!game UnknownGame                            → ❌ Could not find game "UnknownGame" on Twitch
!title Grinding ranked until I rage quit     → ✅ Title updated!
!quoteadd "Never give up" - SDrag 2026-06-01 ← with date
!quoteadd "Never give up" - SDrag            ← defaults to today's date
!quote                                        ← random quote
!quote 3                                      ← quote number 3
!quotesearch never                            ← search by text
!quotesearch SDrag                            ← search by person
```

---

## OBS Setup

### Win/Loss Overlay
1. In OBS add a **Text (GDI+)** source
2. Check **Read from file**
3. Point it at `winslosses.log` in the bot output directory
4. The overlay updates automatically when `!win` or `!lose` is used

### End of Stream Credits
1. Add a **Browser Source** to your end screen scene
2. Check **Local File**
3. Point it at `scroll.html` in the bot output directory
4. Set resolution to match your stream (e.g. 1920x1080)
5. Check **Refresh browser when scene becomes active**

The credits scroll automatically and include:
- ⭐ New Subscribers
- 🎁 Gift Subscribers (sorted by gift count)
- ❤️ New Followers
- 👀 Thanks For Watching (followers who chatted during the stream)

---

## Customizing the Credits

The end of stream credits are fully customizable without touching any C# code:

| File | What to customize |
|---|---|
| `scroll.css` | Colors, fonts, sizing. Swap `#9147ff` for your brand color |
| `scroll.html` | Base HTML structure and layout |
| `scroll.js` | Section order and scroll speed |

To adjust scroll speed edit this line in `scroll.js`:
```javascript
const duration = Math.min(30 + (totalEntries * 2), 180);
```
- Increase the `2` to scroll slower
- Decrease the `2` to scroll faster
- The `180` is the maximum duration in seconds

---

## Ad Vote System

When an ad is approaching the bot will:
1. Post a warning to chat with time remaining
2. Open a `!skipAd` vote for 60 seconds
3. Check if snooze credits are available
4. If more than 50% of current viewers vote — the ad is automatically snoozed
5. Post the result to chat either way

---

## Dual Token System

The bot supports two separate Twitch accounts with different responsibilities:

| Token | Account | Purpose |
|---|---|---|
| `twitch_tokens_broadcaster.json` | Your broadcaster account | Ads, subs, follows, bans, game/title updates |
| `twitch_tokens_bot.json` | Your bot account (optional) | Sending chat messages, reading chat via EventSub |

If no `BotID` is configured, both token files consolidate to a single broadcaster token — everything works the same, messages just send as you.

OAuth scopes are built **dynamically** from whatever commands are registered — add a new command with new scope requirements and the bot requests exactly the scopes it needs, nothing more.

---

## Logging

The bot writes rolling daily log files to the `logs/` folder:
```
logs/twitchbot-20260610.txt
```

Log files are useful for diagnosing crashes or unexpected behavior. The `logs/` folder is excluded from Git so your logs stay local.

---

## Running the Tests
```bash
dotnet test
```
Or use the **Test Explorer** in Visual Studio (`Test → Run All Tests`).

Current test coverage:

| Test Class | Tests | What's Covered |
|---|---|---|
| `StreakKeeperTests` | 10 | Win/loss counting, streak detection, clone/restore |
| `StreamSessionLogTests` | 18 | Follower/sub/gifter tracking, deduplication, ban scrubbing, returning viewers |
| `AdVoteManagerTests` | 9 | Vote open/close, deduplication, vote window |
| `ChatCommandDispatcherTests` | 4 | Command routing, mod checks, case insensitivity, argument parsing |
| `QuoteRepositoryTests` | 22 | Add, load, random, get by id, search, persistence |
| `GameCommandTests` | 8 | Game lookup, update success/failure, spaces in game names |
| `TitleCommandTests` | 5 | Title update success/failure, usage message |

---

## Project Structure

```
MyTwitchBot/
├── Ads/
│   ├── AdMonitor.cs              # Background ad timer and vote management
│   └── AdVoteManager.cs          # Tracks viewer votes
├── Commands/
│   ├── IChatCommand.cs           # Command interface (includes TokenType + RequiredScopes)
│   ├── ChatCommandBase.cs        # Base class with shared LogToFile logic
│   ├── ChatCommandDispatcher.cs  # Routes chat messages to commands
│   ├── ChatContext.cs            # Shared state passed to commands
│   ├── ScopeRequirements.cs      # Core broadcaster and bot OAuth scopes
│   ├── ScopeBuilder.cs           # Builds scope strings dynamically from registered commands
│   ├── WinCommand.cs
│   ├── LoseCommand.cs
│   ├── UndoWinCommand.cs
│   ├── UndoLoseCommand.cs
│   ├── SkipAdCommand.cs
│   ├── EndStreamCommand.cs
│   ├── GameCommand.cs            # !game
│   ├── TitleCommand.cs           # !title
│   ├── QuoteAddCommand.cs        # !quoteadd
│   ├── QuoteCommand.cs           # !quote and !quote N
│   └── QuoteSearch.cs            # !quotesearch
├── EventSub/
│   ├── TwitchEventSubClient.cs   # WebSocket connection to Twitch EventSub
│   ├── StreamSessionLog.cs       # Collects followers/subs/bans during stream
│   ├── ScrollGenerator.cs        # Writes endstream.json
│   ├── scroll.html               # Credits page template
│   ├── scroll.css                # Credits styling — customize me!
│   ├── scroll.js                 # Credits logic and animation
│   └── README.md                 # Credits customization guide
├── Quotes/
│   ├── Quote.cs                  # Quote model
│   └── QuoteRepository.cs        # Load, save, and query quotes.json
├── TwitchBotRunner.cs            # Wires everything together
├── TwitchApplicationClient.cs    # Twitch Helix API + EventSub subscription client
├── ITwitchClientInterface.cs     # Interface for mocking in tests
├── TwitchOAuth.cs                # Device Code Flow OAuth with automatic token refresh
├── FollowerTracker.cs            # Tracks followers who chat during stream
├── StreakKeeper.cs               # Win/loss streak tracking
└── Program.cs                    # Entry point (just 2 lines!)
```

---

## Adding New Commands

Adding a new chat command never requires modifying existing code.

1. Create a new class in the `Commands/` folder
2. Implement `IChatCommand` — declare your scopes and token type
3. Register it in `TwitchBotRunner.BuildDispatcher()`

```csharp
// Example: Commands/UptimeCommand.cs
public class UptimeCommand : IChatCommand
{
    public string CommandText => "!uptime";
    public bool RequiresMod => false;
    public TokenType RequiredToken => TokenType.Broadcaster;
    public IEnumerable<string> RequiredScopes => Array.Empty<string>();

    public async Task ExecuteAsync(ChatContext context)
    {
        await context.AppClient.SendChatMessageAsync("Stream uptime goes here!");
    }
}

// In TwitchBotRunner.BuildDispatcher():
dispatcher.Register(new UptimeCommand());
```

The bot automatically includes any scopes declared in `RequiredScopes` when building the OAuth token — no manual scope string editing needed.

---

## OAuth Scopes

Scopes are built dynamically at startup based on registered commands plus these core scopes:

### Broadcaster Token (always)
| Scope | Purpose |
|---|---|
| `moderator:read:followers` | Follow events via EventSub |
| `channel:read:subscriptions` | Subscriber events via EventSub |
| `moderator:read:banned_users` | Ban events via EventSub |
| `channel:moderate` | Channel moderation actions |
| `channel:bot` | Authorize bot to chat on channel |
| `user:read:chat` | Read chat via EventSub |
| `user:write:chat` | Send chat messages |
| `channel:read:ads` | Read ad schedule *(added by SkipAdCommand)* |
| `channel:manage:ads` | Snooze ads *(added by SkipAdCommand)* |
| `channel:manage:broadcast` | Update game and title *(added by GameCommand/TitleCommand)* |

### Bot Token (only when BotID is configured)
| Scope | Purpose |
|---|---|
| `user:bot` | Identify as a bot user |
| `user:read:chat` | Read chat via EventSub |
| `user:write:chat` | Send chat messages as the bot |

---

## Troubleshooting

**401 errors when sending messages**
Make sure your bot account has been modded (`/mod YourBotUsername`) and that the bot token was authorized by the bot account (not the broadcaster). Delete `twitch_tokens_bot.json` and restart to re-authenticate as the bot.

**403 errors on EventSub subscriptions**
Usually means a missing scope or the wrong account authorized the token. Delete the relevant token file (`twitch_tokens_broadcaster.json` or `twitch_tokens_bot.json`) and restart to re-authenticate. Make sure your bot account is modded on the channel.

**Subscriptions fail on first run but work after restart**
This was a known timing issue where EventSub subscriptions fired before the fresh token was fully ready. Fixed in the current version by eagerly initializing both tokens during startup before any EventSub connections are made.

**Wrong account authorized during Device Code Flow**
Visit the activation URL on any device/browser where the correct account is already logged in — your phone works great for this. The Device Code Flow isn't tied to any specific browser session on your PC.

**Missing subscriptions on startup**
Check the console for `!!!! FAILED: channel.X` messages — these show which subscriptions failed and why. Usually a scope or account mismatch.

---

## Contributing

Pull requests are welcome! Please make sure all tests pass before submitting:
```bash
dotnet test
```

---

## License

MIT License — feel free to use this as a starting point for your own Twitch bot!
