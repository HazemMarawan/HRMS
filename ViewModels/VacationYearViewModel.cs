using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRMS.ViewModels
{
    public class VacationYearViewModel
    {
        public int id { get; set; }
        public int? year { get; set; }
        public int? user_id { get; set; }
        public int? vacation_balance { get; set; }
        public int? remaining { get; set; }
        public int? a3tyady_vacation_counter { get; set; }
        public int? arda_vacation_counter { get; set; }
        public int? medical_vacation_counter { get; set; }
        public int? married_vacation_counter { get; set; }
        public int? work_from_home_vacation_counter { get; set; }
        public int? death_vacation_counter { get; set; }
        public int? active { get; set; }
        public int? created_by { get; set; }
        public int? updated_by { get; set; }
        public int? deleted_by { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? deleted_at { get; set; }
    }
}