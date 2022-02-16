using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HRMS.Models
{
    public class Email
    {
        [Key]
        public int id { get; set; }
        public string subject { get; set; }
        public string body { get; set; }
        public int? from_user { get; set; }
        public List<int> to_users { get; set; }
        public int? related_id { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public virtual ICollection<EmailAttachment> EmailAttachment { get; set; }

    }
}