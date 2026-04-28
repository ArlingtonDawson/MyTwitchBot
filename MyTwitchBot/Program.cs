using Microsoft.Extensions.Configuration;
using MyTwitchBot;
using MyTwitchBot.Ads;
using MyTwitchBot.Commands;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

internal class Program
{        
    private static async Task Main(string[] args)
    {
        var bot = TwitchBotRunner.Create();
        await bot.RunAsync();
    }
}