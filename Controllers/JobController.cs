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
    public class JobController : Controller
    {
        // GET: Job
        HRMSDBContext db = new HRMSDBContext();
        // GET: Nationality
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
                var jobData = (from job in db.Jobs
                                       select new JobViewModel
                                       {
                                           id = job.id,
                                           name = job.name,
                                           active = job.active,
                                           created_at = job.created_at
                                       }).Where(n => n.active == (int)RowStatus.ACTIVE);

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    jobData = jobData.Where(m => m.name.ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()));
                }



                //total number of rows count     
                var displayResult = jobData.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = jobData.Count();

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
        public JsonResult saveJob(JobViewModel jobViewModel)
        {

            if (jobViewModel.id == 0)
            {
                Job job = AutoMapper.Mapper.Map<JobViewModel, Job>(jobViewModel);

                job.created_at = DateTime.Now;
                job.created_by = Session["id"].ToString().ToInt();

                db.Jobs.Add(job);
                db.SaveChanges();
            }
            else
            {

                Job oldJob = db.Jobs.Find(jobViewModel.id);

                oldJob.name = jobViewModel.name;
                oldJob.active = jobViewModel.active;
                oldJob.updated_by = Session["id"].ToString().ToInt();
                oldJob.updated_at = DateTime.Now;

                db.SaveChanges();
            }

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult deleteJob(int id)
        {
            Job deleteJob = db.Jobs.Find(id);
            deleteJob.active = (int)RowStatus.INACTIVE;
            deleteJob.deleted_at = DateTime.Now;
            deleteJob.deleted_by = Session["id"].ToString().ToInt();

            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }
    }
}