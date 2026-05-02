using System;
using System.Windows;
using EnglishTutor.Services;

namespace EnglishTutor.Windows
{
    public partial class RegisterWindow : Window
    {
        /// <summary>
        /// The username that was successfully registered.
        /// Available when DialogResult == true.
        /// </summary>
        public string RegisteredUsername { get; private set; } = string.Empty;

        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            BtnRegister.IsEnabled = false;
            BtnRegister.Content = "Регистрируем...";

            try
            {
                var result = AuthService.RegisterStudent(TxtUsername.Text, PbPassword.Password, PbConfirm.Password, TxtEmail.Text);
                if (!result.Success)
                {
                    ShowError(result.Message);
                    return;
                }

                RegisteredUsername = result.Username;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ShowError("Ошибка: " + ex.Message);
            }
            finally
            {
                BtnRegister.IsEnabled = true;
                BtnRegister.Content = "Зарегистрироваться";
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ShowError(string msg)
        {
            TxtError.Text = msg;
            TxtError.Visibility = Visibility.Visible;
        }
    }
}
