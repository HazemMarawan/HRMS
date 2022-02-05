using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HRMS.Models
{
    public class VacationRequest
    {
        [Key]
        public int id { get; set; }
        public int? user_id { get; set; }
        [ForeignKey("VacationType")]
        public int? vacation_type_id { get; set; }
        public VacationType VacationType { get; set; }
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