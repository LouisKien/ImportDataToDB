using ImportDataToDB.Data;
using ImportDataToDB.Repository;
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

namespace ImportDataToDB
{
    /// <summary>
    /// Interaction logic for Statistics.xaml
    /// </summary>
    public partial class Statistics : Window
    {
        public string selectedYear { get; }
        public Statistics(string selectedYear)
        {
            InitializeComponent();
            this.selectedYear = selectedYear;
            lbBestScore.Content = $"Thống Kê Điểm Thủ Khoa Các Khối Năm {selectedYear}";
            lbBestScoreList.Content = $"Thống Kê Thông Tin Thủ Khoa Các Khối Năm {selectedYear}";
            lvBestScore.ItemsSource = LoadBestScore(selectedYear);
        }

        private List<BestScore> LoadBestScore(string selectedYear)
        {
            List<BestScore> scores = new List<BestScore>();
            
            return scores;
        }
    }
}
