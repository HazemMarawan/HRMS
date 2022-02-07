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