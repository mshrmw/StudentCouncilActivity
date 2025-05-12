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
    /// Логика взаимодействия для PageSectorsStudent.xaml
    /// </summary>
    public partial class PageSectorsStudent : Page
    {
        private studDB _context = studDB.GetContext();
        private int _currentStudentId;
        public PageSectorsStudent()
        {
            InitializeComponent();
            LoadSectors();
            ComboBoxSectors.SelectedIndex = 0;
            _currentStudentId = App.CurrentStudentId;
        }
        private void LoadSectors()
        {
            try
            {
                var sectors = _context.Sectors.OrderBy(s => s.SectorName).ToList();
                DataGridSectors.ItemsSource = sectors;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoadStudentSectors()
        {
            try
            {
                var studentSectors = _context.StudentSectors.Where(ss => ss.IDStudent == _currentStudentId).Select(ss => ss.Sectors).OrderBy(s => s.SectorName).ToList();
                DataGridSectors.ItemsSource = studentSectors;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void FilterSectors_Click(object sender, RoutedEventArgs e)
        {
            if (ComboBoxSectors.SelectedIndex == 0)
            {
                LoadSectors();
            }
            else
            {
                LoadStudentSectors();
            }
        }

        private void JoinFromSectors_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridSectors.SelectedItem is Sectors selectedSector)
            {
                try
                {
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
                    var activistPosition = _context.Positions.FirstOrDefault(p => p.RoleName == "Активист");
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

        private void ExitFromSectors_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridSectors.SelectedItem is Sectors selectedSector)
            {
                try
                {
                    var membership = _context.StudentSectors.FirstOrDefault(ss => ss.IDStudent == _currentStudentId && ss.IDSector == selectedSector.IDSector);
                    if (membership == null)
                    {
                        MessageBox.Show("Вы не состоите в этом секторе!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    var currentPosition = _context.StudentPositions.FirstOrDefault(sp => sp.IDStudent == _currentStudentId && sp.IDSector == selectedSector.IDSector);
                    if(currentPosition != null)
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
                        LoadStudentSectors();
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
    }
}
