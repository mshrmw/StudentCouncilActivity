using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace StudentCouncilActivity
{
    /// <summary>
    /// Логика взаимодействия для RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        public RegistrationWindow()
        {
            InitializeComponent();
        }
        private void HyperlinkClick(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        private bool IsValidStudentCardNumber(string number)
        {
            return number.All(char.IsDigit);
        }
        private void ButtonReg(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FIO.Text) ||
                string.IsNullOrWhiteSpace(StudentsCardNumber.Text) ||
                string.IsNullOrWhiteSpace(Course.Text) ||
                string.IsNullOrWhiteSpace(Group.Text) ||
                string.IsNullOrWhiteSpace(Email.Text) ||
                string.IsNullOrWhiteSpace(login.Text) ||
                string.IsNullOrWhiteSpace(password.Password) ||
                string.IsNullOrWhiteSpace(passwordProverka.Password) ||
                DateOfBirth.SelectedDate == null)
            {
                MessageBox.Show("Все обязательные поля должны быть заполнены", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (App.Users.Any(u => u.Login == login.Text))
            {
                MessageBox.Show("Пользователь с таким логином уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!IsValidEmail(Email.Text))
            {
                MessageBox.Show("Некорректный формат email", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!IsValidStudentCardNumber(StudentsCardNumber.Text))
            {
                MessageBox.Show("Номер студенческого билета должен состоять из цифр!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!int.TryParse(Course.Text, out int course) || course < 1 || course > 4)
            {
                MessageBox.Show("Курс должен быть числом от 1 до 4", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (password.Password != passwordProverka.Password)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            App.User newUser = new App.User
            {
                FIO = FIO.Text,
                DateOfBirth = DateOfBirth.SelectedDate?.ToString("dd-MM-yyyy"),
                StudentsCardNumber = StudentsCardNumber.Text,
                Course = Course.Text,
                Group = Group.Text,
                Email = Email.Text,
                Login = login.Text,
                Password = password.Password,
                Role = 2
            };
            App.Users.Add(newUser);
            MessageBox.Show("Регистрация прошла успешно", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}
