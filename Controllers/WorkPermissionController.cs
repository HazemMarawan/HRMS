using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HRMS.Models;
using HRMS.ViewModels;
using HRMS.Auth;
using HRMS.Enums;
using HRMS.Helpers;

namespace HRMS.Controllers
{
    [CustomAuthenticationFilter]
    public class WorkPermissionController : BaseController
    {
        HRMSDBContext db = new HRMSDBContext();

        // GET: WorkPermission
        public ActionResult Index()
        {
            User currentUser = Session["user"] as User;
            if (!(isA.Employee() || isA.TeamLeader() || isA.Supervisor() || isA.BranchAdmin() || isA.ProjectManager()))
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
                                      join team_leader_approve in db.Users on perReq.approved_by_team_leader equals team_leader_approve.id into tla
                                      from team_leader_approved in tla.DefaultIfEmpty()

                                      join supervisor_approve in db.Users on perReq.approved_by_supervisor equals supervisor_approve.id into tecm
                                      from supervisor_approved in tecm.DefaultIfEmpty()

                                      join branch_admin_approve in db.Users on perReq.approved_by_branch_admin equals branch_admin_approve.id into baa
                                      from branch_admin_approved in baa.DefaultIfEmpty()

                                      join super_admin_approve in db.Users on perReq.approved_by_super_admin equals super_admin_approve.id into sua
                                      from super_admin_approved in sua.DefaultIfEmpty()

                                      join rejected in db.Users on perReq.rejected_by equals rejected.id into re
                                      from rejected_by in re.DefaultIfEmpty()

                                      select new WorkPermissionRequestViewModel
                                      {
                                          id = perReq.id,
                                          user_id = perReq.user_id,
                                          month = perReq.month,
                                          year = perReq.year,
                                          date = perReq.date,
                                          minutes = perReq.minutes,
                                          reason = perReq.reason,
                                          active = perReq.active,
                                          status = perReq.status,
                                          approved_by_super_admin = perReq.approved_by_super_admin,
                                          approved_by_super_admin_at = perReq.approved_by_super_admin_at,
                                          approved_by_branch_admin = perReq.approved_by_branch_admin,
                                          approved_by_branch_admin_at = perReq.approved_by_branch_admin_at,
                                          approved_by_team_leader = perReq.approved_by_team_leader,
                                          approved_by_team_leader_at = perReq.approved_by_team_leader_at,
                                          approved_by_supervisor = perReq.approved_by_supervisor,
                                          approved_by_supervisor_at = perReq.approved_by_supervisor_at,
                                          created_at = perReq.created_at,
                                          full_name = user.full_name,
                                          type = user.type,
                                          team_leader_id = user.team_leader_id,
                                          permission_count = db.WorkPermissionRequests.Where(wo => wo.year == perReq.year && wo.month == perReq.month && wo.status == (int)ApprovementStatus.ApprovedBySuperAdmin && wo.user_id == perReq.user_id).Count(),
                                          team_leader_name = team_leader_approved.full_name,
                                          supervisor_name = supervisor_approved.full_name,
                                          branch_admin_name = branch_admin_approved.full_name,
                                          super_admin_name = super_admin_approved.full_name,
                                          rejected_by_name = rejected_by.full_name,
                                          //permission_count = db.WorkPermissionMonthYears.Where(wo => wo.year == perReq.year && wo.month == perReq.month).Select(s => s.permission_count).FirstOrDefault()

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
                WorkPermissionRequest.minutes = workPermissionRequestViewModel.minutes;
                if(currentUser.type == (int?)UserRole.BranchAdmin)
                {
                    WorkPermissionRequest.status = (int?)ApprovementStatus.ApprovedByBranchAdmin;
                }
                else if (currentUser.type == (int?)UserRole.TeamLeader)
                {
                    WorkPermissionRequest.status = (int?)ApprovementStatus.ApprovedByTeamLeader;
                }
                else if (currentUser.type == (int?)UserRole.Supervisor)
                {
                    WorkPermissionRequest.status = (int?)ApprovementStatus.ApprovedBySupervisor;
                }
                else if(currentUser.type == (int?)UserRole.Employee)
                {
                    WorkPermissionRequest.status = (int?)ApprovementStatus.PendingApprove;
                } else
                {
                    WorkPermissionRequest.status = (int?)ApprovementStatus.Rejected;
                }
                WorkPermissionRequest.date = workPermissionRequestViewModel.date;
                WorkPermissionRequest.year = ((DateTime)(workPermissionRequestViewModel.date)).Year;
                WorkPermissionRequest.month = ((DateTime)(workPermissionRequestViewModel.date)).Month;
                WorkPermissionRequest.active = (int?)RowStatus.ACTIVE;
                WorkPermissionRequest.created_at = DateTime.Now;
                WorkPermissionRequest.created_by = Session["id"].ToString().ToInt();

                if (db.WorkPermissionRequests.Where(w => w.year == WorkPermissionRequest.year && w.month == WorkPermissionRequest.month && w.user_id == currentUser.id).Count() >= 2)
                {
                        return Json(new { message = "faild" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    db.WorkPermissionRequests.Add(WorkPermissionRequest);
                    db.SaveChanges();
                }
              
            }
            else
            {

                WorkPermissionRequest WorkPermissionRequest = db.WorkPermissionRequests.Find(workPermissionRequestViewModel.id);

                WorkPermissionRequest.user_id = currentUser.id;
                WorkPermissionRequest.reason = workPermissionRequestViewModel.reason;
                WorkPermissionRequest.minutes = workPermissionRequestViewModel.minutes;
                //WorkPermissionRequest.status = (int?)ApprovementStatus.PendingApprove;
                WorkPermissionRequest.date = workPermissionRequestViewModel.date;
                WorkPermissionRequest.year = ((DateTime)(workPermissionRequestViewModel.date)).Year;
                WorkPermissionRequest.month = ((DateTime)(workPermissionRequestViewModel.date)).Month;
                //WorkPermissionRequest.active = (int?)RowStatus.ACTIVE;
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