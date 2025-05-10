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
    /// Логика взаимодействия для PageEditEventsCoordinator.xaml
    /// </summary>
    public partial class PageEditEventsCoordinator : Page
    {
        private CoordinatorWindow _coordinatorWindow;
        private studDB _context = studDB.GetContext();
        private Events _currentEvent;
        public PageEditEventsCoordinator(CoordinatorWindow coordinatorWindow, Events selectedEvent)
        {
            InitializeComponent();
            _coordinatorWindow = coordinatorWindow;
            _currentEvent = selectedEvent;
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
            }
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы точно хотите отменить редактирование мероприятия?", "Подтверждение выхода", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _coordinatorWindow.mainFrame.Navigate(new PageEventsCoordinator(_coordinatorWindow));
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
                if (EndDate.SelectedDate != null && EndDate.SelectedDate < StartDate.SelectedDate)
                {
                    MessageBox.Show("Дата окончания не может быть раньше даты начала!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                _currentEvent.EventName = NameEvent.Text;
                _currentEvent.StartDate = StartDate.SelectedDate.Value;
                _currentEvent.EndDate = EndDate.SelectedDate;
                _currentEvent.Descriptions = Description.Text;
                _currentEvent.Location = LocationEvent.Text;
                _context.SaveChanges();
                MessageBox.Show("Мероприятие успешно обновлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                _coordinatorWindow.mainFrame.Navigate(new PageEventsCoordinator(_coordinatorWindow));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении изменений: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
