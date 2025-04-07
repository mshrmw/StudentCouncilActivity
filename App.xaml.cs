using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace StudentCouncilActivity
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static List<User> Users { get; set; } = new List<User>();
        public class User
        {
            public string FIO { get; set; }
            public string DateOfBirth { get; set; }
            public string StudentsCardNumber { get; set; }
            public string Course { get; set; }
            public string Group { get; set; }
            public string Email { get; set; }
            public string Login { get; set; }
            public string Password { get; set; }
            public int Role { get; set; }
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Users.Add(new User
            {
                FIO = "Иванов Иван Иванович",
                DateOfBirth = "2006-01-01",
                StudentsCardNumber = "123456",
                Course = "3",
                Group = "222",
                Email = "ivanov@example.com",
                Login = "student",
                Password = "2025",
                Role = 2
            });
            Users.Add(new User
            {
                FIO = "Шленова Анастасия Сергеевна",
                DateOfBirth = "2004-05-15",
                StudentsCardNumber = "654321",
                Course = "4",
                Group = "821",
                Email = "nastiya@example.com",
                Login = "admin",
                Password = "number1",
                Role = 1
            });
        }
    }
}
