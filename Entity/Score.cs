using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportDataToDB.Entity
{
    public class Score
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        public double? Result { get; set; }
        public Student Student { get; set; }
        public Subject Subject { get; set; }
    }
}
