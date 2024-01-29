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
            LoadBestScore(selectedYear);
        }

        private void LoadBestScore(string selectedYear)
        {
            List<BestScore> bestScores = new List<BestScore>();
            List<StudentBestScore> studentBestScores = new List<StudentBestScore>();

            int id = 0;

            using (var context = new MyDbContext())
            {
                List<SchoolYear> schoolYears = context.SchoolYears.ToList();
                List<Province> provinces = context.Provinces.ToList();

                foreach (var schoolYear in schoolYears)
                {
                    if (schoolYear.ExamYear.Equals(selectedYear))
                    {
                        id = schoolYear.Id;
                        break;
                    }
                }

                var a00 = context.Students.Where(s => s.SchoolYearId == id).Select(s => new{s.Id,s.StudentCode,s.SchoolYearId,s.Status,s.MaTinh,TotalScore = s.Scores.Where(sc => sc.SubjectId == 1 || sc.SubjectId == 3 || sc.SubjectId == 6).Sum(sc => sc.Result),Scores = s.Scores.Where(sc => sc.SubjectId == 1 || sc.SubjectId == 3 || sc.SubjectId == 6)}).OrderByDescending(s => s.TotalScore).FirstOrDefault();
                List<Score> a00scores = a00?.Scores.ToList();
                foreach (var province in provinces)
                {
                    if (a00.MaTinh == province.MaTinh)
                    {
                        studentBestScores.Add(new StudentBestScore("A00", int.Parse(a00.StudentCode), province.TenTinh, a00scores[0].Result, a00scores[1].Result, a00scores[2].Result, a00.TotalScore, "Toán, Lý, Hóa"));
                    }
                }

                var b00 = context.Students.Where(s => s.SchoolYearId == id).Select(s => new { s.Id, s.StudentCode, s.SchoolYearId, s.Status, s.MaTinh, TotalScore = s.Scores.Where(sc => sc.SubjectId == 1 || sc.SubjectId == 6 || sc.SubjectId == 4).Sum(sc => sc.Result), Scores = s.Scores.Where(sc => sc.SubjectId == 1 || sc.SubjectId == 6 || sc.SubjectId == 4) }).OrderByDescending(s => s.TotalScore).FirstOrDefault();
                List<Score> b00scores = b00?.Scores.ToList();
                foreach (var province in provinces)
                {
                    if (b00.MaTinh == province.MaTinh)
                    {
                        studentBestScores.Add(new StudentBestScore("B00", int.Parse(b00.StudentCode), province.TenTinh, b00scores[0].Result, b00scores[1].Result, b00scores[2].Result, b00.TotalScore, "Toán, Hóa, Sinh"));
                    }
                }

                var c00 = context.Students.Where(s => s.SchoolYearId == id).Select(s => new { s.Id, s.StudentCode, s.SchoolYearId, s.Status, s.MaTinh, TotalScore = s.Scores.Where(sc => sc.SubjectId == 2 || sc.SubjectId == 7 || sc.SubjectId == 8).Sum(sc => sc.Result), Scores = s.Scores.Where(sc => sc.SubjectId == 2 || sc.SubjectId == 7 || sc.SubjectId == 8) }).OrderByDescending(s => s.TotalScore).FirstOrDefault();
                List<Score> c00scores = c00?.Scores.ToList();
                foreach (var province in provinces)
                {
                    if (c00.MaTinh == province.MaTinh)
                    {
                        studentBestScores.Add(new StudentBestScore("C00", int.Parse(c00.StudentCode), province.TenTinh, c00scores[0].Result, c00scores[1].Result, c00scores[2].Result, c00.TotalScore, "Văn, Sử, Địa"));
                    }
                }

                var d01 = context.Students.Where(s => s.SchoolYearId == id).Select(s => new { s.Id, s.StudentCode, s.SchoolYearId, s.Status, s.MaTinh, TotalScore = s.Scores.Where(sc => sc.SubjectId == 1 || sc.SubjectId == 2 || sc.SubjectId == 5).Sum(sc => sc.Result), Scores = s.Scores.Where(sc => sc.SubjectId == 1 || sc.SubjectId == 2 || sc.SubjectId == 5) }).OrderByDescending(s => s.TotalScore).FirstOrDefault();
                List<Score> d01scores = d01?.Scores.ToList();
                foreach (var province in provinces)
                {
                    if (d01.MaTinh == province.MaTinh)
                    {
                        studentBestScores.Add(new StudentBestScore("D01", int.Parse(d01.StudentCode), province.TenTinh, d01scores[0].Result, d01scores[1].Result, d01scores[2].Result, d01.TotalScore, "Toán, Văn, Anh"));
                    }
                }

                var a01 = context.Students.Where(s => s.SchoolYearId == id).Select(s => new { s.Id, s.StudentCode, s.SchoolYearId, s.Status, s.MaTinh, TotalScore = s.Scores.Where(sc => sc.SubjectId == 1 || sc.SubjectId == 3 || sc.SubjectId == 5).Sum(sc => sc.Result), Scores = s.Scores.Where(sc => sc.SubjectId == 1 || sc.SubjectId == 3 || sc.SubjectId == 5) }).OrderByDescending(s => s.TotalScore).FirstOrDefault();
                List<Score> a01scores = a01?.Scores.ToList();
                foreach (var province in provinces)
                {
                    if (a01.MaTinh == province.MaTinh)
                    {
                        studentBestScores.Add(new StudentBestScore("A01", int.Parse(a01.StudentCode), province.TenTinh, a01scores[0].Result, a01scores[1].Result, a01scores[2].Result, a01.TotalScore, "Toán, Lý, Anh"));
                    }
                }

                bestScores.Add(new BestScore(selectedYear, a00.TotalScore, b00.TotalScore, c00.TotalScore, d01.TotalScore, a01.TotalScore));

            }
            lvBestScore.ItemsSource = bestScores;
            lvStudentBestScore.ItemsSource = studentBestScores;
        }
    }
}
