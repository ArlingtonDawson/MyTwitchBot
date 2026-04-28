using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTwitchBot.Commands
{
    public class  UndoWinCommand: LogWriteChatCommandBase
    {
        public override string CommandText => "!-win";

        public override bool RequiresMod => true;

        public override Task ExecuteAsync(ChatContext context)
        {
            context.StreakKeeper = context.BackupStreakKeeper;
            LogToFile(context);
            return Task.CompletedTask;
        }
    }
}
