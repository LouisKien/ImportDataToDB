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
            using (var context = new MyDbContext())
            {
                var a00 = context.Students
                    .Where(s => s.SchoolYear.ExamYear == selectedYear && s.Status)
                    .Select(student => student.Scores
                        .Where(score => score.SubjectId == 1 || score.SubjectId == 3 || score.SubjectId == 6) // Adjust subject IDs as needed
                        .Sum(score => score.Result)
                    )
                    .OrderByDescending(totalScore => totalScore)
                    .FirstOrDefault();

                var b00 = context.Students
                    .Where(s => s.SchoolYear.ExamYear == selectedYear && s.Status)
                    .Select(student => student.Scores
                        .Where(score => score.SubjectId == 1 || score.SubjectId == 6 || score.SubjectId == 4) // Adjust subject IDs as needed
                        .Sum(score => score.Result)
                    )
                    .OrderByDescending(totalScore => totalScore)
                    .FirstOrDefault();

                var c00 = context.Students
                    .Where(s => s.SchoolYear.ExamYear == selectedYear && s.Status)
                    .Select(student => student.Scores
                        .Where(score => score.SubjectId == 2 || score.SubjectId == 7 || score.SubjectId == 8) // Adjust subject IDs as needed
                        .Sum(score => score.Result)
                    )
                    .OrderByDescending(totalScore => totalScore)
                    .FirstOrDefault();

                var d01 = context.Students
                    .Where(s => s.SchoolYear.ExamYear == selectedYear && s.Status)
                    .Select(student => student.Scores
                        .Where(score => score.SubjectId == 1 || score.SubjectId == 2 || score.SubjectId == 5) // Adjust subject IDs as needed
                        .Sum(score => score.Result)
                    )
                    .OrderByDescending(totalScore => totalScore)
                    .FirstOrDefault();

                var a01 = context.Students
                    .Where(s => s.SchoolYear.ExamYear == selectedYear && s.Status)
                    .Select(student => student.Scores
                        .Where(score => score.SubjectId == 1 || score.SubjectId == 3 || score.SubjectId == 5) // Adjust subject IDs as needed
                        .Sum(score => score.Result)
                    )
                    .OrderByDescending(totalScore => totalScore)
                    .FirstOrDefault();

                scores.Add(new BestScore(selectedYear, a00, b00, c00, d01, a01));

                var query = context.Students
    .Where(s => s.SchoolYear.ExamYear == selectedYear && s.Status)
    .Select(student => student.Scores
        .Where(score => score.SubjectId == 1 || score.SubjectId == 3 || score.SubjectId == 6)
        .Sum(score => score.Result)
    )
    .OrderByDescending(totalScore => totalScore);

                string sql = query.ToString();
            }
            return scores;
        }
    }
}
