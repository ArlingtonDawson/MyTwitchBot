using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTwitchBot.Commands
{
    public abstract class ChatCommandBase : IChatCommand
    {
        public abstract string CommandText { get; }
        public abstract bool RequiresMod { get; }
        public virtual TokenType RequiredToken => TokenType.Broadcaster;
        public virtual IEnumerable<string> RequiredScopes => Array.Empty<string>();
        public abstract Task ExecuteAsync(ChatContext context);

        protected void LogToFile(ChatContext context)
        {
            string logEntry = $"{context.StreakKeeper.GetScoreLine()}";
            File.WriteAllText("winslosses.log", logEntry + Environment.NewLine);
        }
    }
}
