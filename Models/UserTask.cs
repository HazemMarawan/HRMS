using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HRMS.Models
{
    public class UserTask
    {
        [Key]
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }

       [ForeignKey("TaskClassification")]
        public int? task_classification_id { get; set; }
        public TaskClassification TaskClassification { get; set; }
        public int? user_id { get; set; }
        public int? is_favourite_by_owner { get; set; }
        public int? is_favourite_by_assignee { get; set; }
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