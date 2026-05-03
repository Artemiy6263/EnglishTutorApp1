using EnglishTutor.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Data
{
    public static class DbInitializer
    {
        public static void Initialize()
        {
            using var ctx = new AppDbContext();
            ctx.Database.EnsureCreated();

            // === SCHEMA MIGRATIONS ===
            try
            {
                ctx.Database.ExecuteSqlRaw(@"
                    IF NOT EXISTS (SELECT * FROM sys.columns
                        WHERE object_id = OBJECT_ID(N'Words') AND name = N'ImagePath')
                    ALTER TABLE Words ADD ImagePath NVARCHAR(500) NULL");
            }
            catch { /* Колонка уже существует или другая ошибка */ }

            try
            {
                ctx.Database.ExecuteSqlRaw(@"
                    IF OBJECT_ID(N'WordImportHistories', N'U') IS NULL
                    CREATE TABLE WordImportHistories (
                        WordImportHistoryId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        ImportedAt DATETIME2 NOT NULL,
                        Source NVARCHAR(100) NOT NULL,
                        CategoryName NVARCHAR(200) NOT NULL,
                        RequestedCount INT NOT NULL,
                        Added INT NOT NULL,
                        Updated INT NOT NULL,
                        Skipped INT NOT NULL,
                        LinkedToLesson INT NOT NULL,
                        Message NVARCHAR(1000) NOT NULL
                    )");
            }
            catch { /* Таблица уже существует или другая ошибка */ }

            // === SEED USERS ===
            if (!ctx.Users.Any())
            {
                var admin = new User { Username = "admin", PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), Email = "admin@english.com", Role = UserRole.Admin };
                var student = new User { Username = "student", PasswordHash = BCrypt.Net.BCrypt.HashPassword("student123"), Email = "student@english.com", Role = UserRole.Student };
                ctx.Users.AddRange(admin, student);
                ctx.SaveChanges();
            }

            // === SEED CATEGORIES ===
            var requiredCategories = new List<WordCategory> {
                new(){Name="Animals",Description="Животные",IconEmoji="🐾"},
                new(){Name="Food & Drink",Description="Еда и напитки",IconEmoji="🍎"},
                new(){Name="Travel",Description="Путешествия",IconEmoji="✈️"},
                new(){Name="Technology",Description="Технологии",IconEmoji="💻"},
                new(){Name="Body & Health",Description="Тело и здоровье",IconEmoji="🏥"},
                new(){Name="Nature",Description="Природа",IconEmoji="🌿"},
                new(){Name="Home",Description="Дом",IconEmoji="🏠"},
                new(){Name="Work & Business",Description="Работа и бизнес",IconEmoji="💼"},
                new(){Name="Sports",Description="Спорт",IconEmoji="⚽"},
                new(){Name="Education",Description="Образование",IconEmoji="📚"},
            };
            var existingCategoryNames = ctx.WordCategories.Select(c => c.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var missingCategories = requiredCategories.Where(c => !existingCategoryNames.Contains(c.Name)).ToList();
            if (missingCategories.Count > 0)
            {
                ctx.WordCategories.AddRange(missingCategories);
                ctx.SaveChanges();
            }

            // === SEED WORDS ===
            if (ctx.Words.Count() < 50)
            {
                var cats = ctx.WordCategories.ToList();
                int animals = cats.First(c => c.Name == "Animals").CategoryId,
                    food = cats.First(c => c.Name == "Food & Drink").CategoryId,
                    travel = cats.First(c => c.Name == "Travel").CategoryId,
                    tech = cats.First(c => c.Name == "Technology").CategoryId,
                    health = cats.First(c => c.Name == "Body & Health").CategoryId,
                    nature = cats.First(c => c.Name == "Nature").CategoryId,
                    home = cats.First(c => c.Name == "Home").CategoryId,
                    work = cats.First(c => c.Name == "Work & Business").CategoryId,
                    sports = cats.First(c => c.Name == "Sports").CategoryId,
                    education = cats.First(c => c.Name == "Education").CategoryId;

                var existing = ctx.Words.Select(w => w.EnglishWord.ToLower()).ToHashSet();
                var words = new List<Word>
                {
                    // === ANIMALS (30 words) ===
                    new(){EnglishWord="cat",RussianTranslation="кошка",ExampleSentence="The cat is sleeping on the sofa.",ExampleTranslation="Кошка спит на диване.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=animals,Transcription="[kæt]"},
                    new(){EnglishWord="dog",RussianTranslation="собака",ExampleSentence="The dog is barking loudly.",ExampleTranslation="Собака громко лает.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=animals,Transcription="[dɒɡ]"},
                    new(){EnglishWord="bird",RussianTranslation="птица",ExampleSentence="A bird is singing in the tree.",ExampleTranslation="Птица поёт на дереве.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=animals,Transcription="[bɜːd]"},
                    new(){EnglishWord="fish",RussianTranslation="рыба",ExampleSentence="The fish swims in the pond.",ExampleTranslation="Рыба плавает в пруду.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=animals,Transcription="[fɪʃ]"},
                    new(){EnglishWord="rabbit",RussianTranslation="кролик",ExampleSentence="The rabbit hops across the garden.",ExampleTranslation="Кролик прыгает по саду.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=animals,Transcription="[ˈræbɪt]"},
                    new(){EnglishWord="bear",RussianTranslation="медведь",ExampleSentence="The bear hibernates in winter.",ExampleTranslation="Медведь зимует зимой.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=animals,Transcription="[beə]"},
                    new(){EnglishWord="wolf",RussianTranslation="волк",ExampleSentence="The wolf howls at the moon.",ExampleTranslation="Волк воет на луну.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=animals,Transcription="[wʊlf]"},
                    new(){EnglishWord="fox",RussianTranslation="лиса",ExampleSentence="The fox is very clever.",ExampleTranslation="Лиса очень хитрая.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=animals,Transcription="[fɒks]"},
                    new(){EnglishWord="horse",RussianTranslation="лошадь",ExampleSentence="The horse runs across the field.",ExampleTranslation="Лошадь бежит по полю.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=animals,Transcription="[hɔːs]"},
                    new(){EnglishWord="elephant",RussianTranslation="слон",ExampleSentence="The elephant has a long trunk.",ExampleTranslation="У слона длинный хобот.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=animals,Transcription="[ˈelɪfənt]"},
                    new(){EnglishWord="tiger",RussianTranslation="тигр",ExampleSentence="The tiger is hunting its prey.",ExampleTranslation="Тигр охотится на добычу.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=animals,Transcription="[ˈtaɪɡə]"},
                    new(){EnglishWord="lion",RussianTranslation="лев",ExampleSentence="The lion is the king of animals.",ExampleTranslation="Лев — король зверей.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=animals,Transcription="[ˈlaɪən]"},
                    new(){EnglishWord="monkey",RussianTranslation="обезьяна",ExampleSentence="The monkey swings through the trees.",ExampleTranslation="Обезьяна качается на деревьях.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=animals,Transcription="[ˈmʌŋki]"},
                    new(){EnglishWord="penguin",RussianTranslation="пингвин",ExampleSentence="The penguin lives in Antarctica.",ExampleTranslation="Пингвин живёт в Антарктиде.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=animals,Transcription="[ˈpeŋɡwɪn]"},
                    new(){EnglishWord="giraffe",RussianTranslation="жираф",ExampleSentence="The giraffe has a very long neck.",ExampleTranslation="У жирафа очень длинная шея.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=animals,Transcription="[dʒɪˈrɑːf]"},
                    new(){EnglishWord="rhinoceros",RussianTranslation="носорог",ExampleSentence="The rhinoceros has a sharp horn.",ExampleTranslation="У носорога острый рог.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=animals,Transcription="[raɪˈnɒsərəs]"},
                    new(){EnglishWord="chimpanzee",RussianTranslation="шимпанзе",ExampleSentence="Chimpanzees are very intelligent primates.",ExampleTranslation="Шимпанзе — очень умные приматы.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=animals,Transcription="[ˌtʃɪmpænˈziː]"},
                    new(){EnglishWord="crocodile",RussianTranslation="крокодил",ExampleSentence="The crocodile waited near the river.",ExampleTranslation="Крокодил поджидал у реки.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=animals,Transcription="[ˈkrɒkədaɪl]"},
                    new(){EnglishWord="dolphin",RussianTranslation="дельфин",ExampleSentence="The dolphin jumped out of the water.",ExampleTranslation="Дельфин выпрыгнул из воды.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=animals,Transcription="[ˈdɒlfɪn]"},
                    new(){EnglishWord="snake",RussianTranslation="змея",ExampleSentence="The snake slithers through the grass.",ExampleTranslation="Змея скользит по траве.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=animals,Transcription="[sneɪk]"},
                    new(){EnglishWord="owl",RussianTranslation="сова",ExampleSentence="The owl hoots at night.",ExampleTranslation="Сова ухает ночью.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=animals,Transcription="[aʊl]"},
                    new(){EnglishWord="parrot",RussianTranslation="попугай",ExampleSentence="The parrot can repeat words.",ExampleTranslation="Попугай умеет повторять слова.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=animals,Transcription="[ˈpærət]"},
                    new(){EnglishWord="cow",RussianTranslation="корова",ExampleSentence="The cow gives us milk.",ExampleTranslation="Корова даёт нам молоко.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=animals,Transcription="[kaʊ]"},
                    new(){EnglishWord="pig",RussianTranslation="свинья",ExampleSentence="The pig rolled in the mud.",ExampleTranslation="Свинья каталась в грязи.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=animals,Transcription="[pɪɡ]"},
                    new(){EnglishWord="sheep",RussianTranslation="овца",ExampleSentence="The sheep grazed in the meadow.",ExampleTranslation="Овца паслась на лугу.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=animals,Transcription="[ʃiːp]"},
                    new(){EnglishWord="butterfly",RussianTranslation="бабочка",ExampleSentence="A colourful butterfly landed on the flower.",ExampleTranslation="Разноцветная бабочка села на цветок.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=animals,Transcription="[ˈbʌtəflaɪ]"},
                    new(){EnglishWord="spider",RussianTranslation="паук",ExampleSentence="The spider spun a web in the corner.",ExampleTranslation="Паук сплёл паутину в углу.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=animals,Transcription="[ˈspaɪdə]"},
                    new(){EnglishWord="frog",RussianTranslation="лягушка",ExampleSentence="The frog jumps into the pond.",ExampleTranslation="Лягушка прыгает в пруд.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=animals,Transcription="[frɒɡ]"},
                    new(){EnglishWord="eagle",RussianTranslation="орёл",ExampleSentence="The eagle soared above the mountains.",ExampleTranslation="Орёл парил над горами.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=animals,Transcription="[ˈiːɡəl]"},
                    new(){EnglishWord="whale",RussianTranslation="кит",ExampleSentence="The whale surfaced to breathe.",ExampleTranslation="Кит всплыл на поверхность, чтобы дышать.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=animals,Transcription="[weɪl]"},

                    // === FOOD & DRINK (25 words) ===
                    new(){EnglishWord="apple",RussianTranslation="яблоко",ExampleSentence="I eat an apple every morning.",ExampleTranslation="Я ем яблоко каждое утро.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=food,Transcription="[ˈæpəl]"},
                    new(){EnglishWord="bread",RussianTranslation="хлеб",ExampleSentence="She buys fresh bread every day.",ExampleTranslation="Она покупает свежий хлеб каждый день.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=food,Transcription="[bred]"},
                    new(){EnglishWord="water",RussianTranslation="вода",ExampleSentence="Drink more water every day.",ExampleTranslation="Пейте больше воды каждый день.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=food,Transcription="[ˈwɔːtə]"},
                    new(){EnglishWord="coffee",RussianTranslation="кофе",ExampleSentence="I drink coffee every morning.",ExampleTranslation="Я пью кофе каждое утро.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=food,Transcription="[ˈkɒfi]"},
                    new(){EnglishWord="milk",RussianTranslation="молоко",ExampleSentence="Children should drink milk every day.",ExampleTranslation="Дети должны пить молоко каждый день.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=food,Transcription="[mɪlk]"},
                    new(){EnglishWord="egg",RussianTranslation="яйцо",ExampleSentence="She cooked scrambled eggs for breakfast.",ExampleTranslation="Она приготовила яичницу на завтрак.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=food,Transcription="[eɡ]"},
                    new(){EnglishWord="cheese",RussianTranslation="сыр",ExampleSentence="He put cheese on the pizza.",ExampleTranslation="Он положил сыр на пиццу.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=food,Transcription="[tʃiːz]"},
                    new(){EnglishWord="butter",RussianTranslation="масло сливочное",ExampleSentence="She spread butter on the toast.",ExampleTranslation="Она намазала масло на тост.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=food,Transcription="[ˈbʌtə]"},
                    new(){EnglishWord="strawberry",RussianTranslation="клубника",ExampleSentence="Strawberries are sweet and red.",ExampleTranslation="Клубника сладкая и красная.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=food,Transcription="[ˈstrɔːbəri]"},
                    new(){EnglishWord="cucumber",RussianTranslation="огурец",ExampleSentence="She added cucumber to the salad.",ExampleTranslation="Она добавила огурец в салат.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=food,Transcription="[ˈkjuːkʌmbə]"},
                    new(){EnglishWord="tomato",RussianTranslation="помидор",ExampleSentence="The tomato is red and juicy.",ExampleTranslation="Помидор красный и сочный.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=food,Transcription="[təˈmɑːtəʊ]"},
                    new(){EnglishWord="potato",RussianTranslation="картофель",ExampleSentence="We had baked potatoes for dinner.",ExampleTranslation="На ужин у нас был запечённый картофель.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=food,Transcription="[pəˈteɪtəʊ]"},
                    new(){EnglishWord="onion",RussianTranslation="лук",ExampleSentence="Cut the onion into small pieces.",ExampleTranslation="Нарежьте лук небольшими кусочками.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=food,Transcription="[ˈʌnjən]"},
                    new(){EnglishWord="garlic",RussianTranslation="чеснок",ExampleSentence="Add garlic to the sauce.",ExampleTranslation="Добавьте чеснок в соус.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=food,Transcription="[ˈɡɑːlɪk]"},
                    new(){EnglishWord="avocado",RussianTranslation="авокадо",ExampleSentence="Avocado is rich in healthy fats.",ExampleTranslation="Авокадо богато полезными жирами.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=food,Transcription="[ˌævəˈkɑːdəʊ]"},
                    new(){EnglishWord="mushroom",RussianTranslation="гриб",ExampleSentence="We picked mushrooms in the forest.",ExampleTranslation="Мы собирали грибы в лесу.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=food,Transcription="[ˈmʌʃruːm]"},
                    new(){EnglishWord="pineapple",RussianTranslation="ананас",ExampleSentence="Pineapple juice is very refreshing.",ExampleTranslation="Сок ананаса очень освежает.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=food,Transcription="[ˈpaɪnæpəl]"},
                    new(){EnglishWord="chocolate",RussianTranslation="шоколад",ExampleSentence="She bought a chocolate bar.",ExampleTranslation="Она купила плитку шоколада.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=food,Transcription="[ˈtʃɒklət]"},
                    new(){EnglishWord="sandwich",RussianTranslation="бутерброд",ExampleSentence="He made a cheese sandwich.",ExampleTranslation="Он сделал бутерброд с сыром.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=food,Transcription="[ˈsænwɪdʒ]"},
                    new(){EnglishWord="spaghetti",RussianTranslation="спагетти",ExampleSentence="Spaghetti bolognese is a classic dish.",ExampleTranslation="Спагетти болоньезе — классическое блюдо.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=food,Transcription="[spəˈɡeti]"},
                    new(){EnglishWord="blueberry",RussianTranslation="черника",ExampleSentence="Blueberries are great for your eyes.",ExampleTranslation="Черника полезна для зрения.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=food,Transcription="[ˈbluːbəri]"},
                    new(){EnglishWord="lemon",RussianTranslation="лимон",ExampleSentence="Add a slice of lemon to the tea.",ExampleTranslation="Добавьте ломтик лимона в чай.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=food,Transcription="[ˈlemən]"},
                    new(){EnglishWord="orange",RussianTranslation="апельсин",ExampleSentence="She squeezed oranges for fresh juice.",ExampleTranslation="Она выжала апельсины для свежего сока.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=food,Transcription="[ˈɒrɪndʒ]"},
                    new(){EnglishWord="grape",RussianTranslation="виноград",ExampleSentence="I love eating grapes in summer.",ExampleTranslation="Я люблю есть виноград летом.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=food,Transcription="[ɡreɪp]"},
                    new(){EnglishWord="broccoli",RussianTranslation="брокколи",ExampleSentence="Broccoli is full of vitamins.",ExampleTranslation="Брокколи богата витаминами.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=food,Transcription="[ˈbrɒkəli]"},

                    // === TRAVEL (20 words) ===
                    new(){EnglishWord="airport",RussianTranslation="аэропорт",ExampleSentence="We arrived at the airport early.",ExampleTranslation="Мы рано приехали в аэропорт.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=travel,Transcription="[ˈeəpɔːt]"},
                    new(){EnglishWord="passport",RussianTranslation="паспорт",ExampleSentence="Don't forget your passport!",ExampleTranslation="Не забудьте паспорт!",DifficultyLevel=DifficultyLevel.Easy,CategoryId=travel,Transcription="[ˈpɑːspɔːt]"},
                    new(){EnglishWord="luggage",RussianTranslation="багаж",ExampleSentence="My luggage is too heavy.",ExampleTranslation="Мой багаж слишком тяжёлый.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=travel,Transcription="[ˈlʌɡɪdʒ]"},
                    new(){EnglishWord="ticket",RussianTranslation="билет",ExampleSentence="I bought a return ticket to Paris.",ExampleTranslation="Я купил билет туда и обратно в Париж.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=travel,Transcription="[ˈtɪkɪt]"},
                    new(){EnglishWord="hotel",RussianTranslation="гостиница",ExampleSentence="We stayed at a five-star hotel.",ExampleTranslation="Мы остановились в пятизвёздочном отеле.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=travel,Transcription="[həʊˈtel]"},
                    new(){EnglishWord="reservation",RussianTranslation="бронирование",ExampleSentence="I made a reservation at the restaurant.",ExampleTranslation="Я забронировал стол в ресторане.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=travel,Transcription="[ˌrezəˈveɪʃən]"},
                    new(){EnglishWord="boarding pass",RussianTranslation="посадочный талон",ExampleSentence="Show your boarding pass at the gate.",ExampleTranslation="Предъявите посадочный талон у выхода.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=travel,Transcription="[ˈbɔːdɪŋ pɑːs]"},
                    new(){EnglishWord="customs",RussianTranslation="таможня",ExampleSentence="You must declare goods at customs.",ExampleTranslation="На таможне нужно декларировать товары.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=travel,Transcription="[ˈkʌstəmz]"},
                    new(){EnglishWord="destination",RussianTranslation="пункт назначения",ExampleSentence="What is your final destination?",ExampleTranslation="Какой ваш конечный пункт назначения?",DifficultyLevel=DifficultyLevel.Hard,CategoryId=travel,Transcription="[ˌdestɪˈneɪʃən]"},
                    new(){EnglishWord="itinerary",RussianTranslation="маршрут поездки",ExampleSentence="The travel agency gave us a detailed itinerary.",ExampleTranslation="Туристическое агентство дало нам подробный маршрут.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=travel,Transcription="[aɪˈtɪnərəri]"},
                    new(){EnglishWord="visa",RussianTranslation="виза",ExampleSentence="You need a visa to enter that country.",ExampleTranslation="Вам нужна виза для въезда в эту страну.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=travel,Transcription="[ˈviːzə]"},
                    new(){EnglishWord="embassy",RussianTranslation="посольство",ExampleSentence="I applied for a visa at the embassy.",ExampleTranslation="Я подал заявку на визу в посольстве.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=travel,Transcription="[ˈembəsi]"},
                    new(){EnglishWord="currency",RussianTranslation="валюта",ExampleSentence="I need to exchange some currency.",ExampleTranslation="Мне нужно обменять немного валюты.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=travel,Transcription="[ˈkʌrənsi]"},
                    new(){EnglishWord="sightseeing",RussianTranslation="осмотр достопримечательностей",ExampleSentence="We spent the afternoon sightseeing.",ExampleTranslation="Мы провели вторую половину дня, осматривая достопримечательности.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=travel,Transcription="[ˈsaɪtsiːɪŋ]"},
                    new(){EnglishWord="souvenir",RussianTranslation="сувенир",ExampleSentence="She bought souvenirs for her family.",ExampleTranslation="Она купила сувениры для своей семьи.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=travel,Transcription="[ˌsuːvəˈnɪə]"},
                    new(){EnglishWord="train station",RussianTranslation="железнодорожный вокзал",ExampleSentence="The train leaves from the central station.",ExampleTranslation="Поезд отправляется с центрального вокзала.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=travel,Transcription="[treɪn ˈsteɪʃən]"},
                    new(){EnglishWord="map",RussianTranslation="карта",ExampleSentence="Can you show me on the map?",ExampleTranslation="Вы можете показать мне на карте?",DifficultyLevel=DifficultyLevel.Easy,CategoryId=travel,Transcription="[mæp]"},
                    new(){EnglishWord="guidebook",RussianTranslation="путеводитель",ExampleSentence="I bought a guidebook before the trip.",ExampleTranslation="Я купил путеводитель перед поездкой.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=travel,Transcription="[ˈɡaɪdbʊk]"},
                    new(){EnglishWord="accommodation",RussianTranslation="жильё, проживание",ExampleSentence="We need to book accommodation in advance.",ExampleTranslation="Нам нужно забронировать жильё заранее.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=travel,Transcription="[əˌkɒməˈdeɪʃən]"},
                    new(){EnglishWord="departure",RussianTranslation="вылет, отправление",ExampleSentence="The departure is at 7 AM.",ExampleTranslation="Вылет в 7 утра.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=travel,Transcription="[dɪˈpɑːtʃə]"},

                    // === TECHNOLOGY (20 words) ===
                    new(){EnglishWord="computer",RussianTranslation="компьютер",ExampleSentence="I work on the computer all day.",ExampleTranslation="Я работаю за компьютером весь день.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=tech,Transcription="[kəmˈpjuːtə]"},
                    new(){EnglishWord="keyboard",RussianTranslation="клавиатура",ExampleSentence="The keyboard is wireless.",ExampleTranslation="Клавиатура беспроводная.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=tech,Transcription="[ˈkiːbɔːd]"},
                    new(){EnglishWord="smartphone",RussianTranslation="смартфон",ExampleSentence="Everyone has a smartphone nowadays.",ExampleTranslation="Сегодня у всех есть смартфон.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=tech,Transcription="[ˈsmɑːtfəʊn]"},
                    new(){EnglishWord="internet",RussianTranslation="интернет",ExampleSentence="The internet connects people worldwide.",ExampleTranslation="Интернет соединяет людей по всему миру.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=tech,Transcription="[ˈɪntənet]"},
                    new(){EnglishWord="software",RussianTranslation="программное обеспечение",ExampleSentence="We need to update the software.",ExampleTranslation="Нам нужно обновить ПО.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=tech,Transcription="[ˈsɒftweə]"},
                    new(){EnglishWord="hardware",RussianTranslation="аппаратное обеспечение",ExampleSentence="The hardware needs to be replaced.",ExampleTranslation="Аппаратное обеспечение нужно заменить.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=tech,Transcription="[ˈhɑːdweə]"},
                    new(){EnglishWord="database",RussianTranslation="база данных",ExampleSentence="The data is stored in a database.",ExampleTranslation="Данные хранятся в базе данных.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=tech,Transcription="[ˈdeɪtəbeɪs]"},
                    new(){EnglishWord="network",RussianTranslation="сеть",ExampleSentence="The network is down today.",ExampleTranslation="Сегодня сеть не работает.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=tech,Transcription="[ˈnetwɜːk]"},
                    new(){EnglishWord="algorithm",RussianTranslation="алгоритм",ExampleSentence="The algorithm sorts the data efficiently.",ExampleTranslation="Алгоритм эффективно сортирует данные.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=tech,Transcription="[ˈælɡərɪðəm]"},
                    new(){EnglishWord="encryption",RussianTranslation="шифрование",ExampleSentence="Encryption protects your data.",ExampleTranslation="Шифрование защищает ваши данные.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=tech,Transcription="[ɪnˈkrɪpʃən]"},
                    new(){EnglishWord="application",RussianTranslation="приложение",ExampleSentence="Download the application from the store.",ExampleTranslation="Скачайте приложение из магазина.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=tech,Transcription="[ˌæplɪˈkeɪʃən]"},
                    new(){EnglishWord="processor",RussianTranslation="процессор",ExampleSentence="This processor is very fast.",ExampleTranslation="Этот процессор очень быстрый.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=tech,Transcription="[ˈprəʊsesə]"},
                    new(){EnglishWord="cloud computing",RussianTranslation="облачные вычисления",ExampleSentence="Cloud computing changed how businesses work.",ExampleTranslation="Облачные вычисления изменили бизнес.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=tech,Transcription="[klaʊd kəmˈpjuːtɪŋ]"},
                    new(){EnglishWord="artificial intelligence",RussianTranslation="искусственный интеллект",ExampleSentence="Artificial intelligence is transforming industry.",ExampleTranslation="Искусственный интеллект преобразует промышленность.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=tech,Transcription="[ˌɑːtɪˈfɪʃəl ɪnˈtelɪdʒəns]"},
                    new(){EnglishWord="password",RussianTranslation="пароль",ExampleSentence="Choose a strong password.",ExampleTranslation="Выберите надёжный пароль.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=tech,Transcription="[ˈpɑːswɜːd]"},
                    new(){EnglishWord="download",RussianTranslation="загружать, скачивать",ExampleSentence="Download the file from the website.",ExampleTranslation="Скачайте файл с сайта.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=tech,Transcription="[ˈdaʊnləʊd]"},
                    new(){EnglishWord="upload",RussianTranslation="выгружать, загружать",ExampleSentence="Upload the document to the server.",ExampleTranslation="Загрузите документ на сервер.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=tech,Transcription="[ˈʌpləʊd]"},
                    new(){EnglishWord="cybersecurity",RussianTranslation="кибербезопасность",ExampleSentence="Cybersecurity is crucial for businesses.",ExampleTranslation="Кибербезопасность критически важна для бизнеса.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=tech,Transcription="[ˌsaɪbəsɪˈkjʊərɪti]"},
                    new(){EnglishWord="printer",RussianTranslation="принтер",ExampleSentence="The printer ran out of ink.",ExampleTranslation="В принтере закончились чернила.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=tech,Transcription="[ˈprɪntə]"},
                    new(){EnglishWord="microphone",RussianTranslation="микрофон",ExampleSentence="The microphone is not working.",ExampleTranslation="Микрофон не работает.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=tech,Transcription="[ˈmaɪkrəfəʊn]"},

                    // === BODY & HEALTH (20 words) ===
                    new(){EnglishWord="headache",RussianTranslation="головная боль",ExampleSentence="I have a terrible headache.",ExampleTranslation="У меня ужасная головная боль.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=health,Transcription="[ˈhedeɪk]"},
                    new(){EnglishWord="hospital",RussianTranslation="больница",ExampleSentence="She was taken to the hospital.",ExampleTranslation="Её доставили в больницу.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=health,Transcription="[ˈhɒspɪtəl]"},
                    new(){EnglishWord="medicine",RussianTranslation="лекарство, медицина",ExampleSentence="Take this medicine twice a day.",ExampleTranslation="Принимайте это лекарство дважды в день.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=health,Transcription="[ˈmedsɪn]"},
                    new(){EnglishWord="symptom",RussianTranslation="симптом",ExampleSentence="What are your symptoms?",ExampleTranslation="Каковы ваши симптомы?",DifficultyLevel=DifficultyLevel.Medium,CategoryId=health,Transcription="[ˈsɪmptəm]"},
                    new(){EnglishWord="temperature",RussianTranslation="температура",ExampleSentence="He has a high temperature.",ExampleTranslation="У него высокая температура.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=health,Transcription="[ˈtemprɪtʃə]"},
                    new(){EnglishWord="prescription",RussianTranslation="рецепт",ExampleSentence="You need a prescription for this medicine.",ExampleTranslation="Для этого лекарства нужен рецепт.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=health,Transcription="[prɪˈskrɪpʃən]"},
                    new(){EnglishWord="surgery",RussianTranslation="операция, хирургия",ExampleSentence="The surgery was successful.",ExampleTranslation="Операция прошла успешно.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=health,Transcription="[ˈsɜːdʒəri]"},
                    new(){EnglishWord="vaccination",RussianTranslation="вакцинация",ExampleSentence="Vaccination protects against diseases.",ExampleTranslation="Вакцинация защищает от болезней.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=health,Transcription="[ˌvæksɪˈneɪʃən]"},
                    new(){EnglishWord="allergy",RussianTranslation="аллергия",ExampleSentence="She has an allergy to cats.",ExampleTranslation="У неё аллергия на кошек.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=health,Transcription="[ˈælədʒi]"},
                    new(){EnglishWord="ambulance",RussianTranslation="скорая помощь",ExampleSentence="Call an ambulance immediately!",ExampleTranslation="Немедленно вызовите скорую помощь!",DifficultyLevel=DifficultyLevel.Medium,CategoryId=health,Transcription="[ˈæmbjʊləns]"},
                    new(){EnglishWord="vitamin",RussianTranslation="витамин",ExampleSentence="Take vitamin C during winter.",ExampleTranslation="Принимайте витамин С зимой.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=health,Transcription="[ˈvɪtəmɪn]"},
                    new(){EnglishWord="pharmacy",RussianTranslation="аптека",ExampleSentence="I bought cough syrup at the pharmacy.",ExampleTranslation="Я купил сироп от кашля в аптеке.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=health,Transcription="[ˈfɑːməsi]"},
                    new(){EnglishWord="doctor",RussianTranslation="врач",ExampleSentence="The doctor examined the patient.",ExampleTranslation="Врач осмотрел пациента.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=health,Transcription="[ˈdɒktə]"},
                    new(){EnglishWord="nurse",RussianTranslation="медсестра",ExampleSentence="The nurse gave him an injection.",ExampleTranslation="Медсестра сделала ему укол.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=health,Transcription="[nɜːs]"},
                    new(){EnglishWord="diet",RussianTranslation="диета, питание",ExampleSentence="A healthy diet keeps you fit.",ExampleTranslation="Здоровое питание помогает быть в форме.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=health,Transcription="[ˈdaɪət]"},
                    new(){EnglishWord="exercise",RussianTranslation="упражнение, физкультура",ExampleSentence="Regular exercise is good for health.",ExampleTranslation="Регулярные упражнения полезны для здоровья.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=health,Transcription="[ˈeksəsaɪz]"},
                    new(){EnglishWord="fracture",RussianTranslation="перелом",ExampleSentence="She suffered a fracture in her arm.",ExampleTranslation="Она получила перелом руки.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=health,Transcription="[ˈfræktʃə]"},
                    new(){EnglishWord="injection",RussianTranslation="укол, инъекция",ExampleSentence="The doctor gave him an injection.",ExampleTranslation="Врач сделал ему укол.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=health,Transcription="[ɪnˈdʒekʃən]"},
                    new(){EnglishWord="bandage",RussianTranslation="бинт, повязка",ExampleSentence="The nurse put a bandage on the wound.",ExampleTranslation="Медсестра наложила повязку на рану.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=health,Transcription="[ˈbændɪdʒ]"},
                    new(){EnglishWord="heartbeat",RussianTranslation="сердцебиение",ExampleSentence="His heartbeat was irregular.",ExampleTranslation="Его сердцебиение было нерегулярным.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=health,Transcription="[ˈhɑːtbiːt]"},

                    // === NATURE (20 words) ===
                    new(){EnglishWord="tree",RussianTranslation="дерево",ExampleSentence="The tree is very tall.",ExampleTranslation="Дерево очень высокое.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=nature,Transcription="[triː]"},
                    new(){EnglishWord="mountain",RussianTranslation="гора",ExampleSentence="We climbed the mountain at dawn.",ExampleTranslation="Мы взобрались на гору на рассвете.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=nature,Transcription="[ˈmaʊntɪn]"},
                    new(){EnglishWord="waterfall",RussianTranslation="водопад",ExampleSentence="The waterfall is breathtaking.",ExampleTranslation="Водопад захватывает дух.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=nature,Transcription="[ˈwɔːtəfɔːl]"},
                    new(){EnglishWord="earthquake",RussianTranslation="землетрясение",ExampleSentence="The earthquake damaged many buildings.",ExampleTranslation="Землетрясение повредило много зданий.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=nature,Transcription="[ˈɜːθkweɪk]"},
                    new(){EnglishWord="river",RussianTranslation="река",ExampleSentence="The river flows through the city.",ExampleTranslation="Река течёт через город.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=nature,Transcription="[ˈrɪvə]"},
                    new(){EnglishWord="forest",RussianTranslation="лес",ExampleSentence="We walked through the forest.",ExampleTranslation="Мы шли через лес.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=nature,Transcription="[ˈfɒrɪst]"},
                    new(){EnglishWord="ocean",RussianTranslation="океан",ExampleSentence="The ocean covers most of the Earth.",ExampleTranslation="Океан покрывает большую часть Земли.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=nature,Transcription="[ˈəʊʃən]"},
                    new(){EnglishWord="desert",RussianTranslation="пустыня",ExampleSentence="The Sahara is the largest desert.",ExampleTranslation="Сахара — самая большая пустыня.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=nature,Transcription="[ˈdezət]"},
                    new(){EnglishWord="volcano",RussianTranslation="вулкан",ExampleSentence="The volcano erupted last night.",ExampleTranslation="Вулкан извергся прошлой ночью.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=nature,Transcription="[vɒlˈkeɪnəʊ]"},
                    new(){EnglishWord="tsunami",RussianTranslation="цунами",ExampleSentence="A tsunami hit the coastline.",ExampleTranslation="Цунами обрушилось на побережье.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=nature,Transcription="[tsuːˈnɑːmi]"},
                    new(){EnglishWord="hurricane",RussianTranslation="ураган",ExampleSentence="The hurricane destroyed many homes.",ExampleTranslation="Ураган разрушил много домов.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=nature,Transcription="[ˈhʌrɪkən]"},
                    new(){EnglishWord="glacier",RussianTranslation="ледник",ExampleSentence="The glacier is melting due to climate change.",ExampleTranslation="Ледник тает из-за изменения климата.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=nature,Transcription="[ˈɡlæsiə]"},
                    new(){EnglishWord="lake",RussianTranslation="озеро",ExampleSentence="We swam in the clear lake.",ExampleTranslation="Мы плавали в чистом озере.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=nature,Transcription="[leɪk]"},
                    new(){EnglishWord="cliff",RussianTranslation="утёс, скала",ExampleSentence="The cliff drops into the sea.",ExampleTranslation="Утёс обрывается в море.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=nature,Transcription="[klɪf]"},
                    new(){EnglishWord="meadow",RussianTranslation="луг",ExampleSentence="Cows graze in the meadow.",ExampleTranslation="Коровы пасутся на лугу.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=nature,Transcription="[ˈmedəʊ]"},
                    new(){EnglishWord="cave",RussianTranslation="пещера",ExampleSentence="The explorers entered the cave.",ExampleTranslation="Исследователи вошли в пещеру.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=nature,Transcription="[keɪv]"},
                    new(){EnglishWord="lightning",RussianTranslation="молния",ExampleSentence="Lightning lit up the sky.",ExampleTranslation="Молния осветила небо.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=nature,Transcription="[ˈlaɪtnɪŋ]"},
                    new(){EnglishWord="rainbow",RussianTranslation="радуга",ExampleSentence="A rainbow appeared after the rain.",ExampleTranslation="После дождя появилась радуга.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=nature,Transcription="[ˈreɪnbəʊ]"},
                    new(){EnglishWord="ecosystem",RussianTranslation="экосистема",ExampleSentence="Forests are important ecosystems.",ExampleTranslation="Леса — важные экосистемы.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=nature,Transcription="[ˈiːkəʊsɪstəm]"},
                    new(){EnglishWord="biodiversity",RussianTranslation="биоразнообразие",ExampleSentence="Biodiversity is threatened by pollution.",ExampleTranslation="Биоразнообразию угрожает загрязнение.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=nature,Transcription="[ˌbaɪəʊdaɪˈvɜːsɪti]"},

                    // === HOME (15 words) ===
                    new(){EnglishWord="kitchen",RussianTranslation="кухня",ExampleSentence="She cooks in the kitchen every evening.",ExampleTranslation="Она готовит на кухне каждый вечер.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=home,Transcription="[ˈkɪtʃɪn]"},
                    new(){EnglishWord="bedroom",RussianTranslation="спальня",ExampleSentence="The bedroom has a large window.",ExampleTranslation="В спальне большое окно.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=home,Transcription="[ˈbedrʊm]"},
                    new(){EnglishWord="bathroom",RussianTranslation="ванная комната",ExampleSentence="The bathroom has a bathtub.",ExampleTranslation="В ванной комнате есть ванна.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=home,Transcription="[ˈbɑːθruːm]"},
                    new(){EnglishWord="furniture",RussianTranslation="мебель",ExampleSentence="The furniture is made of wood.",ExampleTranslation="Мебель сделана из дерева.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=home,Transcription="[ˈfɜːnɪtʃə]"},
                    new(){EnglishWord="refrigerator",RussianTranslation="холодильник",ExampleSentence="The refrigerator is full of food.",ExampleTranslation="Холодильник полон еды.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=home,Transcription="[rɪˈfrɪdʒəreɪtə]"},
                    new(){EnglishWord="curtains",RussianTranslation="шторы",ExampleSentence="Close the curtains, please.",ExampleTranslation="Задёрните шторы, пожалуйста.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=home,Transcription="[ˈkɜːtənz]"},
                    new(){EnglishWord="ceiling",RussianTranslation="потолок",ExampleSentence="The ceiling is very high.",ExampleTranslation="Потолок очень высокий.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=home,Transcription="[ˈsiːlɪŋ]"},
                    new(){EnglishWord="wardrobe",RussianTranslation="шкаф для одежды",ExampleSentence="Hang your coat in the wardrobe.",ExampleTranslation="Повесьте пальто в шкаф.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=home,Transcription="[ˈwɔːdrəʊb]"},
                    new(){EnglishWord="fireplace",RussianTranslation="камин",ExampleSentence="We sat by the fireplace.",ExampleTranslation="Мы сидели у камина.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=home,Transcription="[ˈfaɪəpleɪs]"},
                    new(){EnglishWord="staircase",RussianTranslation="лестница",ExampleSentence="The staircase leads to the attic.",ExampleTranslation="Лестница ведёт на чердак.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=home,Transcription="[ˈsteəkeɪs]"},
                    new(){EnglishWord="carpet",RussianTranslation="ковёр",ExampleSentence="The carpet is soft and warm.",ExampleTranslation="Ковёр мягкий и тёплый.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=home,Transcription="[ˈkɑːpɪt]"},
                    new(){EnglishWord="balcony",RussianTranslation="балкон",ExampleSentence="We have breakfast on the balcony.",ExampleTranslation="Мы завтракаем на балконе.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=home,Transcription="[ˈbælkəni]"},
                    new(){EnglishWord="garage",RussianTranslation="гараж",ExampleSentence="The car is in the garage.",ExampleTranslation="Машина в гараже.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=home,Transcription="[ˈɡærɑːʒ]"},
                    new(){EnglishWord="basement",RussianTranslation="подвал",ExampleSentence="The storage is in the basement.",ExampleTranslation="Склад находится в подвале.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=home,Transcription="[ˈbeɪsmənt]"},
                    new(){EnglishWord="attic",RussianTranslation="чердак",ExampleSentence="Old boxes are stored in the attic.",ExampleTranslation="Старые коробки хранятся на чердаке.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=home,Transcription="[ˈætɪk]"},

                    // === WORK & BUSINESS (15 words) ===
                    new(){EnglishWord="meeting",RussianTranslation="встреча, совещание",ExampleSentence="The meeting starts at 9 AM.",ExampleTranslation="Совещание начинается в 9 утра.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=work,Transcription="[ˈmiːtɪŋ]"},
                    new(){EnglishWord="deadline",RussianTranslation="срок сдачи",ExampleSentence="The deadline is tomorrow.",ExampleTranslation="Срок сдачи — завтра.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=work,Transcription="[ˈdedlaɪn]"},
                    new(){EnglishWord="salary",RussianTranslation="зарплата",ExampleSentence="Her salary increased this year.",ExampleTranslation="Её зарплата выросла в этом году.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=work,Transcription="[ˈsæləri]"},
                    new(){EnglishWord="colleague",RussianTranslation="коллега",ExampleSentence="My colleague helped me with the project.",ExampleTranslation="Мой коллега помог мне с проектом.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=work,Transcription="[ˈkɒliːɡ]"},
                    new(){EnglishWord="negotiation",RussianTranslation="переговоры",ExampleSentence="The negotiation lasted three hours.",ExampleTranslation="Переговоры длились три часа.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=work,Transcription="[nɪˌɡəʊʃɪˈeɪʃən]"},
                    new(){EnglishWord="contract",RussianTranslation="контракт, договор",ExampleSentence="Please sign the contract.",ExampleTranslation="Пожалуйста, подпишите контракт.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=work,Transcription="[ˈkɒntrækt]"},
                    new(){EnglishWord="invoice",RussianTranslation="счёт-фактура",ExampleSentence="Send the invoice by email.",ExampleTranslation="Отправьте счёт по электронной почте.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=work,Transcription="[ˈɪnvɔɪs]"},
                    new(){EnglishWord="budget",RussianTranslation="бюджет",ExampleSentence="We need to stay within the budget.",ExampleTranslation="Нам нужно уложиться в бюджет.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=work,Transcription="[ˈbʌdʒɪt]"},
                    new(){EnglishWord="entrepreneur",RussianTranslation="предприниматель",ExampleSentence="She is a successful entrepreneur.",ExampleTranslation="Она успешный предприниматель.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=work,Transcription="[ˌɒntrəprəˈnɜː]"},
                    new(){EnglishWord="presentation",RussianTranslation="презентация",ExampleSentence="He gave an excellent presentation.",ExampleTranslation="Он сделал отличную презентацию.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=work,Transcription="[ˌprezənˈteɪʃən]"},
                    new(){EnglishWord="profit",RussianTranslation="прибыль",ExampleSentence="The company made a large profit.",ExampleTranslation="Компания получила большую прибыль.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=work,Transcription="[ˈprɒfɪt]"},
                    new(){EnglishWord="investment",RussianTranslation="инвестиция",ExampleSentence="It was a wise investment.",ExampleTranslation="Это была мудрая инвестиция.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=work,Transcription="[ɪnˈvestmənt]"},
                    new(){EnglishWord="strategy",RussianTranslation="стратегия",ExampleSentence="We need a new strategy.",ExampleTranslation="Нам нужна новая стратегия.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=work,Transcription="[ˈstrætədʒi]"},
                    new(){EnglishWord="revenue",RussianTranslation="доход, выручка",ExampleSentence="Revenue increased by 20% this quarter.",ExampleTranslation="Выручка выросла на 20% в этом квартале.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=work,Transcription="[ˈrevənjuː]"},
                    new(){EnglishWord="headquarters",RussianTranslation="штаб-квартира",ExampleSentence="The headquarters is in New York.",ExampleTranslation="Штаб-квартира находится в Нью-Йорке.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=work,Transcription="[ˈhedˌkwɔːtəz]"},

                    // === SPORTS (15 words) ===
                    new(){EnglishWord="football",RussianTranslation="футбол",ExampleSentence="He plays football every weekend.",ExampleTranslation="Он играет в футбол каждые выходные.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=sports,Transcription="[ˈfʊtbɔːl]"},
                    new(){EnglishWord="basketball",RussianTranslation="баскетбол",ExampleSentence="She loves playing basketball.",ExampleTranslation="Она любит играть в баскетбол.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=sports,Transcription="[ˈbɑːskɪtbɔːl]"},
                    new(){EnglishWord="swimming",RussianTranslation="плавание",ExampleSentence="Swimming is great exercise.",ExampleTranslation="Плавание — отличное упражнение.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=sports,Transcription="[ˈswɪmɪŋ]"},
                    new(){EnglishWord="gymnastics",RussianTranslation="гимнастика",ExampleSentence="She trained in gymnastics from age 5.",ExampleTranslation="Она занималась гимнастикой с 5 лет.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=sports,Transcription="[dʒɪmˈnæstɪks]"},
                    new(){EnglishWord="championship",RussianTranslation="чемпионат",ExampleSentence="Our team won the championship.",ExampleTranslation="Наша команда выиграла чемпионат.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=sports,Transcription="[ˈtʃæmpiənʃɪp]"},
                    new(){EnglishWord="marathon",RussianTranslation="марафон",ExampleSentence="He finished the marathon in 3 hours.",ExampleTranslation="Он пробежал марафон за 3 часа.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=sports,Transcription="[ˈmærəθən]"},
                    new(){EnglishWord="tournament",RussianTranslation="турнир",ExampleSentence="The tennis tournament starts next week.",ExampleTranslation="Теннисный турнир начнётся на следующей неделе.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=sports,Transcription="[ˈtʊənəmənt]"},
                    new(){EnglishWord="referee",RussianTranslation="судья, рефери",ExampleSentence="The referee blew the whistle.",ExampleTranslation="Судья свистнул.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=sports,Transcription="[ˌrefəˈriː]"},
                    new(){EnglishWord="stadium",RussianTranslation="стадион",ExampleSentence="The stadium holds 80,000 fans.",ExampleTranslation="Стадион вмещает 80 000 болельщиков.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=sports,Transcription="[ˈsteɪdiəm]"},
                    new(){EnglishWord="athlete",RussianTranslation="спортсмен, атлет",ExampleSentence="She is a professional athlete.",ExampleTranslation="Она профессиональный спортсмен.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=sports,Transcription="[ˈæθliːt]"},
                    new(){EnglishWord="volleyball",RussianTranslation="волейбол",ExampleSentence="They played volleyball on the beach.",ExampleTranslation="Они играли в волейбол на пляже.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=sports,Transcription="[ˈvɒlibɔːl]"},
                    new(){EnglishWord="cycling",RussianTranslation="велосипедный спорт",ExampleSentence="Cycling is eco-friendly transport.",ExampleTranslation="Езда на велосипеде — экологичный транспорт.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=sports,Transcription="[ˈsaɪklɪŋ]"},
                    new(){EnglishWord="rowing",RussianTranslation="гребля",ExampleSentence="Rowing is a great full-body workout.",ExampleTranslation="Гребля — отличная тренировка для всего тела.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=sports,Transcription="[ˈrəʊɪŋ]"},
                    new(){EnglishWord="archery",RussianTranslation="стрельба из лука",ExampleSentence="Archery requires great concentration.",ExampleTranslation="Стрельба из лука требует большой концентрации.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=sports,Transcription="[ˈɑːtʃəri]"},
                    new(){EnglishWord="weightlifting",RussianTranslation="тяжёлая атлетика",ExampleSentence="Weightlifting builds muscle and strength.",ExampleTranslation="Тяжёлая атлетика развивает мышцы и силу.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=sports,Transcription="[ˈweɪtlɪftɪŋ]"},

                    // === EDUCATION (15 words) ===
                    new(){EnglishWord="university",RussianTranslation="университет",ExampleSentence="She studies at the university.",ExampleTranslation="Она учится в университете.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=education,Transcription="[ˌjuːnɪˈvɜːsɪti]"},
                    new(){EnglishWord="library",RussianTranslation="библиотека",ExampleSentence="I borrowed a book from the library.",ExampleTranslation="Я взял книгу в библиотеке.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=education,Transcription="[ˈlaɪbrəri]"},
                    new(){EnglishWord="scholarship",RussianTranslation="стипендия",ExampleSentence="She received a scholarship for her studies.",ExampleTranslation="Она получила стипендию для учёбы.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=education,Transcription="[ˈskɒləʃɪp]"},
                    new(){EnglishWord="curriculum",RussianTranslation="учебный план",ExampleSentence="The curriculum includes maths and science.",ExampleTranslation="Учебный план включает математику и науки.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=education,Transcription="[kəˈrɪkjʊləm]"},
                    new(){EnglishWord="examination",RussianTranslation="экзамен",ExampleSentence="She passed all her examinations.",ExampleTranslation="Она сдала все экзамены.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=education,Transcription="[ɪɡˌzæmɪˈneɪʃən]"},
                    new(){EnglishWord="laboratory",RussianTranslation="лаборатория",ExampleSentence="Students do experiments in the laboratory.",ExampleTranslation="Студенты проводят опыты в лаборатории.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=education,Transcription="[ləˈbɒrətri]"},
                    new(){EnglishWord="textbook",RussianTranslation="учебник",ExampleSentence="Open your textbook to page 42.",ExampleTranslation="Откройте учебник на странице 42.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=education,Transcription="[ˈtekstbʊk]"},
                    new(){EnglishWord="homework",RussianTranslation="домашнее задание",ExampleSentence="She always does her homework.",ExampleTranslation="Она всегда делает домашнее задание.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=education,Transcription="[ˈhəʊmwɜːk]"},
                    new(){EnglishWord="graduation",RussianTranslation="окончание учёбы, выпуск",ExampleSentence="His graduation ceremony is next month.",ExampleTranslation="Его выпускная церемония — в следующем месяце.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=education,Transcription="[ˌɡrædʒʊˈeɪʃən]"},
                    new(){EnglishWord="professor",RussianTranslation="профессор",ExampleSentence="The professor gave a lecture.",ExampleTranslation="Профессор читал лекцию.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=education,Transcription="[prəˈfesə]"},
                    new(){EnglishWord="classroom",RussianTranslation="класс, аудитория",ExampleSentence="The classroom has 30 students.",ExampleTranslation="В классе 30 студентов.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=education,Transcription="[ˈklɑːsruːm]"},
                    new(){EnglishWord="essay",RussianTranslation="сочинение, эссе",ExampleSentence="Write a 500-word essay.",ExampleTranslation="Напишите эссе из 500 слов.",DifficultyLevel=DifficultyLevel.Easy,CategoryId=education,Transcription="[ˈeseɪ]"},
                    new(){EnglishWord="dissertation",RussianTranslation="диссертация",ExampleSentence="She is writing her doctoral dissertation.",ExampleTranslation="Она пишет докторскую диссертацию.",DifficultyLevel=DifficultyLevel.Hard,CategoryId=education,Transcription="[ˌdɪsəˈteɪʃən]"},
                    new(){EnglishWord="semester",RussianTranslation="семестр",ExampleSentence="The semester ends in December.",ExampleTranslation="Семестр заканчивается в декабре.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=education,Transcription="[sɪˈmestə]"},
                    new(){EnglishWord="knowledge",RussianTranslation="знание",ExampleSentence="Knowledge is power.",ExampleTranslation="Знание — сила.",DifficultyLevel=DifficultyLevel.Medium,CategoryId=education,Transcription="[ˈnɒlɪdʒ]"},
                };

                var toAdd = words.Where(w => !existing.Contains(w.EnglishWord.ToLower())).ToList();
                if (toAdd.Any()) { ctx.Words.AddRange(toAdd); ctx.SaveChanges(); }
            }

            // === SEED TENSES ===
            SeedTenses(ctx);

            // === SEED LESSONS ===
            if (!ctx.Lessons.Any())
            {
                var lessons = new List<Lesson> {
                    new(){Title="Basic Animals",Description="Изучите базовые названия животных",DifficultyLevel=DifficultyLevel.Easy,OrderNumber=1,IconEmoji="🐾"},
                    new(){Title="Food & Kitchen",Description="Еда, напитки и кухонные принадлежности",DifficultyLevel=DifficultyLevel.Easy,OrderNumber=2,IconEmoji="🍎"},
                    new(){Title="Travel Essentials",Description="Слова для путешественника",DifficultyLevel=DifficultyLevel.Medium,OrderNumber=3,IconEmoji="✈️"},
                    new(){Title="Technology World",Description="Современные технологии",DifficultyLevel=DifficultyLevel.Medium,OrderNumber=4,IconEmoji="💻"},
                    new(){Title="Nature & Environment",Description="Природа и окружающая среда",DifficultyLevel=DifficultyLevel.Hard,OrderNumber=5,IconEmoji="🌿"},
                    new(){Title="Body & Health",Description="Здоровье и медицина",DifficultyLevel=DifficultyLevel.Medium,OrderNumber=6,IconEmoji="🏥"},
                    new(){Title="Home & Living",Description="Дом и быт",DifficultyLevel=DifficultyLevel.Easy,OrderNumber=7,IconEmoji="🏠"},
                    new(){Title="Work & Business",Description="Бизнес и карьера",DifficultyLevel=DifficultyLevel.Hard,OrderNumber=8,IconEmoji="💼"},
                    new(){Title="Sports & Fitness",Description="Спорт и физкультура",DifficultyLevel=DifficultyLevel.Medium,OrderNumber=9,IconEmoji="⚽"},
                    new(){Title="Education",Description="Образование и учёба",DifficultyLevel=DifficultyLevel.Medium,OrderNumber=10,IconEmoji="📚"},
                };
                ctx.Lessons.AddRange(lessons);
                ctx.SaveChanges();

                // Link words to lessons by category
                var allWords = ctx.Words.Include(w => w.Category).ToList();
                var lw = new List<LessonWord>();
                void LinkWords(Lesson lesson, string catName)
                {
                    var catWords = allWords.Where(w => w.Category.Name == catName).ToList();
                    for (int i = 0; i < catWords.Count; i++)
                        lw.Add(new() { LessonId = lesson.LessonId, WordId = catWords[i].WordId, OrderIndex = i + 1 });
                }
                LinkWords(lessons[0], "Animals");
                LinkWords(lessons[1], "Food & Drink");
                LinkWords(lessons[2], "Travel");
                LinkWords(lessons[3], "Technology");
                LinkWords(lessons[4], "Nature");
                LinkWords(lessons[5], "Body & Health");
                LinkWords(lessons[6], "Home");
                LinkWords(lessons[7], "Work & Business");
                LinkWords(lessons[8], "Sports");
                LinkWords(lessons[9], "Education");
                ctx.LessonWords.AddRange(lw);
                ctx.SaveChanges();
            }

            // === SEED EXERCISES ===
            if (ctx.Exercises.Count() < 20)
            {
                SeedExercises(ctx);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // TENSES
        // ─────────────────────────────────────────────────────────────────────
        private static void SeedTenses(AppDbContext ctx)
        {
            var tenseDefs = new[]
            {
                ("Present Simple", 1),
                ("Present Continuous", 2),
                ("Present Perfect", 3),
                ("Present Perfect Continuous", 4),
                ("Past Simple", 5),
                ("Past Continuous", 6),
                ("Past Perfect", 7),
                ("Past Perfect Continuous", 8),
                ("Future Simple", 9),
                ("Future Continuous", 10),
                ("Future Perfect", 11),
                ("Future Perfect Continuous", 12),
                ("Zero Conditional", 13),
                ("First Conditional", 14),
                ("Second Conditional", 15),
                ("Third Conditional", 16),
            };

            foreach (var (name, order) in tenseDefs)
            {
                if (!ctx.Tenses.Any(t => t.Name == name))
                {
                    var t = CreateTense(name, order);
                    ctx.Tenses.Add(t);
                    ctx.SaveChanges();
                    var rules = CreateRules(t.TenseId, name);
                    ctx.GrammarRules.AddRange(rules);
                    ctx.SaveChanges();
                }
            }
        }

        private static Tense CreateTense(string name, int order) => name switch
        {
            "Present Simple" => TenseInfo(name, "Настоящее простое время. Регулярные действия, привычки и общие факты.", "Subject + V1 (+s/es для he/she/it)", "I work every day.\nShe works in a hospital.\nThey play football on Sundays.\nHe doesn't eat meat.\nDo you speak English?", order),
            "Present Continuous" => TenseInfo(name, "Настоящее длительное время. Действия происходят сейчас или в текущий период.", "Subject + am/is/are + V-ing", "I am working now.\nShe is reading a book.\nThey are building a house.\nWe are not watching TV.\nIs he sleeping?", order),
            "Present Perfect" => TenseInfo(name, "Настоящее совершённое время. Прошлое действие важно своим результатом сейчас.", "Subject + have/has + V3", "I have finished my homework.\nShe has already eaten.\nThey have just arrived.\nHe has never seen snow.\nHave you been to Paris?", order),
            "Present Perfect Continuous" => TenseInfo(name, "Действие началось в прошлом и продолжается сейчас или только что закончилось.", "Subject + have/has + been + V-ing", "I have been studying for two hours.\nShe has been working since morning.\nIt has been raining all day.\nThey have been waiting.\nHow long have you been learning English?", order),
            "Past Simple" => TenseInfo(name, "Прошедшее простое время. Завершённые действия в конкретный момент прошлого.", "Subject + V2", "I worked yesterday.\nShe visited Paris last summer.\nThey played football last week.\nHe didn't come.\nWhen did you arrive?", order),
            "Past Continuous" => TenseInfo(name, "Прошедшее длительное время. Действие было в процессе в момент прошлого.", "Subject + was/were + V-ing", "I was working at 5 PM.\nShe was reading when I called.\nThey were sleeping.\nWe were not watching TV.\nWhat were you doing?", order),
            "Past Perfect" => TenseInfo(name, "Прошедшее совершённое время. Действие завершилось до другого момента в прошлом.", "Subject + had + V3", "She had left before I arrived.\nThey had finished dinner.\nI had never tried sushi before.\nHe had already called.\nHad you met her before?", order),
            "Past Perfect Continuous" => TenseInfo(name, "Действие продолжалось до другого момента в прошлом.", "Subject + had + been + V-ing", "She had been working for 10 hours.\nI had been waiting for 30 minutes.\nThey had been arguing.\nHe had been studying all night.\nHow long had you been living there?", order),
            "Future Simple" => TenseInfo(name, "Будущее простое время. Предсказания, обещания и спонтанные решения.", "Subject + will + V1", "I will call you tomorrow.\nShe will be a doctor.\nThey won't come.\nI think it will rain.\nWill you help me?", order),
            "Future Continuous" => TenseInfo(name, "Будущее длительное время. Действие будет происходить в определённый момент будущего.", "Subject + will + be + V-ing", "At 8 PM I will be studying.\nShe will be working when you arrive.\nWe will be flying to Rome.\nThey won't be sleeping.\nWhat will you be doing?", order),
            "Future Perfect" => TenseInfo(name, "Будущее совершённое время. Действие завершится к определённому моменту в будущем.", "Subject + will + have + V3", "I will have finished by Monday.\nShe will have left by then.\nThey will have built the house.\nHe won't have arrived yet.\nWill you have completed it?", order),
            "Future Perfect Continuous" => TenseInfo(name, "Действие будет длиться до определённого момента в будущем.", "Subject + will + have + been + V-ing", "By June I will have been working here for a year.\nShe will have been studying for three hours.\nThey will have been travelling for a week.\nHe won't have been sleeping long.\nHow long will you have been learning English?", order),
            "Zero Conditional" => TenseInfo(name, "Нулевое условное. Общие истины и закономерности.", "If + Present Simple, Present Simple", "If you heat ice, it melts.\nIf people don't eat, they get hungry.\nWater boils if you heat it.\nIf it rains, the ground gets wet.\nPlants die if they don't get water.", order),
            "First Conditional" => TenseInfo(name, "Первое условное. Реальное условие в будущем.", "If + Present Simple, will + V1", "If it rains, I will stay home.\nIf you study, you will pass.\nShe will call if she has time.\nIf we hurry, we won't be late.\nWill you come if I invite you?", order),
            "Second Conditional" => TenseInfo(name, "Второе условное. Нереальная или маловероятная ситуация сейчас/в будущем.", "If + Past Simple, would + V1", "If I had more time, I would travel.\nIf she knew the answer, she would tell us.\nI would buy a car if I had money.\nIf we lived near the sea, we would swim.\nWhat would you do if you won?", order),
            "Third Conditional" => TenseInfo(name, "Третье условное. Нереальная ситуация в прошлом и её возможный результат.", "If + Past Perfect, would have + V3", "If I had studied, I would have passed.\nShe would have called if she had known.\nIf they had left earlier, they wouldn't have missed the train.\nWe would have helped if you had asked.\nWhat would you have done?", order),
            _ => TenseInfo(name, "Грамматическая тема для изучения английского языка.", "See examples", "Study the rule and practise with exercises.", order)
        };

        private static Tense TenseInfo(string name, string description, string formula, string examples, int order) => new()
        {
            Name = name,
            Description = description,
            Formula = formula,
            Examples = examples,
            OrderIndex = order
        };

        private static List<GrammarRule> CreateRules(int tenseId, string tenseName)
        {
            return new List<GrammarRule>
            {
                new()
                {
                    TenseId = tenseId,
                    Title = $"{tenseName}: образование",
                    Content = $"Изучите формулу темы {tenseName}, порядок слов в утверждении, отрицании и вопросе.",
                    Examples = "Positive: I study English.\nNegative: I do not study English.\nQuestion: Do I study English?",
                    DifficultyLevel = DifficultyLevel.Easy
                },
                new()
                {
                    TenseId = tenseId,
                    Title = $"{tenseName}: употребление",
                    Content = $"Обращайте внимание на маркеры времени и ситуацию, в которой используется {tenseName}.",
                    Examples = "Read the examples, compare Russian meaning and build your own sentences.",
                    DifficultyLevel = DifficultyLevel.Medium
                }
            };
        }

        private static void SeedExercises(AppDbContext ctx)
        {
            var words = ctx.Words.OrderBy(w => w.WordId).Take(40).ToList();
            var tenses = ctx.Tenses.OrderBy(t => t.OrderIndex).ToList();

            AddExercise(ctx, "Перевод слов", "Выберите русский перевод английского слова", ExerciseType.Translation, DifficultyLevel.Easy,
                words.Take(12).Select((w, i) => Question($"Как переводится слово '{w.EnglishWord}'?", w.RussianTranslation,
                    words.Select(x => x.RussianTranslation).Where(x => x != w.RussianTranslation).Take(3).Append(w.RussianTranslation).ToList(), i + 1)));

            AddExercise(ctx, "Правописание слов", "Напишите английское слово по русскому переводу", ExerciseType.Spelling, DifficultyLevel.Easy,
                words.Skip(12).Take(12).Select((w, i) => Question($"Напишите по-английски: {w.RussianTranslation}", w.EnglishWord, null, i + 1, $"Первая буква: {w.EnglishWord.First()}")));

            AddExercise(ctx, "Времена английского", "Выберите правильную формулу времени", ExerciseType.Tenses, DifficultyLevel.Medium,
                tenses.Take(12).Select((t, i) => Question($"Какая формула у {t.Name}?", t.Formula,
                    tenses.Select(x => x.Formula).Where(x => x != t.Formula).Take(3).Append(t.Formula).ToList(), i + 1)));
        }

        private static void AddExercise(AppDbContext ctx, string title, string description, ExerciseType type, DifficultyLevel difficulty, IEnumerable<ExerciseQuestion> questions)
        {
            if (ctx.Exercises.Any(e => e.Title == title)) return;
            var exercise = new Exercise
            {
                Title = title,
                Description = description,
                Type = type,
                DifficultyLevel = difficulty,
                TimeLimit = 600,
                IsActive = true,
                Questions = questions.ToList()
            };
            ctx.Exercises.Add(exercise);
            ctx.SaveChanges();
        }

        private static ExerciseQuestion Question(string text, string answer, List<string>? options, int order, string? hint = null) => new()
        {
            QuestionText = text,
            CorrectAnswer = answer,
            Options = options == null ? null : Newtonsoft.Json.JsonConvert.SerializeObject(options.Distinct().ToList()),
            Hint = hint,
            Points = 10,
            OrderIndex = order
        };
    }
}
