using MyTwitchBot.Quotes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTwitchBot.Commands
{
    public class QuoteCommand : IChatCommand
    {
        public string CommandText => "!quote";
        public bool RequiresMod => false;

        private readonly QuoteRepository _repository;

        public QuoteCommand(QuoteRepository repository)
        {
            _repository = repository;
        }

        public async Task ExecuteAsync(ChatContext context)
        {
            if (_repository.Count == 0)
            {
                await context.AppClient.SendChatMessageAsync(
                    "No quotes yet! Quotes can be added with !quoteadd \"quote\" - person");
                return;
            }

            // Check if a number was provided e.g. !quote 5
            var input = context.LastMessage?.Substring(CommandText.Length).Trim();

            if (!string.IsNullOrWhiteSpace(input) && int.TryParse(input, out int id))
            {
                var specificQuote = _repository.GetById(id);

                if (specificQuote == null)
                {
                    await context.AppClient.SendChatMessageAsync(
                        $"No quote found with ID {id}.");
                    return;
                }

                await context.AppClient.SendChatMessageAsync(
                    specificQuote.ToDisplayString());
                return;
            }

            // No number provided — pick a random quote
            var quote = _repository.GetRandom();
            await context.AppClient.SendChatMessageAsync(
                quote.ToDisplayString());
        }
    }
}
