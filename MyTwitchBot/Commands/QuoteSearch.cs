using MyTwitchBot.Quotes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTwitchBot.Commands
{
    public class QuoteSearchCommand : IChatCommand
    {
        public string CommandText => "!quotesearch";
        public bool RequiresMod => false;

        private readonly QuoteRepository _repository;

        public QuoteSearchCommand(QuoteRepository repository)
        {
            _repository = repository;
        }

        public async Task ExecuteAsync(ChatContext context)
        {
            var input = context.LastMessage?.Substring(CommandText.Length).Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                await context.AppClient.SendChatMessageAsync(
                    "Usage: !quotesearch keyword");
                return;
            }

            if (_repository.Count == 0)
            {
                await context.AppClient.SendChatMessageAsync(
                    "No quotes yet! Quotes can be added with !quoteadd \"quote\" - person");
                return;
            }

            var results = _repository.Search(input);

            if (!results.Any())
            {
                await context.AppClient.SendChatMessageAsync(
                    $"No quotes found matching \"{input}\".");
                return;
            }

            // If only one result just show it
            if (results.Count == 1)
            {
                await context.AppClient.SendChatMessageAsync(
                    results[0].ToDisplayString());
                return;
            }

            // Multiple results — show first match and how many others found
            await context.AppClient.SendChatMessageAsync(
                $"Found {results.Count} quotes matching \"{input}\". " +
                $"First match: {results[0].ToDisplayString()}");
        }
    }
}
