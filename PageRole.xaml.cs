using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
    /// Логика взаимодействия для PageRole.xaml
    /// </summary>
    public partial class PageRole : Page
    {
        private int _currentAdminId;
        private studDB _context = studDB.GetContext();
        public PageRole()
        {
            InitializeComponent();
            _currentAdminId = App.CurrentStudentId;
            LoadCoordinators();
            LoadSectors();
            LoadStudents();
        }
        private void LoadCoordinators()
        {
            try
            {
                var coordinatorsData = _context.Users.Where(u => u.Role == "coordinator").Join(_context.Students, u => u.IDStudent, s => s.IDStudent, (u, s) => new
                {
                    IDUser = u.IDUser,
                    IDStudent = s.IDStudent,
                    s.LastName,
                    s.FirstName,
                    s.MiddleName
                }).OrderBy(c => c.LastName).ThenBy(c => c.FirstName).ThenBy(c => c.MiddleName).ToList();
                var coordinators = coordinatorsData.Select(c => new
                {
                    c.IDUser,
                    c.IDStudent,
                    FullName = $"{c.LastName} {c.FirstName} {c.MiddleName}",
                    c.LastName,
                    c.FirstName,
                    c.MiddleName
                }).ToList();
                NewPredsedComboBox.ItemsSource = coordinators;
                NewPredsedComboBox.DisplayMemberPath = "FullName";
                NewPredsedComboBox.SelectedValuePath = "IDUser";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки координаторов: {ex.Message}", "Ошибка",  MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoadSectors()
        {
            try
            {
                var sectors = _context.Sectors.OrderBy(s => s.SectorName).ToList();
                SectorComboBox.ItemsSource = sectors;
                SectorComboBox.DisplayMemberPath = "SectorName";
                SectorComboBox.SelectedValuePath = "IDSector";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки секторов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadStudents()
        {
            try
            {
                var studentsData = _context.Students.Where(s => !_context.Users.Any(u => u.IDStudent == s.IDStudent && u.Role == "admin")).Select(s => new
                {
                    s.IDStudent,
                    s.LastName,
                    s.FirstName,
                    s.MiddleName
                }).OrderBy(s => s.LastName).ThenBy(s => s.FirstName).ThenBy(s => s.MiddleName).ToList();
                var students = studentsData.Select(s => new
                {
                    s.IDStudent,
                    FullName = $"{s.LastName} {s.FirstName} {s.MiddleName}",
                    s.LastName,
                    s.FirstName,
                    s.MiddleName
                }).ToList();
                NewCoordinatorComboBox.ItemsSource = students;
                NewCoordinatorComboBox.DisplayMemberPath = "FullName";
                NewCoordinatorComboBox.SelectedValuePath = "IDStudent";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки студентов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void NewCoordinatorButton_Click(object sender, RoutedEventArgs e)
        {
            if (SectorComboBox.SelectedItem == null || NewCoordinatorComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите сектор и нового координатора", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                var selectedSector = (Sectors)SectorComboBox.SelectedItem;
                dynamic selectedStudent = NewCoordinatorComboBox.SelectedItem;
                int newCoordinatorStudentId = selectedStudent.IDStudent;
                var isAlreadyCoordinator = _context.StudentPositions.Any(sp => sp.IDStudent == newCoordinatorStudentId && sp.IDSector == selectedSector.IDSector && sp.IDPosition == 2 && sp.EndDate == null);
                if (isAlreadyCoordinator)
                {
                    MessageBox.Show("Выбранный студент уже является координатором этого сектора!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var confirmResult = MessageBox.Show("Вы точно хотите назначить нового координатора?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirmResult != MessageBoxResult.Yes)
                {
                    return;
                }    
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        var newCoordinatorUser = _context.Users.FirstOrDefault(u => u.IDStudent == newCoordinatorStudentId);
                        if (newCoordinatorUser == null)
                        {
                            MessageBox.Show("У выбранного студента нет учетной записи!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        if (newCoordinatorUser.Role == "coordinator")
                        {
                            var currentCoordinatorPositions = _context.StudentPositions.Where(sp => sp.IDStudent == newCoordinatorStudentId && sp.IDPosition == 2 && sp.EndDate == null).ToList();
                            foreach (var position in currentCoordinatorPositions)
                            {
                                position.EndDate = DateTime.Today;
                            }
                            foreach (var position in currentCoordinatorPositions)
                            {
                                var newActivistPosition = new StudentPositions
                                {
                                    IDStudent = newCoordinatorStudentId,
                                    IDPosition = 1, 
                                    IDSector = position.IDSector,
                                    StartDate = DateTime.Today,
                                    EndDate = null
                                };
                                _context.StudentPositions.Add(newActivistPosition);
                            }
                        }
                        var oldCoordinator = _context.StudentPositions.FirstOrDefault(sp => sp.IDSector == selectedSector.IDSector && sp.IDPosition == 2 && sp.EndDate == null);
                        if (oldCoordinator != null)
                        {
                            oldCoordinator.EndDate = DateTime.Today;
                            var oldCoordinatorUser = _context.Users.FirstOrDefault(u => u.IDStudent == oldCoordinator.IDStudent);
                            if (oldCoordinatorUser != null)
                            {
                                oldCoordinatorUser.Role = "student";
                            }
                        }
                        var currentPosition = _context.StudentPositions.FirstOrDefault(sp => sp.IDStudent == newCoordinatorStudentId && sp.IDSector == selectedSector.IDSector && sp.EndDate == null);
                        if (currentPosition != null)
                        {
                            currentPosition.EndDate = DateTime.Today;
                        }
                        var newPosition = new StudentPositions
                        {
                            IDStudent = newCoordinatorStudentId,
                            IDPosition = 2,
                            IDSector = selectedSector.IDSector,
                            StartDate = DateTime.Today,
                            EndDate = null
                        };
                        _context.StudentPositions.Add(newPosition);
                        var sectorLink = _context.StudentSectors.FirstOrDefault(ss => ss.IDStudent == newCoordinatorStudentId && ss.IDSector == selectedSector.IDSector);
                        if (sectorLink == null)
                        {
                            var newLink = new StudentSectors
                            {
                                IDStudent = newCoordinatorStudentId,
                                IDSector = selectedSector.IDSector
                            };
                            _context.StudentSectors.Add(newLink);
                        }
                        newCoordinatorUser.Role = "coordinator";
                        _context.SaveChanges();
                        transaction.Commit();
                        MessageBox.Show("Новый координатор успешно назначен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"Ошибка при назначении координатора: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                LoadCoordinators();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void NewPredsedButton_Click(object sender, RoutedEventArgs e)
        {
            if (NewPredsedComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите нового председателя из списка", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                var confirmResult = MessageBox.Show("Вы точно хотите назначить нового председателя?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirmResult != MessageBoxResult.Yes)
                {
                    return;

                }
                dynamic selectedCoordinator = NewPredsedComboBox.SelectedItem;
                int newAdminUserId = selectedCoordinator.IDUser;
                int newAdminStudentId = selectedCoordinator.IDStudent;
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        var coordinatorPositions = _context.StudentPositions.Where(sp => sp.IDStudent == newAdminStudentId && sp.EndDate == null).ToList();
                        foreach (var position in coordinatorPositions)
                        {
                            position.EndDate = DateTime.Today;
                        }
                        var sectorConnections = _context.StudentSectors.Where(ss => ss.IDStudent == newAdminStudentId).ToList();
                        _context.StudentSectors.RemoveRange(sectorConnections);
                        var currentAdmin = _context.Users.FirstOrDefault(u => u.IDUser == _currentAdminId);
                        currentAdmin.Role = "student";
                        var newAdminUser = _context.Users.Find(newAdminUserId);
                        newAdminUser.Role = "admin";
                        _context.SaveChanges();
                        transaction.Commit();
                        MessageBox.Show("Новый председатель успешно назначен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        var mainWindow = new MainWindow();
                        mainWindow.Show();
                        Window.GetWindow(this)?.Close();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при назначении председателя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
