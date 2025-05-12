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
    /// Логика взаимодействия для PageCreateTasksPredsed.xaml
    /// </summary>
    public partial class PageCreateTasksPredsed : Page
    {
        private PredsedWindow _predsedWindow;
        private studDB _context = studDB.GetContext();
        private int _currentStudentId;
        public PageCreateTasksPredsed(PredsedWindow predsedWindow)
        {
            InitializeComponent();
            _predsedWindow = predsedWindow;
            _currentStudentId = App.CurrentStudentId;
            LoadAllData();
            EventDeadline();
            _predsedWindow.Tasks.Style = (Style)FindResource("ShapkaButtonActivity");
            _predsedWindow.Events.Style = (Style)FindResource("ShapkaButton");
        }
        private void LoadAllData()
        {
            try
            {
                var events = _context.Events.OrderByDescending(e => e.StartDate).ToList();
                NameEventComboBox.ItemsSource = events;
                NameEventComboBox.DisplayMemberPath = "EventName";
                NameEventComboBox.SelectedValuePath = "IDEvent";
                var sectors = _context.Sectors.OrderBy(s => s.SectorName).ToList();
                NameSectorComboBox.ItemsSource = sectors;
                NameSectorComboBox.DisplayMemberPath = "SectorName";
                NameSectorComboBox.SelectedValuePath = "IDSector";
                if (events.Any()) NameEventComboBox.SelectedIndex = 0;
                if (sectors.Any()) NameSectorComboBox.SelectedIndex = 0;
                NameEventComboBox.SelectionChanged += NameEventComboBox_SelectionChanged;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void NameEventComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NameEventComboBox.SelectedItem != null)
            {
                EventDeadline();
            }
        }
        private void EventDeadline()
        {
            var selectedEvent = (Events)NameEventComboBox.SelectedItem;
            EndDate.SelectedDate = selectedEvent.StartDate;
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы точно хотите отменить создание задания?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _predsedWindow.mainFrame.Navigate(new PageViewTasksOnEventsPredsed(_predsedWindow));
            }
        }

        private void SaveCreateEvent_Click(object sender, RoutedEventArgs e)
        {
            if (NameEventComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите мероприятие!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(NameTask.Text))
            {
                MessageBox.Show("Введите название задания!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (NameSectorComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите сектор!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (EndDate.SelectedDate == null)
            {
                MessageBox.Show("Укажите дедлайн задания!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (EndDate.SelectedDate <= DateTime.Today)
            {
                MessageBox.Show("Дедлайн не может быть раньше текущей даты!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!int.TryParse(PointsOfTask.Text, out int points) || points <= 0)
            {
                MessageBox.Show("Укажите корректное количество баллов (целое положительное число)!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                var newTask = new EventTasks
                {
                    IDEvent = (int)NameEventComboBox.SelectedValue,
                    IDSector = (int)NameSectorComboBox.SelectedValue,
                    TaskName = NameTask.Text,
                    TasksDescription = Description.Text,
                    Deadline = EndDate.SelectedDate.Value,
                    Points = points,
                    Status = "В поиске"
                };
                _context.EventTasks.Add(newTask);
                _context.SaveChanges();
                MessageBox.Show("Задание успешно создано!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                _predsedWindow.mainFrame.Navigate(new PageViewTasksOnEventsPredsed(_predsedWindow));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании задания: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
