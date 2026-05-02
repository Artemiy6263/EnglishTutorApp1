using System.Windows; using System.Windows.Controls;
using EnglishTutor.Data; using EnglishTutor.Data.Models; using EnglishTutor.Services; using EnglishTutor.Windows;
namespace EnglishTutor.Windows.Admin
{
    public partial class ManageExercisesWindow : Window
    {
        private Exercise? _sel;
        public ManageExercisesWindow(){InitializeComponent();Load();}
        private void Load()=>DgExercises.ItemsSource=ExerciseService.GetAllExercisesAdmin();
        private void DgExercises_SelectionChanged(object s,SelectionChangedEventArgs e){if(DgExercises.SelectedItem is Exercise ex){_sel=ex;TxtTitle.Text=ex.Title;TxtDesc.Text=ex.Description;CbType.SelectedIndex=(int)ex.Type-1;CbLevel.SelectedIndex=(int)ex.DifficultyLevel-1;TxtTime.Text=ex.TimeLimit.ToString();ChkActive.IsChecked=ex.IsActive;}}
        private void BtnSave_Click(object s,RoutedEventArgs e){if(_sel==null){MessageBox.Show("Выберите задание.");return;}using var ctx=new AppDbContext();var ex=ctx.Exercises.Find(_sel.ExerciseId);if(ex==null)return;ex.Title=TxtTitle.Text.Trim();ex.Description=TxtDesc.Text.Trim();ex.Type=(ExerciseType)(CbType.SelectedIndex+1);ex.DifficultyLevel=(DifficultyLevel)(CbLevel.SelectedIndex+1);int.TryParse(TxtTime.Text,out int t);if(t>0)ex.TimeLimit=t;ex.IsActive=ChkActive.IsChecked==true;ctx.SaveChanges();MessageBox.Show("Сохранено!");Load();}
        private void BtnDelete_Click(object s,RoutedEventArgs e){if(_sel==null)return;if(MessageBox.Show("Удалить задание?","Подтверждение",MessageBoxButton.YesNo)==MessageBoxResult.Yes){ExerciseService.DeleteExercise(_sel.ExerciseId);Load();}}
        private void NavDashboard_Click(object s,RoutedEventArgs e)=>new AdminDashboardWindow().Show();
        private void NavUsers_Click(object s,RoutedEventArgs e)=>new ManageUsersWindow().Show();
        private void NavWords_Click(object s,RoutedEventArgs e)=>new ManageWordsWindow().Show();
        private void NavLessons_Click(object s,RoutedEventArgs e)=>new ManageLessonsWindow().Show();
        private void NavStats_Click(object s,RoutedEventArgs e)=>new StatisticsWindow().Show();
        private void NavLogout_Click(object s,RoutedEventArgs e){AuthService.Logout();new LoginWindow().Show();this.Close();}
    }
}
