using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRMS.ViewModels
{
    public class PartViewModel
    {
        public int id { get; set; }
        public string part { get; set; }
        public int? area_id { get; set; }
        public int? project_id { get; set; }
        public double? mvoh { get; set; }
        public double? lvoh { get; set; }
        public double? mvug { get; set; }
        public double? lvug { get; set; }
        public double? mvoh_sum { get; set; }
        public double? lvoh_sum { get; set; }
        public double? mvug_sum { get; set; }
        public double? lvug_sum { get; set; }
        public double? equipment_quantity { get; set; }
        public int? branch_id { get; set; }
        public string area_name { get; set; }
        public int? active { get; set; }
        public int? created_by { get; set; }
        public int? updated_by { get; set; }
        public int? deleted_by { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? deleted_at { get; set; }
    }
}