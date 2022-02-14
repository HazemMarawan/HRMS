using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRMS.ViewModels
{
    public class DashboardViewModel
    {
        public UserViewModel Manager { get; set; }
        public int? Vacations { get; set; }
        public int? VacationsBalance { get; set; }
        public int? Permissions { get; set; }
        public int? Missions { get; set; }

        public DashboardViewModel()
        {
            Vacations = Permissions = Missions = 0;
        }
    }
}