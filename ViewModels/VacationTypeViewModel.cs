using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRMS.ViewModels
{
    public class VacationTypeViewModel
    {
        [Key]
        public int id { get; set; }
        public string name { get; set; }
        public int? must_inform_before_duration { get; set; }
        public int? inform_before_duration { get; set; }
        public int? inform_before_duration_measurement { get; set; }
        public int? need_approve { get; set; }
        public int? closed_at_specific_time { get; set; }
        public TimeSpan? closed_at { get; set; }
        public int? max_days { get; set; }
        public int? include_official_vacation { get; set; }
        public int? active { get; set; }
        public int? created_by { get; set; }
        public int? updated_by { get; set; }
        public int? deleted_by { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? deleted_at { get; set; }
    }
}