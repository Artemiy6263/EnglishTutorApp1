using System.ComponentModel.DataAnnotations;
namespace EnglishTutor.Data.Models
{
    public class WordCategory
    {
        public int CategoryId { get; set; }
        [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
        [MaxLength(500)] public string Description { get; set; } = string.Empty;
        public string IconEmoji { get; set; } = "📚";
        public ICollection<Word> Words { get; set; } = new List<Word>();
    }
}
