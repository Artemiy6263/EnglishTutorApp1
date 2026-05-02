using System; using System.Linq; using System.Windows;
using EnglishTutor.Data; using EnglishTutor.Services; using EnglishTutor.Windows;
using Microsoft.EntityFrameworkCore;
namespace EnglishTutor.Windows.Admin
{
    public partial class AdminDashboardWindow : Window
    {
        public AdminDashboardWindow()
        {
            InitializeComponent();
            TxtWelcome.Text = $"Добро пожаловать, {AuthService.CurrentUser?.Username}!";
            try { using var ctx=new AppDbContext(); TxtUserCount.Text=ctx.Users.Count().ToString(); TxtWordCount.Text=ctx.Words.Count().ToString(); TxtExerciseCount.Text=ctx.Exercises.Count().ToString(); TxtProgressCount.Text=ctx.StudentProgresses.Count().ToString(); DgRecent.ItemsSource=ctx.StudentProgresses.Include(p=>p.User).Include(p=>p.Exercise).OrderByDescending(p=>p.CompletedAt).Take(10).ToList(); }
            catch(Exception ex){MessageBox.Show("Ошибка: "+ex.Message);}
        }
        private void NavUsers_Click(object s,RoutedEventArgs e)=>new ManageUsersWindow().Show();
        private void NavWords_Click(object s,RoutedEventArgs e)=>new ManageWordsWindow().Show();
        private void NavLessons_Click(object s,RoutedEventArgs e)=>new ManageLessonsWindow().Show();
        private void NavExercises_Click(object s,RoutedEventArgs e)=>new ManageExercisesWindow().Show();
        private void NavStats_Click(object s,RoutedEventArgs e)=>new StatisticsWindow().Show();
        private void NavLogout_Click(object s,RoutedEventArgs e){AuthService.Logout();new LoginWindow().Show();this.Close();}
    }
}
