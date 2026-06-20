using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTwitchBot.Commands
{
    public static class ScopeBuilder
    {
        public static string BuildBroadcasterScopes(IEnumerable<IChatCommand> commands)
        {
            var scopes = new HashSet<string>(ScopeRequirements.CoreBroadcasterScopes);

            foreach (var command in commands)
            {
                if (command.RequiredToken == TokenType.Broadcaster)
                {
                    foreach (var scope in command.RequiredScopes)
                        scopes.Add(scope);
                }
            }

            return string.Join(" ", scopes);
        }

        public static string BuildBotScopes(IEnumerable<IChatCommand> commands)
        {
            var scopes = new HashSet<string>(ScopeRequirements.CoreBotScopes);

            foreach (var command in commands)
            {
                if (command.RequiredToken == TokenType.Bot)
                {
                    foreach (var scope in command.RequiredScopes)
                        scopes.Add(scope);
                }
            }

            return string.Join(" ", scopes);
        }
    }
}
