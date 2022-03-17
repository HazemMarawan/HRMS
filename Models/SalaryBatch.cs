using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HRMS.Models
{
    public class SalaryBatch
    {
        [Key]
        public int id { get; set; }
        public int? month { get; set; }
        public int? year { get; set; }
        public int? count { get; set; }
        public double? total { get; set; }
        public int? active { get; set; }
        public int? created_by { get; set; }
        public string notes { get; set; }
        public string file_path { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public virtual ICollection<SalaryBatchDetail> SalaryBatchDetails { get; set; }


    }
}