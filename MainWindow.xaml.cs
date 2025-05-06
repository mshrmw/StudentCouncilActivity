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
            if (login.Text == "Введите логин")
            {
                login.Text = "";
            }
        }
        private void HyperlinkClick(object sender, RoutedEventArgs e)
        {
            RegistrationWindow registrationWindow = new RegistrationWindow();
            registrationWindow.Show();
            this.Close();
        }
        public static string GetHash(string password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }
        private void ButtonWelcome(object sender, RoutedEventArgs e)
        {
            string pass = GetHash(password.Password);
            Auth(login.Text, pass);
        }
        public bool Auth(String log, String pass)
        {
            if (string.IsNullOrEmpty(log) && string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Введите логин и пароль");
                return false;
            }
            if (string.IsNullOrEmpty(log))
            {
                MessageBox.Show("Введите логин");
                return false;
            }
            if (string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Введите пароль");
                return false;
            }
            using (var db = new studDB())
            {
                var user = db.Users.AsNoTracking().FirstOrDefault(u => u.Login == log && u.Password == pass);

                if (user == null)
                {
                    MessageBox.Show("Пользователь с такими данными не найден");
                    return false;
                }
                else if (user != null)
                {
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
                }
                this.Close();
                return true;
            }
        }
    }
}
