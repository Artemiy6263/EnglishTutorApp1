using System.ComponentModel.DataAnnotations;
namespace EnglishTutor.Data.Models
{
    public class Tense
    {
        public int TenseId { get; set; }
        [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
        [MaxLength(1000)] public string Description { get; set; } = string.Empty;
        [MaxLength(300)] public string Formula { get; set; } = string.Empty;
        public string Examples { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public ICollection<GrammarRule> GrammarRules { get; set; } = new List<GrammarRule>();
        public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
    }
}
