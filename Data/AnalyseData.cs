using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportDataToDB.Data
{
    public class AnalyseData
    {
        public string year { get; set; }
        public long studentCount {  get; set; }
        public long mathematicsCount { get; set; }
        public long litetureCount { get; set; }
        public long physicsCount { get; set; }
        public long biologyCount { get; set; }
        public long englishCount { get; set; }
        public long chemistryCount { get; set; }
        public long historyCount { get; set; }
        public long geographyCount { get; set; }
        public long civicEducationCount { get; set; }

        public AnalyseData(string year, long studentCount, long mathematicsCount, long litetureCount, long physicsCount, long biologyCount, long englishCount, long chemistryCount, long historyCount, long geographyCount, long civicEducationCount)
        {
            this.year = year;
            this.studentCount = studentCount;
            this.mathematicsCount = mathematicsCount;
            this.litetureCount = litetureCount;
            this.physicsCount = physicsCount;
            this.biologyCount = biologyCount;
            this.englishCount = englishCount;
            this.chemistryCount = chemistryCount;
            this.historyCount = historyCount;
            this.geographyCount = geographyCount;
            this.civicEducationCount = civicEducationCount;
        }
    }
}
