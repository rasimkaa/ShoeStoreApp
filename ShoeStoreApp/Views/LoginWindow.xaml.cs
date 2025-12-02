using System;
using System.Linq;
using System.Windows;
using System.Data.Entity;

namespace ShoeStoreApp.Views
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public static User CurrentUser { get; set; }

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = TxtLogin.Text.Trim();
            string password = TxtPassword.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ShowError("Введите логин и пароль!");
                return;
            }

            try
            {
                using (var db = new OrderManagementDBEntities())
                {
                    // ИСПРАВЛЕНО: добавлен Include("Role") для загрузки связанной сущности
                    var user = db.Users
                        .Include("Role")
                        .FirstOrDefault(u => u.Login == login && u.Password == password);

                    if (user != null)
                    {
                        CurrentUser = user;
                        MainWindow mainWindow = new MainWindow();
                        mainWindow.Show();
                        this.Close();
                    }
                    else
                    {
                        ShowError("Неверный логин или пароль!");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Ошибка подключения к БД: " + ex.Message +
                    (ex.InnerException != null ? "\n\nВнутреннее исключение: " + ex.InnerException.Message : ""));
            }
        }

        private void BtnGuest_Click(object sender, RoutedEventArgs e)
        {
            CurrentUser = null;
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void ShowError(string message)
        {
            TxtError.Text = message;
            TxtError.Visibility = Visibility.Visible;
        }
    }
}