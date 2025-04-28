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
    /// Логика взаимодействия для PageCreateTasksCoordinator.xaml
    /// </summary>
    public partial class PageCreateTasksCoordinator : Page
    {
        private CoordinatorWindow _coordinatorWindow;
        public PageCreateTasksCoordinator(CoordinatorWindow coordinatorWindow)
        {
            InitializeComponent();
            _coordinatorWindow = coordinatorWindow;
            _coordinatorWindow.Tasks.Style = (Style)FindResource("ShapkaButtonActivity");
            _coordinatorWindow.Events.Style = (Style)FindResource("ShapkaButton");
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы точно хотите отменить регистрацию задания?", "Подтверждение выхода", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _coordinatorWindow.mainFrame.Navigate(new PageTasksCoordinator(_coordinatorWindow));
            }
        }

        private void SaveCreateEvent_Click(object sender, RoutedEventArgs e)
        {
            // код сохранпения задания

            _coordinatorWindow.mainFrame.Navigate(new PageTasksCoordinator(_coordinatorWindow));
        }
    }
}
