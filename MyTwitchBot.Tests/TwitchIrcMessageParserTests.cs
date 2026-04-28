using System;
using System.Collections.Generic;
using System.Text;

// TwitchIrcMessageParserTests.cs
namespace MyTwitchBot.Tests
{
    public class TwitchIrcMessageParserTests
    {
        // Real Twitch IRC message format for reference:
        // @badges=moderator/1;mod=1 :username!username@username.tmi.twitch.tv PRIVMSG #channel :hello

        private const string FullModMessage =
            "@badges=moderator/1;mod=1;subscriber=0 :testmod!testmod@testmod.tmi.twitch.tv PRIVMSG #channel :!win";

        private const string FullViewerMessage =
            "@badges=;mod=0;subscriber=0 :viewer!viewer@viewer.tmi.twitch.tv PRIVMSG #channel :hello";

        private const string FullBroadcasterMessage =
            "@badges=broadcaster/1;mod=0;subscriber=0 :streamer!streamer@streamer.tmi.twitch.tv PRIVMSG #channel :!win";

        // What gets passed to GetUsername and GetMessageText after Substring
        private const string ModMessageBody =
            ":testmod!testmod@testmod.tmi.twitch.tv PRIVMSG #channel :!win";

        private const string ViewerMessageBody =
            ":viewer!viewer@viewer.tmi.twitch.tv PRIVMSG #channel :hello";

        [Fact]
        public void GetUsername_ReturnsCorrectUsername_ForMod()
        {
            var username = TwitchIrcMessageParser.GetUsername(ModMessageBody);
            Assert.Equal("testmod", username);
        }

        [Fact]
        public void GetUsername_ReturnsCorrectUsername_ForViewer()
        {
            var username = TwitchIrcMessageParser.GetUsername(ViewerMessageBody);
            Assert.Equal("viewer", username);
        }

        [Fact]
        public void GetUsername_ReturnsUnknown_WhenNoExclamation()
        {
            var username = TwitchIrcMessageParser.GetUsername("PING :tmi.twitch.tv");
            Assert.Equal("unknown", username);
        }

        [Fact]
        public void GetMessageText_ReturnsCommandText_ForMod()
        {
            var text = TwitchIrcMessageParser.GetMessageText(ModMessageBody);
            Assert.Equal("!win", text);
        }

        [Fact]
        public void GetMessageText_ReturnsFullMessage_ForViewer()
        {
            var text = TwitchIrcMessageParser.GetMessageText(ViewerMessageBody);
            Assert.Equal("hello", text);
        }

        [Fact]
        public void GetMessageText_ReturnsEmpty_WhenNoColon()
        {
            var text = TwitchIrcMessageParser.GetMessageText("PING tmi.twitch.tv");
            Assert.Equal("", text);
        }

        [Fact]
        public void IsModerator_ReturnsTrue_ForModerator()
        {
            Assert.True(TwitchIrcMessageParser.IsModerator(FullModMessage));
        }

        [Fact]
        public void IsModerator_ReturnsTrue_ForBroadcaster()
        {
            Assert.True(TwitchIrcMessageParser.IsModerator(FullBroadcasterMessage));
        }

        [Fact]
        public void IsModerator_ReturnsFalse_ForViewer()
        {
            Assert.False(TwitchIrcMessageParser.IsModerator(FullViewerMessage));
        }

        [Fact]
        public void IsModerator_ReturnsFalse_WhenMessageDoesNotStartWithAt()
        {
            Assert.False(TwitchIrcMessageParser.IsModerator("PING :tmi.twitch.tv"));
        }
    }
}
