﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRMS.ViewModels
{
    public class BranchProjectViewModel
    {
        public int id { get; set; }
        public int? branch_id { get; set; }
        public string branch_name { get; set; }
        public int? project_id { get; set; }
        public string project_name { get; set; }
        public int? created_by { get; set; }
        public int? updated_by { get; set; }
        public int? deleted_by { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? deleted_at { get; set; }
    }
}