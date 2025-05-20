using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static StudentCouncilActivity.App;

namespace StudentCouncilActivity
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void loginGotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (login.Text == "Введите логин")
                {
                    login.Text = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обработке ввода логина: {ex.Message}");
            }
        }
        private void HyperlinkClick(object sender, RoutedEventArgs e)
        {
            try
            {
                RegistrationWindow registrationWindow = new RegistrationWindow();
                registrationWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии окна регистрации: {ex.Message}");
            }
        }
        public static string GetHash(string password)
        {
            try
            {
                using (var hash = SHA1.Create())
                {
                    return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при хешировании пароля: {ex.Message}");
                return string.Empty;
            }
        }
        private void ButtonWelcome(object sender, RoutedEventArgs e)
        {
            try
            {
                string pass = password.Password;
                Auth(login.Text, pass);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при авторизации: {ex.Message}");
            }
        }
        public bool Auth(string log, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(log) || log == "Введите логин" && string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Введите логин и пароль");
                    return false;
                }
                if (string.IsNullOrEmpty(log) || log == "Введите логин")
                {
                    MessageBox.Show("Введите логин");
                    return false;
                }
                if (string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Введите пароль");
                    return false;
                }
                string pass = GetHash(password);
                using (var db = new studDB())
                {
                    var user = db.Users.AsNoTracking().FirstOrDefault(u => u.Login == log && u.Password == pass);
                    if (user == null)
                    {
                        MessageBox.Show("Пользователь с такими данными не найден");
                        return false;
                    }
                    App.CurrentStudentId = user.IDStudent ?? 0;
                    if (user.Role == "student")
                    {
                        MessageBox.Show("Успешный вход в систему!");
                        StudentsWindow studentsWindow = new StudentsWindow();
                        studentsWindow.Show();
                    }
                    else if (user.Role == "coordinator")
                    {
                        MessageBox.Show("Успешный вход в систему!");
                        CoordinatorWindow coordinatorWindow = new CoordinatorWindow();
                        coordinatorWindow.Show();
                    }
                    else if (user.Role == "admin")
                    {
                        MessageBox.Show("Успешный вход в систему!");
                        PredsedWindow predsedWindow = new PredsedWindow();
                        predsedWindow.Show();
                    }
                    this.Close();
                    return true;
                }
            }
            catch (System.Data.Entity.Core.EntityException ex)
            {
                MessageBox.Show($"Ошибка подключения к базе данных: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}");
                return false;
            }
        }
    }
}
