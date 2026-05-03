using System.Net.Http;
using System.Text;
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
        private const string DefaultDatamuseUrl = "https://api.datamuse.com/words";
        private static readonly Dictionary<string, string> LocalTranslations = new(StringComparer.OrdinalIgnoreCase)
        {
            ["cat"] = "кот", ["dog"] = "собака", ["bird"] = "птица", ["horse"] = "лошадь", ["cow"] = "корова", ["pig"] = "свинья", ["sheep"] = "овца", ["goat"] = "коза", ["lion"] = "лев", ["tiger"] = "тигр", ["bear"] = "медведь", ["wolf"] = "волк", ["fox"] = "лиса", ["rabbit"] = "кролик", ["monkey"] = "обезьяна", ["snake"] = "змея", ["elephant"] = "слон", ["giraffe"] = "жираф",
            ["food"] = "еда", ["water"] = "вода", ["bread"] = "хлеб", ["milk"] = "молоко", ["meat"] = "мясо", ["fish"] = "рыба", ["rice"] = "рис", ["soup"] = "суп", ["salad"] = "салат", ["apple"] = "яблоко", ["banana"] = "банан", ["orange"] = "апельсин", ["potato"] = "картофель", ["tomato"] = "помидор", ["cheese"] = "сыр", ["egg"] = "яйцо", ["tea"] = "чай", ["coffee"] = "кофе", ["juice"] = "сок", ["restaurant"] = "ресторан",
            ["travel"] = "путешествие", ["airport"] = "аэропорт", ["hotel"] = "отель", ["ticket"] = "билет", ["passport"] = "паспорт", ["train"] = "поезд", ["bus"] = "автобус", ["car"] = "машина", ["taxi"] = "такси", ["plane"] = "самолёт", ["ship"] = "корабль", ["road"] = "дорога", ["map"] = "карта", ["city"] = "город", ["country"] = "страна", ["beach"] = "пляж", ["station"] = "станция", ["luggage"] = "багаж", ["trip"] = "поездка", ["tour"] = "тур",
            ["computer"] = "компьютер", ["phone"] = "телефон", ["internet"] = "интернет", ["software"] = "программа", ["hardware"] = "оборудование", ["keyboard"] = "клавиатура", ["mouse"] = "мышь", ["screen"] = "экран", ["network"] = "сеть", ["server"] = "сервер", ["data"] = "данные", ["file"] = "файл", ["folder"] = "папка", ["code"] = "код", ["program"] = "программа", ["device"] = "устройство", ["battery"] = "батарея", ["camera"] = "камера", ["robot"] = "робот", ["science"] = "наука",
            ["body"] = "тело", ["head"] = "голова", ["hand"] = "рука", ["arm"] = "рука", ["leg"] = "нога", ["foot"] = "ступня", ["eye"] = "глаз", ["ear"] = "ухо", ["nose"] = "нос", ["mouth"] = "рот", ["tooth"] = "зуб", ["heart"] = "сердце", ["health"] = "здоровье", ["doctor"] = "врач", ["hospital"] = "больница", ["medicine"] = "лекарство", ["pain"] = "боль", ["blood"] = "кровь", ["skin"] = "кожа", ["exercise"] = "упражнение",
            ["nature"] = "природа", ["tree"] = "дерево", ["flower"] = "цветок", ["grass"] = "трава", ["forest"] = "лес", ["river"] = "река", ["lake"] = "озеро", ["sea"] = "море", ["ocean"] = "океан", ["mountain"] = "гора", ["weather"] = "погода", ["rain"] = "дождь", ["snow"] = "снег", ["wind"] = "ветер", ["sun"] = "солнце", ["moon"] = "луна", ["star"] = "звезда", ["sky"] = "небо", ["earth"] = "земля", ["plant"] = "растение",
            ["home"] = "дом", ["house"] = "дом", ["room"] = "комната", ["kitchen"] = "кухня", ["bathroom"] = "ванная", ["bedroom"] = "спальня", ["door"] = "дверь", ["window"] = "окно", ["table"] = "стол", ["chair"] = "стул", ["bed"] = "кровать", ["sofa"] = "диван", ["floor"] = "пол", ["wall"] = "стена", ["family"] = "семья", ["mother"] = "мама", ["father"] = "папа", ["brother"] = "брат", ["sister"] = "сестра", ["apartment"] = "квартира",
            ["work"] = "работа", ["business"] = "бизнес", ["office"] = "офис", ["job"] = "работа", ["company"] = "компания", ["manager"] = "менеджер", ["money"] = "деньги", ["bank"] = "банк", ["meeting"] = "встреча", ["project"] = "проект", ["career"] = "карьера", ["client"] = "клиент", ["market"] = "рынок", ["price"] = "цена", ["sale"] = "продажа", ["shop"] = "магазин", ["email"] = "электронная почта", ["document"] = "документ", ["contract"] = "контракт",
            ["sport"] = "спорт", ["sports"] = "спорт", ["football"] = "футбол", ["basketball"] = "баскетбол", ["tennis"] = "теннис", ["game"] = "игра", ["player"] = "игрок", ["team"] = "команда", ["coach"] = "тренер", ["competition"] = "соревнование", ["winner"] = "победитель", ["ball"] = "мяч", ["goal"] = "гол", ["run"] = "бегать", ["swim"] = "плавать", ["jump"] = "прыгать", ["fitness"] = "фитнес", ["gym"] = "спортзал", ["race"] = "гонка", ["match"] = "матч",
            ["education"] = "образование", ["school"] = "школа", ["university"] = "университет", ["student"] = "студент", ["teacher"] = "учитель", ["lesson"] = "урок", ["class"] = "класс", ["book"] = "книга", ["notebook"] = "тетрадь", ["pen"] = "ручка", ["pencil"] = "карандаш", ["test"] = "тест", ["exam"] = "экзамен", ["homework"] = "домашняя работа", ["learn"] = "учиться", ["study"] = "изучать", ["read"] = "читать", ["write"] = "писать", ["question"] = "вопрос", ["answer"] = "ответ"
        };

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

        public static async Task<string> GetRussianTranslationForWordAsync(string word, string fallback = "")
        {
            return await GetRussianTranslationOrFallbackAsync(word, fallback, true);
        }

        public static int SyncCategoryToLesson(int categoryId)
        {
            using var ctx = new AppDbContext();
            return SyncCategoryWordsToLesson(ctx, categoryId);
        }

        public static async Task<WordImportResult> ImportWordsFromWordsApiAsync(int categoryId, DifficultyLevel difficulty, int count)
        {
            return await ImportWordsForCategoryAsync(categoryId, difficulty, count, true, true, "", true);
        }

        public static async Task<WordImportResult> AutoImportWordsForAllCategoriesAsync(int countPerCategory)
        {
            countPerCategory = Math.Clamp(countPerCategory, 1, 1000);
            using var ctx = new AppDbContext();
            var categories = ctx.WordCategories.OrderBy(c => c.Name).ToList();
            if (categories.Count == 0)
                return WordImportResult.Failed("Категории слов не найдены.");

            var result = new WordImportResult();
            var lines = new List<string>();
            foreach (var category in categories)
            {
                var categoryResult = await ImportWordsForCategoryAsync(category.CategoryId, DifficultyLevel.Easy, countPerCategory, false, false, "Datamuse автоимпорт по темам", true);
                result.Added += categoryResult.Added;
                result.Updated += categoryResult.Updated;
                result.Skipped += categoryResult.Skipped;
                result.LinkedToLesson += categoryResult.LinkedToLesson;
                lines.Add($"{category.Name}: +{categoryResult.Added}, обновлено {categoryResult.Updated}, в урок {categoryResult.LinkedToLesson}");
            }

            result.Message = $"Автоимпорт завершён. Категорий: {categories.Count}. На категорию: {countPerCategory}. Всего добавлено: {result.Added}. Обновлено: {result.Updated}. Добавлено в уроки: {result.LinkedToLesson}.\n" + string.Join("\n", lines);
            return result;
        }

        public static async Task<WordImportResult> ImproveWordsWithDeepSeekAsync(List<int> selectedWordIds, int maxCount)
        {
            var apiKey = Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY")?.Trim();
            if (string.IsNullOrWhiteSpace(apiKey))
                return WordImportResult.Failed("DeepSeek API key не найден. Добавьте переменную окружения Windows DEEPSEEK_API_KEY и перезапустите Visual Studio.");

            maxCount = Math.Clamp(maxCount, 1, 200);
            using var ctx = new AppDbContext();
            var categories = ctx.WordCategories.OrderBy(c => c.Name).ToList();
            var categoryByName = categories.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);
            var wordsQuery = ctx.Words.AsQueryable();
            var words = selectedWordIds.Count > 0
                ? wordsQuery.Where(w => selectedWordIds.Contains(w.WordId)).Take(maxCount).ToList()
                : wordsQuery
                    .AsEnumerable()
                    .Where(NeedsImportedMetadataUpdate)
                    .Take(maxCount)
                    .ToList();

            if (words.Count == 0)
                return WordImportResult.Failed("Нет слов для улучшения. Выберите слова в таблице или импортируйте слова без перевода.");

            var updated = 0;
            var skipped = 0;
            var touchedWordIds = new HashSet<int>();
            var touchedCategoryIds = new HashSet<int>();

            foreach (var chunk in words.Chunk(30))
            {
                var suggestions = await AskDeepSeekForWordMetadataAsync(apiKey, chunk.ToList(), categories);
                foreach (var suggestion in suggestions)
                {
                    var word = words.FirstOrDefault(w => string.Equals(w.EnglishWord, suggestion.EnglishWord, StringComparison.OrdinalIgnoreCase));
                    if (word == null)
                    {
                        skipped++;
                        continue;
                    }

                    var changed = false;
                    if (IsUsefulTranslation(word.EnglishWord, suggestion.RussianTranslation))
                    {
                        word.RussianTranslation = Truncate(suggestion.RussianTranslation, 200);
                        changed = true;
                    }

                    if (categoryByName.TryGetValue(suggestion.CategoryName, out var category))
                    {
                        word.CategoryId = category.CategoryId;
                        touchedCategoryIds.Add(category.CategoryId);
                        changed = true;
                    }

                    if (Enum.TryParse<DifficultyLevel>(suggestion.Difficulty, true, out var difficulty))
                    {
                        word.DifficultyLevel = difficulty;
                        changed = true;
                    }

                    if (changed)
                    {
                        touchedWordIds.Add(word.WordId);
                        touchedCategoryIds.Add(word.CategoryId);
                        updated++;
                    }
                    else
                    {
                        skipped++;
                    }
                }
            }

            if (updated == 0)
                return WordImportResult.Failed("DeepSeek не вернул подходящих улучшений.");

            ctx.SaveChanges();
            var links = ctx.LessonWords.Where(lw => touchedWordIds.Contains(lw.WordId)).ToList();
            ctx.LessonWords.RemoveRange(links);
            ctx.SaveChanges();

            var linked = 0;
            foreach (var categoryId in touchedCategoryIds)
                linked += SyncCategoryWordsToLesson(ctx, categoryId);

            var message = $"DeepSeek AI: улучшено слов: {updated}. Пропущено: {skipped}. Обновлено связей с уроками: {linked}.";
            SaveImportHistory(ctx, "DeepSeek AI", "Все категории", words.Count, 0, updated, skipped, linked, message);
            return new WordImportResult { Added = 0, Updated = updated, Skipped = skipped, LinkedToLesson = linked, Message = message };
        }

        private static async Task<List<DeepSeekWordMetadata>> AskDeepSeekForWordMetadataAsync(string apiKey, List<Word> words, List<WordCategory> categories)
        {
            var allowedCategories = string.Join(", ", categories.Select(c => c.Name));
            var inputWords = new JArray(words.Select(w => new JObject
            {
                ["englishWord"] = w.EnglishWord,
                ["currentTranslation"] = w.RussianTranslation,
                ["currentCategoryId"] = w.CategoryId
            }));
            var prompt = "You improve vocabulary for a Russian English-learning app. " +
                $"Allowed categoryName values: {allowedCategories}. " +
                "Return only a JSON array. Each item must have englishWord, russianTranslation, categoryName, difficulty. " +
                "difficulty must be Easy, Medium, or Hard. Use natural Russian translations, not explanations. Words: " + inputWords.ToString();
            var requestBody = new JObject
            {
                ["model"] = "deepseek-chat",
                ["messages"] = new JArray
                {
                    new JObject { ["role"] = "system", ["content"] = "Return strict JSON only. No markdown." },
                    new JObject { ["role"] = "user", ["content"] = prompt }
                },
                ["temperature"] = 0.2
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.deepseek.com/chat/completions");
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {apiKey}");
            request.Content = new StringContent(requestBody.ToString(), Encoding.UTF8, "application/json");
            using var response = await _http.SendAsync(request);
            var responseText = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"DeepSeek API вернул ошибку: {(int)response.StatusCode} {Truncate(responseText, 300)}");

            var content = JObject.Parse(responseText)["choices"]?.FirstOrDefault()?["message"]?["content"]?.ToString() ?? "";
            var json = ExtractJsonArray(content);
            var array = JArray.Parse(json);
            return array.Select(item => new DeepSeekWordMetadata
            {
                EnglishWord = item["englishWord"]?.ToString() ?? "",
                RussianTranslation = item["russianTranslation"]?.ToString() ?? "",
                CategoryName = item["categoryName"]?.ToString() ?? "",
                Difficulty = item["difficulty"]?.ToString() ?? ""
            }).Where(item => !string.IsNullOrWhiteSpace(item.EnglishWord)).ToList();
        }

        private static async Task<WordImportResult> ImportWordsForCategoryAsync(int categoryId, DifficultyLevel difficulty, int count, bool allowWordsApi, bool allowBroadDatamuseQueries, string sourceOverride, bool requireRussianTranslation)
        {
            var apiKey = App.Configuration["ApiKeys:WordsApiKey"]?.Trim();
            count = Math.Clamp(count, 1, 5000);
            var usingFreeApi = !allowWordsApi || string.IsNullOrWhiteSpace(apiKey);
            var suggestions = usingFreeApi
                ? await FetchWordsFromFreeDatamuseApiAsync(categoryId, count, allowBroadDatamuseQueries)
                : await FetchWordsFromWordsApiAsync(apiKey!, count);
            if (suggestions.Count == 0)
                return WordImportResult.Failed(usingFreeApi
                    ? "Бесплатный Datamuse API не вернул слова. Проверьте подключение к интернету."
                    : "WordsAPI не вернул слова. Проверьте ключ и доступ к API.");

            using var ctx = new AppDbContext();
            var category = ctx.WordCategories.Find(categoryId);
            if (category == null)
                return WordImportResult.Failed("Выбранная категория не найдена.");

            var existing = ctx.Words
                .AsEnumerable()
                .GroupBy(w => w.EnglishWord.ToLower())
                .ToDictionary(g => g.Key, g => g.First());
            var enrichLimit = GetEnrichLimit();
            var translateLimit = GetTranslateLimit();
            var added = 0;
            var skipped = 0;
            var updated = 0;
            var enrichedCount = 0;
            var translatedCount = 0;

            foreach (var suggestion in suggestions)
            {
                var word = NormalizeWord(suggestion.EnglishWord);
                if (string.IsNullOrEmpty(word))
                    continue;

                if (existing.TryGetValue(word, out var existingWord))
                {
                    skipped++;
                    if (existingWord.CategoryId == categoryId && NeedsImportedMetadataUpdate(existingWord) && translatedCount < translateLimit)
                    {
                        existingWord.DifficultyLevel = DetectDifficulty(word, suggestion.Score, difficulty);
                        var updatedTranslation = await GetRussianTranslationOrFallbackAsync(word, existingWord.ExampleSentence, translatedCount < translateLimit);
                        translatedCount++;
                        if (!requireRussianTranslation || IsUsefulTranslation(word, updatedTranslation))
                        {
                            existingWord.RussianTranslation = updatedTranslation;
                            updated++;
                        }
                    }
                    continue;
                }

                var enriched = enrichedCount < enrichLimit ? await EnrichWordAsync(suggestion) : suggestion;
                if (enrichedCount < enrichLimit) enrichedCount++;
                var translation = await GetRussianTranslationOrFallbackAsync(word, enriched.Definition, translatedCount < translateLimit);
                if (translatedCount < translateLimit) translatedCount++;
                if (requireRussianTranslation && !IsUsefulTranslation(word, translation))
                {
                    skipped++;
                    continue;
                }
                var newWord = new Word
                {
                    EnglishWord = word,
                    RussianTranslation = translation,
                    ExampleSentence = Truncate(string.IsNullOrWhiteSpace(enriched.Example) ? enriched.Definition : enriched.Example, 500),
                    ExampleTranslation = "",
                    DifficultyLevel = DetectDifficulty(word, suggestion.Score, difficulty),
                    CategoryId = categoryId,
                    Transcription = enriched.Phonetic
                };
                ctx.Words.Add(newWord);
                existing.Add(word, newWord);
                added++;
            }

            ctx.SaveChanges();
            var linked = SyncCategoryWordsToLesson(ctx, categoryId);
            var source = string.IsNullOrWhiteSpace(sourceOverride) ? usingFreeApi ? "Datamuse API без ключа" : "WordsAPI" : sourceOverride;
            var message = $"Источник: {source}. Категория: {category.Name}. Добавлено слов: {added}. Обновлено: {updated}. Пропущено дублей: {skipped}. Добавлено в урок: {linked}.";
            SaveImportHistory(ctx, source, category.Name, count, added, updated, skipped, linked, message);
            return new WordImportResult { Added = added, Skipped = skipped, Updated = updated, LinkedToLesson = linked, Message = message };
        }

        private static async Task<List<WordSuggestion>> FetchWordsFromFreeDatamuseApiAsync(int categoryId, int count, bool includeBroadQueries)
        {
            var topics = GetDatamuseTopics(categoryId);
            var result = new List<WordSuggestion>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var topic in topics)
            {
                if (result.Count >= count) break;
                var max = Math.Min(1000, count - result.Count + 100);
                await AddDatamuseWordsAsync(result, seen, $"topics={Uri.EscapeDataString(topic)}&max={max}", count);
            }

            if (!includeBroadQueries)
                return result;

            foreach (var query in GetBroadDatamuseQueries())
            {
                if (result.Count >= count) break;
                await AddDatamuseWordsAsync(result, seen, query, count);
            }

            return result;
        }

        private static async Task AddDatamuseWordsAsync(List<WordSuggestion> result, HashSet<string> seen, string query, int count)
        {
            try
            {
                var baseUrl = App.Configuration["ApiKeys:DatamuseBaseUrl"]?.Trim();
                if (string.IsNullOrWhiteSpace(baseUrl)) baseUrl = DefaultDatamuseUrl;

                var json = await _http.GetStringAsync($"{baseUrl}?{query}");
                var items = JArray.Parse(json);
                foreach (var item in items)
                {
                    var word = NormalizeWord(item["word"]?.ToString() ?? "");
                    if (string.IsNullOrEmpty(word) || !seen.Add(word))
                        continue;

                    result.Add(new WordSuggestion { EnglishWord = word, Score = item["score"]?.Value<int>() ?? 0 });
                    if (result.Count >= count) break;
                }
            }
            catch { }
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

        private static async Task<string> GetRussianTranslationOrFallbackAsync(string word, string fallback, bool allowOnlineTranslation)
        {
            if (LocalTranslations.TryGetValue(word, out var localTranslation))
                return localTranslation;

            var yandexTranslation = await TryTranslateToRussianAsync(word);
            if (IsUsefulTranslation(word, yandexTranslation))
                return Truncate(yandexTranslation, 200);

            if (allowOnlineTranslation)
            {
                var myMemoryTranslation = await TryTranslateWithMyMemoryAsync(word);
                if (IsUsefulTranslation(word, myMemoryTranslation))
                    return Truncate(myMemoryTranslation, 200);
            }

            return Truncate(fallback, 200, "перевод не найден");
        }

        private static async Task<string> TryTranslateWithMyMemoryAsync(string word)
        {
            try
            {
                var url = $"https://api.mymemory.translated.net/get?q={Uri.EscapeDataString(word)}&langpair=en%7Cru";
                var email = App.Configuration["ApiKeys:MyMemoryEmail"]?.Trim();
                if (!string.IsNullOrWhiteSpace(email))
                    url += $"&de={Uri.EscapeDataString(email)}";

                var json = await _http.GetStringAsync(url);
                var token = JObject.Parse(json);
                if (token["responseStatus"]?.Value<int>() != 200)
                    return "";

                return token["responseData"]?["translatedText"]?.ToString() ?? "";
            }
            catch
            {
                return "";
            }
        }

        private static bool IsUsefulTranslation(string englishWord, string translation)
        {
            if (string.IsNullOrWhiteSpace(translation)) return false;
            var normalizedTranslation = translation.Trim().ToLower();
            if (normalizedTranslation == englishWord.Trim().ToLower()) return false;
            return normalizedTranslation.Any(ch => ch >= 'а' && ch <= 'я' || ch == 'ё');
        }

        private static bool NeedsImportedMetadataUpdate(Word word)
        {
            var translation = word.RussianTranslation.Trim().ToLower();
            return string.IsNullOrWhiteSpace(translation)
                || translation == "перевод не указан"
                || translation == "перевод не найден"
                || !translation.Any(ch => ch >= 'а' && ch <= 'я' || ch == 'ё');
        }

        private static int SyncCategoryWordsToLesson(AppDbContext ctx, int categoryId)
        {
            var category = ctx.WordCategories.Find(categoryId);
            if (category == null) return 0;

            var lesson = FindOrCreateLessonForCategory(ctx, category);
            var words = ctx.Words
                .Where(w => w.CategoryId == categoryId)
                .OrderBy(w => w.EnglishWord)
                .ToList();
            if (words.Count == 0) return 0;

            var existingWordIds = ctx.LessonWords
                .Where(lw => lw.LessonId == lesson.LessonId)
                .Select(lw => lw.WordId)
                .ToHashSet();
            var lessonOrderIndexes = ctx.LessonWords
                .Where(lw => lw.LessonId == lesson.LessonId)
                .Select(lw => lw.OrderIndex)
                .ToList();
            var nextOrder = lessonOrderIndexes.Count == 0 ? 1 : lessonOrderIndexes.Max() + 1;
            var linked = 0;

            foreach (var word in words)
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

        private static void SaveImportHistory(AppDbContext ctx, string source, string categoryName, int requestedCount, int added, int updated, int skipped, int linked, string message)
        {
            try
            {
                ctx.WordImportHistories.Add(new WordImportHistory
                {
                    ImportedAt = DateTime.Now,
                    Source = source,
                    CategoryName = categoryName,
                    RequestedCount = requestedCount,
                    Added = added,
                    Updated = updated,
                    Skipped = skipped,
                    LinkedToLesson = linked,
                    Message = Truncate(message, 1000)
                });
                ctx.SaveChanges();
            }
            catch { }
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

        private static List<string> GetDatamuseTopics(int categoryId)
        {
            using var ctx = new AppDbContext();
            var categoryName = ctx.WordCategories.Find(categoryId)?.Name ?? "";
            return categoryName switch
            {
                "Animals" => new() { "animals", "pets", "wildlife", "mammals", "birds", "fish", "insects" },
                "Food & Drink" => new() { "food", "drink", "cooking", "kitchen", "fruit", "vegetables", "restaurant" },
                "Travel" => new() { "travel", "airport", "hotel", "transport", "tourism", "vacation", "city" },
                "Technology" => new() { "technology", "computer", "internet", "software", "hardware", "science", "engineering" },
                "Body & Health" => new() { "body", "health", "medicine", "hospital", "fitness", "disease", "doctor" },
                "Nature" => new() { "nature", "weather", "plants", "environment", "forest", "ocean", "mountain" },
                "Home" => new() { "home", "house", "furniture", "family", "room", "cleaning", "apartment" },
                "Work & Business" => new() { "work", "business", "office", "money", "career", "company", "job" },
                "Sports" => new() { "sports", "fitness", "football", "basketball", "exercise", "competition", "team" },
                "Education" => new() { "education", "school", "university", "learning", "classroom", "student", "teacher" },
                _ => new() { categoryName.ToLower(), "english", "learning", "vocabulary" }
            };
        }

        private static List<string> GetBroadDatamuseQueries() => new()
        {
            "sp=???&max=1000",
            "sp=????&max=1000",
            "sp=?????&max=1000",
            "sp=??????&max=1000",
            "sp=???????&max=1000",
            "sp=????????&max=1000",
            "sp=?????????&max=1000",
            "rel_jjb=person&max=1000",
            "rel_jjb=place&max=1000",
            "rel_jjb=thing&max=1000",
            "rel_jjb=good&max=1000",
            "rel_jjb=bad&max=1000",
            "rel_trg=school&max=1000",
            "rel_trg=work&max=1000",
            "rel_trg=home&max=1000",
            "rel_trg=travel&max=1000",
            "rel_trg=food&max=1000",
            "rel_trg=health&max=1000",
            "rel_trg=technology&max=1000",
            "rel_trg=nature&max=1000"
        };

        private static string NormalizeWord(string word)
        {
            var normalized = word.Trim().ToLower();
            return normalized.All(ch => char.IsLetter(ch) || ch == '-' || ch == '\'') ? normalized : "";
        }

        private static DifficultyLevel DetectDifficulty(string word, int score, DifficultyLevel fallback)
        {
            var length = word.Replace("-", "").Replace("'", "").Length;
            if (score >= 20000 || length <= 5) return DifficultyLevel.Easy;
            if (score >= 5000 || length <= 8) return DifficultyLevel.Medium;
            if (length >= 9 || score > 0) return DifficultyLevel.Hard;
            return fallback;
        }

        private static string Truncate(string value, int maxLength, string fallback = "")
        {
            var text = string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
            return text.Length <= maxLength ? text : text[..maxLength];
        }

        private static string ExtractJsonArray(string value)
        {
            var text = value.Trim();
            var start = text.IndexOf('[');
            var end = text.LastIndexOf(']');
            if (start < 0 || end < start)
                throw new InvalidOperationException("DeepSeek не вернул JSON-массив.");
            return text[start..(end + 1)];
        }

        private static int GetEnrichLimit()
        {
            var configured = App.Configuration["ApiKeys:EnrichImportedWordsLimit"]?.Trim();
            return int.TryParse(configured, out var limit) ? Math.Clamp(limit, 0, 5000) : 200;
        }

        private static int GetTranslateLimit()
        {
            var configured = App.Configuration["ApiKeys:TranslateImportedWordsLimit"]?.Trim();
            return int.TryParse(configured, out var limit) ? Math.Clamp(limit, 0, 5000) : 500;
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
        public int Updated { get; set; }
        public int LinkedToLesson { get; set; }
        public string Message { get; set; } = string.Empty;

        public static WordImportResult Failed(string message) => new() { Message = message };
    }

    public class DeepSeekWordMetadata
    {
        public string EnglishWord { get; set; } = string.Empty;
        public string RussianTranslation { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
    }
}
