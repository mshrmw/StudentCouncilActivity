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
    /// Логика взаимодействия для PageCreateTasksPredsed.xaml
    /// </summary>
    public partial class PageCreateTasksPredsed : Page
    {
        private PredsedWindow _predsedWindow;
        public PageCreateTasksPredsed(PredsedWindow predsedWindow)
        {
            InitializeComponent();
            _predsedWindow = predsedWindow;
            _predsedWindow.Tasks.Style = (Style)FindResource("ShapkaButtonActivity");
            _predsedWindow.Events.Style = (Style)FindResource("ShapkaButton");
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            _predsedWindow.mainFrame.Navigate(new PageTasksPredsed(_predsedWindow));
        }

        private void SaveCreateEvent_Click(object sender, RoutedEventArgs e)
        {
            _predsedWindow.mainFrame.Navigate(new PageTasksPredsed(_predsedWindow));
        }
    }
}
