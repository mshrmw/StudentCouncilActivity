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
    /// Логика взаимодействия для PageEventsPredsed.xaml
    /// </summary>
    public partial class PageEventsPredsed : Page
    {
        private PredsedWindow _predsedWindow;
        public PageEventsPredsed(PredsedWindow predsedWindow)
        {
            InitializeComponent();
            _predsedWindow = predsedWindow;
        }

        private void CreateEvent_Click(object sender, RoutedEventArgs e)
        {
            _predsedWindow.mainFrame.Navigate(new PageCreateEventsPredsed(_predsedWindow));
        }

        private void EditEvent_Click(object sender, RoutedEventArgs e)
        {
            _predsedWindow.mainFrame.Navigate(new PageEditEventsPredsed(_predsedWindow));
        }
    }
}
