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
using System.Windows.Shapes;

namespace StudentCouncilActivity
{
    /// <summary>
    /// Логика взаимодействия для PredsedWindow.xaml
    /// </summary>
    public partial class PredsedWindow : Window
    {
        public PredsedWindow()
        {
            InitializeComponent();
        }
        private void SetActiveButton(Button activeButton, Page page)
        {
            Profil.Style = (Style)FindResource("ShapkaButton");
            Sectors.Style = (Style)FindResource("ShapkaButton");
            Registration.Style = (Style)FindResource("ShapkaButton");
            Events.Style = (Style)FindResource("ShapkaButton");
            Tasks.Style = (Style)FindResource("ShapkaButton");
            Tops.Style = (Style)FindResource("ShapkaButton");
            Roles.Style = (Style)FindResource("ShapkaButton");
            Exit.Style = (Style)FindResource("ShapkaButton");
            activeButton.Style = (Style)FindResource("ShapkaButtonActivity");
            mainFrame.Navigate(page);
        }
        private void Roles_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(Roles, new PageRole());
        }

        private void Profil_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(Profil, new PageProfilStudent());
        }

        private void Sectors_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(Sectors, new PageSectorPredsed(this));
        }

        private void Events_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(Events, new PageEventsPredsed(this));
        }

        private void Registration_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(Registration, new PageRegistrationPredsed(this));
        }

        private void Tasks_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(Tasks, new PageTasksPredsed(this));
        }

        private void Tops_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(Tops, new PageTops());
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Profil.Style = (Style)FindResource("ShapkaButton");
            Sectors.Style = (Style)FindResource("ShapkaButton");
            Registration.Style = (Style)FindResource("ShapkaButton");
            Events.Style = (Style)FindResource("ShapkaButton");
            Tasks.Style = (Style)FindResource("ShapkaButton");
            Tops.Style = (Style)FindResource("ShapkaButton");
            Exit.Style = (Style)FindResource("ShapkaButtonActivity");
            MessageBoxResult result = MessageBox.Show(
                "Вы точно хотите выйти?",
                "Подтверждение выхода",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
        }
    }
}
