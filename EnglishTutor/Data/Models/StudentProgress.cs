using System.ComponentModel.DataAnnotations;

namespace EnglishTutor.Data.Models
{
    public class StudentProgress
    {

        [Key]
        public int ProgressId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int ExerciseId { get; set; }
        public Exercise Exercise { get; set; } = null!;
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public int Attempts { get; set; } = 1;
        public int TimeSpentSeconds { get; set; }
        public DateTime CompletedAt { get; set; } = DateTime.Now;
        public bool IsPassed => MaxScore > 0 && (double)Score / MaxScore >= 0.6;
    }
}
