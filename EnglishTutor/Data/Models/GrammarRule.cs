using System.ComponentModel.DataAnnotations;
namespace EnglishTutor.Data.Models
{
    public class GrammarRule
    {
        public int RuleId { get; set; }
        [Required, MaxLength(200)] public string Title { get; set; } = string.Empty;
        [Required] public string Content { get; set; } = string.Empty;
        public string Examples { get; set; } = string.Empty;
        public DifficultyLevel DifficultyLevel { get; set; } = DifficultyLevel.Easy;
        public int? TenseId { get; set; }
        public Tense? Tense { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
