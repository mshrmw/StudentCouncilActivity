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
    /// Логика взаимодействия для PageSectorPredsed.xaml
    /// </summary>
    public partial class PageSectorPredsed : Page
    {
        private PredsedWindow _predsedWindow;
        private studDB _context = studDB.GetContext();
        public PageSectorPredsed(PredsedWindow predsedWindow)
        {
            InitializeComponent();
            _predsedWindow = predsedWindow;
            LoadSectors();
        }
        private void LoadSectors()
        {
            try
            {
                var sectors = _context.Sectors.OrderBy(s => s.SectorName).ToList();
                ComboBoxSectors.ItemsSource = sectors;
                ComboBoxSectors.DisplayMemberPath = "SectorName";
                ComboBoxSectors.SelectedValuePath = "IDSector";
                if (sectors.Any())
                {
                    ComboBoxSectors.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки секторов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void JoinFromSectors_Click(object sender, RoutedEventArgs e)
        {
            _predsedWindow.mainFrame.Navigate(new PageCreateSectorsPredsed(_predsedWindow));
        }

        private void DeleteSector_Click(object sender, RoutedEventArgs e)
        {
            if (ComboBoxSectors.SelectedItem == null)
            {
                MessageBox.Show("Выберите сектор для удаления", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                var selectedSector = (Sectors)ComboBoxSectors.SelectedItem;
                var result = MessageBox.Show($"Вы точно хотите удалить сектор '{selectedSector.SectorName}'?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes)
                {
                    return;
                }
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        int sectorId = selectedSector.IDSector;
                        var registrationsToDelete = _context.Registrations.Where(r => _context.EventTasks.Any(t => t.IDTask == r.IDTask && t.IDSector == sectorId)).ToList();
                        _context.Registrations.RemoveRange(registrationsToDelete);
                        var tasksToDelete = _context.EventTasks.Where(t => t.IDSector == sectorId).ToList();
                        _context.EventTasks.RemoveRange(tasksToDelete);
                        var coordinator = _context.StudentPositions.FirstOrDefault(sp => sp.IDSector == sectorId && sp.IDPosition == 2 && sp.EndDate == null);
                        if (coordinator != null)
                        {
                            var user = _context.Users.FirstOrDefault(u => u.IDStudent == coordinator.IDStudent);
                            if (user != null)
                            {
                                user.Role = "student";
                            }
                        }
                        var studentSectors = _context.StudentSectors.Where(ss => ss.IDSector == sectorId).ToList();
                        _context.StudentSectors.RemoveRange(studentSectors);
                        var positions = _context.StudentPositions.Where(sp => sp.IDSector == sectorId).ToList();
                        _context.StudentPositions.RemoveRange(positions);
                        _context.Sectors.Remove(selectedSector);
                        _context.SaveChanges();
                        transaction.Commit();
                        MessageBox.Show("Сектор успешно удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadSectors();
                        DataGridSectorsMember.ItemsSource = null;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"Ошибка при удалении сектора: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении сектора: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ShowMembers_Click(object sender, RoutedEventArgs e)
        {
            if (ComboBoxSectors.SelectedItem == null)
            {
                MessageBox.Show("Выберите сектор", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                var selectedSector = (Sectors)ComboBoxSectors.SelectedItem;
                var members = _context.StudentSectors.Where(ss => ss.IDSector == selectedSector.IDSector).Join(_context.Students, ss => ss.IDStudent, s => s.IDStudent, (ss, s) => new
                {
                    s.LastName,
                    s.FirstName,
                    s.MiddleName,
                    s.Course,
                    s.Groupp
                }).OrderBy(s=> s.LastName).ThenBy(s => s.FirstName).ThenBy(s => s.MiddleName).ToList();
                DataGridSectorsMember.ItemsSource = members;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке участников: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReportMembers_Click(object sender, RoutedEventArgs e)
        {
            if (ComboBoxSectors.SelectedItem == null)
            {
                MessageBox.Show("Выберите сектор для формирования отчёта", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                var selectedSector = (Sectors)ComboBoxSectors.SelectedItem;
                int sectorId = selectedSector.IDSector;
                var reportData = _context.StudentSectors.Where(ss => ss.IDSector == sectorId).Join(_context.Students, ss => ss.IDStudent, s => s.IDStudent, (ss, s) => new
                {
                    LastName = s.LastName,
                    FirstName = s.FirstName,
                    MiddleName = s.MiddleName,
                    Course = s.Course,
                    Group = s.Groupp,
                    SectorId = ss.IDSector
                }).Join(_context.Sectors, ss => ss.SectorId, sec => sec.IDSector, (ss, sec) => new
                {
                    ss.LastName,
                    ss.FirstName,
                    ss.MiddleName,
                    ss.Course,
                    ss.Group,
                    SectorName = sec.SectorName
                }).OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToList();
                if (reportData.Count == 0)
                {
                    MessageBox.Show("В выбранном секторе нет участников", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add($"Сектор {selectedSector.SectorName}");
                    worksheet.Cell(1, 1).Value = "Фамилия";
                    worksheet.Cell(1, 2).Value = "Имя";
                    worksheet.Cell(1, 3).Value = "Отчество";
                    worksheet.Cell(1, 4).Value = "Курс";
                    worksheet.Cell(1, 5).Value = "Группа";
                    worksheet.Cell(1, 6).Value = "Сектор";
                    for (int i = 0; i < reportData.Count; i++)
                    {
                        var row = reportData[i];
                        worksheet.Cell(i + 2, 1).Value = row.LastName;
                        worksheet.Cell(i + 2, 2).Value = row.FirstName;
                        worksheet.Cell(i + 2, 3).Value = row.MiddleName;
                        worksheet.Cell(i + 2, 4).Value = row.Course;
                        worksheet.Cell(i + 2, 5).Value = row.Group;
                        worksheet.Cell(i + 2, 6).Value = row.SectorName;
                    }
                    var headerRange = worksheet.Range(1, 1, 1, 6);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Columns().AdjustToContents();
                    var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                    {
                        Filter = "Excel files (*.xlsx)|*.xlsx",
                        FileName = $"Отчёт_{selectedSector.SectorName}_сектор_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}"
                    };
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        workbook.SaveAs(saveFileDialog.FileName);
                        MessageBox.Show($"Отчёт {selectedSector.SectorName} сектор успешно сохранён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
