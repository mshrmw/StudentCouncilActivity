using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StudentCouncilActivity
{
    /// <summary>
    /// Логика взаимодействия для PageProfilStudent.xaml
    /// </summary>
    public partial class PageProfilStudent : Page
    {
        private studDB _context = studDB.GetContext();
        private int _currentStudentId;
        private Students _currentStudent;
        public PageProfilStudent()
        {
            InitializeComponent();
            _currentStudentId = App.CurrentStudentId;
            LoadStudentData();
        }
        private void LoadStudentData()
        {
            try
            {
                _currentStudent = _context.Students.Find(_currentStudentId);
                if (_currentStudent != null)
                {
                    LastName.Text = _currentStudent.LastName;
                    FirstName.Text = _currentStudent.FirstName;
                    MiddleName.Text = _currentStudent.MiddleName ?? string.Empty;
                    DateOfBirth.SelectedDate = _currentStudent.DateOfBirth;
                    StudentsCardNumber.Text = _currentStudent.StudentCardNumber.ToString();
                    Course.Text = _currentStudent.Course.ToString();
                    Group.Text = _currentStudent.Groupp.ToString();
                    Email.Text = _currentStudent.Email;
                    Points.Text = _currentStudent.Points.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
        private bool RussianLetters(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }
            foreach (char c in input)
            {
                    if (!(c >= 'А' && c <= 'Я') && !(c >= 'а' && c <= 'я') && c != 'ё' && c != 'Ё')
                {
                    return false;
                }
            }
            return true;
        }
        private void save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LastName.Text) ||
                string.IsNullOrWhiteSpace(FirstName.Text) ||
                string.IsNullOrWhiteSpace(StudentsCardNumber.Text) ||
                string.IsNullOrWhiteSpace(Course.Text) ||
                string.IsNullOrWhiteSpace(Group.Text) ||
                string.IsNullOrWhiteSpace(Email.Text) ||
                DateOfBirth.SelectedDate == null)
            {
                MessageBox.Show("Заполните все обязательные поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!RussianLetters(LastName.Text) || !RussianLetters(FirstName.Text) || (!string.IsNullOrWhiteSpace(MiddleName.Text) && !RussianLetters(MiddleName.Text)))
            {
                MessageBox.Show("ФИО должно содержать только русские буквы", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!IsValidEmail(Email.Text))
            {
                MessageBox.Show("Введите корректный email адрес", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!int.TryParse(StudentsCardNumber.Text, out _) || !int.TryParse(Course.Text, out _) || !int.TryParse(Group.Text, out _))
            {
                MessageBox.Show("Номер студенческого, курс и группа должны быть числами", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!Regex.IsMatch(StudentsCardNumber.Text, @"^\d{6}$"))
            {
                MessageBox.Show("Номер студенческого билета должен содержать ровно 6 цифр");
                return;
            }
            if (!int.TryParse(Course.Text, out int course) || course < 1 || course > 4)
            {
                MessageBox.Show("Курс должен быть числом от 1 до 4");
                return;
            }
            if (!Regex.IsMatch(Group.Text, @"^\d{3,4}$"))
            {
                MessageBox.Show("Номер группы должен содержать 3 или 4 цифры");
                return;
            }
            if (DateOfBirth.SelectedDate >= DateTime.Today)
            {
                MessageBox.Show("Дата рождения должна быть раньше текущей даты", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                if (_currentStudent.Email != Email.Text && _context.Students.Any(s => s.Email == Email.Text && s.IDStudent != _currentStudentId))
                {
                    MessageBox.Show("Этот email уже используется другим студентом", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (int.TryParse(StudentsCardNumber.Text, out int cardNumber) && _currentStudent.StudentCardNumber != cardNumber && _context.Students.Any(s => s.StudentCardNumber == cardNumber && s.IDStudent != _currentStudentId))
                {
                    MessageBox.Show("Этот номер студенческого билета уже используется", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                _currentStudent.LastName = LastName.Text;
                _currentStudent.FirstName = FirstName.Text;
                _currentStudent.MiddleName = MiddleName.Text;
                _currentStudent.Email = Email.Text;
                _currentStudent.DateOfBirth = DateOfBirth.SelectedDate.Value;

                if (int.TryParse(StudentsCardNumber.Text, out int newCardNumber))
                {
                    _currentStudent.StudentCardNumber = newCardNumber;

                }
                if (int.TryParse(Course.Text, out int ccourse))
                {
                    _currentStudent.Course = ccourse;
                }
                if (int.TryParse(Group.Text, out int group))
                {
                    _currentStudent.Groupp = group;
                }
                _context.SaveChanges();
                MessageBox.Show("Данные успешно сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }
}
