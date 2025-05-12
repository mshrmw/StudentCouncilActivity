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
    /// Логика взаимодействия для PageTasksCoordinator.xaml
    /// </summary>
    public partial class PageTasksCoordinator : Page
    {
        private CoordinatorWindow _coordinatorWindow;
        private studDB _context = studDB.GetContext();
        private int _currentStudentId;
        public PageTasksCoordinator(CoordinatorWindow coordinatorWindow)
        {
            InitializeComponent();
            _coordinatorWindow = coordinatorWindow;
            _currentStudentId = App.CurrentStudentId;
            ComboBoxTasks.SelectedIndex = 2;
            LoadTasks("Принят");
        }
        private void LoadTasks(string statusFilter = null)
        {
            try
            {
                var query = from reg in _context.Registrations join task in _context.EventTasks on reg.IDTask equals task.IDTask join ev in _context.Events on task.IDEvent equals ev.IDEvent join sec in _context.Sectors on task.IDSector equals sec.IDSector where reg.IDStudent == _currentStudentId select new
                {
                    reg.IDRegistration,
                    reg.RegistrationStatus,
                    task.TaskName,
                    task.TasksDescription,
                    task.Deadline,
                    task.Points,
                    EventName = ev.EventName,
                    SectorName = sec.SectorName,
                    task.Status,
                    task.IDTask
                };
                if (!string.IsNullOrEmpty(statusFilter))
                {
                    query = query.Where(t => t.RegistrationStatus == statusFilter);
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
            _coordinatorWindow.mainFrame.Navigate(new PageViewTasksOnEvents(_coordinatorWindow));
        }

        private void FilterTasks_Click(object sender, RoutedEventArgs e)
        {
            string statusFilter = null;
            switch (ComboBoxTasks.SelectedIndex)
            {
                case 0:
                    statusFilter = null;
                    break;
                case 1:
                    statusFilter = "Зарегистрирован";
                    break;
                case 2:
                    statusFilter = "Принят";
                    break;
                case 3:
                    statusFilter = "Выполнен";
                    break;
                default:
                    statusFilter = null;
                    break;
            }
            LoadTasks(statusFilter);
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridTasks.SelectedItem == null)
            {
                MessageBox.Show("Выберите задание для отказа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                dynamic selectedTask = DataGridTasks.SelectedItem;
                int registrationId = selectedTask.IDRegistration;
                int taskId = selectedTask.IDTask;
                var registration = _context.Registrations.Find(registrationId);
                var task = _context.EventTasks.Find(taskId);
                if (task.Status == "Выполнен" || registration.RegistrationStatus == "Выполнен")
                {
                    MessageBox.Show("Нельзя отказаться от выполненного задания", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var result = MessageBox.Show("Вы уверены, что хотите отказаться от задания?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    _context.Registrations.Remove(registration);
                    _context.SaveChanges();
                    MessageBox.Show("Вы отказались от задания", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    FilterTasks_Click(null, null);
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
                if (selectedTask.RegistrationStatus != "Принят")
                {
                    MessageBox.Show("Можно выполнять только принятые задания", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                int registrationId = selectedTask.IDRegistration;
                int taskId = selectedTask.IDTask;
                int points = selectedTask.Points;
                var task = _context.EventTasks.Find(taskId);
                if (task.Status != "Выполнен")
                {
                    MessageBox.Show("Дождитесь, пока координатор поставит статус задания 'Выполнен'", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var registration = _context.Registrations.Find(registrationId);
                var student = _context.Students.Find(_currentStudentId);
                registration.RegistrationStatus = "Выполнен";
                student.Points += points;
                _context.SaveChanges();
                MessageBox.Show("Задание выполнено! Баллы добавлены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                FilterTasks_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выполнении задания: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
