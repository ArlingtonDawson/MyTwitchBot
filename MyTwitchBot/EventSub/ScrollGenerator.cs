using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MyTwitchBot.EventSub
{
    public class ScrollGenerator
    {
        private readonly string _outputPath;

        public ScrollGenerator(string outputPath = "endstream.json")
        {
            _outputPath = outputPath;
        }

        public async Task GenerateAsync(StreamSessionLog log)
        {
            var data = new
            {
                subscribers = log.NewSubscribers,
                gifters = log.Gifters
                    .OrderByDescending(g => g.Value)
                    .Select(g => new { username = g.Key, count = g.Value }),
                followers = log.NewFollowers
            };

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(_outputPath, json);
            Console.WriteLine($"Scroll data written to {_outputPath}");
        }
    }
}

