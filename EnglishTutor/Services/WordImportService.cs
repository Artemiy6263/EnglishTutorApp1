using System.Net.Http;
using EnglishTutor.Data;
using EnglishTutor.Data.Models;
using Newtonsoft.Json.Linq;

namespace EnglishTutor.Services
{
    public class WordImportService
    {
        private static readonly HttpClient _http = new();
        private const string DefaultWordsApiUrl = "https://wordsapiv1.p.rapidapi.com/words";
        private const string DefaultWordsApiHost = "wordsapiv1.p.rapidapi.com";

        public static async Task<List<WordSuggestion>> FetchWordsByTopicAsync(string topic, int maxCount = 30)
        {
            try
            {
                var url = $"https://api.datamuse.com/words?topics={Uri.EscapeDataString(topic)}&max={maxCount}";
                var json = await _http.GetStringAsync(url);
                var items = JArray.Parse(json);
                var result = new List<WordSuggestion>();
                foreach (var item in items)
                {
                    var word = item["word"]?.ToString();
                    if (!string.IsNullOrEmpty(word))
                        result.Add(new WordSuggestion { EnglishWord = word, Score = item["score"]?.Value<int>() ?? 0 });
                }
                return result;
            }
            catch { return new List<WordSuggestion>(); }
        }

        public static async Task<WordSuggestion> EnrichWordAsync(WordSuggestion suggestion)
        {
            var info = await DictionaryApiService.GetWordInfoAsync(suggestion.EnglishWord);
            if (info != null)
            {
                suggestion.Phonetic = info.Phonetic;
                suggestion.Definition = info.Definitions.FirstOrDefault() ?? "";
                suggestion.Example = info.Examples.FirstOrDefault() ?? "";
            }
            return suggestion;
        }

        public static async Task<WordImportResult> ImportWordsFromWordsApiAsync(int categoryId, DifficultyLevel difficulty, int count)
        {
            var apiKey = App.Configuration["ApiKeys:WordsApiKey"]?.Trim();
            if (string.IsNullOrWhiteSpace(apiKey))
                return WordImportResult.Failed("Укажите API ключ WordsAPI в appsettings.json: ApiKeys:WordsApiKey.");

            count = Math.Clamp(count, 1, 500);
            var suggestions = await FetchWordsFromWordsApiAsync(apiKey, count);
            if (suggestions.Count == 0)
                return WordImportResult.Failed("WordsAPI не вернул слова. Проверьте ключ и доступ к API.");

            using var ctx = new AppDbContext();
            var existing = ctx.Words.Select(w => w.EnglishWord.ToLower()).ToHashSet();
            var added = 0;
            var skipped = 0;

            foreach (var suggestion in suggestions)
            {
                var word = suggestion.EnglishWord.Trim().ToLower();
                if (existing.Contains(word))
                {
                    skipped++;
                    continue;
                }

                var enriched = await EnrichWordAsync(suggestion);
                var translation = await TryTranslateToRussianAsync(word);
                ctx.Words.Add(new Word
                {
                    EnglishWord = word,
                    RussianTranslation = string.IsNullOrWhiteSpace(translation) ? enriched.Definition : translation,
                    ExampleSentence = enriched.Example,
                    ExampleTranslation = "",
                    DifficultyLevel = difficulty,
                    CategoryId = categoryId,
                    Transcription = enriched.Phonetic
                });
                existing.Add(word);
                added++;
            }

            ctx.SaveChanges();
            return new WordImportResult { Added = added, Skipped = skipped, Message = $"Добавлено слов: {added}. Пропущено дублей: {skipped}." };
        }

        private static async Task<List<WordSuggestion>> FetchWordsFromWordsApiAsync(string apiKey, int count)
        {
            var result = new List<WordSuggestion>();
            var baseUrl = App.Configuration["ApiKeys:WordsApiBaseUrl"]?.Trim();
            if (string.IsNullOrWhiteSpace(baseUrl)) baseUrl = DefaultWordsApiUrl;

            for (var page = 1; result.Count < count && page <= 20; page++)
            {
                var limit = Math.Min(100, count - result.Count);
                var url = $"{baseUrl}?lettersMin=3&lettersMax=12&limit={limit}&page={page}";
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.TryAddWithoutValidation("X-RapidAPI-Key", apiKey);
                request.Headers.TryAddWithoutValidation("X-Mashape-Key", apiKey);
                request.Headers.TryAddWithoutValidation("X-RapidAPI-Host", App.Configuration["ApiKeys:WordsApiHost"]?.Trim() ?? DefaultWordsApiHost);
                request.Headers.TryAddWithoutValidation("Accept", "application/json");

                using var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode) break;

                var json = await response.Content.ReadAsStringAsync();
                var data = JObject.Parse(json)["results"]?["data"] as JArray;
                if (data == null || data.Count == 0) break;

                foreach (var item in data)
                {
                    var word = item.ToString().Trim();
                    if (word.All(char.IsLetter) && !result.Any(x => x.EnglishWord.Equals(word, StringComparison.OrdinalIgnoreCase)))
                        result.Add(new WordSuggestion { EnglishWord = word });
                    if (result.Count >= count) break;
                }
            }

            return result;
        }

        private static async Task<string> TryTranslateToRussianAsync(string word)
        {
            var key = App.Configuration["ApiKeys:YandexDictionaryKey"]?.Trim();
            if (string.IsNullOrWhiteSpace(key)) return "";

            try
            {
                var url = $"https://dictionary.yandex.net/api/v1/dicservice.json/lookup?key={Uri.EscapeDataString(key)}&lang=en-ru&text={Uri.EscapeDataString(word)}";
                var json = await _http.GetStringAsync(url);
                return JObject.Parse(json)["def"]?.FirstOrDefault()?["tr"]?.FirstOrDefault()?["text"]?.ToString() ?? "";
            }
            catch
            {
                return "";
            }
        }

        public static readonly Dictionary<string, string> AvailableTopics = new()
        {
            { "animals", "Животные" },
            { "food", "Еда" },
            { "travel", "Путешествия" },
            { "technology", "Технологии" },
            { "nature", "Природа" },
            { "sports", "Спорт" },
            { "health", "Здоровье" },
            { "business", "Бизнес" },
            { "education", "Образование" },
            { "home", "Дом" },
            { "clothing", "Одежда" },
            { "weather", "Погода" },
            { "emotions", "Эмоции" },
            { "colors", "Цвета" },
            { "numbers", "Числа" },
        };
    }

    public class WordSuggestion
    {
        public string EnglishWord { get; set; } = string.Empty;
        public string Phonetic { get; set; } = string.Empty;
        public string Definition { get; set; } = string.Empty;
        public string Example { get; set; } = string.Empty;
        public int Score { get; set; }
        // Заполняется вручную администратором
        public string RussianTranslation { get; set; } = string.Empty;
    }

    public class WordImportResult
    {
        public int Added { get; set; }
        public int Skipped { get; set; }
        public string Message { get; set; } = string.Empty;

        public static WordImportResult Failed(string message) => new() { Message = message };
    }
}
