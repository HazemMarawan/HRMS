using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRMS.ViewModels
{
    public class ProjectViewModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public int? project_type_id { get; set; }
        public string project_type_name { get; set; }
        public DateTime? start_date { get; set; }
        public DateTime? end_date { get; set; }
        public double? mvoh { get; set; }
        public double? lvoh { get; set; }
        public double? mvug { get; set; }
        public double? lvug { get; set; }
        public double? equipment_quantity { get; set; }
        public int? active { get; set; }
        public int? created_by { get; set; }
        public int? updated_by { get; set; }
        public int? deleted_by { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? deleted_at { get; set; }
        public List<AreaViewModel> areas { get; set; }
    }

}