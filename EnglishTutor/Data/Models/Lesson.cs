using System.ComponentModel.DataAnnotations;
namespace EnglishTutor.Data.Models
{
    public class Lesson
    {
        public int LessonId { get; set; }
        [Required, MaxLength(200)] public string Title { get; set; } = string.Empty;
        [MaxLength(1000)] public string Description { get; set; } = string.Empty;
        public DifficultyLevel DifficultyLevel { get; set; } = DifficultyLevel.Easy;
        public int OrderNumber { get; set; }
        public string IconEmoji { get; set; } = "📖";
        public bool IsActive { get; set; } = true;
        public ICollection<LessonWord> LessonWords { get; set; } = new List<LessonWord>();
        public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
    }
}
