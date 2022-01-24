﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HRMS.Models
{
    public class Project
    {
        [Key]
        public int id { get; set; }
        public string name { get; set; }
        [ForeignKey("ProjectType")]
        public int? project_type_id { get; set; }
        public ProjectType ProjectType { get; set; }
        public DateTime? start_date { get; set; }
        public DateTime? end_date { get; set; }
        public int? active { get; set; }
        public int? created_by { get; set; }
        public int? updated_by { get; set; }
        public int? deleted_by { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? deleted_at { get; set; }
        public virtual ICollection<BranchProject> BranchProjects { get; set; }
        public virtual ICollection<UserProject> UserProjects { get; set; }


    }
}