using System;
using System.Windows;
using EnglishTutor.Services;
using EnglishTutor.Windows.Admin;
using EnglishTutor.Windows.Student;

namespace EnglishTutor.Windows
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var username = TxtUsername.Text.Trim();
            var password = PbPassword.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("Введите логин и пароль.");
                return;
            }

            BtnLogin.IsEnabled = false;
            BtnLogin.Content = "Входим...";

            try
            {
                if (AuthService.Login(username, password))
                {
                    if (AuthService.IsAdmin)
                        new AdminDashboardWindow().Show();
                    else
                        new StudentDashboardWindow().Show();

                    this.Close();
                }
                else
                {
                    ShowError("Неверный логин или пароль.");
                }
            }
            catch (Exception ex)
            {
                ShowError("Ошибка подключения: " + ex.Message);
            }
            finally
            {
                BtnLogin.IsEnabled = true;
                BtnLogin.Content = "Войти";
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            var regWin = new RegisterWindow();
            regWin.Owner = this;

            if (regWin.ShowDialog() == true)
            {
                TxtUsername.Text = regWin.RegisteredUsername;
                PbPassword.Password = "";
                ShowSuccess("Студент зарегистрирован. Теперь войдите с вашим логином и паролем.");
            }
        }

        private void ShowError(string msg)
        {
            TxtError.Text = msg;
            TxtError.Foreground = System.Windows.Media.Brushes.Red;
            TxtError.Visibility = Visibility.Visible;
        }

        private void ShowSuccess(string msg)
        {
            TxtError.Text = msg;
            TxtError.Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0x3B, 0xAE, 0x6E));
            TxtError.Visibility = Visibility.Visible;
        }
    }
}
