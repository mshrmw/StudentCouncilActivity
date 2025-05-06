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
    /// Логика взаимодействия для PageTasksStudent.xaml
    /// </summary>
    public partial class PageTasksStudent : Page
    {
        private studDB _context = studDB.GetContext();
        private int _currentStudentId;
        public PageTasksStudent()
        {
            InitializeComponent();
            _currentStudentId = App.CurrentStudentId;
            ComboBoxTasks.SelectedIndex = 2;
            LoadTasks("Принят");
        }
        private void LoadTasks(string statusFilter = null)
        {
            try
            {
                var query = from reg in _context.Registrations
                            join task in _context.EventTasks on reg.IDTask equals task.IDTask
                            join ev in _context.Events on task.IDEvent equals ev.IDEvent
                            join sec in _context.Sectors on task.IDSector equals sec.IDSector
                            where reg.IDStudent == _currentStudentId
                            select new
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
                DataGridTasks.ItemsSource = query.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заданий: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void FilterTasks_Click(object sender, RoutedEventArgs e)
        {
            string statusFilter = null;
            switch (ComboBoxTasks.SelectedIndex)
            {
                case 1: // В обработке
                    statusFilter = "Зарегистрирован";
                    break;
                case 2: // Принятые
                    statusFilter = "Принят";
                    break;
                case 3: // Выполненные
                    statusFilter = "Выполнен";
                    break;
                    // case 0: Все - оставляем statusFilter = null
            }
            LoadTasks(statusFilter);
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
                int registrationId = selectedTask.IDRegistration;
                var registration = _context.Registrations.Find(registrationId);
                if (registration != null)
                {
                    _context.Registrations.Remove(registration);
                    _context.SaveChanges();
                    MessageBox.Show("Задание успешно удалено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadTasks(ComboBoxTasks.SelectedIndex == 0 ? null : ComboBoxTasks.SelectedIndex == 1 ? "Зарегистрирован" : ComboBoxTasks.SelectedIndex == 2 ? "Принят" : "Выполнен");
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
                var registration = _context.Registrations.Find(registrationId);
                registration.RegistrationStatus = "Выполнен";
                var task = _context.EventTasks.FirstOrDefault(t => t.IDTask == taskId);
                task.Status = "Выполнен";
                var student = _context.Students.Find(_currentStudentId);
                student.Points += points;
                _context.SaveChanges();
                MessageBox.Show("Задание успешно выполнено! Баллы добавлены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                string currentFilter = ComboBoxTasks.SelectedIndex == 0 ? null : ComboBoxTasks.SelectedIndex == 1 ? "Зарегистрирован" : ComboBoxTasks.SelectedIndex == 2 ? "Принят" : "Выполнен";
                LoadTasks(currentFilter);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выполнении задания: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
