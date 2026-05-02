using EnglishTutor.Data.Models;
using Microsoft.EntityFrameworkCore;
namespace EnglishTutor.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<WordCategory> WordCategories { get; set; }
        public DbSet<Word> Words { get; set; }
        public DbSet<GrammarRule> GrammarRules { get; set; }
        public DbSet<Tense> Tenses { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<LessonWord> LessonWords { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<ExerciseQuestion> ExerciseQuestions { get; set; }
        public DbSet<StudentProgress> StudentProgresses { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder o) =>
            o.UseSqlServer(App.ConnectionString);
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
            modelBuilder.Entity<WordCategory>().HasKey(e => e.CategoryId);
            modelBuilder.Entity<GrammarRule>().HasKey(e => e.RuleId);
            modelBuilder.Entity<ExerciseQuestion>().HasKey(e => e.QuestionId);
            modelBuilder.Entity<StudentProgress>().HasKey(e => e.ProgressId);

            
            modelBuilder.Entity<LessonWord>()
                .HasOne(lw => lw.Lesson)
                .WithMany(l => l.LessonWords)
                .HasForeignKey(lw => lw.LessonId);

            modelBuilder.Entity<LessonWord>()
                .HasOne(lw => lw.Word)
                .WithMany(w => w.LessonWords)
                .HasForeignKey(lw => lw.WordId);

            modelBuilder.Entity<StudentProgress>()
                .HasOne(sp => sp.User)
                .WithMany(u => u.Progresses)
                .HasForeignKey(sp => sp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudentProgress>()
                .HasOne(sp => sp.Exercise)
                .WithMany(e => e.Progresses)
                .HasForeignKey(sp => sp.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Achievement>()
                .HasOne(a => a.User)
                .WithMany(u => u.Achievements)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
