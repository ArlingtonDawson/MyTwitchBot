using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTwitchBot.Commands
{
    public class ChatCommandDispatcher
    {
        private readonly Dictionary<string, IChatCommand> _commands = new();

        public void Register(IChatCommand command)
        {
            _commands[command.CommandText.ToLower()] = command;
        }

        public async Task DispatchAsync(string commandText, bool isMod, ChatContext context)
        {
            if (!_commands.TryGetValue(commandText.ToLower().Trim(), out var command))
                return; // unknown command, ignore

            if (command.RequiresMod && !isMod)
                return; // not authorized

            await command.ExecuteAsync(context);
        }
    }
}
