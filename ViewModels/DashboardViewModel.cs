using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HRMS.Models;
namespace HRMS.ViewModels
{
    public class DashboardViewModel
    {
        public UserViewModel Manager { get; set; }
        public int? RegularVacations { get; set; }
        public int? RegularVacationsBalance { get; set; }
        public int? CasualVacations { get; set; }
        public int? CasualVacationsBalance { get; set; }
        public int? Permissions { get; set; }
        public int? Missions { get; set; }

        public DashboardViewModel()
        {
            RegularVacations = CasualVacations = Permissions = Missions = 0;
        }
    }
}