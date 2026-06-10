using MyTwitchBot.Quotes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTwitchBot.Commands
{
    public class QuoteAddCommand : IChatCommand
    {
        public string CommandText => "!quoteadd";
        public bool RequiresMod => false;

        private readonly QuoteRepository _repository;

        public QuoteAddCommand(QuoteRepository repository)
        {
            _repository = repository;
        }

        public async Task ExecuteAsync(ChatContext context)
        {
            // Expected format: !quoteadd "quote text" - person 2026-06-01
            var input = context.LastMessage?.Substring(CommandText.Length).Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                await context.AppClient.SendChatMessageAsync(
                    "Usage: !quoteadd \"quote text\" - person [date]");
                return;
            }

            // Extract quoted text
            var quoteStart = input.IndexOf('"');
            var quoteEnd = input.IndexOf('"', quoteStart + 1);

            if (quoteStart == -1 || quoteEnd == -1)
            {
                await context.AppClient.SendChatMessageAsync(
                    "Usage: !quoteadd \"quote text\" - person [date]");
                return;
            }

            var quoteText = input.Substring(quoteStart + 1, quoteEnd - quoteStart - 1).Trim();

            // Extract everything after the closing quote
            var remainder = input.Substring(quoteEnd + 1).Trim();

            // Remove leading dash if present
            if (remainder.StartsWith("-"))
                remainder = remainder.Substring(1).Trim();

            // Split remainder into person and optional date
            var parts = remainder.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
            {
                await context.AppClient.SendChatMessageAsync(
                    "Usage: !quoteadd \"quote text\" - person [date]");
                return;
            }

            var person = parts[0];
            var date = parts.Length > 1 ? parts[1] : null;

            var quote = await _repository.AddQuoteAsync(quoteText, person, date);

            await context.AppClient.SendChatMessageAsync(
                $"Quote added! {quote.ToDisplayString()}");
        }
    }
}
