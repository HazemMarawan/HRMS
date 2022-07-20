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
    public class TaskClassificationController : BaseController
    {
        HRMSDBContext db = new HRMSDBContext();
        // GET: ProjectType
        public ActionResult Index(int? branch_id)
        {
            User currentUser = Session["user"] as User;
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
                var assetsData = (from taskClassification in db.TaskClassifications
                                  select new TaskClassificationViewModel
                                  {
                                      id = taskClassification.id,
                                      name = taskClassification.name,
                                      active = taskClassification.active,
                                      created_at = taskClassification.created_at
                                  }).Where(n => n.active == (int)RowStatus.ACTIVE);

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    assetsData = assetsData.Where(m => m.name.ToLower().Contains(searchValue.ToLower())
                    || m.id.ToString().ToLower().Contains(searchValue.ToLower())
                    || m.name.ToString().ToLower().Contains(searchValue.ToLower())
                    );
                }

                //total number of rows count     
                var displayResult = assetsData.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = assetsData.Count();

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
        public JsonResult saveTaskClassification(TaskClassificationViewModel taskClassificationViewModel)
        {

            if (taskClassificationViewModel.id == 0)
            {
                TaskClassification taskClassification = AutoMapper.Mapper.Map<TaskClassificationViewModel, TaskClassification>(taskClassificationViewModel);

                taskClassification.active = (int)RowStatus.ACTIVE;
                taskClassification.created_at = DateTime.Now; ;
                taskClassification.created_by = Session["id"].ToString().ToInt();

                db.TaskClassifications.Add(taskClassification);
                db.SaveChanges();
            }
            else
            {

                TaskClassification oldTaskClassification = db.TaskClassifications.Find(taskClassificationViewModel.id);

                oldTaskClassification.name = taskClassificationViewModel.name;
                oldTaskClassification.updated_by = Session["id"].ToString().ToInt();
                oldTaskClassification.updated_at = DateTime.Now; ;

                db.SaveChanges();
            }

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult deleteTaskClassification(int id)
        {
            TaskClassification deleteTaskClassification = db.TaskClassifications.Find(id);
            deleteTaskClassification.active = (int)RowStatus.INACTIVE;
            deleteTaskClassification.deleted_by = Session["id"].ToString().ToInt();
            deleteTaskClassification.deleted_at = DateTime.Now; ;
            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }
    }
}