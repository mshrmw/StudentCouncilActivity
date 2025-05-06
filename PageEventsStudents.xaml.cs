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
    /// Логика взаимодействия для PageEventsStudents.xaml
    /// </summary>
    public partial class PageEventsStudents : Page
    {
        private studDB _context = studDB.GetContext();
        private int _currentStudentId;
        public PageEventsStudents()
        {
            InitializeComponent();
            LoadEvents();
            ComboBoxEvents.SelectedIndex = 0;
            _currentStudentId = App.CurrentStudentId;
        }
        private void LoadEvents()
        {
            try
            {
                var events = _context.Events.OrderBy(e => e.StartDate).ToList();
                DataGridEvents.ItemsSource = events;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterEvents_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var currentDate = DateTime.Today;
                IQueryable<Events> query = _context.Events;
                switch (ComboBoxEvents.SelectedIndex)
                {
                    case 0:
                        LoadEvents();
                        break;
                    case 1: 
                        query = query.Where(e1 => e1.StartDate >= currentDate);
                        break;
                    case 2:
                        query = query.Where(e2 => e2.StartDate < currentDate);
                        break;
                }

                DataGridEvents.ItemsSource = query.OrderBy(ee => ee.StartDate).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при фильтрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
