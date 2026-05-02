using System.Windows; using System.Windows.Input;
using EnglishTutor.Services; using EnglishTutor.Windows;
namespace EnglishTutor.Windows.Student
{
    public partial class StudentDashboardWindow : Window
    {
        public StudentDashboardWindow(){InitializeComponent();var u=AuthService.CurrentUser!;TxtUser.Text=u.Username;TxtGreeting.Text=$"Привет, {u.Username}! 👋";try{var s=ExerciseService.GetUserStats(u.UserId);TxtStat1.Text=s["TotalExercises"].ToString();TxtStat2.Text=s["PassedExercises"].ToString();TxtStat3.Text=ExerciseService.GetUserAchievements(u.UserId).Count.ToString();}catch{}}
        private void GoLessons(object s,MouseButtonEventArgs e)=>new LessonsWindow().Show();
        private void GoExercises(object s,MouseButtonEventArgs e)=>new ExercisesListWindow().Show();
        private void GoGrammar(object s,MouseButtonEventArgs e)=>new GrammarRulesWindow().Show();
        private void GoProgress(object s,MouseButtonEventArgs e)=>new ProgressWindow().Show();
        private void BtnLogout_Click(object s,RoutedEventArgs e){AuthService.Logout();new LoginWindow().Show();this.Close();}
    }
}
