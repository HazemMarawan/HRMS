using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HRMS.Models;
using HRMS.ViewModels;
using HRMS.Auth;
namespace HRMS.Controllers
{
    [CustomAuthenticationFilter]
    public class DashboardController : Controller
    {
        HRMSDBContext db = new HRMSDBContext();
        // GET: Dashboard
        public ActionResult Index()
        {
            return View();
        }
    }
}