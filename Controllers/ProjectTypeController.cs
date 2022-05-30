using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HRMS.Models;
using HRMS.ViewModels;
using HRMS.Auth;
using HRMS.Helpers;
using HRMS.Enums;

namespace HRMS.Controllers
{
    [CustomAuthenticationFilter]
    public class ProjectTypeController : BaseController
    {
        HRMSDBContext db = new HRMSDBContext();
        // GET: ProjectType
        public ActionResult Index()
        {
            if (!isA.SuperAdmin())
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
                var projectTypesData = (from projectType in db.ProjectTypes
                                       select new NationalityViewModel
                                       {
                                           id = projectType.id,
                                           name = projectType.name,
                                           active = projectType.active,
                                           created_at = projectType.created_at
                                       }).Where(n => n.active == (int)RowStatus.ACTIVE);

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    projectTypesData = projectTypesData.Where(m => m.name.ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()));
                }

                //total number of rows count     
                var displayResult = projectTypesData.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = projectTypesData.Count();

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
        public JsonResult saveProjectType(ProjectTypeViewModel projectTypeViewModel)
        {

            if (projectTypeViewModel.id == 0)
            {
                ProjectType projectType = AutoMapper.Mapper.Map<ProjectTypeViewModel, ProjectType>(projectTypeViewModel);

                projectType.created_at = DateTime.Now.AddHours(-3);
                projectType.created_by = Session["id"].ToString().ToInt();

                db.ProjectTypes.Add(projectType);
                db.SaveChanges();
            }
            else
            {

                ProjectType oldProjectType = db.ProjectTypes.Find(projectTypeViewModel.id);

                oldProjectType.name = projectTypeViewModel.name;
                oldProjectType.active = projectTypeViewModel.active;
                oldProjectType.updated_by = Session["id"].ToString().ToInt();
                oldProjectType.updated_at = DateTime.Now.AddHours(-3);

                db.SaveChanges();
            }

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult deleteProjectType(int id)
        {
            ProjectType deleteProjectType = db.ProjectTypes.Find(id);
            deleteProjectType.active = (int)RowStatus.INACTIVE;
            deleteProjectType.deleted_by = Session["id"].ToString().ToInt();
            deleteProjectType.deleted_at = DateTime.Now.AddHours(-3);
            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }
    }
}