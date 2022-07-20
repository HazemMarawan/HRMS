using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRMS.ViewModels
{
    public class TaskManagementViewModel
    {
        public List<UserTaskViewModel> allTasks { get; set; }
        public List<UserTaskViewModel> assignedTasks { get; set; }
        public List<UserTaskViewModel> myTasks { get; set; }
        public List<UserTaskViewModel> favTasks { get; set; }
    }
}