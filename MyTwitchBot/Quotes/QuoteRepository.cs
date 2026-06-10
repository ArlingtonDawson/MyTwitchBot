using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyTwitchBot.Quotes
    {
        public class QuoteRepository
        {
            private readonly string _filePath;
            private List<Quote> _quotes = new();
            private static readonly JsonSerializerOptions _jsonOptions = new()
            {
                WriteIndented = true
            };

            public QuoteRepository(string filePath = "quotes.json")
            {
                _filePath = filePath;
            }

            public async Task LoadAsync()
            {
                if (!File.Exists(_filePath))
                {
                    _quotes = new List<Quote>();
                    return;
                }

                var json = await File.ReadAllTextAsync(_filePath);
                _quotes = JsonSerializer.Deserialize<List<Quote>>(json) ?? new List<Quote>();
            }

            public async Task<Quote> AddQuoteAsync(string text, string person, string? date = null)
            {
                var quote = new Quote
                {
                    Id = _quotes.Count > 0 ? _quotes.Max(q => q.Id) + 1 : 1,
                    Text = text,
                    Person = person,
                    Date = date ?? DateTime.Now.ToString("yyyy-MM-dd")
                };

                _quotes.Add(quote);
                await SaveAsync();
                return quote;
            }

            public Quote? GetRandom()
            {
                if (!_quotes.Any()) return null;
                var index = Random.Shared.Next(_quotes.Count);
                return _quotes[index];
            }

            public Quote? GetById(int id)
            {
                return _quotes.FirstOrDefault(q => q.Id == id);
            }

            public List<Quote> Search(string keyword)
            {
                var lower = keyword.ToLower();
                return _quotes
                    .Where(q => q.Text.ToLower().Contains(lower) ||
                                q.Person.ToLower().Contains(lower))
                    .ToList();
            }

            public int Count => _quotes.Count;

            private async Task SaveAsync()
            {
                var json = JsonSerializer.Serialize(_quotes, _jsonOptions);
                await File.WriteAllTextAsync(_filePath, json);
            }
        }
    }
