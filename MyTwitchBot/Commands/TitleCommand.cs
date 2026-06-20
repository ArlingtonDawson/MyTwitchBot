using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTwitchBot.Commands
{
    public class TitleCommand : IChatCommand
    {
        public string CommandText => "!title";
        public bool RequiresMod => true;
        public TokenType RequiredToken => TokenType.Broadcaster;
        public IEnumerable<string> RequiredScopes => new[] { "channel:manage:broadcast" };

        public async Task ExecuteAsync(ChatContext context)
        {
            var newTitle = context.LastMessage?
                .Substring(CommandText.Length).Trim();

            if (string.IsNullOrWhiteSpace(newTitle))
            {
                await context.AppClient.SendChatMessageAsync(
                    "Usage: !title <new stream title>");
                return;
            }

            var success = await context.AppClient.UpdateChannelTitleAsync(newTitle);

            if (success)
            {
                await context.AppClient.SendChatMessageAsync(
                    $"✅ Title updated to \"{newTitle}\"!");
            }
            else
            {
                await context.AppClient.SendChatMessageAsync(
                    $"❌ Unable to update title. Please try again.");
            }
        }
    }
}