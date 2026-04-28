# MyTwitchBot 🤖

A feature-rich Twitch chat bot built in C# that helps streamers manage their channel 
with win/loss tracking, ad management, and end-of-stream credits.

---

## Features

- 🎮 **Win/Loss Tracker** — Track your game results live with an OBS overlay
- 🔥 **Streak Detection** — Automatically detects and displays win/loss streaks
- 📺 **Ad Management** — Alerts chat before ads and lets viewers vote to snooze them
- ❤️ **Follower & Subscriber Tracking** — Tracks new followers, subs, and gifters during your stream
- 🎬 **End of Stream Credits** — Generates a cinematic scrolling credits page for OBS
- 🔐 **OAuth Support** — Full Twitch OAuth2 flow with automatic token refresh

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
3. Set **OAuth Redirect URL** to `http://localhost:3000`
4. Set **Category** to `Chat Bot`
5. Copy your **Client ID** and generate a **Client Secret**

### 3. Set up configuration

Open `appsettings.json` and fill in each value:
```json
{
  "Twitch": {
    "AppClientID": "your_app_client_id_here",
    "AppSecret": "your_app_secret_here",
    "OAuthToken": "your_oauth_token_here",
    "Username": "your_bot_username_here",
    "Channel": "your_channel_name_here",
    "BroadcasterId": "your_broadcaster_id_here"
  }
}
```

### 4. Finding your config values

| Config Key | Where to find it |
|---|---|
| `AppClientID` | Twitch Developer Console → Your Application → Client ID |
| `AppSecret` | Twitch Developer Console → Your Application → New Secret |
| `OAuthToken` | Generated when you first run the bot via the OAuth flow |
| `Username` | The Twitch username of your bot account |
| `Channel` | Your channel name — the part after twitch.tv/ (no # needed) |
| `BroadcasterId` | See below |

#### Finding your Broadcaster ID
1. Go to [https://streamscharts.com/tools/convert-username](https://streamscharts.com/tools/convert-username)
2. Enter your channel name
3. Copy the numeric ID

### 5. Run the bot
```bash
dotnet run --project MyTwitchBot
```
The first run will open a browser window for you to authorize the bot with your Twitch account.

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
| `!endstream` | Generates end of stream credits and posts summary to chat |

### Viewer Commands

| Command | Description |
|---|---|
| `!skipAd` | Vote to snooze an upcoming ad (only active during voting window) |

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

## Running the Tests
```bash
dotnet test
```
Or use the **Test Explorer** in Visual Studio (`Test → Run All Tests`).

Current test coverage:

| Test Class | Tests | What's Covered |
|---|---|---|
| `StreakKeeperTests` | 10 | Win/loss counting, streak detection, clone/restore |
| `StreamSessionLogTests` | 9 | Follower/sub/gifter tracking and deduplication |
| `AdVoteManagerTests` | 9 | Vote open/close, deduplication, vote window |
| `TwitchIrcMessageParserTests` | 10 | Username, message and mod parsing |
| `ChatCommandDispatcherTests` | 4 | Command routing, mod checks, case insensitivity |

---

## Project Structure
MyTwitchBot/
├── Ads/
│   ├── AdMonitor.cs              # Background ad timer and vote management
│   └── AdVoteManager.cs          # Tracks viewer votes
├── Commands/
│   ├── IChatCommand.cs           # Command interface
│   ├── ChatCommandBase.cs        # Base class for commands that log to file
│   ├── ChatCommandDispatcher.cs  # Routes chat messages to commands
│   ├── ChatContext.cs            # Shared state passed to commands
│   ├── WinCommand.cs
│   ├── LoseCommand.cs
│   ├── UndoWinCommand.cs
│   ├── UndoLoseCommand.cs
│   ├── SkipAdCommand.cs
│   └── EndStreamCommand.cs
├── EventSub/
│   ├── TwitchEventSubClient.cs   # WebSocket connection to Twitch EventSub
│   ├── StreamSessionLog.cs       # Collects followers/subs during stream
│   ├── ScrollGenerator.cs        # Writes endstream.json
│   ├── scroll.html               # Credits page template
│   ├── scroll.css                # Credits styling — customize me!
│   └── scroll.js                 # Credits logic and animation
├── TwitchBotRunner.cs            # Wires everything together
├── TwitchIrcClient.cs            # Twitch IRC connection
├── TwitchIrcMessageParser.cs     # Parses IRC message format
├── TwitchApplicationClient.cs    # Twitch Helix API client
├── TwitchOAuth.cs                # OAuth2 flow and token refresh
├── StreakKeeper.cs               # Win/loss streak tracking
└── Program.cs                    # Entry point (just 2 lines!)

---

## Adding New Commands

Adding a new chat command never requires modifying existing code:

1. Create a new class in the `Commands/` folder
2. Implement `IChatCommand`
3. Register it in `TwitchBotRunner.Create()`

```csharp
// Example: Commands/UptimeCommand.cs
public class UptimeCommand : IChatCommand
{
    public string CommandText => "!uptime";
    public bool RequiresMod => false;

    public async Task ExecuteAsync(ChatContext context)
    {
        await context.IrcClient.SendMessageAsync(
            context.ChannelName, "Stream uptime goes here!");
    }
}

// In TwitchBotRunner.Create():
dispatcher.Register(new UptimeCommand());
```

---

## Contributing

Pull requests are welcome! Please make sure all tests pass before submitting:
```bash
dotnet test
```

---

## License

MIT License — feel free to use this as a starting point for your own Twitch bot!