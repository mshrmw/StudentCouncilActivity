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
    /// Логика взаимодействия для PageTops.xaml
    /// </summary>
    public partial class PageTops : Page
    {
        private studDB _context = studDB.GetContext();
        public PageTops()
        {
            InitializeComponent();
            LoadStudentRatings();
        }
        private void LoadStudentRatings()
        {
            try
            {
                var query = _context.Students.OrderByDescending(s => s.Points).Select(s => new { s.LastName, s.FirstName, s.MiddleName, s.Points, s.Course, s.Groupp });
                DataGridTops.ItemsSource = query.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки рейтинга: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
