using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HRMS.Models;
using HRMS.ViewModels;
using HRMS.Auth;
using HRMS.Helpers;
using HRMS.Enum;

namespace HRMS.Controllers
{
    [CustomAuthenticationFilter]
    public class ProductivityController : Controller
    {
        HRMSDBContext db = new HRMSDBContext();
        // GET: Productivity
        public ActionResult Index(int? branch_id)
        {
            User currentUser = Session["user"] as User;
            if (!(isA.SuperAdmin() || (isA.BranchAdmin() && branch_id == currentUser.branch_id)))
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
                var nationalityData = (from user in db.Users
                                       join userProject in db.UserProjects on user.id equals userProject.user_id
                                       join project in db.Projects on userProject.project_id equals project.id
                                       join branchProject in db.BranchProjects on project.id equals branchProject.project_id
                                       select new UserProjectViewModel
                                       {
                                           id = userProject.id,
                                           project_name = project.name,
                                           user_name = user.first_name + " " + user.middle_name + " "+user.last_name,
                                           working_date = userProject.working_date,
                                           no_of_numbers = userProject.no_of_numbers,
                                           branch_id = branchProject.branch_id
                                       }).Where(n => n.branch_id == branch_id);

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    nationalityData = nationalityData.Where(m => m.project_name.ToLower().Contains(searchValue.ToLower())
                    || m.id.ToString().ToLower().Contains(searchValue.ToLower())
                    || m.user_name.ToString().ToLower().Contains(searchValue.ToLower())
                    );
                }

                //total number of rows count     
                var displayResult = nationalityData.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = nationalityData.Count();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = displayResult

                }, JsonRequestBehavior.AllowGet);

            }
            ViewBag.branchId = branch_id;
            if (branch_id != null)
                ViewBag.branchName = db.Branches.Where(b => b.id == branch_id).FirstOrDefault().name;
            return View();
        }
        public ActionResult Employee()
        {
            if (!(isA.Employee() || isA.TeamLeader()))
                return RedirectToAction("Index", "Dashboard");

            User currentUser = Session["user"] as User;
            if (Request.IsAjaxRequest())
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                // Getting all data    
                var nationalityData = (
                                       from userProject in db.UserProjects
                                       join project in db.Projects on userProject.project_id equals project.id
                                       select new UserProjectViewModel
                                       {
                                           id = userProject.id,
                                           user_id = userProject.user_id,
                                           project_id = userProject.project_id,
                                           project_name = project.name,
                                           working_date = userProject.working_date,
                                           no_of_numbers = userProject.no_of_numbers,
                                       }).Where(p => p.user_id == currentUser.id) ;

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    nationalityData = nationalityData.Where(m => m.project_name.ToLower().Contains(searchValue.ToLower())
                    || m.id.ToString().ToLower().Contains(searchValue.ToLower())
                    || m.user_name.ToString().ToLower().Contains(searchValue.ToLower())
                    );
                }

                //total number of rows count     
                var displayResult = nationalityData.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = nationalityData.Count();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = displayResult

                }, JsonRequestBehavior.AllowGet);

            }
            List<int?> branchProjects = db.BranchProjects.Where(b => b.branch_id == currentUser.branch_id).Select(b => b.project_id).ToList();
            ViewBag.Projects = db.Projects.Where(p => branchProjects.Contains(p.id));
            return View();
        }

        [HttpPost]
        public JsonResult saveProductivity(UserProjectViewModel userProjectViewModel)
        {
            User currentUser = Session["user"] as User;
            if (userProjectViewModel.id == 0)
            {
                UserProject userProject = AutoMapper.Mapper.Map<UserProjectViewModel, UserProject>(userProjectViewModel);
                userProject.user_id = currentUser.id;
                userProject.created_at = DateTime.Now;
                userProject.created_by = Session["id"].ToString().ToInt();

                db.UserProjects.Add(userProject);
                db.SaveChanges();
            }
            else
            {

                UserProject oldUserProject = db.UserProjects.Find(userProjectViewModel.id);

                oldUserProject.project_id = userProjectViewModel.project_id;
                oldUserProject.working_date = userProjectViewModel.working_date;
                oldUserProject.no_of_numbers = userProjectViewModel.no_of_numbers;

                oldUserProject.updated_by = Session["id"].ToString().ToInt();
                oldUserProject.updated_at = DateTime.Now;

                db.SaveChanges();
            }

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult deleteProductivity(int id)
        {
            UserProject deleteUserProject = db.UserProjects.Find(id);
            db.UserProjects.Remove(deleteUserProject);

            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }
    }
}