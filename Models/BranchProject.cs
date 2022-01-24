﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HRMS.Models
{
    public class BranchProject
    {
        [Key]
        public int id { get; set; }
        [ForeignKey("Branch")]
        public int? branch_id { get; set; }
        public Branch Branch { get; set; }
        [ForeignKey("Project")]
        public int? project_id { get; set; }
        public Project Project { get; set; }
        public int? created_by { get; set; }
        public int? updated_by { get; set; }
        public int? deleted_by { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? deleted_at { get; set; }

    }
}