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
    public class PermissionListController : Controller
    {
        HRMSDBContext db = new HRMSDBContext();

        // GET: PermissionList
        public ActionResult Index(int? branch_id)
        {
            User currentUser = Session["user"] as User;
            if (!(isA.SuperAdmin() || isA.TeamLeader() || (isA.BranchAdmin() && (currentUser.branch_id == branch_id || branch_id==null))))
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
                                          rejected_by_at = perReq.rejected_by_at,
                                          created_at = perReq.created_at,
                                          full_name = user.full_name,
                                          branch_id = user.branch_id,
                                          type = user.type,
                                          team_leader_id = user.team_leader_id,
                                          permission_count = db.WorkPermissionRequests.Where(wo => wo.year == perReq.year && wo.month == perReq.month && wo.status == (int)ApprovementStatus.ApprovedBySuperAdmin && wo.user_id == perReq.user_id).Count(),
                                          team_leader_name = team_leader_approved.full_name,
                                          branch_admin_name = branch_admin_approved.full_name,
                                          super_admin_name = super_admin_approved.full_name,
                                          rejected_by_name = rejected_by.full_name,
                                          
                                      }).Where(n => n.active == (int)RowStatus.ACTIVE);

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    permissionData = permissionData.Where(m => m.full_name.ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()));
                }
                if (isA.TeamLeader())
                {
                    permissionData = permissionData.Where(t => t.team_leader_id == currentUser.id && t.type == (int)UserRole.Employee && t.user_id != currentUser.id && t.status == (int)ApprovementStatus.PendingApprove);
                }
                else if (isA.BranchAdmin())
                {
                    permissionData = permissionData.Where(t => t.branch_id == currentUser.branch_id && (t.type == (int)UserRole.Employee || t.type == (int)UserRole.TeamLeader) && t.user_id != currentUser.id && t.status == (int)ApprovementStatus.ApprovedByTeamLeader);
                } 
                else if(isA.SuperAdmin())
                {
                    permissionData = permissionData.Where(t => t.status == (int)ApprovementStatus.ApprovedByBranchAdmin);
                    if(branch_id != null)
                    {
                        permissionData = permissionData.Where(t => t.branch_id == branch_id);
                    }
                }
                else
                {
                    permissionData = permissionData.Where(t => t.id == -1);
                }

                int? totalPermissions = 0;
                int? approvedPermissions = 0;
                int? unCompletedPermissions = 0;
                int? rejectedPermissions = 0;

                totalPermissions = permissionData.ToList().Count();
                approvedPermissions = permissionData.Select(c => c.status == (int?)ApprovementStatus.ApprovedBySuperAdmin).ToList().Count();
                rejectedPermissions = permissionData.Select(c => c.status == (int?)ApprovementStatus.Rejected).ToList().Count();
                unCompletedPermissions = totalPermissions - approvedPermissions - rejectedPermissions;

                //total number of rows count     
                var displayResult = permissionData.OrderBy(u => u.status).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = permissionData.Count();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = displayResult,
                    totalPermissions = totalPermissions,
                    approvedPermissions = approvedPermissions,
                    rejectedPermissions = rejectedPermissions,
                    unCompletedPermissions = unCompletedPermissions
                }, JsonRequestBehavior.AllowGet);

            }
            ViewBag.years = db.WorkPermissionRequests.Select(y => new { id = y.id, year = y.year }).GroupBy(w => w.year).ToList();
            ViewBag.months = db.WorkPermissionRequests.Select(y => new { id = y.id, month = y.month }).GroupBy(w => w.month).ToList();
            if (isA.BranchAdmin() || isA.TeamLeader())
            {
                branch_id = currentUser.branch_id;
            }
            ViewBag.branchId = branch_id;
            if (branch_id != null)
            {
                ViewBag.branchName = db.Branches.Where(b => b.id == branch_id).FirstOrDefault().name;
            }
            else
            {
                ViewBag.branchName = "Company";
            }
            return View();
        }

        public JsonResult actionToPermission(int id, int status)
        {
            User currentUser = Session["user"] as User;

            WorkPermissionRequest workPermissionRequest = db.WorkPermissionRequests.Find(id);
            if(workPermissionRequest != null)
            {
                //accepted
                if (status == 1)
                {
                    workPermissionRequest.status += 1;
                    if (isA.TeamLeader())
                    {
                        workPermissionRequest.approved_by_team_leader = currentUser.id;
                        workPermissionRequest.approved_by_team_leader_at = DateTime.Now;
                    }
                    else if (isA.BranchAdmin())
                    {
                        workPermissionRequest.approved_by_branch_admin = currentUser.id;
                        workPermissionRequest.approved_by_branch_admin_at = DateTime.Now;
                    }
                    else if(isA.SuperAdmin())
                    {
                        workPermissionRequest.approved_by_super_admin = currentUser.id;
                        workPermissionRequest.approved_by_super_admin_at = DateTime.Now;
                    }
                    db.SaveChanges();

                    WorkPermissionMonthYear workPermissionMonthYear = db.WorkPermissionMonthYears.Where(w => w.year == workPermissionRequest.year && w.month == workPermissionRequest.month).FirstOrDefault();
                    if (workPermissionMonthYear == null)
                    {
                        workPermissionMonthYear = new WorkPermissionMonthYear();
                        workPermissionMonthYear.month = workPermissionRequest.month;
                        workPermissionMonthYear.year = workPermissionRequest.year;
                        workPermissionMonthYear.user_id = workPermissionRequest.user_id;
                        workPermissionMonthYear.permission_count = 1;
                        workPermissionMonthYear.active = (int?)RowStatus.ACTIVE;
                        workPermissionMonthYear.created_at = DateTime.Now;
                        workPermissionMonthYear.created_by = Session["id"].ToString().ToInt();
                        db.WorkPermissionMonthYears.Add(workPermissionMonthYear);

                    }
                    else
                    {
                        workPermissionMonthYear.permission_count += 1;
                        workPermissionMonthYear.active = (int?)RowStatus.ACTIVE;
                        workPermissionMonthYear.updated_at = DateTime.Now;
                        workPermissionMonthYear.updated_by = Session["id"].ToString().ToInt();
                    }

                    db.SaveChanges();
                }
                else
                {
                    workPermissionRequest.status = (int?)ApprovementStatus.Rejected;
                    workPermissionRequest.rejected_by = currentUser.id;
                    workPermissionRequest.rejected_by_at = DateTime.Now;
                    db.SaveChanges();
                }
            }
            
            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }
    }
}