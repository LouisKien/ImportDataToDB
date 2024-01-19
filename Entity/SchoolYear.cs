using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportDataToDB.Entity
{
    public class SchoolYear
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ExamYear { get; set; }
        public bool Status { get; set; }
        public ICollection<Student> Students { get; set; }
    }
}
