using ClosedXML.Excel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
    /// Логика взаимодействия для PageEventsPredsed.xaml
    /// </summary>
    public partial class PageEventsPredsed : Page
    {
        private PredsedWindow _predsedWindow;
        private studDB _context = studDB.GetContext();
        private int _currentStudentId;
        public PageEventsPredsed(PredsedWindow predsedWindow)
        {
            InitializeComponent();
            _predsedWindow = predsedWindow;
            ComboBoxEvents.SelectedIndex = 2; 
            FilterEvents_Click(null, null);
            _currentStudentId = App.CurrentStudentId;
        }

        private void CreateEvent_Click(object sender, RoutedEventArgs e)
        {
            _predsedWindow.mainFrame.Navigate(new PageCreateEventsPredsed(_predsedWindow));
        }

        private void EditEvent_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridEvents.SelectedItem != null)
            {
                dynamic selectedItem = DataGridEvents.SelectedItem;
                int eventId = selectedItem.IDEvent;
                var selectedEvent = _context.Events.Find(eventId);
                if (selectedEvent != null)
                {
                    _predsedWindow.mainFrame.Navigate(new PageEditEventsPredsed(_predsedWindow, selectedEvent));
                }
                else
                {
                    MessageBox.Show("Не удалось найти выбранное мероприятие!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Выберите мероприятие для редактирования!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void DeleteEvent_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridEvents.SelectedItem == null)
            {
                MessageBox.Show("Выберите мероприятие для удаления!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var result = MessageBox.Show("Вы точно хотите удалить это мероприятие?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }
            try
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        dynamic selectedItem = DataGridEvents.SelectedItem;
                        int eventId = selectedItem.IDEvent;
                        var registrationsToDelete = _context.Registrations.Where(r => _context.EventTasks.Any(t => t.IDTask == r.IDTask && t.IDEvent == eventId)).ToList();
                        _context.Registrations.RemoveRange(registrationsToDelete);
                        var tasksToDelete = _context.EventTasks.Where(t => t.IDEvent == eventId).ToList();
                        _context.EventTasks.RemoveRange(tasksToDelete);
                        var participationsToDelete = _context.StudentEventParticipation.Where(sep => sep.IDEvent == eventId).ToList();
                        _context.StudentEventParticipation.RemoveRange(participationsToDelete);
                        var eventToDelete = _context.Events.Find(eventId);
                        if (eventToDelete != null)
                        {
                            _context.Events.Remove(eventToDelete);
                        }
                        _context.SaveChanges();
                        transaction.Commit();
                        MessageBox.Show("Мероприятие успешно удалено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        FilterEvents_Click(null, null);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"Ошибка при удалении мероприятия: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении мероприятия: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void FilterEvents_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var currentDate = DateTime.Today;
                var query = from ev in _context.Events join student in _context.Students on ev.IDOrganizer equals student.IDStudent select new
                {
                    Event = ev,
                    StudentLastName = student.LastName,
                    StudentFirstName = student.FirstName,
                    StudentMiddleName = student.MiddleName
                };
                switch (ComboBoxEvents.SelectedIndex)
                {
                    case 0:
                        query = query.OrderBy(x => x.Event.StartDate);
                        break;

                    case 1: 
                        query = query.Where(x => x.Event.IDOrganizer == _currentStudentId).OrderBy(x => x.Event.StartDate);
                        break;

                    case 2: 
                        query = query.Where(x => x.Event.StartDate >= currentDate).OrderBy(x => x.Event.StartDate);
                        break;

                    case 3:
                        query = query.Where(x => x.Event.StartDate < currentDate).OrderBy(x => x.Event.StartDate);
                        break;
                }
                var dbResults = query.ToList();
                var result = dbResults.Select(x => new
                {
                    x.Event.IDEvent,
                    x.Event.EventName,
                    x.Event.StartDate,
                    x.Event.EndDate,
                    x.Event.Location,
                    x.Event.Descriptions,
                    OrganizerName = $"{x.StudentLastName} {x.StudentFirstName} {x.StudentMiddleName}"
                }).ToList();
                DataGridEvents.ItemsSource = result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при фильтрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReportEvents_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridEvents.SelectedItem == null)
            {
                MessageBox.Show("Выберите мероприятие для формирования отчёта", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                dynamic selectedItem = DataGridEvents.SelectedItem;
                int eventId = selectedItem.IDEvent;
                var selectedEvent = _context.Events.FirstOrDefault(ev => ev.IDEvent == eventId);
                if (selectedEvent == null)
                {
                    MessageBox.Show("Мероприятие не найдено", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var participants = _context.StudentEventParticipation.Where(sep => sep.IDEvent == eventId).Join(_context.Students, sep => sep.IDStudent, s => s.IDStudent, (sep, s) => new
                {
                    LastName = s.LastName,
                    FirstName = s.FirstName,
                    MiddleName = s.MiddleName,
                    Course = s.Course,
                    Group = s.Groupp
                }).OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToList();
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Отчёт");
                    var titleRange = worksheet.Range(1, 1, 1, 5);
                    titleRange.Merge();
                    titleRange.Value = $"Отчёт по мероприятию: {selectedEvent.EventName}";
                    titleRange.Style.Font.Bold = true;
                    titleRange.Style.Font.FontSize = 14;
                    titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    int currentRow = 3;
                    worksheet.Cell(currentRow, 1).Value = "Дата начала:";
                    worksheet.Cell(currentRow, 2).Value = selectedEvent.StartDate.ToString("dd.MM.yyyy");
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = "Дата окончания:";
                    worksheet.Cell(currentRow, 2).Value = selectedEvent.EndDate?.ToString("dd.MM.yyyy") ?? "Отсутствует";
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = "Место проведения:";
                    worksheet.Cell(currentRow, 2).Value = selectedEvent.Location;
                    currentRow++;

                    worksheet.Cell(currentRow, 1).Value = "Описание:";
                    worksheet.Cell(currentRow, 2).Value = selectedEvent.Descriptions ?? "Нет описания";
                    currentRow += 2;
                    worksheet.Cell(currentRow, 1).Value = "Список участников:";
                    worksheet.Range(currentRow, 1, currentRow, 5).Merge();
                    worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = "Фамилия";
                    worksheet.Cell(currentRow, 2).Value = "Имя";
                    worksheet.Cell(currentRow, 3).Value = "Отчество";
                    worksheet.Cell(currentRow, 4).Value = "Курс";
                    worksheet.Cell(currentRow, 5).Value = "Группа";
                    var headerRange = worksheet.Range(currentRow, 1, currentRow, 5);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    currentRow++;
                    foreach (var participant in participants)
                    {
                        worksheet.Cell(currentRow, 1).Value = participant.LastName;
                        worksheet.Cell(currentRow, 2).Value = participant.FirstName;
                        worksheet.Cell(currentRow, 3).Value = participant.MiddleName;
                        worksheet.Cell(currentRow, 4).Value = participant.Course;
                        worksheet.Cell(currentRow, 5).Value = participant.Group;
                        currentRow++;
                    }
                    worksheet.Columns().AdjustToContents();
                    var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                    {
                        Filter = "Excel файлы (*.xlsx)|*.xlsx",
                        FileName = $"Отчёт_мероприятие_'{selectedEvent.EventName}'_{DateTime.Now:yyyyMMdd_HHmmss}"
                    };
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        workbook.SaveAs(saveFileDialog.FileName);
                        MessageBox.Show($"Отчёт по мероприятию '{selectedEvent.EventName}' успешно сохранён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
