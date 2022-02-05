using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HRMS.Models;
using HRMS.ViewModels;
using HRMS.Auth;
using HRMS.Enum;
using HRMS.Helpers;
using HRMS.ViewModel;

namespace HRMS.Controllers
{
    [CustomAuthenticationFilter]
    public class WorkPermissionController : Controller
    {
        HRMSDBContext db = new HRMSDBContext();

        // GET: WorkPermission
        public ActionResult Index()
        {
            User currentUser = Session["user"] as User;
            if (!(isA.Employee() || isA.TeamLeader() || isA.BranchAdmin()))
                return RedirectToAction("Index", "Dashboard");

            if (Request.IsAjaxRequest())
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                // Getting all data
                var permissionData = (from perReq in db.WorkPermissionRequests
                                      join user in db.Users on perReq.user_id equals user.id
                                      select new WorkPermissionRequestViewModel
                                      {
                                          id = perReq.id,
                                          user_id = perReq.user_id,
                                          month = perReq.month,
                                          year = perReq.year,
                                          day = perReq.day,
                                          minutes = perReq.minutes,
                                          active = perReq.active,
                                          status = perReq.status,
                                          created_at = perReq.created_at,
                                          full_name = user.full_name,
                                          permission_count = db.WorkPermissionMonthYears.Where(wo => wo.year == perReq.year && wo.month == perReq.month).Select(s => s.permission_count).FirstOrDefault()
                                  
                                      }).Where(n => n.active == (int)RowStatus.ACTIVE && n.user_id == currentUser.id);

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    permissionData = permissionData.Where(m => m.full_name.ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()));
                }
               
                //total number of rows count     
                var displayResult = permissionData.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = permissionData.Count();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = displayResult

                }, JsonRequestBehavior.AllowGet);

            }

            return View();
        }
    }
}