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
        private void ButtonWelcome(object sender, RoutedEventArgs e)
        {
            string log = login.Text;
            string pass = password.Password;
            if (log == "" || pass == "")
            {
                MessageBox.Show("Введите логин и пароль");
                return;
            }
            User user = App.Users.FirstOrDefault(u => u.Login == log && u.Password == pass);

            if (user != null)
            {
                MessageBox.Show("Успешный вход!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                if (user.Role == 1)
                {
                    CoordinatorWindow coordinatorWindow = new CoordinatorWindow();
                    coordinatorWindow.Show();
                }
                else
                {
                    StudentsWindow studentsWindow = new StudentsWindow();
                    studentsWindow.Show();
                }

                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль");
                return;
            }

        }
    }
}
