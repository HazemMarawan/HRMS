using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HRMS.Models
{
    public class User
    {
        [Key]
        public int id { get; set; }
        public string code { get; set; }
        public string user_name { get; set; }
        public string password { get; set; }
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string last_name { get; set; }

        [ForeignKey("IDType")]
        public int? id_type { get; set; }
        public IDType IDType { get; set; }
        public string id_number { get; set; }
        public DateTime? birth_date { get; set; }
        public string phone { get; set; }
        public string address { get; set; }

        [ForeignKey("Nationality")]
        public int? nationality_id { get; set; }
        public Nationality Nationality { get; set; }

        [ForeignKey("Branch")]
        public int? branch_id { get; set; }
        public Branch Branch { get; set; }

        [ForeignKey("Department")]
        public int? department_id { get; set; }
        public Department Department { get; set; }

        [ForeignKey("Job")]
        public int? job_id { get; set; }
        public Job Job { get; set; }
        public int? gender { get; set; }
        public DateTime? hiring_date { get; set; }
        public int? vacations_balance { get; set; }
        public string image { get; set; }
        public string notes { get; set; }
        public int? type { get; set; }
        public int? active { get; set; }
        public int? created_by { get; set; }
        public int? updated_by { get; set; }
        public int? deleted_by { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? deleted_at { get; set; }
        public virtual ICollection<UserProject> UserProjects { get; set; }

    }
}