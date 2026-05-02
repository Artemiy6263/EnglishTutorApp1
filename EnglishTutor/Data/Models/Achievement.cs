using System.ComponentModel.DataAnnotations;
namespace EnglishTutor.Data.Models
{
    public class Achievement
    {
        [Key]
        public int AchievementId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        [Required, MaxLength(200)] public string Title { get; set; } = string.Empty;
        [MaxLength(500)] public string Description { get; set; } = string.Empty;
        public string IconEmoji { get; set; } = "🏆";
        public DateTime EarnedAt { get; set; } = DateTime.Now;
    }
}
