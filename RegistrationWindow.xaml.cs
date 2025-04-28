using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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
        private bool IsRussianLetter(char c)
        {
            return (c >= 'а' && c <= 'я') || (c >= 'А' && c <= 'Я') || c == 'ё' || c == 'Ё';
        }
        public static bool ValidatePassword(string password)
        {
            bool hasLetter = false;
            bool hasDigit = false;
            bool hasInvalidChar = false;
            foreach (char c in password)
            {
                if (char.IsLetter(c))
                {
                    if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
                    {
                        hasLetter = true;
                    }
                    else
                    {
                        hasInvalidChar = true;
                    }
                }
                else if (char.IsDigit(c))
                {
                    hasDigit = true;
                }
                else
                {
                    hasInvalidChar = true;
                }
            }
            if (hasInvalidChar)
            {
                MessageBox.Show("Пароль может содержать только английские буквы и цифры");
                return false;
            }
            if (!hasLetter)
            {
                MessageBox.Show("Пароль должен содержать хотя бы одну английскую букву");
                return false;
            }
            if (!hasDigit)
            {
                MessageBox.Show("Пароль должен содержать хотя бы одну цифру");
                return false;
            }
            return true;
        }
        public static string GetHash(string password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }
        private void ButtonReg(object sender, RoutedEventArgs e)
        {
            Reg(FIO.Text, DateOfBirth.SelectedDate, StudentsCardNumber.Text, Course.Text, Group.Text, Email.Text, login.Text, password.Password, passwordProverka.Password);
        }
        public bool Reg(String FIO, DateTime? DateOfBirth, String CardNumber, String Course, String Group, String email, String log, String pass, String passProverka)
        {
            if (string.IsNullOrWhiteSpace(FIO) ||
                string.IsNullOrWhiteSpace(CardNumber) ||
                string.IsNullOrWhiteSpace(Course) ||
                string.IsNullOrWhiteSpace(Group) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(log) ||
                string.IsNullOrWhiteSpace(pass) ||
                string.IsNullOrWhiteSpace(passProverka) ||
                DateOfBirth == null)
            {
                MessageBox.Show("Все обязательные поля должны быть заполнены", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            int studentCard = Convert.ToInt32(CardNumber);
            using (var db = new studDB())
            {
                var user = db.Users.AsNoTracking().FirstOrDefault(u => u.Login == log);
                if (user != null)
                {
                    MessageBox.Show("Пользователь с такими данными уже существует");
                    return false;
                }
                var emaildb = db.Students.AsNoTracking().FirstOrDefault(s => s.Email == email);
                if (emaildb != null)
                {
                    MessageBox.Show("Пользователь с таким email уже существует");
                    return false;
                }
                var cardNumb = db.Students.AsNoTracking().FirstOrDefault(s => s.StudentCardNumber == studentCard);
                if (cardNumb != null)
                {
                    MessageBox.Show("Пользователь с таким номером студенческого билета уже существует");
                    return false;
                }
            }
            if (log.Length > 50)
            {
                MessageBox.Show("Логин должен быть меньше 50 символов");
                return false;
            }
            if (log.Length < 2)
            {
                MessageBox.Show("Логин должен быть больше 1 символа");
                return false;
            }
            if (pass.Length < 8)
            {
                MessageBox.Show("Пароль должен быть больше 8 символов");
                return false;
            }
            if (pass.Length > 25)
            {
                MessageBox.Show("Пароль должен быть меньше 25 символов");
                return false;
            }
            if (!ValidatePassword(pass))
            {
                return false;
            }
            if (pass != passProverka)
            {
                MessageBox.Show("Пароль должен совпадать с 'Подтверждение пароля'");
                return false;
            }
            var nameParts = FIO.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (nameParts.Length < 2)
            {
                MessageBox.Show("ФИО должно содержать минимум 2 слова");
                return false;
            }
            foreach (var part in nameParts)
            {
                if (!part.All(c => IsRussianLetter(c)))
                {
                    MessageBox.Show("ФИО должно состоять из русских букв");
                    return false;
                }
            }
            if (DateOfBirth >= DateTime.Today)
            {
                MessageBox.Show("Дата рождения должна быть раньше текущей даты");
                return false;
            }
            if (!Regex.IsMatch(CardNumber, @"^\d{6}$"))
            {
                MessageBox.Show("Номер студенческого билета должен содержать ровно 6 цифр");
                return false;
            }
            if (!int.TryParse(Course, out int course) || course < 1 || course > 4)
            {
                MessageBox.Show("Курс должен быть числом от 1 до 4");
                return false;
            }
            if (!Regex.IsMatch(Group, @"^\d{3,4}$"))
            {
                MessageBox.Show("Номер группы должен содержать 3 или 4 цифры");
                return false;
            }
            if (!IsValidEmail(email))
            {
                MessageBox.Show("Неверный формат email");
                return false;
            }
            try
            {
                using (var db = new studDB())
                {
                    var student = new Students
                    {
                        FirstName = FIO.Split(' ')[1],
                        LastName = FIO.Split(' ')[0],
                        MiddleName = FIO.Split(' ').Length > 2 ? FIO.Split(' ')[2] : null,
                        DateOfBirth = DateOfBirth.Value,
                        StudentCardNumber = studentCard,
                        Course = int.Parse(Course),
                        Groupp = int.Parse(Group),
                        Email = email,
                        JoinDate = DateTime.Today,
                        Points = 0
                    };
                    db.Students.Add(student);
                    db.SaveChanges();
                    var user = new Users
                    {
                        Login = log,
                        Password = GetHash(pass),
                        Role = "student",
                        IDStudent = student.IDStudent
                    };
                    db.Users.Add(user);
                    db.SaveChanges();
                    student.IDUser = user.IDUser;
                    db.SaveChanges();
                    MessageBox.Show("Регистрация прошла успешно", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}
