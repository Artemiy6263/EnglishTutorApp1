using System.Windows; using System.Windows.Input;
using EnglishTutor.Data.Models; using EnglishTutor.Services;
namespace EnglishTutor.Windows.Student
{
    public partial class ExercisesListWindow : Window
    {
        public ExercisesListWindow(){InitializeComponent();IcTranslation.ItemsSource=ExerciseService.GetExercises(ExerciseType.Translation);IcSpelling.ItemsSource=ExerciseService.GetExercises(ExerciseType.Spelling);IcTenses.ItemsSource=ExerciseService.GetExercises(ExerciseType.Tenses);}
        private void ExerciseSelected(object s,MouseButtonEventArgs e){if((s as FrameworkElement)?.DataContext is Exercise ex){Window w=ex.Type switch{ExerciseType.Translation=>new ExerciseTranslationWindow(ex.ExerciseId),ExerciseType.Spelling=>new ExerciseSpellingWindow(ex.ExerciseId),ExerciseType.Tenses=>new ExerciseTensesWindow(ex.ExerciseId),_=>new ExercisesListWindow()};w.Show();this.Close();}}
        private void BtnBack_Click(object s,RoutedEventArgs e){new StudentDashboardWindow().Show();this.Close();}
    }
}
