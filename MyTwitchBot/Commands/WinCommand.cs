using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTwitchBot.Commands
{
    public class WinCommand : LogWriteChatCommandBase
    {
        public override string CommandText => "!win";

        public override bool RequiresMod => true;

        public override Task ExecuteAsync(ChatContext context)
        {
            context.BackupStreakKeeper = context.StreakKeeper.Clone();
            context.StreakKeeper.Win();
            LogToFile(context);
            return Task.CompletedTask;
        }
    }
}
