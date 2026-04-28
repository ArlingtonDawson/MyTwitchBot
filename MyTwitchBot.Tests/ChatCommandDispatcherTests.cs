using MyTwitchBot.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyTwitchBot.Tests
{
    public class ChatCommandDispatcherTests
    {
        [Fact]
        public async Task Dispatch_IsCaseInsensitive()
        {
            var dispatcher = new ChatCommandDispatcher();
            var executed = false;

            dispatcher.Register(new TestCommand("!win", () => executed = true));

            await dispatcher.DispatchAsync("!WIN", true, new ChatContext());
            Assert.True(executed);
        }

        [Fact]
        public async Task Dispatch_DoesNotExecute_WhenCommandUnknown()
        {
            var dispatcher = new ChatCommandDispatcher();
            var executed = false;

            dispatcher.Register(new TestCommand("!win", () => executed = true));

            await dispatcher.DispatchAsync("!lose", true, new ChatContext());
            Assert.False(executed);
        }

        [Fact]
        public async Task Dispatch_DoesNotExecute_WhenUserIsNotMod()
        {
            var dispatcher = new ChatCommandDispatcher();
            var executed = false;

            dispatcher.Register(new TestCommand("!win", () => executed = true, requiresMod: true));

            await dispatcher.DispatchAsync("!win", false, new ChatContext());
            Assert.False(executed);
        }

        [Fact]
        public async Task Dispatch_Executes_WhenModCommandAndUserIsMod()
        {
            var dispatcher = new ChatCommandDispatcher();
            var executed = false;

            dispatcher.Register(new TestCommand("!win", () => executed = true, requiresMod: true));

            await dispatcher.DispatchAsync("!win", true, new ChatContext());
            Assert.True(executed);
        }

        // Helper class for testing
        private class TestCommand : IChatCommand
        {
            private readonly Action _action;
            public string CommandText { get; }
            public bool RequiresMod { get; }

            public TestCommand(string commandText, Action action, bool requiresMod = false)
            {
                CommandText = commandText;
                _action = action;
                RequiresMod = requiresMod;
            }

            public Task ExecuteAsync(ChatContext context)
            {
                _action();
                return Task.CompletedTask;
            }
        }
    }
}
