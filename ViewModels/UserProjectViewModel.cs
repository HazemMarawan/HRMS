using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRMS.ViewModels
{
    public class UserProjectViewModel
    {
        public int id { get; set; }
        public int? user_id { get; set; }
        public int? type { get; set; }
        public int? team_leader_id { get; set; }
        public string user_name { get; set; }
        public int? project_id { get; set; }
        public string project_name { get; set; }
        public int? area_id { get; set; }
        public string area_name { get; set; }
        public int? branch_id { get; set; }
        public string branch_name { get; set; }
        public DateTime? working_date { get; set; }
        public int? no_of_numbers { get; set; }
        public int? productivity_type { get; set; }
        public int? productivity_work_place { get; set; }
        public string part_id { get; set; }
        public double? equipment_quantity { get; set; }
        public double? mvoh { get; set; }
        public double? lvoh { get; set; }
        public double? mvug { get; set; }
        public double? lvug { get; set; }
        public double? cost { get; set; }
        public string note { get; set; }
        public int? status { get; set; }
        public int? approved_by { get; set; }
        public int? rejected_by { get; set; }
        public int? created_by { get; set; }
        public int? updated_by { get; set; }
        public int? deleted_by { get; set; }
        public DateTime? rejected_at { get; set; }
        public DateTime? approved_at { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? deleted_at { get; set; }
        public DateTime? from_date { get; set; }
        public DateTime? to_date { get; set; }
    }
}