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
    /// Логика взаимодействия для PageRegistrationPredsed.xaml
    /// </summary>
    public partial class PageRegistrationPredsed : Page
    {
        private PredsedWindow _predsedWindow;
        private studDB _context = studDB.GetContext();
        private int _currentStudentId;
        public PageRegistrationPredsed(PredsedWindow predsedWindow)
        {
            InitializeComponent();
            _predsedWindow = predsedWindow;
            _currentStudentId = App.CurrentStudentId;
            LoadAllSectors();
            LoadTasks();
        }
        private void LoadAllSectors()
        {
            try
            {
                ComboBoxTask.Items.Clear();
                ComboBoxTask.Items.Add("Все сектора");
                var allSectors = _context.Sectors.OrderBy(s => s.SectorName).Select(s => s.SectorName).Distinct().ToList();
                foreach (var sector in allSectors)
                {
                    ComboBoxTask.Items.Add(sector);
                }
                ComboBoxTask.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки секторов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadTasks(string sectorFilter = null)
        {
            try
            {
                var query = from task in _context.EventTasks join ev in _context.Events on task.IDEvent equals ev.IDEvent join sec in _context.Sectors on task.IDSector equals sec.IDSector where task.Status == "В поиске" && task.Deadline >= DateTime.Today && !_context.Registrations.Any(r => r.IDTask == task.IDTask && r.IDStudent == _currentStudentId) select new
                {
                    task.IDTask,
                    task.TaskName,
                    task.TasksDescription,
                    task.Deadline,
                    task.Points,
                    EventName = ev.EventName,
                    SectorName = sec.SectorName,
                    task.IDSector
                };
                if (!string.IsNullOrEmpty(sectorFilter) && sectorFilter != "Все задания")
                {
                    query = query.Where(t => t.SectorName == sectorFilter);
                }
                DataGridTasks.ItemsSource = query.OrderBy(t => t.Deadline).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заданий: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SeeRegOnEvent_Click(object sender, RoutedEventArgs e)
        {
            _predsedWindow.mainFrame.Navigate(new PageViewEventRegistrationPredsed());
        }

        private void RegTask_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridTasks.SelectedItem == null)
            {
                MessageBox.Show("Выберите задание для регистрации", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                dynamic selectedTask = DataGridTasks.SelectedItem;
                int taskId = selectedTask.IDTask;
                bool alreadyRegistered = _context.Registrations.Any(r => r.IDTask == taskId && r.IDStudent == _currentStudentId);
                if (alreadyRegistered)
                {
                    MessageBox.Show("Вы уже зарегистрированы на это задание", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                var newRegistration = new Registrations
                {
                    IDStudent = _currentStudentId,
                    IDTask = taskId,
                    RegistrationDate = DateTime.Today,
                    RegistrationStatus = "Зарегистрирован"
                };
                _context.Registrations.Add(newRegistration);
                _context.SaveChanges();
                MessageBox.Show("Регистрация на задание успешно выполнена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                string filter = ComboBoxTask.SelectedIndex == 0 ? null : ComboBoxTask.SelectedItem.ToString();
                LoadTasks(filter);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterTasks_Click(object sender, RoutedEventArgs e)
        {
            string selectedSector = ComboBoxTask.SelectedIndex > 0 ? ComboBoxTask.SelectedItem.ToString() : null;
            LoadTasks(selectedSector);
        }
    }
}
