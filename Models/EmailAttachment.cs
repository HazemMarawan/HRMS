using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace HRMS.Models
{
    public class EmailAttachment
    {
        [Key]
        public int id { get; set; }
        public string attachmentPath { get; set; }

        [ForeignKey("Email")]
        public int? email_id { get; set; }
        public Email Email { get; set; }
    }
}