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
    public class MissionController : BaseController
    {
        HRMSDBContext db = new HRMSDBContext();

        // GET: Mission
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
                var missionData = (from mission in db.MissionRequests
                                      join user in db.Users on mission.user_id equals user.id
                                      join team_leader_approve in db.Users on mission.approved_by_team_leader equals team_leader_approve.id into tla
                                      from team_leader_approved in tla.DefaultIfEmpty()

                                      join supervisor_approve in db.Users on mission.approved_by_supervisor equals supervisor_approve.id into tecm
                                      from supervisor_approved in tecm.DefaultIfEmpty()

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
                                          approved_by_team_leader = mission.approved_by_team_leader,
                                          approved_by_team_leader_at = mission.approved_by_team_leader_at,
                                          approved_by_supervisor = mission.approved_by_supervisor,
                                          approved_by_supervisor_at = mission.approved_by_supervisor_at,
                                          created_at = mission.created_at,
                                          full_name = user.full_name,
                                          type = user.type,
                                          team_leader_id = user.team_leader_id,
                                          permission_count = db.MissionRequests.Where(wo => wo.year == mission.year && wo.month == mission.month && wo.status == (int)ApprovementStatus.ApprovedBySuperAdmin && wo.user_id == mission.user_id).Count(),
                                          team_leader_name = team_leader_approved.full_name,
                                          supervisor_name = supervisor_approved.full_name,
                                          branch_admin_name = branch_admin_approved.full_name,
                                          super_admin_name = super_admin_approved.full_name,
                                          rejected_by_name = rejected_by.full_name,
                                          //permission_count = db.WorkPermissionMonthYears.Where(wo => wo.year == mission.year && wo.month == mission.month).Select(s => s.permission_count).FirstOrDefault()

                                      }).Where(n => n.active == (int)RowStatus.ACTIVE && n.user_id == currentUser.id);

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    missionData = missionData.Where(m => m.full_name.ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()));
                }
               
                //total number of rows count     
                var displayResult = missionData.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = missionData.Count();

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
        public JsonResult saveMission(MissionRequestViewModel MissionRequestViewModel)
        {
            User currentUser = Session["user"] as User;

            if (MissionRequestViewModel.id == 0)
            {

                MissionRequest MissionRequest = AutoMapper.Mapper.Map<MissionRequestViewModel, MissionRequest>(MissionRequestViewModel);

                MissionRequest.user_id = currentUser.id;
                MissionRequest.destination = MissionRequestViewModel.destination;
                MissionRequest.cost = MissionRequestViewModel.cost;
                MissionRequest.reason = MissionRequestViewModel.reason;

                if(currentUser.type == (int?)UserRole.BranchAdmin)
                {
                    MissionRequest.status = (int?)ApprovementStatus.ApprovedByBranchAdmin;
                }
                else if (currentUser.type == (int?)UserRole.TeamLeader)
                {
                    MissionRequest.status = (int?)ApprovementStatus.ApprovedByTeamLeader;
                }
                else if (currentUser.type == (int?)UserRole.Supervisor)
                {
                    MissionRequest.status = (int?)ApprovementStatus.ApprovedBySupervisor;
                }
                else if(currentUser.type == (int?)UserRole.Employee)
                {
                    MissionRequest.status = (int?)ApprovementStatus.PendingApprove;
                } else
                {
                    MissionRequest.status = (int?)ApprovementStatus.Rejected;
                }
                MissionRequest.date = MissionRequestViewModel.date;
                MissionRequest.year = ((DateTime)(MissionRequestViewModel.date)).Year;
                MissionRequest.month = ((DateTime)(MissionRequestViewModel.date)).Month;
                MissionRequest.active = (int?)RowStatus.ACTIVE;
                MissionRequest.created_at = DateTime.Now;
                MissionRequest.created_by = Session["id"].ToString().ToInt();

                db.MissionRequests.Add(MissionRequest);
                db.SaveChanges();

                //if (db.MissionRequests.Where(w => w.year == MissionRequest.year && w.month == MissionRequest.month && w.user_id == currentUser.id).Count() >= 2)
                //{
                //        return Json(new { message = "faild" }, JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                //    db.MissionRequests.Add(MissionRequest);
                //    db.SaveChanges();
                //}

            }
            else
            {

                MissionRequest MissionRequest = db.MissionRequests.Find(MissionRequestViewModel.id);

                MissionRequest.user_id = currentUser.id;
                MissionRequest.destination = MissionRequestViewModel.destination;
                MissionRequest.cost = MissionRequestViewModel.cost;
                MissionRequest.reason = MissionRequestViewModel.reason;
                //MissionRequest.minutes = MissionRequestViewModel.minutes;
                //MissionRequest.status = (int?)ApprovementStatus.PendingApprove;
                MissionRequest.date = MissionRequestViewModel.date;
                MissionRequest.year = ((DateTime)(MissionRequestViewModel.date)).Year;
                MissionRequest.month = ((DateTime)(MissionRequestViewModel.date)).Month;
                //MissionRequest.active = (int?)RowStatus.ACTIVE;
                MissionRequest.updated_by = Session["id"].ToString().ToInt();
                MissionRequest.updated_at = DateTime.Now;

                db.SaveChanges();
            }

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult deleteMission(int id)
        {
            MissionRequest deleteMissionRequest = db.MissionRequests.Find(id);
            deleteMissionRequest.active = (int)RowStatus.INACTIVE;
            deleteMissionRequest.deleted_at = DateTime.Now;
            deleteMissionRequest.deleted_by = Session["id"].ToString().ToInt();

            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }
    }
}