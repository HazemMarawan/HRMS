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
        [ForeignKey("Area")]
        public int? area_id { get; set; }
        public Area Area { get; set; }
        [ForeignKey("Part")]
        public int? part_id_fk { get; set; }
        public Part Part { get; set; }
        [ForeignKey("Task")]
        public int? task_id { get; set; }
        public Task Task { get; set; }
        public DateTime? working_date { get; set; }
        public int? no_of_numbers { get; set; }
        public int? productivity_type { get; set; }
        public int? productivity_work_place { get; set; }
        public string part_id { get; set; }
        public double? substation { get; set; }
        public double? equipment_quantity { get; set; }
        public double? mvoh { get; set; }
        public double? lvoh { get; set; }
        public double? mvug { get; set; }
        public double? lvug { get; set; }
        public double? transformer { get; set; }
        public double? pole { get; set; }
        public double? meter { get; set; }
        public double? distribution_box { get; set; }
        public double? rmu { get; set; }
        public double? switchh { get; set; }
        public double? mvoh_target { get; set; }
        public double? lvoh_target { get; set; }
        public double? mvug_target { get; set; }
        public double? lvug_target { get; set; }
        public double? transformer_target { get; set; }
        public double? pole_target { get; set; }
        public double? meter_target { get; set; }
        public double? distribution_box_target { get; set; }
        public double? rmu_target { get; set; }
        public double? switchh_target { get; set; }
        public double? cost { get; set; }
        public string note { get; set; }
        public string returned_by_note { get; set; }
        public string rejected_by_note { get; set; }
        public string accepted_by_note { get; set; }
        public int? status { get; set; }
        public int? created_by { get; set; }
        public int? updated_by { get; set; }
        public int? deleted_by { get; set; }
        public int? approved_by { get; set; }
        public int? rejected_by { get; set; }
        public int? returned_by { get; set; }
        public DateTime? returned_at { get; set; }
        public DateTime? rejected_at { get; set; }
        public DateTime? approved_at { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? deleted_at { get; set; }
    }
}