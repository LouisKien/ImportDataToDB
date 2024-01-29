using ImportDataToDB.Data;
using ImportDataToDB.Entity;
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
            List<BestScore> bestScores = new List<BestScore>();

            int id = 1;

            using (var context = new MyDbContext())
            {
                List<SchoolYear> schoolYears = context.SchoolYears.ToList();
                
                foreach(var schoolYear in schoolYears)
                {
                    if (schoolYear.ExamYear.Equals(selectedYear))
                    {
                        id = schoolYear.Id;
                        break;
                    }
                }

                var a00 = context.Students.Where(s => s.SchoolYearId == id).Select(s => new { s.Id, s.StudentCode, s.SchoolYearId, s.Status, TotalScore = s.Scores.Where(sc => sc.SubjectId == 1 || sc.SubjectId == 3 || sc.SubjectId == 6).Sum(sc => sc.Result)}).OrderByDescending(s => s.TotalScore).FirstOrDefault();

                var b00 = context.Students.Where(s => s.SchoolYearId == id).Select(s => new { s.Id, s.StudentCode, s.SchoolYearId, s.Status, TotalScore = s.Scores.Where(sc => sc.SubjectId == 1 || sc.SubjectId == 3 || sc.SubjectId == 6).Sum(sc => sc.Result) }).OrderByDescending(s => s.TotalScore).FirstOrDefault();

                var c00 = context.Students.Where(s => s.SchoolYearId == id).Select(s => new { s.Id, s.StudentCode, s.SchoolYearId, s.Status, TotalScore = s.Scores.Where(sc => sc.SubjectId == 1 || sc.SubjectId == 3 || sc.SubjectId == 6).Sum(sc => sc.Result) }).OrderByDescending(s => s.TotalScore).FirstOrDefault();

                var d01 = context.Students.Where(s => s.SchoolYearId == id).Select(s => new { s.Id, s.StudentCode, s.SchoolYearId, s.Status, TotalScore = s.Scores.Where(sc => sc.SubjectId == 1 || sc.SubjectId == 3 || sc.SubjectId == 6).Sum(sc => sc.Result) }).OrderByDescending(s => s.TotalScore).FirstOrDefault();

                var a01 = context.Students.Where(s => s.SchoolYearId == id).Select(s => new { s.Id, s.StudentCode, s.SchoolYearId, s.Status, TotalScore = s.Scores.Where(sc => sc.SubjectId == 1 || sc.SubjectId == 3 || sc.SubjectId == 6).Sum(sc => sc.Result) }).OrderByDescending(s => s.TotalScore).FirstOrDefault();

                bestScores.Add(new BestScore(selectedYear, a00.TotalScore, b00.TotalScore, c00.TotalScore, d01.TotalScore, a01.TotalScore));
            }

            return bestScores;
        }
    }
}
