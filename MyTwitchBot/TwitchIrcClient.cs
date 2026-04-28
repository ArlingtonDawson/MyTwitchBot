using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MyTwitchBot
{
    public class TwitchIrcClient : IDisposable
    {
        private const string TwitchIrcHost = "irc.chat.twitch.tv";
        private const int TwitchIrcPort = 6667;

        private TcpClient _tcpClient;
        private StreamReader _reader;
        private StreamWriter _writer;

        public async Task ConnectAsync(string username, string oauthToken, string channelName)
        {
            _tcpClient = new TcpClient(TwitchIrcHost, TwitchIrcPort);
            var stream = _tcpClient.GetStream();
            _reader = new StreamReader(stream);
            _writer = new StreamWriter(stream) { AutoFlush = true };

            await _writer.WriteLineAsync($"PASS oauth:{oauthToken}");
            await _writer.WriteLineAsync($"NICK {username}");
            await _writer.WriteLineAsync("CAP REQ :twitch.tv/tags");
            await _writer.WriteLineAsync($"JOIN #{channelName}");

            Console.WriteLine($"Connected to Twitch chat as {username}.");
        }

        public async Task<string> ReadMessageAsync()
        {
            return await _reader.ReadLineAsync();
        }

        public async Task SendMessageAsync(string channelName, string message)
        {
            await _writer.WriteLineAsync($"PRIVMSG #{channelName} :{message}");
        }

        public async Task PongAsync()
        {
            await _writer.WriteLineAsync("PONG :tmi.twitch.tv");
        }

        public void Dispose()
        {
            _writer?.Dispose();
            _reader?.Dispose();
            _tcpClient?.Dispose();
        }
    }
}
