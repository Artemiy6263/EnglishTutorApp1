using System; using System.Linq; using System.Windows;
using EnglishTutor.Data; using EnglishTutor.Data.Models; using EnglishTutor.Services; using EnglishTutor.Windows;
using Microsoft.EntityFrameworkCore;
namespace EnglishTutor.Windows.Admin
{
    public partial class StatisticsWindow : Window
    {
        public StatisticsWindow(){InitializeComponent();Load();}
        private void Load(){try{using var ctx=new AppDbContext();TxtStudents.Text=ctx.Users.Count(u=>u.Role==UserRole.Student).ToString();TxtPassed.Text=ctx.StudentProgresses.Count(p=>p.MaxScore>0&&(double)p.Score/p.MaxScore>=0.6).ToString();TxtTotalScore.Text=(ctx.StudentProgresses.Sum(p=>(int?)p.Score)??0).ToString();DgByStudent.ItemsSource=ctx.StudentProgresses.Include(p=>p.User).GroupBy(p=>p.User).Select(g=>new{Student=g.Key.Username,Count=g.Count(),BestScore=g.Max(x=>x.Score),Last=g.Max(x=>x.CompletedAt)}).ToList();DgByExercise.ItemsSource=ctx.StudentProgresses.Include(p=>p.Exercise).GroupBy(p=>p.Exercise).Select(g=>new{Exercise=g.Key.Title,Type=g.Key.Type.ToString(),Attempts=g.Sum(x=>x.Attempts),AvgScore=g.Average(x=>(double)x.Score)}).ToList();}catch(Exception ex){MessageBox.Show("Ошибка: "+ex.Message);}}
        private void NavDashboard_Click(object s,RoutedEventArgs e)=>new AdminDashboardWindow().Show();
        private void NavUsers_Click(object s,RoutedEventArgs e)=>new ManageUsersWindow().Show();
        private void NavWords_Click(object s,RoutedEventArgs e)=>new ManageWordsWindow().Show();
        private void NavLessons_Click(object s,RoutedEventArgs e)=>new ManageLessonsWindow().Show();
        private void NavExercises_Click(object s,RoutedEventArgs e)=>new ManageExercisesWindow().Show();
        private void NavLogout_Click(object s,RoutedEventArgs e){AuthService.Logout();new LoginWindow().Show();this.Close();}
    }
}
