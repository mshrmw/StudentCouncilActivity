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
    /// Логика взаимодействия для PageCreateEventsCoordinator.xaml
    /// </summary>
    public partial class PageCreateEventsCoordinator : Page
    {
        private CoordinatorWindow _coordinatorWindow;
        private studDB _context = studDB.GetContext();
        private int _currentStudentId;
        public PageCreateEventsCoordinator(CoordinatorWindow coordinatorWindow)
        {
            InitializeComponent();
            _coordinatorWindow = coordinatorWindow;
            _currentStudentId = App.CurrentStudentId;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы точно хотите отменить регистрацию мероприятия?", "Подтверждение выхода", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _coordinatorWindow.mainFrame.Navigate(new PageEventsCoordinator(_coordinatorWindow));
            }

        }
        private void SaveCreateEvent_Click(object sender, RoutedEventArgs e)
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
                if (string.IsNullOrWhiteSpace(LocationEvent.Text))
                {
                    MessageBox.Show("Укажите место проведения мероприятия!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (EndDate.SelectedDate != null && EndDate.SelectedDate <= StartDate.SelectedDate)
                {
                    MessageBox.Show("Дата окончания не может быть раньше даты начала!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (StartDate.SelectedDate <= DateTime.Today)
                {
                    MessageBox.Show("Дата начала мероприятия должна быть позже сегодняшней даты!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var newEvent = new Events
                {
                    EventName = NameEvent.Text,
                    StartDate = StartDate.SelectedDate.Value,
                    EndDate = EndDate.SelectedDate,
                    Descriptions = Description.Text,
                    Location = LocationEvent.Text,
                    IDOrganizer = _currentStudentId 
                };
                _context.Events.Add(newEvent);
                _context.SaveChanges();
                var participation = new StudentEventParticipation
                {
                    IDStudent = _currentStudentId,
                    IDEvent = newEvent.IDEvent
                };
                _context.StudentEventParticipation.Add(participation);
                _context.SaveChanges();
                MessageBox.Show("Мероприятие успешно создано! Теперь вы можете добавить задачи", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                _coordinatorWindow.mainFrame.Navigate(new PageCreateTasksCoordinator(_coordinatorWindow));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании мероприятия: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
