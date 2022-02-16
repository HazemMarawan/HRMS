using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HRMS.Models
{
    public class EmailUser
    {
        [Key]
        public int id { get; set; }
        public int? email_id { get; set; }
        public int? user_id { get; set; }
        public int is_recieved { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
    }
}