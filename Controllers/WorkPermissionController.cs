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
                                          date = perReq.date,
                                          minutes = perReq.minutes,
                                          active = perReq.active,
                                          status = perReq.status,
                                          approved_by_super_admin = perReq.approved_by_super_admin,
                                          approved_by_super_admin_at = perReq.approved_by_super_admin_at,
                                          approved_by_branch_admin = perReq.approved_by_branch_admin,
                                          approved_by_branch_admin_at = perReq.approved_by_branch_admin_at,
                                          approved_by_team_leader = perReq.approved_by_team_leader,
                                          approved_by_team_leader_at = perReq.approved_by_team_leader_at,
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
        [HttpPost]
        public JsonResult saveWorkPermission(WorkPermissionRequestViewModel workPermissionRequestViewModel)
        {
            User currentUser = Session["user"] as User;

            if (workPermissionRequestViewModel.id == 0)
            {
                WorkPermissionRequest WorkPermissionRequest = AutoMapper.Mapper.Map<WorkPermissionRequestViewModel, WorkPermissionRequest>(workPermissionRequestViewModel);

                WorkPermissionRequest.user_id = currentUser.id;
                WorkPermissionRequest.status = (int?)ApprovementStatus.PendingApprove;
                WorkPermissionRequest.date = workPermissionRequestViewModel.date;
                WorkPermissionRequest.year = ((DateTime)(workPermissionRequestViewModel.date)).Year;
                WorkPermissionRequest.month = ((DateTime)(workPermissionRequestViewModel.date)).Month;
                WorkPermissionRequest.active = workPermissionRequestViewModel.active;
                WorkPermissionRequest.updated_by = Session["id"].ToString().ToInt();
                WorkPermissionRequest.created_at = DateTime.Now;
                WorkPermissionRequest.created_by = Session["id"].ToString().ToInt();
                WorkPermissionRequest.active = (int?)RowStatus.ACTIVE;

                db.WorkPermissionRequests.Add(WorkPermissionRequest);
                db.SaveChanges();
            }
            else
            {

                WorkPermissionRequest WorkPermissionRequest = db.WorkPermissionRequests.Find(workPermissionRequestViewModel.id);

                WorkPermissionRequest.user_id = currentUser.id;
                WorkPermissionRequest.reason = workPermissionRequestViewModel.reason;
                WorkPermissionRequest.status = (int?)ApprovementStatus.PendingApprove;
                WorkPermissionRequest.date = workPermissionRequestViewModel.date;
                WorkPermissionRequest.year = ((DateTime)(workPermissionRequestViewModel.date)).Year;
                WorkPermissionRequest.month = ((DateTime)(workPermissionRequestViewModel.date)).Month;
                WorkPermissionRequest.active = (int?)RowStatus.ACTIVE;
                WorkPermissionRequest.updated_by = Session["id"].ToString().ToInt();
                WorkPermissionRequest.updated_at = DateTime.Now;

                db.SaveChanges();
            }

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult deleteWorkPermission(int id)
        {
            WorkPermissionRequest deleteWorkPermissionRequest = db.WorkPermissionRequests.Find(id);
            deleteWorkPermissionRequest.active = (int)RowStatus.INACTIVE;
            deleteWorkPermissionRequest.deleted_at = DateTime.Now;
            deleteWorkPermissionRequest.deleted_by = Session["id"].ToString().ToInt();

            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }
    }
}