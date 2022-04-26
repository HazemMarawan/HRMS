using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HRMS.Models
{
    public class Target
    {
        [Key]
        public int id { get; set; }
        public double? mvoh { get; set; }
        public double? lvoh { get; set; }
        public double? mvug { get; set; }
        public double? lvug { get; set; }
        public double? transformer { get; set; }
        public double? pole { get; set; }
        public double? meter { get; set; }
        public double? distribution_box { get; set; }
        public double? rmu { get; set; }
        public double? switchh { get; set; }
        public int? active { get; set; }
        public int? created_by { get; set; }
        public int? updated_by { get; set; }
        public int? deleted_by { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? deleted_at { get; set; }
    }
}