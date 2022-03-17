using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRMS.ViewModels
{
    public class SalaryBatchViewModel
    {
        public int id { get; set; }
        public int? month { get; set; }
        public int? year { get; set; }
        public int? count { get; set; }
        public double? total { get; set; }
        public int? type { get; set; }
        public int? branch_id { get; set; }
        public int? active { get; set; }
        public string string_created_by { get; set; }
        public int? created_by { get; set; }
        public string notes { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public HttpPostedFileBase file { get; set; }
        public string file_path { get; set; }
    }
}