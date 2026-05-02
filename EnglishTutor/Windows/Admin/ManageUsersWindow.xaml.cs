using System.Windows; using System.Windows.Controls;
using EnglishTutor.Data.Models; using EnglishTutor.Services; using EnglishTutor.Windows;
namespace EnglishTutor.Windows.Admin
{
    public partial class ManageUsersWindow : Window
    {
        private User? _sel;
        public ManageUsersWindow(){InitializeComponent();Load();}
        private void Load()=>DgUsers.ItemsSource=AuthService.GetAllUsers();
        private void DgUsers_SelectionChanged(object s,SelectionChangedEventArgs e){if(DgUsers.SelectedItem is User u){_sel=u;TxtLogin.Text=u.Username;TxtEmail.Text=u.Email;TxtPassword.Text="";CbRole.SelectedIndex=u.Role==UserRole.Admin?1:0;ChkActive.IsChecked=u.IsActive;}}
        private void BtnSave_Click(object s,RoutedEventArgs e){var login=TxtLogin.Text.Trim();var email=TxtEmail.Text.Trim();var pass=TxtPassword.Text.Trim();var role=CbRole.SelectedIndex==1?UserRole.Admin:UserRole.Student;if(_sel==null){if(string.IsNullOrEmpty(login)||string.IsNullOrEmpty(pass)){MessageBox.Show("Введите логин и пароль.");return;}if(AuthService.CreateUser(login,pass,email,role)){MessageBox.Show("Создан!");Load();Clear();}else MessageBox.Show("Логин занят.");}else{AuthService.UpdateUser(_sel.UserId,email,role,ChkActive.IsChecked==true);if(!string.IsNullOrEmpty(pass))AuthService.ChangePassword(_sel.UserId,pass);MessageBox.Show("Сохранено!");Load();Clear();}}
        private void BtnDelete_Click(object s,RoutedEventArgs e){if(_sel==null)return;if(MessageBox.Show($"Удалить {_sel.Username}?","Подтверждение",MessageBoxButton.YesNo)==MessageBoxResult.Yes){AuthService.DeleteUser(_sel.UserId);Load();Clear();}}
        private void BtnNew_Click(object s,RoutedEventArgs e)=>Clear();
        private void Clear(){_sel=null;TxtLogin.Text=TxtEmail.Text=TxtPassword.Text="";CbRole.SelectedIndex=0;ChkActive.IsChecked=true;DgUsers.SelectedItem=null;}
        private void NavDashboard_Click(object s,RoutedEventArgs e)=>new AdminDashboardWindow().Show();
        private void NavWords_Click(object s,RoutedEventArgs e)=>new ManageWordsWindow().Show();
        private void NavLessons_Click(object s,RoutedEventArgs e)=>new ManageLessonsWindow().Show();
        private void NavExercises_Click(object s,RoutedEventArgs e)=>new ManageExercisesWindow().Show();
        private void NavStats_Click(object s,RoutedEventArgs e)=>new StatisticsWindow().Show();
        private void NavLogout_Click(object s,RoutedEventArgs e){AuthService.Logout();new LoginWindow().Show();this.Close();}
    }
}
