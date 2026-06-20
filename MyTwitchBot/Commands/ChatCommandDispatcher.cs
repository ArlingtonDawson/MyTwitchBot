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

        public IEnumerable<IChatCommand> GetAllCommands() => _commands.Values;

        public async Task DispatchAsync(string messageText, bool isMod, ChatContext context)
        {
            var commandText = messageText.Split(' ')[0].ToLower();

            if (!_commands.TryGetValue(commandText, out var command))
                return;

            if (command.RequiresMod && !isMod)
                return;

            await command.ExecuteAsync(context);
        }
    }
}
