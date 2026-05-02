using EnglishTutor.Data;
using EnglishTutor.Data.Models;
using Microsoft.EntityFrameworkCore;
namespace EnglishTutor.Services
{
    public class ExerciseService
    {
        public static List<Exercise> GetExercises(ExerciseType? type=null, DifficultyLevel? difficulty=null)
        {
            using var ctx = new AppDbContext();
            var q = ctx.Exercises.Include(e=>e.Lesson).Include(e=>e.Tense).Include(e=>e.Questions).Where(e=>e.IsActive);
            if(type.HasValue) q=q.Where(e=>e.Type==type.Value);
            if(difficulty.HasValue) q=q.Where(e=>e.DifficultyLevel==difficulty.Value);
            return q.OrderBy(e=>e.DifficultyLevel).ThenBy(e=>e.Title).ToList();
        }
        public static List<ExerciseQuestion> GetQuestions(int exerciseId) { using var ctx=new AppDbContext(); return ctx.ExerciseQuestions.Where(q=>q.ExerciseId==exerciseId).OrderBy(q=>q.OrderIndex).ToList(); }
        public static void SaveProgress(int userId, int exerciseId, int score, int maxScore, int timeSeconds)
        {
            using var ctx = new AppDbContext();
            var ex = ctx.StudentProgresses.FirstOrDefault(p=>p.UserId==userId&&p.ExerciseId==exerciseId);
            if(ex!=null) { ex.Attempts++; if(score>ex.Score){ex.Score=score;ex.MaxScore=maxScore;} ex.CompletedAt=DateTime.Now; }
            else ctx.StudentProgresses.Add(new StudentProgress{UserId=userId,ExerciseId=exerciseId,Score=score,MaxScore=maxScore,TimeSpentSeconds=timeSeconds,CompletedAt=DateTime.Now});
            ctx.SaveChanges();
            var count = ctx.StudentProgresses.Count(p=>p.UserId==userId);
            var achs = ctx.Achievements.Where(a=>a.UserId==userId).ToList();
            void TryAdd(string title,string desc,string icon){ if(!achs.Any(a=>a.Title==title)) ctx.Achievements.Add(new Achievement{UserId=userId,Title=title,Description=desc,IconEmoji=icon,EarnedAt=DateTime.Now}); }
            if(count>=1) TryAdd("Первый шаг","Выполните первое задание","🎯");
            if(count>=5) TryAdd("Отличный старт","Выполните 5 заданий","⭐");
            if(count>=10) TryAdd("Усердный студент","Выполните 10 заданий","📚");
            ctx.SaveChanges();
        }
        public static List<StudentProgress> GetUserProgress(int userId) { using var ctx=new AppDbContext(); return ctx.StudentProgresses.Include(p=>p.Exercise).Where(p=>p.UserId==userId).OrderByDescending(p=>p.CompletedAt).ToList(); }
        public static Dictionary<string,int> GetUserStats(int userId) { using var ctx=new AppDbContext(); var p=ctx.StudentProgresses.Where(x=>x.UserId==userId).ToList(); return new(){["TotalExercises"]=p.Count,["PassedExercises"]=p.Count(x=>x.IsPassed),["TotalScore"]=p.Sum(x=>x.Score),["TotalTime"]=p.Sum(x=>x.TimeSpentSeconds)}; }
        public static List<Achievement> GetUserAchievements(int userId) { using var ctx=new AppDbContext(); return ctx.Achievements.Where(a=>a.UserId==userId).OrderByDescending(a=>a.EarnedAt).ToList(); }
        public static List<Exercise> GetAllExercisesAdmin() { using var ctx=new AppDbContext(); return ctx.Exercises.Include(e=>e.Questions).Include(e=>e.Lesson).Include(e=>e.Tense).OrderBy(e=>e.Type).ThenBy(e=>e.DifficultyLevel).ToList(); }
        public static void DeleteExercise(int id) { using var ctx=new AppDbContext(); var e=ctx.Exercises.Include(x=>x.Questions).Include(x=>x.Progresses).FirstOrDefault(x=>x.ExerciseId==id); if(e!=null){ctx.Exercises.Remove(e);ctx.SaveChanges();} }
    }
}
