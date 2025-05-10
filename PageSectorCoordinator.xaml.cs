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
    /// Логика взаимодействия для PageSectorCoordinator.xaml
    /// </summary>
    public partial class PageSectorCoordinator : Page
    {
        private studDB _context = studDB.GetContext();
        private int _currentStudentId;
        private int _coordinatorSectorId;
        public PageSectorCoordinator()
        {
            InitializeComponent();
            _currentStudentId = App.CurrentStudentId;
            ComboBoxSectors.SelectedIndex = 0;
            LoadCoordinatorData();
            LoadSectorMembers();
            LoadAllSectors();
        }
        private void LoadCoordinatorData()
        {
            try
            {
                var coordinatorPosition = _context.StudentPositions.FirstOrDefault(sp => sp.IDStudent == _currentStudentId && sp.IDPosition == 2 && sp.EndDate == null);
                if (coordinatorPosition != null)
                {
                    _coordinatorSectorId = coordinatorPosition.IDSector;
                    var sector = _context.Sectors.FirstOrDefault(s => s.IDSector == _coordinatorSectorId);
                    if (sector != null)
                    {
                        LabelSectorName.Content += sector.SectorName;
                    }
                }
                else
                {
                    MessageBox.Show("Вы не являетесь координатором ни одного сектора!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    NavigationService?.GoBack();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных координатора: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoadSectorMembers()
        {
            try
            {
                var members = _context.StudentPositions.Where(sp => sp.IDSector == _coordinatorSectorId && sp.IDPosition == 1 && sp.EndDate == null).Join(_context.Students, sp => sp.IDStudent, s => s.IDStudent, (sp, s) => new
                        {
                            s.LastName,
                            s.FirstName,
                            s.MiddleName,
                            s.Course,
                            s.Groupp,
                            s.IDStudent
                        }).ToList();
                DataGridSectorsMember.ItemsSource = members;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки участников сектора: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoadAllSectors()
        {
            try
            {
                var sectors = _context.Sectors.ToList();
                DataGridSectors.ItemsSource = sectors;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки секторов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoadCoordinatorSector()
        {
            try
            {
                var sector = _context.StudentSectors.Where(ss => ss.IDStudent == _currentStudentId).Select(ss => ss.Sectors).ToList();
                DataGridSectors.ItemsSource = sector;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки сектора координатора: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void DeleteMember_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridSectorsMember.SelectedItem == null)
            {
                MessageBox.Show("Выберите участника для удаления", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                dynamic selectedMember = DataGridSectorsMember.SelectedItem;
                int studentId = selectedMember.IDStudent;
                var result = MessageBox.Show("Вы уверены, что хотите удалить этого участника из сектора?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    var studentSector = _context.StudentSectors.FirstOrDefault(ss => ss.IDStudent == studentId && ss.IDSector == _coordinatorSectorId);
                    if (studentSector != null)
                    {
                        _context.StudentSectors.Remove(studentSector);
                    }
                    var studentPosition = _context.StudentPositions .FirstOrDefault(sp => sp.IDStudent == studentId && sp.IDSector == _coordinatorSectorId && sp.EndDate == null);
                    if (studentPosition != null)
                    {
                        studentPosition.EndDate = DateTime.Today;
                    }
                    _context.SaveChanges();
                    MessageBox.Show("Участник успешно удален из сектора", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadSectorMembers();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении участника: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void FilterSectors_Click(object sender, RoutedEventArgs e)
        {
            if (ComboBoxSectors.SelectedIndex == 0)
            {
                LoadAllSectors();
            }
            else
            {
                LoadCoordinatorSector();
            }
        }

        private void ExitFromSectors_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridSectors.SelectedItem is Sectors selectedSector)
            {
                try
                {
                    var isCoordinatorSector = _context.StudentPositions.Any(sp => sp.IDStudent == _currentStudentId && sp.IDSector == selectedSector.IDSector && sp.IDPosition == 2 && sp.EndDate == null);
                    if (isCoordinatorSector)
                    {
                        MessageBox.Show("Вы не можете выйти из сектора, которым руководите", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    var membership = _context.StudentSectors.FirstOrDefault(ss => ss.IDStudent == _currentStudentId && ss.IDSector == selectedSector.IDSector);
                    if (membership == null)
                    {
                        MessageBox.Show("Вы не состоите в этом секторе!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    var currentPosition = _context.StudentPositions.FirstOrDefault(sp => sp.IDStudent == _currentStudentId && sp.IDSector == selectedSector.IDSector && sp.EndDate == null);
                    if (currentPosition != null)
                    {
                        var result = MessageBox.Show("Вы уверены, что хотите выйти из сектора?", "Подтверждение выхода", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.No)
                        {
                            return;
                        }
                        currentPosition.EndDate = DateTime.Today;
                    }
                    _context.StudentSectors.Remove(membership);
                    _context.SaveChanges();
                    MessageBox.Show("Вы успешно вышли из сектора!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (ComboBoxSectors.SelectedIndex == 1)
                    {
                        LoadCoordinatorSector();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при выходе из сектора: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Для выхода из сектора необходимо выбрать его из списка", "Не выбран сектор", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void JoinFromSectors_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridSectors.SelectedItem is Sectors selectedSector)
            {
                try
                {
                    bool isCoordinatorOfThisSector = _context.StudentPositions.Any(sp => sp.IDStudent == _currentStudentId && sp.IDSector == selectedSector.IDSector && sp.IDPosition == 2 && sp.EndDate == null);
                    if (isCoordinatorOfThisSector)
                    {
                        MessageBox.Show("Вы уже являетесь координатором этого сектора и не можете вступить в него повторно!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    bool alreadyMember = _context.StudentSectors.Any(ss => ss.IDStudent == _currentStudentId && ss.IDSector == selectedSector.IDSector);
                    if (alreadyMember)
                    {
                        MessageBox.Show("Вы уже состоите в этом секторе!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    var newMembership = new StudentSectors
                    {
                        IDStudent = _currentStudentId,
                        IDSector = selectedSector.IDSector
                    };
                    _context.StudentSectors.Add(newMembership);
                    var activistPosition = _context.Positions.FirstOrDefault(p => p.IDPosition == 1);
                    var positionStudent = new StudentPositions
                    {
                        IDStudent = _currentStudentId,
                        IDPosition = activistPosition.IDPosition,
                        IDSector = selectedSector.IDSector,
                        StartDate = DateTime.Today,
                        EndDate = null
                    };
                    _context.StudentPositions.Add(positionStudent);
                    _context.SaveChanges();
                    MessageBox.Show("Вы успешно вступили в сектор!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при вступлении в сектор: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите сектор из списка!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
