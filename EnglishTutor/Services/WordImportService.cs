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

            count = Math.Clamp(count, 1, 5000);
            var suggestions = await FetchWordsFromWordsApiAsync(apiKey, count);
            if (suggestions.Count == 0)
                return WordImportResult.Failed("WordsAPI не вернул слова. Проверьте ключ и доступ к API.");

            using var ctx = new AppDbContext();
            var existing = ctx.Words.ToDictionary(w => w.EnglishWord.ToLower());
            var wordsForLesson = new List<Word>();
            var enrichLimit = GetEnrichLimit();
            var added = 0;
            var skipped = 0;
            var enrichedCount = 0;

            foreach (var suggestion in suggestions)
            {
                var word = NormalizeWord(suggestion.EnglishWord);
                if (string.IsNullOrEmpty(word))
                    continue;

                if (existing.TryGetValue(word, out var existingWord))
                {
                    skipped++;
                    if (existingWord.CategoryId == categoryId)
                        wordsForLesson.Add(existingWord);
                    continue;
                }

                var enriched = enrichedCount < enrichLimit ? await EnrichWordAsync(suggestion) : suggestion;
                if (enrichedCount < enrichLimit) enrichedCount++;
                var translation = await TryTranslateToRussianAsync(word);
                var newWord = new Word
                {
                    EnglishWord = word,
                    RussianTranslation = Truncate(string.IsNullOrWhiteSpace(translation) ? enriched.Definition : translation, 200, "перевод не указан"),
                    ExampleSentence = Truncate(enriched.Example, 500),
                    ExampleTranslation = "",
                    DifficultyLevel = difficulty,
                    CategoryId = categoryId,
                    Transcription = enriched.Phonetic
                };
                ctx.Words.Add(newWord);
                existing.Add(word, newWord);
                wordsForLesson.Add(newWord);
                added++;
            }

            ctx.SaveChanges();
            var linked = LinkWordsToLesson(ctx, categoryId, wordsForLesson);
            return new WordImportResult { Added = added, Skipped = skipped, LinkedToLesson = linked, Message = $"Добавлено слов: {added}. Пропущено дублей: {skipped}. Добавлено в урок: {linked}." };
        }

        private static async Task<List<WordSuggestion>> FetchWordsFromWordsApiAsync(string apiKey, int count)
        {
            var result = new List<WordSuggestion>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var baseUrl = App.Configuration["ApiKeys:WordsApiBaseUrl"]?.Trim();
            if (string.IsNullOrWhiteSpace(baseUrl)) baseUrl = DefaultWordsApiUrl;

            var partsOfSpeech = new[] { "noun", "verb", "adjective", "adverb", "" };
            var letterRanges = new[] { (3, 5), (6, 8), (9, 12) };
            foreach (var partOfSpeech in partsOfSpeech)
            {
                foreach (var (lettersMin, lettersMax) in letterRanges)
                {
                    if (result.Count >= count) break;
                    for (var page = 1; result.Count < count && page <= 100; page++)
                    {
                        var limit = Math.Min(100, count - result.Count);
                        var url = $"{baseUrl}?lettersMin={lettersMin}&lettersMax={lettersMax}&limit={limit}&page={page}";
                        if (!string.IsNullOrWhiteSpace(partOfSpeech))
                            url += $"&partOfSpeech={Uri.EscapeDataString(partOfSpeech)}";

                        using var request = new HttpRequestMessage(HttpMethod.Get, url);
                        request.Headers.TryAddWithoutValidation("X-RapidAPI-Key", apiKey);
                        request.Headers.TryAddWithoutValidation("X-Mashape-Key", apiKey);
                        request.Headers.TryAddWithoutValidation("X-RapidAPI-Host", App.Configuration["ApiKeys:WordsApiHost"]?.Trim() ?? DefaultWordsApiHost);
                        request.Headers.TryAddWithoutValidation("Accept", "application/json");

                        using var response = await _http.SendAsync(request);
                        if (!response.IsSuccessStatusCode) break;

                        var json = await response.Content.ReadAsStringAsync();
                        var words = ParseWordsApiResponse(json);
                        if (words.Count == 0) break;

                        var beforePage = result.Count;
                        foreach (var rawWord in words)
                        {
                            var word = NormalizeWord(rawWord);
                            if (string.IsNullOrEmpty(word) || !seen.Add(word))
                                continue;

                            result.Add(new WordSuggestion { EnglishWord = word });
                            if (result.Count >= count) break;
                        }

                        if (result.Count == beforePage) break;
                    }
                }
            }

            return result;
        }

        private static List<string> ParseWordsApiResponse(string json)
        {
            var token = JToken.Parse(json);
            if (token is JArray array)
                return array.Select(item => item.ToString()).ToList();

            var data = token["results"]?["data"] as JArray ?? token["data"] as JArray;
            if (data != null)
                return data.Select(item => item.ToString()).ToList();

            var word = token["word"]?.ToString();
            return string.IsNullOrWhiteSpace(word) ? new List<string>() : new List<string> { word };
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

        private static int LinkWordsToLesson(AppDbContext ctx, int categoryId, List<Word> words)
        {
            if (words.Count == 0) return 0;

            var category = ctx.WordCategories.Find(categoryId);
            if (category == null) return 0;

            var lesson = FindOrCreateLessonForCategory(ctx, category);
            var existingWordIds = ctx.LessonWords
                .Where(lw => lw.LessonId == lesson.LessonId)
                .Select(lw => lw.WordId)
                .ToHashSet();
            var nextOrder = ctx.LessonWords
                .Where(lw => lw.LessonId == lesson.LessonId)
                .Select(lw => lw.OrderIndex)
                .DefaultIfEmpty(0)
                .Max() + 1;
            var linked = 0;

            foreach (var word in words.Where(w => w.WordId > 0).DistinctBy(w => w.WordId))
            {
                if (!existingWordIds.Add(word.WordId))
                    continue;

                ctx.LessonWords.Add(new LessonWord
                {
                    LessonId = lesson.LessonId,
                    WordId = word.WordId,
                    OrderIndex = nextOrder++
                });
                linked++;
            }

            ctx.SaveChanges();
            return linked;
        }

        private static Lesson FindOrCreateLessonForCategory(AppDbContext ctx, WordCategory category)
        {
            var title = GetLessonTitle(category.Name);
            var lesson = ctx.Lessons.FirstOrDefault(l => l.Title == title)
                ?? ctx.Lessons.FirstOrDefault(l => l.Title.Contains(category.Name))
                ?? ctx.Lessons.FirstOrDefault(l => category.Name.Contains(l.Title));
            if (lesson != null) return lesson;

            var nextOrder = ctx.Lessons.Select(l => l.OrderNumber).DefaultIfEmpty(0).Max() + 1;
            lesson = new Lesson
            {
                Title = title,
                Description = $"Слова по теме: {category.Description}",
                DifficultyLevel = DifficultyLevel.Easy,
                OrderNumber = nextOrder,
                IconEmoji = category.IconEmoji,
                IsActive = true
            };
            ctx.Lessons.Add(lesson);
            ctx.SaveChanges();
            return lesson;
        }

        private static string GetLessonTitle(string categoryName) => categoryName switch
        {
            "Animals" => "Basic Animals",
            "Food & Drink" => "Food & Kitchen",
            "Travel" => "Travel Essentials",
            "Technology" => "Technology World",
            "Nature" => "Nature & Environment",
            "Body & Health" => "Body & Health",
            "Home" => "Home & Living",
            "Work & Business" => "Work & Business",
            "Sports" => "Sports & Fitness",
            "Education" => "Education",
            _ => categoryName
        };

        private static string NormalizeWord(string word)
        {
            var normalized = word.Trim().ToLower();
            return normalized.All(ch => char.IsLetter(ch) || ch == '-' || ch == '\'') ? normalized : "";
        }

        private static string Truncate(string value, int maxLength, string fallback = "")
        {
            var text = string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
            return text.Length <= maxLength ? text : text[..maxLength];
        }

        private static int GetEnrichLimit()
        {
            var configured = App.Configuration["ApiKeys:EnrichImportedWordsLimit"]?.Trim();
            return int.TryParse(configured, out var limit) ? Math.Clamp(limit, 0, 5000) : 200;
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
        public int LinkedToLesson { get; set; }
        public string Message { get; set; } = string.Empty;

        public static WordImportResult Failed(string message) => new() { Message = message };
    }
}
