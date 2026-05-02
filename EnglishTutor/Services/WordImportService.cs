using System.Net.Http;
using EnglishTutor.Data.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EnglishTutor.Services
{
    public class WordImportService
    {
        private static readonly HttpClient _http = new();

        // Получить слова по теме через Datamuse API (бесплатно, без ключа)
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

        // Получить детали слова через Free Dictionary API
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

        // Доступные темы для импорта
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
}
