using System;
using System.Collections.Generic;
using System.Data;
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
    /// Логика взаимодействия для PageCreateTasksCoordinator.xaml
    /// </summary>
    public partial class PageCreateTasksCoordinator : Page
    {
        private CoordinatorWindow _coordinatorWindow;
        private studDB _context = studDB.GetContext();
        private int _currentStudentId;
        public PageCreateTasksCoordinator(CoordinatorWindow coordinatorWindow)
        {
            InitializeComponent();
            _coordinatorWindow = coordinatorWindow;
            _currentStudentId = App.CurrentStudentId;
            LoadEvents();
            _coordinatorWindow.Tasks.Style = (Style)FindResource("ShapkaButtonActivity");
            _coordinatorWindow.Events.Style = (Style)FindResource("ShapkaButton");
        }
        private void LoadEvents()
        {
            try
            {
                var events = _context.Events.OrderByDescending(e => e.StartDate).ToList();
                NameEventComboBox.ItemsSource = events;
                NameEventComboBox.DisplayMemberPath = "EventName";
                NameEventComboBox.SelectedValuePath = "IDEvent";
                if (events.Any())
                {
                    NameEventComboBox.SelectedIndex = 0;
                    LoadSectorsBasedOnEventSelection(); 
                }
                NameEventComboBox.SelectionChanged += NameEventComboBox_SelectionChanged;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки мероприятий: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void NameEventComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NameEventComboBox.SelectedItem != null)
            {
                LoadSectorsBasedOnEventSelection();
            }
        }
        private void LoadSectorsBasedOnEventSelection()
        {
            try
            {
                var selectedEvent = (Events)NameEventComboBox.SelectedItem;
                List<Sectors> sectors;
                if (selectedEvent.IDOrganizer == _currentStudentId)
                {
                    sectors = _context.Sectors.ToList();
                }
                else
                {
                    var coordinatorSectorId = _context.StudentSectors.Where(ss => ss.IDStudent == _currentStudentId).Select(ss => ss.IDSector).FirstOrDefault();
                    sectors = _context.Sectors.Where(s => s.IDSector == coordinatorSectorId).ToList();
                }
                NameSectorComboBox.ItemsSource = sectors;
                NameSectorComboBox.DisplayMemberPath = "SectorName";
                NameSectorComboBox.SelectedValuePath = "IDSector";
                if (sectors.Any())
                {
                    NameSectorComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки секторов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы точно хотите отменить регистрацию задания?", "Подтверждение выхода", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _coordinatorWindow.mainFrame.Navigate(new PageViewTasksOnEvents(_coordinatorWindow));
            }
        }

        private void SaveCreateEvent_Click(object sender, RoutedEventArgs e)
        {
            try
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
                if (EndDate.SelectedDate < DateTime.Today)
                {
                    MessageBox.Show("Укажите корректный дедлайн", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (!int.TryParse(PointsOfTask.Text, out int points) || points <= 0)
                {
                    MessageBox.Show("Укажите корректное количество баллов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
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
                _coordinatorWindow.mainFrame.Navigate(new PageViewTasksOnEvents(_coordinatorWindow));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании задания: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
