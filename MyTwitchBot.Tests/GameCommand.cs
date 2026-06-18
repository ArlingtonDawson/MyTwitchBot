// GameCommandTests.cs
using Moq;
using MyTwitchBot.Commands;

namespace MyTwitchBot.Tests
{
    public class GameCommandTests
    {
        private readonly Mock<ITwitchApplicationClient> _mockAppClient;
        private readonly ChatContext _context;
        private readonly GameCommand _command;

        public GameCommandTests()
        {
            _mockAppClient = new Mock<ITwitchApplicationClient>();
            _context = new ChatContext
            {
                AppClient = _mockAppClient.Object,
                ChannelName = "testchannel",
                Username = "testmod"
            };
            _command = new GameCommand();
        }

        [Fact]
        public async Task ExecuteAsync_SendsUsageMessage_WhenNoGameProvided()
        {
            _context.LastMessage = "!game";

            await _command.ExecuteAsync(_context);

            _mockAppClient.Verify(c => c.SendChatMessageAsync(
                It.Is<string>(s => s.Contains("Usage"))), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_SendsUsageMessage_WhenGameIsWhitespace()
        {
            _context.LastMessage = "!game   ";

            await _command.ExecuteAsync(_context);

            _mockAppClient.Verify(c => c.SendChatMessageAsync(
                It.Is<string>(s => s.Contains("Usage"))), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_SendsNotFound_WhenGameDoesNotExist()
        {
            _context.LastMessage = "!game UnknownGame123";

            _mockAppClient
                .Setup(c => c.GetGameIdAsync("UnknownGame123"))
                .ReturnsAsync((string?)null);

            await _command.ExecuteAsync(_context);

            _mockAppClient.Verify(c => c.SendChatMessageAsync(
                It.Is<string>(s => s.Contains("Could not find"))), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_UpdatesGame_WhenGameFound()
        {
            _context.LastMessage = "!game Overwatch";

            _mockAppClient
                .Setup(c => c.GetGameIdAsync("Overwatch"))
                .ReturnsAsync("1234567");

            _mockAppClient
                .Setup(c => c.UpdateChannelGameAsync("1234567"))
                .ReturnsAsync(true);

            await _command.ExecuteAsync(_context);

            _mockAppClient.Verify(c => c.UpdateChannelGameAsync("1234567"), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_SendsSuccessMessage_WhenGameUpdated()
        {
            _context.LastMessage = "!game Overwatch";

            _mockAppClient
                .Setup(c => c.GetGameIdAsync("Overwatch"))
                .ReturnsAsync("1234567");

            _mockAppClient
                .Setup(c => c.UpdateChannelGameAsync("1234567"))
                .ReturnsAsync(true);

            await _command.ExecuteAsync(_context);

            _mockAppClient.Verify(c => c.SendChatMessageAsync(
                It.Is<string>(s => s.Contains("✅") && s.Contains("Overwatch"))), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_SendsFailureMessage_WhenUpdateFails()
        {
            _context.LastMessage = "!game Overwatch";

            _mockAppClient
                .Setup(c => c.GetGameIdAsync("Overwatch"))
                .ReturnsAsync("1234567");

            _mockAppClient
                .Setup(c => c.UpdateChannelGameAsync("1234567"))
                .ReturnsAsync(false);

            await _command.ExecuteAsync(_context);

            _mockAppClient.Verify(c => c.SendChatMessageAsync(
                It.Is<string>(s => s.Contains("❌") && s.Contains("Unable to update"))), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_DoesNotCallUpdate_WhenGameNotFound()
        {
            _context.LastMessage = "!game UnknownGame123";

            _mockAppClient
                .Setup(c => c.GetGameIdAsync("UnknownGame123"))
                .ReturnsAsync((string?)null);

            await _command.ExecuteAsync(_context);

            _mockAppClient.Verify(c => c.UpdateChannelGameAsync(
                It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_PassesFullGameName_WithSpaces()
        {
            _context.LastMessage = "!game League of Legends";

            _mockAppClient
                .Setup(c => c.GetGameIdAsync("League of Legends"))
                .ReturnsAsync("9898");

            _mockAppClient
                .Setup(c => c.UpdateChannelGameAsync("9898"))
                .ReturnsAsync(true);

            await _command.ExecuteAsync(_context);

            _mockAppClient.Verify(c => c.GetGameIdAsync("League of Legends"), Times.Once);
        }
    }
}