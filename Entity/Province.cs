using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportDataToDB.Entity
{
    public class Province
    {
        [Key]
        public int MaTinh { get; set; }
        public string TenTinh { get; set; }
        public ICollection<Student> Students { get; set; }
    }
}
