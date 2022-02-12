using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HRMS.ViewModels
{
    public class WorkPermissionRequestViewModel
    {
        public int id { get; set; }
        public int? year { get; set; }
        public int? month { get; set; }
        public int? user_id { get; set; }
        public string reason { get; set; }
        public DateTime? date { get; set; }
        public int? minutes { get; set; }
        public int? status { get; set; }
        public int? approved_by_super_admin { get; set; }
        public DateTime? approved_by_super_admin_at { get; set; }
        public int? approved_by_branch_admin { get; set; }
        public DateTime? approved_by_branch_admin_at { get; set; }
        public int? approved_by_technical_manager { get; set; }
        public DateTime? approved_by_technical_manager_at { get; set; }
        public int? approved_by_team_leader { get; set; }
        public DateTime? approved_by_team_leader_at { get; set; }
        public int? rejected_by { get; set; }
        public DateTime? rejected_by_at { get; set; }
        public int? active { get; set; }
        public int? created_by { get; set; }
        public int? updated_by { get; set; }
        public int? deleted_by { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? deleted_at { get; set; }
        public string full_name { get; set; }
        public int permission_count { get; set; }
        public int? branch_id { get; set; }
        public int? type { get; set; }
        public int? team_leader_id { get; set; }
        public string super_admin_name { get; set; }
        public string branch_admin_name { get; set; }
        public string team_leader_name { get; set; }
        public string technical_manager_name { get; set; }
        public string rejected_by_name { get; set; }

    }
}