// QuoteRepositoryTests.cs
using MyTwitchBot.Quotes;

namespace MyTwitchBot.Tests
{
    public class QuoteRepositoryTests : IDisposable
    {
        private readonly string _testFilePath;
        private readonly QuoteRepository _repository;

        public QuoteRepositoryTests()
        {
            // Each test gets its own unique file so tests don't interfere
            _testFilePath = $"test_quotes_{Guid.NewGuid()}.json";
            _repository = new QuoteRepository(_testFilePath);
        }

        public void Dispose()
        {
            // Clean up test file after each test
            if (File.Exists(_testFilePath))
                File.Delete(_testFilePath);
        }

        // AddQuoteAsync tests
        [Fact]
        public async Task AddQuoteAsync_AddsQuoteToRepository()
        {
            await _repository.AddQuoteAsync("Never give up", "SDrag");
            Assert.Equal(1, _repository.Count);
        }

        [Fact]
        public async Task AddQuoteAsync_AssignsIncrementingIds()
        {
            await _repository.AddQuoteAsync("Quote one", "SDrag");
            await _repository.AddQuoteAsync("Quote two", "SDrag");
            await _repository.AddQuoteAsync("Quote three", "SDrag");

            var quote = _repository.GetById(3);
            Assert.NotNull(quote);
            Assert.Equal(3, quote.Id);
        }

        [Fact]
        public async Task AddQuoteAsync_DefaultsToTodaysDate_WhenNoDateProvided()
        {
            var quote = await _repository.AddQuoteAsync("Never give up", "SDrag");
            Assert.Equal(DateTime.Now.ToString("yyyy-MM-dd"), quote.Date);
        }

        [Fact]
        public async Task AddQuoteAsync_UsesProvidedDate_WhenDateGiven()
        {
            var quote = await _repository.AddQuoteAsync("Never give up", "SDrag", "2026-01-01");
            Assert.Equal("2026-01-01", quote.Date);
        }

        [Fact]
        public async Task AddQuoteAsync_SavesToDisk()
        {
            await _repository.AddQuoteAsync("Never give up", "SDrag");
            Assert.True(File.Exists(_testFilePath));
        }

        [Fact]
        public async Task AddQuoteAsync_IdsDoNotCollide_AfterHighestId()
        {
            await _repository.AddQuoteAsync("Quote one", "SDrag");
            await _repository.AddQuoteAsync("Quote two", "SDrag");

            var ids = new[]
            {
                _repository.GetById(1)?.Id,
                _repository.GetById(2)?.Id
            };

            Assert.Equal(2, ids.Distinct().Count());
        }

        // LoadAsync tests
        [Fact]
        public async Task LoadAsync_LoadsExistingQuotes()
        {
            await _repository.AddQuoteAsync("Never give up", "SDrag");

            // Create a fresh repository pointing at same file
            var freshRepository = new QuoteRepository(_testFilePath);
            await freshRepository.LoadAsync();

            Assert.Equal(1, freshRepository.Count);
        }

        [Fact]
        public async Task LoadAsync_HandlesNonExistentFile()
        {
            // File doesn't exist yet — should not throw
            var exception = await Record.ExceptionAsync(() => _repository.LoadAsync());
            Assert.Null(exception);
        }

        [Fact]
        public async Task LoadAsync_StartsEmpty_WhenFileDoesNotExist()
        {
            await _repository.LoadAsync();
            Assert.Equal(0, _repository.Count);
        }

        // GetRandom tests
        [Fact]
        public async Task GetRandom_ReturnsNull_WhenNoQuotes()
        {
            var quote = _repository.GetRandom();
            Assert.Null(quote);
        }

        [Fact]
        public async Task GetRandom_ReturnsQuote_WhenQuotesExist()
        {
            await _repository.AddQuoteAsync("Never give up", "SDrag");
            var quote = _repository.GetRandom();
            Assert.NotNull(quote);
        }

        [Fact]
        public async Task GetRandom_ReturnsOnlyQuote_WhenOneExists()
        {
            await _repository.AddQuoteAsync("Never give up", "SDrag");
            var quote = _repository.GetRandom();
            Assert.Equal("Never give up", quote.Text);
        }

        // GetById tests
        [Fact]
        public async Task GetById_ReturnsCorrectQuote()
        {
            await _repository.AddQuoteAsync("Quote one", "SDrag");
            await _repository.AddQuoteAsync("Quote two", "SDrag");

            var quote = _repository.GetById(2);
            Assert.NotNull(quote);
            Assert.Equal("Quote two", quote.Text);
        }

        [Fact]
        public async Task GetById_ReturnsNull_WhenIdDoesNotExist()
        {
            await _repository.AddQuoteAsync("Never give up", "SDrag");
            var quote = _repository.GetById(99);
            Assert.Null(quote);
        }

        // Search tests
        [Fact]
        public async Task Search_FindsQuoteByText()
        {
            await _repository.AddQuoteAsync("Never give up", "SDrag");
            var results = _repository.Search("never");
            Assert.Single(results);
        }

        [Fact]
        public async Task Search_FindsQuoteByPerson()
        {
            await _repository.AddQuoteAsync("Never give up", "SDrag");
            var results = _repository.Search("SDrag");
            Assert.Single(results);
        }

        [Fact]
        public async Task Search_IsCaseInsensitive()
        {
            await _repository.AddQuoteAsync("Never give up", "SDrag");
            var results = _repository.Search("NEVER");
            Assert.Single(results);
        }

        [Fact]
        public async Task Search_ReturnsMultipleMatches()
        {
            await _repository.AddQuoteAsync("Never give up", "SDrag");
            await _repository.AddQuoteAsync("Never stop trying", "SDrag");
            await _repository.AddQuoteAsync("Just keep swimming", "SDrag");

            var results = _repository.Search("never");
            Assert.Equal(2, results.Count);
        }

        [Fact]
        public async Task Search_ReturnsEmpty_WhenNoMatches()
        {
            await _repository.AddQuoteAsync("Never give up", "SDrag");
            var results = _repository.Search("xyz123");
            Assert.Empty(results);
        }

        [Fact]
        public async Task Search_ReturnsEmpty_WhenNoQuotes()
        {
            var results = _repository.Search("never");
            Assert.Empty(results);
        }

        // ToDisplayString tests
        [Fact]
        public async Task ToDisplayString_FormatsCorrectly()
        {
            var quote = await _repository.AddQuoteAsync("Never give up", "SDrag", "2026-01-01");
            Assert.Equal("\"Never give up\" - SDrag 2026-01-01", quote.ToDisplayString());
        }
    }
}