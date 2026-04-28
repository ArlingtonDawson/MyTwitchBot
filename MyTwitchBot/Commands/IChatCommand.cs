using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTwitchBot.Commands
{
    public interface IChatCommand
    {
        string CommandText { get; }
        bool RequiresMod { get; }
        Task ExecuteAsync(ChatContext context);
    }
}
