using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HRMS.Models;
using HRMS.ViewModels;
using HRMS.Auth;
using System.Configuration;
using System.Data.SqlClient;
using HRMS.Helpers;
using HRMS.Enum;

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
        [HttpGet]
        public JsonResult productivityByDate()
        {
            User currentUser = Session["user"] as User;

            DateTime dateTime = DateTime.Now;
            var currentYear = dateTime.Year;
            var currentMonth = dateTime.Month;
            string cs = ConfigurationManager.ConnectionStrings["HRMSDBContextADO"].ConnectionString;

            SqlConnection sql = new SqlConnection(cs);
            sql.Open();
            string query = String.Empty;
            if (isA.Employee() || isA.TeamLeader() || isA.TeamLeader())
            {
                query = @"select concat(month(working_date),'-',year(working_date)) as date_of_work,sum(no_of_numbers) as number_of_hours from UserProjects
                                            where user_id = " + Session["id"].ToString() + @"
                                            group by year(working_date), month(working_date)";
            }
            if (isA.BranchAdmin())
            {
                query = @"select concat(month(working_date),'-',year(working_date)) as date_of_work,sum(no_of_numbers) as number_of_hours from UserProjects
                                                inner join users on UserProjects.user_id = users.id 
                                                where users.branch_id = " + currentUser.branch_id + @"
                                                group by year(working_date),month(working_date)";
            }
            if (isA.SuperAdmin())
            {
                query = @"select concat(month(working_date),'-',year(working_date)) as date_of_work,sum(no_of_numbers) as number_of_hours from UserProjects
                                                inner join users on UserProjects.user_id = users.id 
                                                group by year(working_date),month(working_date)";

            }
            SqlCommand comm = new SqlCommand(query, sql);
            SqlDataReader reader = comm.ExecuteReader();
            List<int> no_of_hours = new List<int>();
            List<string> xAxis = new List<string>();

            while (reader.Read())
            {
                no_of_hours.Add(reader["number_of_hours"].ToString().ToInt());
                xAxis.Add(reader["date_of_work"].ToString());
            }

            reader.Close();

            sql.Close();

            return Json(new { no_of_hours = no_of_hours, xAxis = xAxis, message = "done" }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult productivityByProject()
        {
            User currentUser = Session["user"] as User;
            DateTime dateTime = DateTime.Now;
            var currentYear = dateTime.Year;
            var currentMonth = dateTime.Month;
            string cs = ConfigurationManager.ConnectionStrings["HRMSDBContextADO"].ConnectionString;

            SqlConnection sql = new SqlConnection(cs);
            sql.Open();

            string query = String.Empty;
            if(isA.Employee() || isA.TeamLeader() || isA.TeamLeader())
            {
                query = @"select projects.name ,sum(no_of_numbers) as number_of_hours 
                                            from UserProjects
                                            inner join projects on UserProjects.project_id = projects.id
                                            where user_id = " + Session["id"].ToString() + @"
                                            group by UserProjects.project_id,projects.name";
            }
            if(isA.BranchAdmin())
            {
                query = @"select projects.name ,sum(UserProjects.no_of_numbers) as number_of_hours 
                                            from UserProjects
                                            inner join users on UserProjects.user_id = users.id 
                                            inner join projects on UserProjects.project_id = projects.id
                                            where users.branch_id = " + currentUser.branch_id + @"
                                            group by UserProjects.project_id,projects.name";
            }
            if (isA.SuperAdmin())
            {
                query = @"select projects.name ,sum(UserProjects.no_of_numbers) as number_of_hours 
                                            from UserProjects
                                            inner join users on UserProjects.user_id = users.id 
                                            inner join projects on UserProjects.project_id = projects.id
                                            group by UserProjects.project_id,projects.name";

            }

            SqlCommand comm = new SqlCommand(query, sql);
            SqlDataReader reader = comm.ExecuteReader();
            List<int> no_of_hours = new List<int>();
            List<string> xAxis = new List<string>();

            while (reader.Read())
            {
                no_of_hours.Add(reader["number_of_hours"].ToString().ToInt());
                xAxis.Add(reader["name"].ToString());
            }

            reader.Close();

            sql.Close();

            return Json(new { no_of_hours = no_of_hours, xAxis = xAxis, message = "done" }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult VacationsNotifications()
        {
            User currentUser = Session["user"] as User;
            var vacationsRequestsData = (from vacationRequest in db.VacationRequests
                                         join vacationType in db.VacationTypes on vacationRequest.vacation_type_id equals vacationType.id
                                         join user in db.Users on vacationRequest.user_id equals user.id
                                         select new VacationRequestViewModel
                                         {
                                             id = vacationRequest.id,
                                             user_id = vacationRequest.user_id,
                                             branch_id = user.branch_id,
                                             team_leader_id = user.team_leader_id,
                                             user_type = user.type,
                                             full_name = user.full_name,
                                             year = vacationRequest.year,
                                             vacation_type_id = vacationRequest.vacation_type_id,
                                             vacation_name = vacationType.name,
                                             vacation_from = vacationRequest.vacation_from,
                                             vacation_to = vacationRequest.vacation_to,
                                             days = vacationRequest.days,
                                             status = vacationRequest.status,
                                             active = vacationRequest.active,
                                             created_at = vacationRequest.created_at,
                                         }).Where(n => n.active == (int)RowStatus.ACTIVE);


            if (isA.SuperAdmin())
            {
                vacationsRequestsData = vacationsRequestsData.Where(p => p.user_id != currentUser.id && p.status == (int)ApprovementStatus.ApprovedByBranchAdmin && (p.user_type == (int)UserRole.Employee || p.user_type == (int)UserRole.TeamLeader || p.user_type == (int)UserRole.TechnicalManager || p.user_type == (int)UserRole.BranchAdmin));
            }

            if (isA.BranchAdmin())
            {
                vacationsRequestsData = vacationsRequestsData.Where(p => p.branch_id == currentUser.branch_id && p.status == (int)ApprovementStatus.ApprovedByTechnicalManager && p.user_id != currentUser.id && (p.user_type == (int)UserRole.Employee || p.user_type == (int)UserRole.TeamLeader || p.user_type == (int)UserRole.TechnicalManager));

            }

            if (isA.TeamLeader())
            {
                vacationsRequestsData = vacationsRequestsData.Where(p => p.team_leader_id == currentUser.id && p.status == (int)ApprovementStatus.PendingApprove && p.user_id != currentUser.id && p.user_type == (int)UserRole.Employee);

            }

            if (isA.TechnicalManager())
            {
                vacationsRequestsData = vacationsRequestsData.Where(p => p.branch_id == currentUser.branch_id && p.status == (int)ApprovementStatus.ApprovedByTeamLeader && p.user_id != currentUser.id && (p.user_type == (int)UserRole.Employee || p.user_type == (int)UserRole.TeamLeader));

            }

            List<VacationRequestViewModel> vacationsRequests = vacationsRequestsData.Take(5).ToList();
            return Json(new { vacationsRequests = vacationsRequests }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult PermissionsNotifications()
        {
            User currentUser = Session["user"] as User;
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
                                      reason = perReq.reason,
                                      active = perReq.active,
                                      status = perReq.status,
                                      created_at = perReq.created_at,
                                      full_name = user.full_name,
                                      branch_id = user.branch_id,
                                      type = user.type,
                                      team_leader_id = user.team_leader_id,
                                  }).Where(n => n.active == (int)RowStatus.ACTIVE);

            if (isA.TeamLeader())
            {
                permissionData = permissionData.Where(t => t.team_leader_id == currentUser.id && t.type == (int)UserRole.Employee && t.user_id != currentUser.id && t.status == (int)ApprovementStatus.PendingApprove);
            }
            else if (isA.TechnicalManager())
            {
                permissionData = permissionData.Where(t => t.branch_id == currentUser.branch_id && (t.type == (int)UserRole.Employee || t.type == (int)UserRole.TeamLeader) && t.user_id != currentUser.id && t.status == (int)ApprovementStatus.ApprovedByTeamLeader);
            }
            else if (isA.BranchAdmin())
            {
                permissionData = permissionData.Where(t => t.branch_id == currentUser.branch_id && (t.type == (int)UserRole.Employee || t.type == (int)UserRole.TeamLeader || t.type == (int)UserRole.TechnicalManager) && t.user_id != currentUser.id && t.status == (int)ApprovementStatus.ApprovedByTechnicalManager);
            }
            else if (isA.SuperAdmin())
            {
                permissionData = permissionData.Where(t => t.status == (int)ApprovementStatus.ApprovedByBranchAdmin);
            }
            else
            {
                permissionData = permissionData.Where(t => t.id == -1);
            }


            List<WorkPermissionRequestViewModel> permissionsRequests = permissionData.Take(5).ToList();
            return Json(new { permissionsRequests = permissionsRequests }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult MissionsNotifications()
        {
            User currentUser = Session["user"] as User;
            var missionData = (from mission in db.MissionRequests
                               join user in db.Users on mission.user_id equals user.id

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
                                   created_at = mission.created_at,
                                   full_name = user.full_name,
                                   branch_id = user.branch_id,
                                   type = user.type,
                                   team_leader_id = user.team_leader_id,
                               }).Where(n => n.active == (int)RowStatus.ACTIVE);

            //Search    
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
            else if (isA.SuperAdmin())
            {
                missionData = missionData.Where(t => t.status == (int)ApprovementStatus.ApprovedByBranchAdmin);
            }
            else
            {
                missionData = missionData.Where(t => t.id == -1);
            }


            List<MissionRequestViewModel> missionsRequests = missionData.Take(5).ToList();
            return Json(new { missionsRequests = missionsRequests }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getDashboardData()
        {
            User currentUser = Session["user"] as User;
            DashboardViewModel dashboardViewModel = new DashboardViewModel();

            if (isA.Employee())
            {
                dashboardViewModel.Manager = db.Users.Where(u => u.id == currentUser.team_leader_id).Select(u => new UserViewModel { id = u.id, full_name = u.full_name, imagePath = u.image }).FirstOrDefault();
            }

            if (isA.TeamLeader())
            {
                dashboardViewModel.Manager = db.Users.Where(u => u.branch_id == currentUser.branch_id && u.type == (int)UserRole.TechnicalManager).Select(u => new UserViewModel { id = u.id, full_name = u.full_name, imagePath = u.image }).FirstOrDefault();
            }

            if (isA.TechnicalManager())
            {
                dashboardViewModel.Manager = db.Users.Where(u => u.branch_id == currentUser.branch_id && u.type == (int)UserRole.BranchAdmin).Select(u => new UserViewModel { id = u.id, full_name = u.full_name, imagePath = u.image }).FirstOrDefault();
            }

            if (HRMS.Auth.isA.Employee() || HRMS.Auth.isA.TeamLeader() || HRMS.Auth.isA.TechnicalManager() || HRMS.Auth.isA.BranchAdmin())
            {
                dashboardViewModel.Vacations = db.VacationRequests.Where(vr => vr.year == DateTime.Now.Year && vr.user_id == currentUser.id && vr.status == (int)ApprovementStatus.ApprovedBySuperAdmin).Select(vr => vr.days).Sum();
                dashboardViewModel.Vacations = dashboardViewModel.Vacations != null ? dashboardViewModel.Vacations : 0;
                dashboardViewModel.VacationsBalance = currentUser.vacations_balance != null ? currentUser.vacations_balance : 21;

                dashboardViewModel.Permissions = db.WorkPermissionRequests.Where(vr => vr.year == DateTime.Now.Year && vr.user_id == currentUser.id && vr.status == (int)ApprovementStatus.ApprovedBySuperAdmin).Count();
                dashboardViewModel.Permissions = dashboardViewModel.Permissions != null ? dashboardViewModel.Permissions : 0;

                dashboardViewModel.Missions = db.MissionRequests.Where(vr => vr.year == DateTime.Now.Year && vr.user_id == currentUser.id && vr.status == (int)ApprovementStatus.ApprovedBySuperAdmin).Count();
                dashboardViewModel.Missions = dashboardViewModel.Missions != null ? dashboardViewModel.Missions : 0;

            }

            return Json(new { dashboardData = dashboardViewModel }, JsonRequestBehavior.AllowGet);
        }

    }
}