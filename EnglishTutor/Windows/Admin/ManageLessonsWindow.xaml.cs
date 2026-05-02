using System; using System.Linq; using System.Windows; using System.Windows.Controls;
using EnglishTutor.Data; using EnglishTutor.Data.Models; using EnglishTutor.Services; using EnglishTutor.Windows;
namespace EnglishTutor.Windows.Admin
{
    public partial class ManageLessonsWindow : Window
    {
        private Lesson? _sel;
        public ManageLessonsWindow(){InitializeComponent();Load();}
        private void Load(){using var ctx=new AppDbContext();DgLessons.ItemsSource=ctx.Lessons.OrderBy(l=>l.OrderNumber).ToList();}
        private void DgLessons_SelectionChanged(object s,SelectionChangedEventArgs e){if(DgLessons.SelectedItem is Lesson l){_sel=l;TxtTitle.Text=l.Title;TxtDesc.Text=l.Description;CbLevel.SelectedIndex=(int)l.DifficultyLevel-1;TxtOrder.Text=l.OrderNumber.ToString();TxtIcon.Text=l.IconEmoji;ChkActive.IsChecked=l.IsActive;}}
        private void BtnSave_Click(object s,RoutedEventArgs e){if(string.IsNullOrWhiteSpace(TxtTitle.Text)){MessageBox.Show("Введите название.");return;}using var ctx=new AppDbContext();var l=_sel!=null?ctx.Lessons.Find(_sel.LessonId)??new Lesson():new Lesson();l.Title=TxtTitle.Text.Trim();l.Description=TxtDesc.Text.Trim();l.DifficultyLevel=(DifficultyLevel)(CbLevel.SelectedIndex+1);int.TryParse(TxtOrder.Text,out int o);l.OrderNumber=o;l.IconEmoji=string.IsNullOrEmpty(TxtIcon.Text)?"📖":TxtIcon.Text.Trim();l.IsActive=ChkActive.IsChecked==true;if(_sel==null)ctx.Lessons.Add(l);ctx.SaveChanges();MessageBox.Show("Сохранено!");Load();Clear();}
        private void BtnDelete_Click(object s,RoutedEventArgs e){if(_sel==null)return;if(MessageBox.Show("Удалить урок?","Подтверждение",MessageBoxButton.YesNo)==MessageBoxResult.Yes){using var ctx=new AppDbContext();var l=ctx.Lessons.Find(_sel.LessonId);if(l!=null){ctx.Lessons.Remove(l);ctx.SaveChanges();}Load();Clear();}}
        private void BtnNew_Click(object s,RoutedEventArgs e)=>Clear();
        private void Clear(){_sel=null;TxtTitle.Text=TxtDesc.Text=TxtOrder.Text="";TxtIcon.Text="📖";CbLevel.SelectedIndex=0;ChkActive.IsChecked=true;DgLessons.SelectedItem=null;}
        private void NavDashboard_Click(object s,RoutedEventArgs e)=>new AdminDashboardWindow().Show();
        private void NavUsers_Click(object s,RoutedEventArgs e)=>new ManageUsersWindow().Show();
        private void NavWords_Click(object s,RoutedEventArgs e)=>new ManageWordsWindow().Show();
        private void NavExercises_Click(object s,RoutedEventArgs e)=>new ManageExercisesWindow().Show();
        private void NavStats_Click(object s,RoutedEventArgs e)=>new StatisticsWindow().Show();
        private void NavLogout_Click(object s,RoutedEventArgs e){AuthService.Logout();new LoginWindow().Show();this.Close();}
    }
}
