using System.ComponentModel.DataAnnotations;
namespace EnglishTutor.Data.Models
{
    public class User
    {
        public int UserId { get; set; }
        [Required, MaxLength(100)] public string Username { get; set; } = string.Empty;
        [Required, MaxLength(255)] public string PasswordHash { get; set; } = string.Empty;
        [MaxLength(200)] public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Student;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public ICollection<StudentProgress> Progresses { get; set; } = new List<StudentProgress>();
        public ICollection<Achievement> Achievements { get; set; } = new List<Achievement>();
    }
    public enum UserRole { Student = 1, Admin = 2 }
}
