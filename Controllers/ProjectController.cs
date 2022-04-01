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
    public class ProjectController : Controller
    {
        HRMSDBContext db = new HRMSDBContext();
        // GET: Project
        public ActionResult Index(int? branch_id)
        {
            User user = Session["user"] as User;
            if (!(isA.SuperAdmin() || (isA.BranchAdmin() && (user.branch_id == branch_id || branch_id == null))))
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
                var productitvityData = (from project in db.Projects
                                       join projectType in db.ProjectTypes on project.project_type_id equals projectType.id
                                       select new ProjectViewModel
                                       {
                                           id = project.id,
                                           name = project.name,
                                           project_type_id = project.project_type_id,
                                           project_type_name = projectType.name,
                                           start_date = project.start_date,
                                           end_date = project.end_date,
                                           created_at = project.created_at,
                                           active = project.active,
                                           areas = db.Areas.Where(a=>a.project_id == project.id && a.active == (int)RowStatus.ACTIVE).Select(a=>new AreaViewModel
                                           {
                                               id = a.id,
                                               name = a.name,
                                               active = a.active
                                           }).ToList()
                                       }).Where(n => n.active == (int)RowStatus.ACTIVE);

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    productitvityData = productitvityData.Where(m => m.name.ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()));
                }
                if(isA.SuperAdmin())
                {
                    if (branch_id != null)
                    {
                        List<int?> branchProjects = db.BranchProjects.Where(b => b.branch_id == branch_id).Select(b => b.project_id).ToList();
                        productitvityData = productitvityData.Where(p => branchProjects.Contains(p.id));
                    }
                }
                else if (isA.BranchAdmin())
                {
                    List<int?> branchProjects = db.BranchProjects.Where(b => b.branch_id == user.branch_id).Select(b => b.project_id).ToList();
                    productitvityData = productitvityData.Where(p => branchProjects.Contains(p.id));
                }
                

                //total number of rows count     
                var displayResult = productitvityData.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = productitvityData.Count();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = displayResult

                }, JsonRequestBehavior.AllowGet);

            }
            ViewBag.ProjectTypes = db.ProjectTypes.Where(p => p.active == (int)RowStatus.ACTIVE).Select(p => new { p.id, p.name }).ToList();
            if (isA.BranchAdmin())
            {
                branch_id = user.branch_id;
            }
            ViewBag.branchId = branch_id;
            if(branch_id != null)
                ViewBag.branchName = db.Branches.Where(b => b.id == branch_id).FirstOrDefault().name;
            return View();
        }
        [HttpPost]
        public JsonResult saveProject(ProjectViewModel projectViewModel)
        {

            if (projectViewModel.id == 0)
            {
                Project project = AutoMapper.Mapper.Map<ProjectViewModel, Project>(projectViewModel);

                project.created_at = DateTime.Now;
                project.created_by = Session["id"].ToString().ToInt();

                db.Projects.Add(project);
                db.SaveChanges();
            }
            else
            {

                Project oldProject = db.Projects.Find(projectViewModel.id);

                oldProject.name = projectViewModel.name;
                oldProject.project_type_id = projectViewModel.project_type_id;
                oldProject.start_date = projectViewModel.start_date;
                oldProject.end_date = projectViewModel.end_date;
                oldProject.active = projectViewModel.active;
                oldProject.updated_by = Session["id"].ToString().ToInt();
                oldProject.updated_at = DateTime.Now;

                db.SaveChanges();
            }

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult deleteProject(int id)
        {
            Project deleteProject = db.Projects.Find(id);
            deleteProject.active = (int)RowStatus.INACTIVE;
            deleteProject.deleted_at = DateTime.Now;
            deleteProject.deleted_by = Session["id"].ToString().ToInt();

            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }
        
        [HttpGet]
        public JsonResult getAreaByProjectId(int id)
        {
            List<AreaViewModel> areas = db.Areas.Where(a => a.project_id == id).Select(a => new AreaViewModel
            {
                id = a.id,
                name = a.name,
                active = a.active
            }).Where(a=>a.active == (int)RowStatus.ACTIVE).ToList();
            return Json(new { areas = areas }, JsonRequestBehavior.AllowGet);
        }
    }
}