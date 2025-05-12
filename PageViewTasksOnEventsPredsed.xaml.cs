using ClosedXML.Excel;
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
    /// Логика взаимодействия для PageViewTasksOnEventsPredsed.xaml
    /// </summary>
    public partial class PageViewTasksOnEventsPredsed : Page
    {
        private PredsedWindow _predsedWindow;
        private studDB _context = studDB.GetContext();
        private int _currentStudentId;
        public PageViewTasksOnEventsPredsed(PredsedWindow predsed)
        {
            InitializeComponent();
            _predsedWindow = predsed;
            _currentStudentId = App.CurrentStudentId;
            LoadAllEventsToComboBox();
            LoadTasks();
        }
        private void LoadAllEventsToComboBox()
        {
            try
            {
                ComboBoxTasks.Items.Clear();
                ComboBoxTasks.Items.Add("Все мероприятия");
                var events = _context.Events.OrderBy(e => e.StartDate).ToList();
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
                var query = from task in _context.EventTasks join ev in _context.Events on task.IDEvent equals ev.IDEvent join sec in _context.Sectors on task.IDSector equals sec.IDSector select new
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
        private void FilterTasks_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int? selectedEventId = null;
                if (ComboBoxTasks.SelectedIndex > 0)
                {
                    string selectedEventName = ComboBoxTasks.SelectedItem.ToString();
                    selectedEventId = _context.Events.FirstOrDefault(ev => ev.EventName == selectedEventName)?.IDEvent;
                }
                LoadTasks(selectedEventId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при фильтрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                if (selectedTask.Status == "Выполнен")
                {
                    MessageBox.Show("Нельзя удалять выполненные задания", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
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
                var acceptedRegistrations = _context.Registrations.Where(r => r.IDTask == taskId && r.RegistrationStatus == "Принят").ToList();
                foreach (var registration in acceptedRegistrations)
                {
                    bool alreadyParticipates = _context.StudentEventParticipation.Any(p => p.IDStudent == registration.IDStudent && p.IDEvent == taskToUpdate.IDEvent);
                    if (!alreadyParticipates)
                    {
                        var participation = new StudentEventParticipation
                        {
                            IDStudent = registration.IDStudent,
                            IDEvent = taskToUpdate.IDEvent
                        };
                        _context.StudentEventParticipation.Add(participation);
                        var student = _context.Students.Find(registration.IDStudent);
                        if (student != null)
                        {
                            student.Points += taskToUpdate.Points;
                        }
                    }
                }
                _context.SaveChanges();
                MessageBox.Show("Статус задания изменен на 'Выполнен'. Участники добавлены в список мероприятия", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                FilterTasks_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении статуса задания: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateTask_Click(object sender, RoutedEventArgs e)
        {
            _predsedWindow.mainFrame.Navigate(new PageCreateTasksPredsed(_predsedWindow));
        }

        private void ReportTasks_Click(object sender, RoutedEventArgs e)
        {
            if (ComboBoxTasks.SelectedItem == null || ComboBoxTasks.SelectedItem.ToString() == "Все мероприятия")
            {
                MessageBox.Show("Выберите конкретное мероприятие для формирования отчёта", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                string selectedEventName = ComboBoxTasks.SelectedItem.ToString();
                var selectedEvent = _context.Events.FirstOrDefault(ev => ev.EventName == selectedEventName);
                if (selectedEvent == null)
                {
                    MessageBox.Show("Мероприятие не найдено", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var tasks = _context.EventTasks.Where(t => t.IDEvent == selectedEvent.IDEvent).Select(t => new
                {
                    t.IDTask,
                    t.TaskName,
                    t.TasksDescription,
                    t.Points,
                    t.Status,
                    SectorName = _context.Sectors.Where(s => s.IDSector == t.IDSector).Select(s => s.SectorName).FirstOrDefault(),
                    CompletedStudents = _context.Registrations.Where(r => r.IDTask == t.IDTask && (r.RegistrationStatus == "Принят" || r.RegistrationStatus == "Выполнен")).Join(_context.Students, r => r.IDStudent, s => s.IDStudent, (r, s) => new { s.LastName, s.FirstName, s.MiddleName, s.Course, s.Groupp }).ToList()
                }).ToList();
                if (!tasks.Any())
                {
                    MessageBox.Show("Нет заданий для выбранного мероприятия", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Отчёт по заданиям");
                    worksheet.Cell(1, 1).Value = $"Отчёт по заданиям мероприятия: {selectedEvent.EventName}";
                    worksheet.Range(1, 1, 1, 5).Merge();
                    worksheet.Range(1, 1, 1, 5).Style.Font.Bold = true;
                    worksheet.Range(1, 1, 1, 5).Style.Font.FontSize = 14;
                    worksheet.Range(1, 1, 1, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    int row = 3;
                    worksheet.Cell(row, 1).Value = "Дата начала:";
                    worksheet.Cell(row, 2).Value = selectedEvent.StartDate.ToString("dd.MM.yyyy");
                    row++;
                    worksheet.Cell(row, 1).Value = "Дата окончания:";
                    worksheet.Cell(row, 2).Value = selectedEvent.EndDate?.ToString("dd.MM.yyyy") ?? "не указана";
                    row++;
                    worksheet.Cell(row, 1).Value = "Место проведения:";
                    worksheet.Cell(row, 2).Value = selectedEvent.Location ?? "не указано";
                    row += 2;
                    string[] headers = { "Название задания", "Описание", "Сектор", "Баллы", "Выполнившие студенты" };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cell(row, i + 1).Value = headers[i];
                        worksheet.Cell(row, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(row, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    }
                    row++;
                    foreach (var task in tasks)
                    {
                        worksheet.Cell(row, 1).Value = task.TaskName;
                        worksheet.Cell(row, 2).Value = task.TasksDescription;
                        worksheet.Cell(row, 3).Value = task.SectorName ?? "не указан";
                        worksheet.Cell(row, 4).Value = task.Points;
                        var students = task.CompletedStudents.Select(s => $"{s.LastName} {s.FirstName} {s.MiddleName}, {s.Course} курс, гр. {s.Groupp}").ToList();
                        worksheet.Cell(row, 5).Value = string.Join("\n", students);
                        worksheet.Cell(row, 5).Style.Alignment.WrapText = true;
                        row++;
                    }
                    worksheet.Columns().AdjustToContents();
                    var saveDialog = new Microsoft.Win32.SaveFileDialog
                    {
                        Filter = "Excel файлы (*.xlsx)|*.xlsx",
                        FileName = $"Отчёт_по_заданиям_'{selectedEvent.EventName}'_{DateTime.Now:yyyyMMdd_HHmmss}"
                    };
                    if (saveDialog.ShowDialog() == true)
                    {
                        workbook.SaveAs(saveDialog.FileName);
                        MessageBox.Show("Отчёт успешно сохранён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при формировании отчёта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
