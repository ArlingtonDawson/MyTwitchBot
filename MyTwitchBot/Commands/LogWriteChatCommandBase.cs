using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTwitchBot.Commands
{
    public abstract class LogWriteChatCommandBase : IChatCommand
    {
        public abstract string CommandText { get; }
        public abstract bool RequiresMod { get; }
        public abstract Task ExecuteAsync(ChatContext context);

        protected void LogToFile(ChatContext context)
        {
            string logEntry = $"{context.StreakKeeper.GetScoreLine}";
            File.WriteAllText("winslosses.log", logEntry + Environment.NewLine);
        }
    }
}
