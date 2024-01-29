using ImportDataToDB.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportDataToDB.Data
{
    public class StudentBestScore
    {
        public string grade { get; set; }
        public int studentId { get; set; }
        public string province { get; set; }
        public double? subject_1 { get; set; }
        public double? subject_2 { get; set; }
        public double? subject_3 { get; set; }
        public double? total { get; set; }
        public string grade_name { get; set; }

        public StudentBestScore (string grade, int studentId, string province, double? subject_1, double? subject_2, double? subject_3, double? total, string grade_name)
        {
            this.grade = grade;
            this.studentId = studentId;
            this.province = province;
            this.subject_1 = RoundToTwoDecimalPlaces(subject_1);
            this.subject_2 = RoundToTwoDecimalPlaces(subject_2);
            this.subject_3 = RoundToTwoDecimalPlaces(subject_3);
            this.total = RoundToTwoDecimalPlaces(total);
            this.grade_name = grade_name;
        }

        private double? RoundToTwoDecimalPlaces(double? value)
        {
            return value.HasValue ? Math.Round(value.Value, 2) : (double?)null;
        }
    }
}
