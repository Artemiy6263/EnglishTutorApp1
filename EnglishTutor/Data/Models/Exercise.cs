using System.ComponentModel.DataAnnotations;
namespace EnglishTutor.Data.Models
{
    public class Exercise
    {
        public int ExerciseId { get; set; }
        [Required, MaxLength(200)] public string Title { get; set; } = string.Empty;
        [MaxLength(500)] public string Description { get; set; } = string.Empty;
        public ExerciseType Type { get; set; }
        public DifficultyLevel DifficultyLevel { get; set; }
        public int? LessonId { get; set; }
        public Lesson? Lesson { get; set; }
        public int? TenseId { get; set; }
        public Tense? Tense { get; set; }
        public bool IsActive { get; set; } = true;
        public int TimeLimit { get; set; } = 300;
        public ICollection<ExerciseQuestion> Questions { get; set; } = new List<ExerciseQuestion>();
        public ICollection<StudentProgress> Progresses { get; set; } = new List<StudentProgress>();
    }
    public enum ExerciseType { Translation = 1, Spelling = 2, Tenses = 3 }
}
