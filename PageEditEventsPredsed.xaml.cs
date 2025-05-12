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
    /// Логика взаимодействия для PageEditEventsPredsed.xaml
    /// </summary>
    public partial class PageEditEventsPredsed : Page
    {
        private PredsedWindow _predsedWindow;
        private Events _currentEvent;
        private studDB _context = studDB.GetContext();
        public PageEditEventsPredsed(PredsedWindow predsedWindow, Events currentEvent)
        {
            InitializeComponent();
            _predsedWindow = predsedWindow;
            _currentEvent = currentEvent;
            LoadOrganizers();
            LoadEventData();
        }
        private void LoadEventData()
        {
            if (_currentEvent != null)
            {
                NameEvent.Text = _currentEvent.EventName;
                StartDate.SelectedDate = _currentEvent.StartDate;
                EndDate.SelectedDate = _currentEvent.EndDate;
                Description.Text = _currentEvent.Descriptions;
                LocationEvent.Text = _currentEvent.Location;
                if (_currentEvent.IDOrganizer > 0)
                {
                    OrganizatorEvent.SelectedValue = _currentEvent.IDOrganizer;
                }
            }
        }

        private void LoadOrganizers()
        {
            try
            {
                var organizersData = _context.Users.Where(u => u.Role == "coordinator" || u.Role == "admin" || u.Role == "predsed").Join(_context.Students, u => u.IDStudent, s => s.IDStudent, (u, s) => new
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки организаторов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveEditEvent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NameEvent.Text))
                {
                    MessageBox.Show("Название мероприятия не может быть пустым!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (StartDate.SelectedDate == null)
                {
                    MessageBox.Show("Укажите дату начала мероприятия!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (StartDate.SelectedDate < DateTime.Today)
                {
                    MessageBox.Show("Дата начала мероприятия должна быть позже сегодняшней даты!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                if (EndDate.SelectedDate != null && EndDate.SelectedDate < StartDate.SelectedDate)
                {
                    MessageBox.Show("Дата окончания не может быть раньше даты начала!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                int newOrganizerId = (int)OrganizatorEvent.SelectedValue;
                bool organizerChanged = _currentEvent.IDOrganizer != newOrganizerId;
                _currentEvent.EventName = NameEvent.Text;
                _currentEvent.StartDate = StartDate.SelectedDate.Value;
                _currentEvent.EndDate = EndDate.SelectedDate;
                _currentEvent.Descriptions = Description.Text;
                _currentEvent.Location = LocationEvent.Text;
                if (organizerChanged)
                {
                    var oldParticipation = _context.StudentEventParticipation.FirstOrDefault(sep => sep.IDStudent == _currentEvent.IDOrganizer && sep.IDEvent == _currentEvent.IDEvent);
                    if (oldParticipation != null)
                    {
                        _context.StudentEventParticipation.Remove(oldParticipation);
                    }
                    var newParticipation = new StudentEventParticipation
                    {
                        IDStudent = newOrganizerId,
                        IDEvent = _currentEvent.IDEvent
                    };
                    _context.StudentEventParticipation.Add(newParticipation);
                    _currentEvent.IDOrganizer = newOrganizerId;
                }
                _context.SaveChanges();
                MessageBox.Show("Мероприятие успешно обновлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                _predsedWindow.mainFrame.Navigate(new PageEventsPredsed(_predsedWindow));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении изменений: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            _predsedWindow.mainFrame.Navigate(new PageEventsPredsed(_predsedWindow));
        }
    }
}
