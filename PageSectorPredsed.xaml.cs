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
    /// Логика взаимодействия для PageSectorPredsed.xaml
    /// </summary>
    public partial class PageSectorPredsed : Page
    {
        private PredsedWindow _predsedWindow ;

        public PageSectorPredsed(PredsedWindow predsedWindow)
        {
            InitializeComponent();
            _predsedWindow = predsedWindow;
        }

        private void JoinFromSectors_Click(object sender, RoutedEventArgs e)
        {
            _predsedWindow.mainFrame.Navigate(new PageCreateSectorsPredsed(_predsedWindow));
        }
    }
}
