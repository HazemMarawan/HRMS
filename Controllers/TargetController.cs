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
    public class TargetController : BaseController
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
                var targetData = (from target in db.Targets
                                        select new TargetViewModel
                                        {
                                            id = target.id,
                                            mvoh = target.mvoh,
                                            lvoh = target.lvoh,
                                            mvug = target.mvug,
                                            lvug = target.lvug,
                                            active = target.active,
                                            created_at = target.created_at
                                        }).Where(n => n.active == (int)RowStatus.ACTIVE);

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    targetData = targetData.Where(
                        m => m.mvoh.ToString().ToLower().Contains(searchValue.ToLower()) 
                    || m.lvoh.ToString().ToLower().Contains(searchValue.ToLower()) 
                    || m.mvug.ToString().ToLower().Contains(searchValue.ToLower()) 
                    || m.lvug.ToString().ToLower().Contains(searchValue.ToLower()) 
                    || m.id.ToString().ToLower().Contains(searchValue.ToLower())
                    );
                }

                //total number of rows count     
                var displayResult = targetData.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = targetData.Count();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = displayResult

                }, JsonRequestBehavior.AllowGet);

            }
            ViewBag.showAddTarget = db.Targets.Where(t=>t.active == (int)RowStatus.ACTIVE).ToList().Count();
            return View();
        }
        [HttpPost]
        public JsonResult saveTarget(TargetViewModel targetViewModel)
        {

            if (targetViewModel.id == 0)
            {
                Target target = AutoMapper.Mapper.Map<TargetViewModel, Target>(targetViewModel);

                target.created_at = DateTime.Now;
                target.created_by = Session["id"].ToString().ToInt();

                db.Targets.Add(target);
                db.SaveChanges();
            }
            else
            {

                Target oldTarget = db.Targets.Find(targetViewModel.id);

                oldTarget.mvoh = targetViewModel.mvoh;
                oldTarget.lvoh = targetViewModel.lvoh;
                oldTarget.mvug = targetViewModel.mvug;
                oldTarget.lvug = targetViewModel.lvug;
                oldTarget.active = targetViewModel.active;
                oldTarget.updated_by = Session["id"].ToString().ToInt();
                oldTarget.updated_at = DateTime.Now;

                db.SaveChanges();
            }

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult deleteTarget(int id)
        {
            Target deleteTarget = db.Targets.Find(id);
            deleteTarget.active = (int)RowStatus.INACTIVE;
            deleteTarget.deleted_by = Session["id"].ToString().ToInt();
            deleteTarget.deleted_at = DateTime.Now;
            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }
    }
}