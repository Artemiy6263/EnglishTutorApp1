using System.Windows;
using EnglishTutor.Services;
namespace EnglishTutor.Windows.Student
{
    public partial class ProgressWindow : Window
    {
        public ProgressWindow(){InitializeComponent();Load();}
        private void Load(){int uid=AuthService.CurrentUser!.UserId;var p=ExerciseService.GetUserProgress(uid);var s=ExerciseService.GetUserStats(uid);var a=ExerciseService.GetUserAchievements(uid);TxtTotalEx.Text=s["TotalExercises"].ToString();TxtPassedEx.Text=s["PassedExercises"].ToString();TxtTotalScore.Text=s["TotalScore"].ToString();DgProgress.ItemsSource=p;WpAchievements.ItemsSource=a;}
        private void BtnBack_Click(object s,RoutedEventArgs e){new StudentDashboardWindow().Show();this.Close();}
    }
}
