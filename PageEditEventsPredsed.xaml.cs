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
    /// Логика взаимодействия для PageEditEventsPredsed.xaml
    /// </summary>
    public partial class PageEditEventsPredsed : Page
    {
        private PredsedWindow _predsedWindow;
        public PageEditEventsPredsed(PredsedWindow predsedWindow)
        {
            InitializeComponent();
            _predsedWindow = predsedWindow;
        }

        private void SaveEditEvent_Click(object sender, RoutedEventArgs e)
        {
            _predsedWindow.mainFrame.Navigate(new PageEventsPredsed(_predsedWindow));
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            _predsedWindow.mainFrame.Navigate(new PageEventsPredsed(_predsedWindow));
        }
    }
}
