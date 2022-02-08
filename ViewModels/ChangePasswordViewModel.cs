using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRMS.ViewModels
{
    public class ChangePasswordViewModel
    {
        public string old_password { get; set; }
        public string new_password { get; set; }
        public string new_password_confirm { get; set; }
    }
}