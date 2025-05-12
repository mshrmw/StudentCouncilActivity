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

namespace StudentCouncilActivity
{
    /// <summary>
    /// Логика взаимодействия для PageCreateEventsPredsed.xaml
    /// </summary>
    public partial class PageCreateEventsPredsed : Page
    {
        private PredsedWindow _predsedWindow;
        private studDB _context = studDB.GetContext();
        private int _currentAdminId;
        public PageCreateEventsPredsed(PredsedWindow predsedWindow)
        {
            InitializeComponent();
            _predsedWindow = predsedWindow;
            _currentAdminId = App.CurrentStudentId;
            LoadOrganizers();
        }
        private void LoadOrganizers()
        {
            try
            {
                var organizersData = _context.Users.Where(u => u.Role == "coordinator" || u.Role == "admin").Join(_context.Students, u => u.IDStudent, s => s.IDStudent, (u, s) => new
                {
                    IDUser = u.IDUser,
                    IDStudent = s.IDStudent,
                    s.LastName,
                    s.FirstName,
                    s.MiddleName,
                    Role = u.Role
                }).OrderBy(o => o.Role).ThenBy(o => o.LastName).ThenBy(o => o.FirstName).ThenBy(o => o.MiddleName).ToList();
                var organizers = organizersData.Select(o => new
                {
                    o.IDUser,
                    o.IDStudent,
                    FullName = $"{o.LastName} {o.FirstName} {o.MiddleName}",
                    o.Role
                }).ToList();
                OrganizatorEvent.ItemsSource = organizers;
                OrganizatorEvent.DisplayMemberPath = "FullName";
                OrganizatorEvent.SelectedValuePath = "IDStudent";
                var currentAdmin = organizers.FirstOrDefault(o => o.IDStudent == _currentAdminId);
                if (currentAdmin != null)
                {
                    OrganizatorEvent.SelectedValue = currentAdmin.IDStudent;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки организаторов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SaveCreateEvent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NameEvent.Text))
                {
                    MessageBox.Show("Введите название мероприятия!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (StartDate.SelectedDate == null)
                {
                    MessageBox.Show("Укажите дату начала мероприятия!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(LocationEvent.Text))
                {
                    MessageBox.Show("Укажите место проведения мероприятия!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (OrganizatorEvent.SelectedValue == null)
                {
                    MessageBox.Show("Выберите организатора мероприятия!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (EndDate.SelectedDate != null && EndDate.SelectedDate <= StartDate.SelectedDate)
                {
                    MessageBox.Show("Дата окончания не может быть раньше даты начала!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (StartDate.SelectedDate <= DateTime.Today)
                {
                    MessageBox.Show("Дата начала должна быть не раньше сегодняшнего дня!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var newEvent = new Events
                {
                    EventName = NameEvent.Text,
                    StartDate = StartDate.SelectedDate.Value,
                    EndDate = EndDate.SelectedDate,
                    Location = LocationEvent.Text,
                    Descriptions = Description.Text,
                    IDOrganizer = (int)OrganizatorEvent.SelectedValue
                };
                _context.Events.Add(newEvent);
                _context.SaveChanges();
                var participation = new StudentEventParticipation
                {
                    IDStudent = (int)OrganizatorEvent.SelectedValue,
                    IDEvent = newEvent.IDEvent
                };
                _context.StudentEventParticipation.Add(participation);
                _context.SaveChanges();
                MessageBox.Show("Мероприятие успешно создано! Теперь вы можете добавить задачи", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                _predsedWindow.mainFrame.Navigate(new PageCreateTasksPredsed(_predsedWindow));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании мероприятия: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы точно хотите отменить создание мероприятия?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _predsedWindow.mainFrame.Navigate(new PageEventsPredsed(_predsedWindow));
            }
        }
    }
}
