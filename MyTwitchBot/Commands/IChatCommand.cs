using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTwitchBot.Commands
{
    public enum TokenType
    {
        Broadcaster,
        Bot
    }

    public interface IChatCommand
    {
        string CommandText { get; }
        bool RequiresMod { get; }
        TokenType RequiredToken { get; }
        IEnumerable<string> RequiredScopes { get; }
        Task ExecuteAsync(ChatContext context);
    }
}
