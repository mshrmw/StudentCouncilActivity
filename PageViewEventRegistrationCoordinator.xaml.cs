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
    /// Логика взаимодействия для PageViewEventRegistrationCoordinator.xaml
    /// </summary>
    public partial class PageViewEventRegistrationCoordinator : Page
    {
        private CoordinatorWindow _coordinatorWindow;
        private studDB _context = studDB.GetContext();
        private int _currentStudentId;
        public PageViewEventRegistrationCoordinator(CoordinatorWindow coordinatorWindow)
        {
            InitializeComponent();
            _coordinatorWindow = coordinatorWindow;
            _currentStudentId = App.CurrentStudentId;
            LoadEvents();
            LoadRegistrations(null, "Зарегистрирован");
            ComboBoxTasks.SelectedIndex = 1;
        }
        private void LoadEvents()
        {
            try
            {
                var events = _context.Events.Where(e => e.IDOrganizer == _currentStudentId).OrderBy(e => e.StartDate).ToList();
                ComboBoxEvents.Items.Clear();
                ComboBoxEvents.Items.Add("Все мероприятия");
                foreach (var ev in events)
                {
                    ComboBoxEvents.Items.Add(ev.EventName);
                }
                ComboBoxEvents.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки мероприятий: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoadRegistrations(int? eventId = null, string statusFilter = null)
        {
            try
            {
                var query = from reg in _context.Registrations join task in _context.EventTasks on reg.IDTask equals task.IDTask join ev in _context.Events on task.IDEvent equals ev.IDEvent join sec in _context.Sectors on task.IDSector equals sec.IDSector join student in _context.Students on reg.IDStudent equals student.IDStudent where ev.IDOrganizer == _currentStudentId select new
                {
                    reg.IDRegistration,
                    student.LastName,
                    student.FirstName,
                    student.MiddleName,
                    TaskName = task.TaskName,
                    task.Deadline,
                    EventName = ev.EventName,
                    SectorName = sec.SectorName,
                    reg.RegistrationStatus,
                    reg.IDTask,
                    reg.IDStudent,
                    task.IDEvent
                };
                if (eventId != null)
                {
                    query = query.Where(x => x.IDEvent == eventId);
                }
                if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "Все")
                {
                    if (statusFilter == "В обработке")
                    {
                        statusFilter = "Зарегистрирован";
                    }    
                    else if (statusFilter == "Приняты")
                    {
                        statusFilter = "Принят";
                    }
                    else if (statusFilter == "Выполненные")
                    {
                        statusFilter = "Выполнен";
                    }
                    query = query.Where(x => x.RegistrationStatus == statusFilter);
                }
                DataGridTasks.ItemsSource = query.OrderBy(x => x.Deadline).ThenBy(x => x.TaskName).ThenBy(x => x.LastName).ThenBy(x => x.FirstName).ThenBy(x => x.MiddleName).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки регистраций: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void FilterRegistration_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int? eventId = null;
                string statusFilter = null;
                if (ComboBoxTasks.SelectedIndex != -1)
                {
                    if(ComboBoxTasks.SelectedIndex == 0) 
                    { 
                        statusFilter = null;
                    }
                    else if(ComboBoxTasks.SelectedIndex == 1)
                    {
                        statusFilter = "Зарегистрирован";
                    }
                    else if(ComboBoxTasks.SelectedIndex == 2)
                    {
                        statusFilter = "Принят";
                    }
                    else if(ComboBoxTasks.SelectedIndex == 3)
                    {
                        statusFilter = "Выполнен";
                    }
                }
                if (ComboBoxEvents.SelectedIndex > 0)
                {
                    string selectedEventName = ComboBoxEvents.SelectedItem.ToString();
                    eventId = _context.Events.FirstOrDefault(e1 => e1.EventName == selectedEventName && e1.IDOrganizer == _currentStudentId)?.IDEvent;
                }
                LoadRegistrations(eventId, statusFilter);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при фильтрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void DeleteRegistration_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridTasks.SelectedItem == null)
            {
                MessageBox.Show("Выберите регистрацию для отказа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                dynamic selectedRegistration = DataGridTasks.SelectedItem;
                if (selectedRegistration.RegistrationStatus == "Выполнен")
                {
                    MessageBox.Show("Нельзя отказать от выполненного задания", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                int registrationId = selectedRegistration.IDRegistration;
                var result = MessageBox.Show("Вы уверены, что хотите отказать в регистрации?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    var registration = _context.Registrations.Find(registrationId);
                    if (registration != null)
                    {
                        _context.Registrations.Remove(registration);
                        _context.SaveChanges();
                        MessageBox.Show("Регистрация отклонена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        FilterRegistration_Click(null, null);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отклонении регистрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AcceptRegistration_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridTasks.SelectedItem == null)
            {
                MessageBox.Show("Выберите регистрацию для подтверждения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                dynamic selectedRegistration = DataGridTasks.SelectedItem;
                if (selectedRegistration.RegistrationStatus != "Зарегистрирован")
                {
                    MessageBox.Show("Можно принять только регистрации со статусом 'Зарегистрирован'", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                int registrationId = selectedRegistration.IDRegistration;
                var registration = _context.Registrations.Find(registrationId);
                if (registration != null)
                {
                    registration.RegistrationStatus = "Принят";
                    _context.SaveChanges();
                    MessageBox.Show("Регистрация успешно подтверждена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    FilterRegistration_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подтверждении регистрации: {ex.Message}", "Ошибка",  MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
