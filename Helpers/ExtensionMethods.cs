using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRMS.Helpers
{
    public static class ExtensionMethods
    {
        public static int ToInt(this string str)
        {
            return Convert.ToInt32(str);
        }
    }
}