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
    /// Логика взаимодействия для PageCreateSectorsPredsed.xaml
    /// </summary>
    public partial class PageCreateSectorsPredsed : Page
    {
        private PredsedWindow _predsedWindow;
        public PageCreateSectorsPredsed(PredsedWindow predsedWindow)
        {
            InitializeComponent();
            _predsedWindow = predsedWindow;
        }

        private void SaveCreateSector_Click(object sender, RoutedEventArgs e)
        {
            _predsedWindow.mainFrame.Navigate(new PageSectorPredsed(_predsedWindow));
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            _predsedWindow.mainFrame.Navigate(new PageSectorPredsed(_predsedWindow));
        }
    }
}
