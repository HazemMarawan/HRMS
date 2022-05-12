using HRMS.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRMS.Auth
{
    public class isA
    {
        public static bool SuperAdmin()
        {
            if (Convert.ToInt32(HttpContext.Current.Session["type"].ToString()) == (int)UserRole.SuperAdmin)
                return true;
            return false;
        }
        public static bool BranchAdmin()
        {
            if (Convert.ToInt32(HttpContext.Current.Session["type"].ToString()) == (int)UserRole.BranchAdmin)
                return true;
            return false;
        }
        public static bool Employee()
        {
            if (Convert.ToInt32(HttpContext.Current.Session["type"].ToString()) == (int)UserRole.Employee)
                return true;
            return false;
        }

        public static bool TeamLeader()
        {
            if (Convert.ToInt32(HttpContext.Current.Session["type"].ToString()) == (int)UserRole.TeamLeader)
                return true;
            return false;
        }

        public static bool Supervisor()
        {
            if (Convert.ToInt32(HttpContext.Current.Session["type"].ToString()) == (int)UserRole.Supervisor)
                return true;
            return false;
        }

        public static bool ProjectManager()
        {
            if (Convert.ToInt32(HttpContext.Current.Session["type"].ToString()) == (int)UserRole.ProjectManager)
                return true;
            return false;
        }
    }
}