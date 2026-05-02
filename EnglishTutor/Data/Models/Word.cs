using System.ComponentModel.DataAnnotations;
namespace EnglishTutor.Data.Models
{
    public class Word
    {
        public int WordId { get; set; }
        [Required, MaxLength(200)] public string EnglishWord { get; set; } = string.Empty;
        [Required, MaxLength(200)] public string RussianTranslation { get; set; } = string.Empty;
        [MaxLength(500)] public string ExampleSentence { get; set; } = string.Empty;
        [MaxLength(500)] public string ExampleTranslation { get; set; } = string.Empty;
        public DifficultyLevel DifficultyLevel { get; set; } = DifficultyLevel.Easy;
        public int CategoryId { get; set; }
        public WordCategory Category { get; set; } = null!;
        public string? Transcription { get; set; }
        public string? AudioUrl { get; set; }
        // Путь к изображению слова. Пример: @"Images\Words\cat.jpg"
        // Чтобы добавить картинку: положите файл в папку Images/Words/ и укажите путь здесь
        public string? ImagePath { get; set; }
        public ICollection<LessonWord> LessonWords { get; set; } = new List<LessonWord>();
    }
    public enum DifficultyLevel { Easy = 1, Medium = 2, Hard = 3 }
}
