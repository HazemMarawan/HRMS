using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRMS.Auth
{
    public class can
    {
        public static bool makeProductivity()
        {
            if (Convert.ToInt32(HttpContext.Current.Session["required_productivity"].ToString()) == 1)
                return true;
            return false;
        }
    }
}