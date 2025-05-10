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
    /// Логика взаимодействия для PageViewTasksOnEvents.xaml
    /// </summary>
    public partial class PageViewTasksOnEvents : Page
    {
        private CoordinatorWindow _coordinatorWindow;
        private studDB _context = studDB.GetContext();
        private int _currentStudentId;
        public PageViewTasksOnEvents(CoordinatorWindow coordinatorWindow)
        {
            InitializeComponent();
            _coordinatorWindow = coordinatorWindow;
            _currentStudentId = App.CurrentStudentId;
            LoadEventsToComboBox();
            LoadTasks();
        }
        private void LoadEventsToComboBox()
        {
            try
            {
                ComboBoxTasks.Items.Clear();
                ComboBoxTasks.Items.Add("Все мероприятия");

                var events = _context.Events.Where(e => e.IDOrganizer == _currentStudentId).OrderBy(e => e.StartDate).ToList();
                foreach (var ev in events)
                {
                    ComboBoxTasks.Items.Add(ev.EventName);
                }
                ComboBoxTasks.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки мероприятий: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoadTasks(int? eventId = null)
        {
            try
            {
                var query = from task in _context.EventTasks
                            join ev in _context.Events on task.IDEvent equals ev.IDEvent
                            join sec in _context.Sectors on task.IDSector equals sec.IDSector
                            where ev.IDOrganizer == _currentStudentId
                            select new
                            {
                                task.IDTask,
                                task.TaskName,
                                task.TasksDescription,
                                task.Deadline,
                                task.Points,
                                task.Status,
                                EventName = ev.EventName,
                                SectorName = sec.SectorName,
                                task.IDEvent
                            };
                if (eventId != null)
                {
                    query = query.Where(t => t.IDEvent == eventId);
                }
                DataGridTasks.ItemsSource = query.OrderBy(t => t.Deadline).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заданий: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ViewTasksOnEvents_Click(object sender, RoutedEventArgs e)
        {
            _coordinatorWindow.mainFrame.Navigate(new PageCreateTasksCoordinator(_coordinatorWindow));
        }

        private void FilterTasks_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int? selectedEventId = null;
                if (ComboBoxTasks.SelectedIndex > 0)
                {
                    string selectedEventName = ComboBoxTasks.SelectedItem.ToString();
                    selectedEventId = _context.Events.FirstOrDefault(e1 => e1.EventName == selectedEventName && e1.IDOrganizer == _currentStudentId)?.IDEvent;
                }
                LoadTasks(selectedEventId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при фильтрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridTasks.SelectedItem == null)
            {
                MessageBox.Show("Выберите задание для удаления", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                dynamic selectedTask = DataGridTasks.SelectedItem;
                int taskId = selectedTask.IDTask;
                var result = MessageBox.Show("Вы уверены, что хотите удалить это задание?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    var taskToDelete = _context.EventTasks.Find(taskId);
                    if (taskToDelete != null)
                    {
                        var registrations = _context.Registrations.Where(r => r.IDTask == taskId);
                        _context.Registrations.RemoveRange(registrations);
                        _context.EventTasks.Remove(taskToDelete);
                        _context.SaveChanges();
                        MessageBox.Show("Задание успешно удалено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        FilterTasks_Click(null, null);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении задания: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteTask_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridTasks.SelectedItem == null)
            {
                MessageBox.Show("Выберите задание для выполнения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                dynamic selectedTask = DataGridTasks.SelectedItem;
                int taskId = selectedTask.IDTask;
                var taskToUpdate = _context.EventTasks.Find(taskId);
                if (taskToUpdate == null)
                {
                    MessageBox.Show("Задание не найдено", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (taskToUpdate.Status != "В процессе")
                {
                    MessageBox.Show("Можно выполнить только задание со статусом 'В процессе'", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                taskToUpdate.Status = "Выполнен";
                _context.SaveChanges();
                MessageBox.Show("Статус задания изменен на 'Выполнен'", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                FilterTasks_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении статуса задания: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChangeStatus_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridTasks.SelectedItem == null)
            {
                MessageBox.Show("Выберите задание для изменения статуса", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                dynamic selectedTask = DataGridTasks.SelectedItem;
                if (selectedTask.Status != "В поиске" && selectedTask.Status != "В процессе")
                {
                    MessageBox.Show("Можно изменять только статусы 'В поиске' и 'В процессе'", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                int taskId = selectedTask.IDTask;
                var task = _context.EventTasks.Find(taskId);
                if (task != null)
                {
                    task.Status = task.Status == "В поиске" ? "В процессе" : "В поиске";
                    _context.SaveChanges();
                    MessageBox.Show($"Статус задания изменён на '{task.Status}'", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    FilterTasks_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении статуса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
