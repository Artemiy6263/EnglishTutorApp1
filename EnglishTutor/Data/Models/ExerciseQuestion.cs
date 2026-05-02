using System.ComponentModel.DataAnnotations;

namespace EnglishTutor.Data.Models
{
    public class ExerciseQuestion
    {
        [Key]
        public int QuestionId { get; set; }

        public int ExerciseId { get; set; }
        public Exercise Exercise { get; set; } = null!;

        [Required]
        public string QuestionText { get; set; } = string.Empty;

        [Required]
        public string CorrectAnswer { get; set; } = string.Empty;

        public string? Options { get; set; }
        public string? Hint { get; set; }
        public int Points { get; set; } = 10;
        public int OrderIndex { get; set; }
    }
}
