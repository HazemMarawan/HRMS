using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRMS.ViewModels
{
    public class UserTaskViewModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int? task_classification_id { get; set; }
        public string task_classification_name { get; set; }
        public int? user_id { get; set; }
        public string full_name { get; set; }
        public string from_full_name { get; set; }
        public int? is_favourite_by_owner { get; set; }
        public int? is_favourite_by_assignee { get; set; }
        public int? is_favourite { get; set; }
        public int? own_status { get; set; }
        public int? status { get; set; }
        public int? active { get; set; }
        public int? created_by { get; set; }
        public int? updated_by { get; set; }
        public int? deleted_by { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? deleted_at { get; set; }
    }
}