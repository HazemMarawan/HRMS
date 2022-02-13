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
    public class MissionListController : Controller
    {
        HRMSDBContext db = new HRMSDBContext();

        // GET: PermissionList
        public ActionResult Index(int? branch_id)
        {
            User currentUser = Session["user"] as User;
            if (!(isA.SuperAdmin() || isA.TeamLeader() || isA.TechnicalManager() || (isA.BranchAdmin() && (currentUser.branch_id == branch_id || branch_id==null))))
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
                var missionData = (from mission in db.MissionRequests
                                      join user in db.Users on mission.user_id equals user.id

                                      join team_leader_approve in db.Users on mission.approved_by_team_leader equals team_leader_approve.id into tla
                                      from team_leader_approved in tla.DefaultIfEmpty()

                                      join technical_manager_approve in db.Users on mission.approved_by_technical_manager equals technical_manager_approve.id into tecm
                                      from technical_manager_approved in tecm.DefaultIfEmpty()

                                      join branch_admin_approve in db.Users on mission.approved_by_branch_admin equals branch_admin_approve.id into baa
                                      from branch_admin_approved in baa.DefaultIfEmpty()
                                     
                                      join super_admin_approve in db.Users on mission.approved_by_super_admin equals super_admin_approve.id into sua
                                      from super_admin_approved in sua.DefaultIfEmpty()

                                      join rejected in db.Users on mission.rejected_by equals rejected.id into re
                                      from rejected_by in re.DefaultIfEmpty()

                                      select new MissionRequestViewModel
                                      {
                                          id = mission.id,
                                          user_id = mission.user_id,
                                          month = mission.month,
                                          year = mission.year,
                                          date = mission.date,
                                          cost = mission.cost,
                                          destination = mission.destination,
                                          reason = mission.reason,
                                          active = mission.active,
                                          status = mission.status,
                                          approved_by_super_admin = mission.approved_by_super_admin,
                                          approved_by_super_admin_at = mission.approved_by_super_admin_at,
                                          approved_by_branch_admin = mission.approved_by_branch_admin,
                                          approved_by_branch_admin_at = mission.approved_by_branch_admin_at,
                                          approved_by_technical_manager = mission.approved_by_technical_manager,
                                          approved_by_technical_manager_at = mission.approved_by_technical_manager_at,
                                          approved_by_team_leader = mission.approved_by_team_leader,
                                          approved_by_team_leader_at = mission.approved_by_team_leader_at,
                                          rejected_by_at = mission.rejected_by_at,
                                          created_at = mission.created_at,
                                          full_name = user.full_name,
                                          branch_id = user.branch_id,
                                          type = user.type,
                                          team_leader_id = user.team_leader_id,
                                          permission_count = db.MissionRequests.Where(wo => wo.year == mission.year && wo.month == mission.month && wo.status == (int)ApprovementStatus.ApprovedBySuperAdmin && wo.user_id == mission.user_id).Count(),
                                          team_leader_name = team_leader_approved.full_name,
                                          technical_manager_name = technical_manager_approved.full_name,
                                          branch_admin_name = branch_admin_approved.full_name,
                                          super_admin_name = super_admin_approved.full_name,
                                          rejected_by_name = rejected_by.full_name,
                                          
                                      }).Where(n => n.active == (int)RowStatus.ACTIVE);

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    missionData = missionData.Where(m => m.full_name.ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()));
                }
                if (isA.TeamLeader())
                {
                    missionData = missionData.Where(t => t.team_leader_id == currentUser.id && t.type == (int)UserRole.Employee && t.user_id != currentUser.id && t.status == (int)ApprovementStatus.PendingApprove);
                }
                else if (isA.TechnicalManager())
                {
                    missionData = missionData.Where(t => t.branch_id == currentUser.branch_id && (t.type == (int)UserRole.Employee || t.type == (int)UserRole.TeamLeader) && t.user_id != currentUser.id && t.status == (int)ApprovementStatus.ApprovedByTeamLeader);
                }
                else if (isA.BranchAdmin())
                {
                    missionData = missionData.Where(t => t.branch_id == currentUser.branch_id && (t.type == (int)UserRole.Employee || t.type == (int)UserRole.TeamLeader || t.type == (int)UserRole.TechnicalManager) && t.user_id != currentUser.id && t.status == (int)ApprovementStatus.ApprovedByTechnicalManager);
                } 
                else if(isA.SuperAdmin())
                {
                    missionData = missionData.Where(t => t.status == (int)ApprovementStatus.ApprovedByBranchAdmin);
                    if(branch_id != null)
                    {
                        missionData = missionData.Where(t => t.branch_id == branch_id);
                    }
                }
                else
                {
                    missionData = missionData.Where(t => t.id == -1);
                }

                int? totalPermissions = 0;
                int? approvedPermissions = 0;
                int? unCompletedPermissions = 0;
                int? rejectedPermissions = 0;

                totalPermissions = missionData.ToList().Count();
                approvedPermissions = missionData.Select(c => c.status == (int?)ApprovementStatus.ApprovedBySuperAdmin).ToList().Count();
                rejectedPermissions = missionData.Select(c => c.status == (int?)ApprovementStatus.Rejected).ToList().Count();
                unCompletedPermissions = totalPermissions - approvedPermissions - rejectedPermissions;

                //total number of rows count     
                var displayResult = missionData.OrderBy(u => u.status).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = missionData.Count();

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
            ViewBag.years = db.MissionRequests.Select(y => new { id = y.id, year = y.year }).GroupBy(w => w.year).ToList();
            ViewBag.months = db.MissionRequests.Select(y => new { id = y.id, month = y.month }).GroupBy(w => w.month).ToList();
            if (isA.BranchAdmin() || isA.TeamLeader() || isA.TechnicalManager())
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

        public JsonResult actionToMission(int id, int status)
        {
            User currentUser = Session["user"] as User;

            MissionRequest MissionRequest = db.MissionRequests.Find(id);
            if(MissionRequest != null)
            {
                //accepted
                if (status == 1)
                {
                    
                    if (isA.TeamLeader())
                    {
                        MissionRequest.status = (int)ApprovementStatus.ApprovedByTeamLeader;
                        MissionRequest.approved_by_team_leader = currentUser.id;
                        MissionRequest.approved_by_team_leader_at = DateTime.Now;
                    }
                    else if (isA.TechnicalManager())
                    {
                        MissionRequest.status = (int)ApprovementStatus.ApprovedByTechnicalManager;
                        MissionRequest.approved_by_technical_manager = currentUser.id;
                        MissionRequest.approved_by_technical_manager_at = DateTime.Now;
                    }
                    else if (isA.BranchAdmin())
                    {
                        MissionRequest.status = (int)ApprovementStatus.ApprovedByBranchAdmin;
                        MissionRequest.approved_by_branch_admin = currentUser.id;
                        MissionRequest.approved_by_branch_admin_at = DateTime.Now;
                    }
                    else if(isA.SuperAdmin())
                    {
                        MissionRequest.status = (int)ApprovementStatus.ApprovedBySuperAdmin;
                        MissionRequest.approved_by_super_admin = currentUser.id;
                        MissionRequest.approved_by_super_admin_at = DateTime.Now;
                    }
                    db.SaveChanges();

                    MissionMonthYear missionMonthYear = db.MissionMonthYears.Where(w => w.year == MissionRequest.year && w.month == MissionRequest.month).FirstOrDefault();
                    if (missionMonthYear == null)
                    {
                        missionMonthYear = new MissionMonthYear();
                        missionMonthYear.month = MissionRequest.month;
                        missionMonthYear.year = MissionRequest.year;
                        missionMonthYear.user_id = MissionRequest.user_id;
                        missionMonthYear.cost = MissionRequest.cost;
                        missionMonthYear.mission_count = 1;
                        missionMonthYear.active = (int?)RowStatus.ACTIVE;
                        missionMonthYear.created_at = DateTime.Now;
                        missionMonthYear.created_by = Session["id"].ToString().ToInt();
                        db.MissionMonthYears.Add(missionMonthYear);

                    }
                    else
                    {
                        missionMonthYear.mission_count += 1;
                        missionMonthYear.cost += MissionRequest.cost;
                        missionMonthYear.active = (int?)RowStatus.ACTIVE;
                        missionMonthYear.updated_at = DateTime.Now;
                        missionMonthYear.updated_by = Session["id"].ToString().ToInt();
                    }

                    db.SaveChanges();
                }
                else
                {
                    MissionRequest.status = (int?)ApprovementStatus.Rejected;
                    MissionRequest.rejected_by = currentUser.id;
                    MissionRequest.rejected_by_at = DateTime.Now;
                    db.SaveChanges();
                }
            }
            
            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }
    }
}