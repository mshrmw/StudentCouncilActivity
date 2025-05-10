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
using static StudentCouncilActivity.CoordinatorWindow;

namespace StudentCouncilActivity
{
    /// <summary>
    /// Логика взаимодействия для PageEventsCoordinator.xaml
    /// </summary>
    public partial class PageEventsCoordinator : Page
    {
        private CoordinatorWindow _coordinatorWindow;
        private studDB _context = studDB.GetContext();
        private int _currentStudentId;
        public PageEventsCoordinator(CoordinatorWindow coordinatorWindow)
        {
            InitializeComponent();
            _coordinatorWindow = coordinatorWindow;
            ComboBoxEvents.SelectedIndex = 2;
            FilterEvents_Click(null, null);
            _currentStudentId = App.CurrentStudentId;
        }
        private void CreateEvent_Click(object sender, RoutedEventArgs e)
        {
            _coordinatorWindow.mainFrame.Navigate(new PageCreateEventsCoordinator(_coordinatorWindow));
        }

        private void EditEvent_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridEvents.SelectedItem is Events selectedEvent)
            {
                if (selectedEvent.IDOrganizer == _currentStudentId)
                {
                    _coordinatorWindow.mainFrame.Navigate(new PageEditEventsCoordinator(_coordinatorWindow, selectedEvent));
                }
                else
                {
                    MessageBox.Show("Вы можете редактировать только свои мероприятия!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Выберите мероприятие для редактирования!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                        query = query.OrderBy(e0 => e0.StartDate);
                        break;

                    case 1: 
                        query = query.Where(e1 => e1.IDOrganizer == _currentStudentId).OrderBy(e1 => e1.StartDate);
                        break;

                    case 2: 
                        query = query.Where(e2 => e2.StartDate >= currentDate).OrderBy(e2 => e2.StartDate);
                        break;

                    case 3:
                        query = query.Where(e3 => e3.StartDate < currentDate).OrderBy(e3 => e3.StartDate);
                        break;
                }
                DataGridEvents.ItemsSource = query.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при фильтрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
