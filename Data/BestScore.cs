using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportDataToDB.Data
{
    public class BestScore
    {
        public string year { get; set; }
        public double? a00 { get; set; }
        public double? b00 { get; set;}
        public double? c00 { get; set; }
        public double? d01 { get; set;}
        public double? a01 { get; set;}

        public BestScore(string year, double? a00, double? b00, double? c00, double? d01, double? a01)
        {
            this.year = year;
            this.a00 = a00;
            this.b00 = b00;
            this.c00 = c00;
            this.d01 = d01;
            this.a01 = a01;
        }
    }
}
