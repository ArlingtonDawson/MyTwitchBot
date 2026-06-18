using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Commands/GameCommand.cs
namespace MyTwitchBot.Commands
{
    public class GameCommand : IChatCommand
    {
        public string CommandText => "!game";
        public bool RequiresMod => true;

        public async Task ExecuteAsync(ChatContext context)
        {
            var gameName = context.LastMessage?
                .Substring(CommandText.Length).Trim();

            if (string.IsNullOrWhiteSpace(gameName))
            {
                await context.AppClient.SendChatMessageAsync(
                    "Usage: !game <game name>");
                return;
            }

            // Look up the game ID
            var gameId = await context.AppClient.GetGameIdAsync(gameName);

            if (gameId == null)
            {
                await context.AppClient.SendChatMessageAsync(
                    $"❌ Could not find game \"{gameName}\" on Twitch. " +
                    $"Check the spelling and try again!");
                return;
            }

            // Update the channel
            var success = await context.AppClient.UpdateChannelGameAsync(gameId);

            if (success)
            {
                await context.AppClient.SendChatMessageAsync(
                    $"✅ Game updated to {gameName}!");
            }
            else
            {
                await context.AppClient.SendChatMessageAsync(
                    $"❌ Unable to update game to {gameName}. Please try again.");
            }
        }
    }
}
