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

            if(HRMS.Auth.isA.Employee() || HRMS.Auth.isA.TeamLeader() || HRMS.Auth.isA.TechnicalManager() || HRMS.Auth.isA.BranchAdmin())
            {
                dashboardViewModel.Vacations = db.VacationRequests.Where(vr => vr.year == DateTime.Now.Year && vr.user_id == currentUser.id && vr.status != (int)ApprovementStatus.Rejected).Select(vr => vr.days).Sum();
                dashboardViewModel.Vacations = dashboardViewModel.Vacations != null ? dashboardViewModel.Vacations : 0;
                dashboardViewModel.VacationsBalance = currentUser.vacations_balance != null ? currentUser.vacations_balance : 21;

                dashboardViewModel.Permissions = db.WorkPermissionRequests.Where(vr => vr.year == DateTime.Now.Year && vr.user_id == currentUser.id && vr.status != (int)ApprovementStatus.Rejected).Count();
                dashboardViewModel.Permissions = dashboardViewModel.Permissions != null ? dashboardViewModel.Permissions : 0;

                dashboardViewModel.Missions = db.MissionRequests.Where(vr => vr.year == DateTime.Now.Year && vr.user_id == currentUser.id && vr.status != (int)ApprovementStatus.Rejected).Count();
                dashboardViewModel.Missions = dashboardViewModel.Missions != null ? dashboardViewModel.Missions : 0;

            }

            return View(dashboardViewModel);
        }
        [HttpGet]
        public JsonResult productivityByDate()
        {

            DateTime dateTime = DateTime.Now;
            var currentYear = dateTime.Year;
            var currentMonth = dateTime.Month;
            string cs = ConfigurationManager.ConnectionStrings["HRMSDBContextADO"].ConnectionString;

            SqlConnection sql = new SqlConnection(cs);
            sql.Open();

            SqlCommand comm = new SqlCommand(@"select concat(month(working_date),'-',year(working_date)) as date_of_work,sum(no_of_numbers) as number_of_hours from UserProjects
                                            where user_id = " + Session["id"].ToString() + @"
                                            group by year(working_date), month(working_date)", sql);
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

            DateTime dateTime = DateTime.Now;
            var currentYear = dateTime.Year;
            var currentMonth = dateTime.Month;
            string cs = ConfigurationManager.ConnectionStrings["HRMSDBContextADO"].ConnectionString;

            SqlConnection sql = new SqlConnection(cs);
            sql.Open();

            SqlCommand comm = new SqlCommand(@"select projects.name ,sum(no_of_numbers) as number_of_hours 
                                            from UserProjects
                                            inner join projects on UserProjects.project_id = projects.id
                                            where user_id = " + Session["id"].ToString() + @"
                                            group by UserProjects.project_id,projects.name", sql);
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
        [HttpGet]
        public JsonResult branchProductivityByDate()
        {
            if (isA.BranchAdmin())
            {
                User currentUser = Session["user"] as User;
                DateTime dateTime = DateTime.Now;
                var currentYear = dateTime.Year;
                var currentMonth = dateTime.Month;
                string cs = ConfigurationManager.ConnectionStrings["HRMSDBContextADO"].ConnectionString;

                SqlConnection sql = new SqlConnection(cs);
                sql.Open();

                SqlCommand comm = new SqlCommand(@"select concat(month(working_date),'-',year(working_date)) as date_of_work,sum(no_of_numbers) as number_of_hours from UserProjects
                                                inner join users on UserProjects.user_id = users.id 
                                                where users.branch_id = " + currentUser.branch_id + @"
                                                group by year(working_date),month(working_date)", sql);
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
            return Json(new { no_of_hours = new List<int>(), xAxis = new List<string>(), message = "done" }, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public JsonResult branchProductivityProject()
        {
            if (isA.BranchAdmin())
            {
                User currentUser = Session["user"] as User;
                DateTime dateTime = DateTime.Now;
                var currentYear = dateTime.Year;
                var currentMonth = dateTime.Month;
                string cs = ConfigurationManager.ConnectionStrings["HRMSDBContextADO"].ConnectionString;

                SqlConnection sql = new SqlConnection(cs);
                sql.Open();

                SqlCommand comm = new SqlCommand(@"select projects.name ,sum(UserProjects.no_of_numbers) as number_of_hours 
                                            from UserProjects
                                            inner join users on UserProjects.user_id = users.id 
                                            inner join projects on UserProjects.project_id = projects.id
                                            where users.branch_id = " + currentUser.branch_id + @"
                                            group by UserProjects.project_id,projects.name", sql);
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
            return Json(new { no_of_hours = new List<int>(), xAxis = new List<string>(), message = "done" }, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public JsonResult systemProductivityByDate()
        {
            User currentUser = Session["user"] as User;
            DateTime dateTime = DateTime.Now;
            var currentYear = dateTime.Year;
            var currentMonth = dateTime.Month;
            string cs = ConfigurationManager.ConnectionStrings["HRMSDBContextADO"].ConnectionString;

            SqlConnection sql = new SqlConnection(cs);
            sql.Open();

            SqlCommand comm = new SqlCommand(@"select concat(month(working_date),'-',year(working_date)) as date_of_work,sum(no_of_numbers) as number_of_hours from UserProjects
                                                inner join users on UserProjects.user_id = users.id 
                                                group by year(working_date),month(working_date)", sql);
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
        public JsonResult systemProductivityProject()
        {
            User currentUser = Session["user"] as User;
            DateTime dateTime = DateTime.Now;
            var currentYear = dateTime.Year;
            var currentMonth = dateTime.Month;
            string cs = ConfigurationManager.ConnectionStrings["HRMSDBContextADO"].ConnectionString;

            SqlConnection sql = new SqlConnection(cs);
            sql.Open();

            SqlCommand comm = new SqlCommand(@"select projects.name ,sum(UserProjects.no_of_numbers) as number_of_hours 
                                            from UserProjects
                                            inner join users on UserProjects.user_id = users.id 
                                            inner join projects on UserProjects.project_id = projects.id
                                            group by UserProjects.project_id,projects.name", sql);
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
    }
}