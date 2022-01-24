using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HRMS.Models
{
    public class UserProject
    {
        [Key]
        public int id { get; set; }
        [ForeignKey("User")]
        public int? user_id { get; set; }
        public User User { get; set; }
        [ForeignKey("Project")]
        public int? project_id { get; set; }
        public Project Project { get; set; }
        public DateTime? working_date { get; set; }
        public int? no_of_numbers { get; set; }
        public int? status { get; set; }
        public int? created_by { get; set; }
        public int? updated_by { get; set; }
        public int? deleted_by { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? deleted_at { get; set; }
    }
}