using System;
using System.Collections.Generic;
using System.Text;

// TitleCommandTests.cs
using Moq;
using MyTwitchBot.Commands;

namespace MyTwitchBot.Tests
{
    public class TitleCommandTests
    {
        private readonly Mock<ITwitchApplicationClient> _mockAppClient;
        private readonly ChatContext _context;
        private readonly TitleCommand _command;

        public TitleCommandTests()
        {
            _mockAppClient = new Mock<ITwitchApplicationClient>();
            _context = new ChatContext
            {
                AppClient = _mockAppClient.Object,
                ChannelName = "testchannel",
                Username = "testmod"
            };
            _command = new TitleCommand();
        }

        [Fact]
        public async Task ExecuteAsync_SendsUsageMessage_WhenNoTitleProvided()
        {
            _context.LastMessage = "!title";

            await _command.ExecuteAsync(_context);

            _mockAppClient.Verify(c => c.SendChatMessageAsync(
                It.Is<string>(s => s.Contains("Usage"))), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_SendsUsageMessage_WhenTitleIsWhitespace()
        {
            _context.LastMessage = "!title    ";

            await _command.ExecuteAsync(_context);

            _mockAppClient.Verify(c => c.SendChatMessageAsync(
                It.Is<string>(s => s.Contains("Usage"))), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_CallsUpdateWithFullTitle()
        {
            _context.LastMessage = "!title Grinding ranked tonight";

            _mockAppClient
                .Setup(c => c.UpdateChannelTitleAsync("Grinding ranked tonight"))
                .ReturnsAsync(true);

            await _command.ExecuteAsync(_context);

            _mockAppClient.Verify(c => c.UpdateChannelTitleAsync(
                "Grinding ranked tonight"), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_SendsSuccessMessage_WhenTitleUpdated()
        {
            _context.LastMessage = "!title New Title";

            _mockAppClient
                .Setup(c => c.UpdateChannelTitleAsync("New Title"))
                .ReturnsAsync(true);

            await _command.ExecuteAsync(_context);

            _mockAppClient.Verify(c => c.SendChatMessageAsync(
                It.Is<string>(s => s.Contains("✅") && s.Contains("New Title"))), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_SendsFailureMessage_WhenUpdateFails()
        {
            _context.LastMessage = "!title New Title";

            _mockAppClient
                .Setup(c => c.UpdateChannelTitleAsync("New Title"))
                .ReturnsAsync(false);

            await _command.ExecuteAsync(_context);

            _mockAppClient.Verify(c => c.SendChatMessageAsync(
                It.Is<string>(s => s.Contains("❌"))), Times.Once);
        }
    }
}