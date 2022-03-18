using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRMS.ViewModels
{
    public class SalaryBatchDetailViewModel
    {
        public int id { get; set; }
        public int? salary_batch_id { get; set; }
        public string salary_batch_notes { get; set; }
        public int? month { get; set; }
        public int? year { get; set; }
        public int? user_id { get; set; }
        public string full_name { get; set; }
        public string bank_code { get; set; }
        public double? salary { get; set; }
        public double? insurance_deductions { get; set; }
        public double? tax_deductions { get; set; }
        public int? absense_days { get; set; }
        public double? absense_deductions { get; set; }
        public double? gm_amount { get; set; }
        public double? reserved_amount { get; set; }
        public int? addtional_hours { get; set; }
        public double? addtional_hours_amount { get; set; }
        public double? total_kilos { get; set; }
        public double? total_salary { get; set; }
        public string notes { get; set; }
        public int? active { get; set; }
        public int? created_by { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
    }
}