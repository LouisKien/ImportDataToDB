using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportDataToDB.Entity
{
    public class Student
    {
        public int Id { get; set; }
        public string StudentCode { get; set; }
        public int SchoolYearId { get; set; }
        public bool Status { get; set; }
        public SchoolYear SchoolYear { get; set; }
        public ICollection<Score> Scores { get; set; }
    }
}
