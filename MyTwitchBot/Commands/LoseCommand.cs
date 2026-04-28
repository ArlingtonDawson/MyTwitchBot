using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTwitchBot.Commands
{
    public class LoseCommand : LogWriteChatCommandBase
    {
        public override string CommandText => "!lose";

        public override bool RequiresMod => true;

        public override Task ExecuteAsync(ChatContext context)
        {
            context.BackupStreakKeeper = context.StreakKeeper.Clone();
            context.StreakKeeper.Lose();
            LogToFile(context);
            return Task.CompletedTask;
        }
    }
}
