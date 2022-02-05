using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRMS.ViewModels
{
    public class VacationRequestViewModel
    {
        public int id { get; set; }
        public int? user_id { get; set; }
        public string full_name { get; set; }
        public int? vacation_type_id { get; set; }
        public string vacation_name { get; set; }
        public int? status { get; set; }
        public int? active { get; set; }
        public int? year { get; set; }
        public int? created_by { get; set; }
        public int? updated_by { get; set; }
        public int? deleted_by { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? deleted_at { get; set; }
    }
}