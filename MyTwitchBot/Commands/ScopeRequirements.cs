using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTwitchBot.Commands
{
    public static class ScopeRequirements
    {
        public static readonly string[] CoreBroadcasterScopes = new[]
{
            "moderator:read:followers",
            "channel:read:subscriptions",
            "moderator:read:banned_users",
            "channel:moderate",
            "channel:bot",
            "user:read:chat",
            "user:write:chat"
        };

        public static readonly string[] CoreBotScopes = new[]
{
            "user:bot",
            "user:read:chat",
            "user:write:chat"
        };
    }
}
