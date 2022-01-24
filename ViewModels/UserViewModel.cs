﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRMS.ViewModels
{
    public class UserViewModel
    {
        public int id { get; set; }
        public string code { get; set; }
        public string user_name { get; set; }
        public string password { get; set; }
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string last_name { get; set; }
        public string full_name { get; set; }
        public int? id_type { get; set; }
        public string id_type_name { get; set; }
        public string id_number { get; set; }
        public DateTime? birth_date { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public int? nationality_id { get; set; }
        public string nationality_name { get; set; }
        public int? branch_id { get; set; }
        public string branch_name { get; set; }
        public int? department_id { get; set; }
        public string department_name { get; set; }
        public string job_title { get; set; }
        public int? gender { get; set; }
        public DateTime? hiring_date { get; set; }
        public int? vacations_balance { get; set; }
        public string imagePath { get; set; }
        public HttpPostedFileBase image { get; set; }
        public string notes { get; set; }
        public int? type { get; set; }
        public int? active { get; set; }
        public int? created_by { get; set; }
        public int? updated_by { get; set; }
        public int? deleted_by { get; set; }
    }
}