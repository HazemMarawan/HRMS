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
    public class TaskController : BaseController
    {
        HRMSDBContext db = new HRMSDBContext();
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
                var tasksData = (from task in db.Tasks
                                      select new TaskViewModel
                                      {
                                          id = task.id,
                                          name = task.name,
                                          active = task.active,
                                          created_at = task.created_at
                                      }).Where(n => n.active == (int)RowStatus.ACTIVE);

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    tasksData = tasksData.Where(m => m.name.ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()));
                }

                if (!isA.SuperAdmin())
                {
                    tasksData = tasksData.Where(n => n.id == -1);
                }
                //total number of rows count     
                var displayResult = tasksData.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = tasksData.Count();

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
        public JsonResult saveTask(TaskViewModel taskViewModel)
        {

            if (taskViewModel.id == 0)
            {
                Task task = AutoMapper.Mapper.Map<TaskViewModel, Task>(taskViewModel);

                task.created_at = DateTime.Now.AddHours(-3);
                task.created_by = Session["id"].ToString().ToInt();

                db.Tasks.Add(task);
                db.SaveChanges();
            }
            else
            {

                Task oldTask = db.Tasks.Find(taskViewModel.id);

                oldTask.name = taskViewModel.name;
                oldTask.active = taskViewModel.active;
                oldTask.updated_by = Session["id"].ToString().ToInt();
                oldTask.updated_at = DateTime.Now.AddHours(-3);

                db.SaveChanges();
            }

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult deleteTask(int id)
        {
            Task deleteTask = db.Tasks.Find(id);
            deleteTask.active = (int)RowStatus.INACTIVE;
            deleteTask.deleted_by = Session["id"].ToString().ToInt();
            deleteTask.deleted_at = DateTime.Now.AddHours(-3);
            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }
    }
}