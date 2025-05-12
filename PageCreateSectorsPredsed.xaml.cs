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
        private studDB _context = studDB.GetContext();
        public PageCreateSectorsPredsed(PredsedWindow predsedWindow)
        {
            InitializeComponent();
            _predsedWindow = predsedWindow;
        }

        private void SaveCreateSector_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NameSector.Text))
                {
                    MessageBox.Show("Введите название сектора!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (NameSector.Text.Length > 100)
                {
                    MessageBox.Show("Название сектора не должно превышать 100 символов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (Description.Text.Length > 255)
                {
                    MessageBox.Show("Описание не должно превышать 255 символов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var existingSector = _context.Sectors.FirstOrDefault(s => s.SectorName == NameSector.Text);
                if (existingSector != null)
                {
                    MessageBox.Show("Сектор с таким названием уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var newSector = new Sectors
                {
                    SectorName = NameSector.Text,
                    Descriptions = Description.Text
                };
                _context.Sectors.Add(newSector);
                _context.SaveChanges();
                MessageBox.Show("Сектор успешно создан! Теперь назначьте координатора", "Успех", MessageBoxButton.YesNo, MessageBoxImage.Information);
                _predsedWindow.mainFrame.Navigate(new PageRole());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании сектора: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            _predsedWindow.mainFrame.Navigate(new PageSectorPredsed(_predsedWindow));
        }
    }
}
