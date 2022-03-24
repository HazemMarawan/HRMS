using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HRMS.Models;
using HRMS.ViewModels;
using HRMS.Auth;
using HRMS.Enum;
using HRMS.Helpers;

namespace HRMS.Controllers
{
    [CustomAuthenticationFilter]
    public class PartController : Controller
    {
        HRMSDBContext db = new HRMSDBContext();
        // GET: Department
        public ActionResult Index()
        {
            if (!isA.BranchAdmin())
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
                var partData = (from part in db.Parts
                                      select new PartViewModel
                                      {
                                          id = part.id,
                                          part = part.part,
                                          active = part.active,
                                          created_at = part.created_at
                                      }).Where(n => n.active == (int)RowStatus.ACTIVE);

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    partData = partData.Where(m => m.part.ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()));
                }
             
                //total number of rows count     
                var displayResult = partData.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = partData.Count();

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
        public JsonResult savePart(PartViewModel partViewModel)
        {

            if (partViewModel.id == 0)
            {
                Part part = AutoMapper.Mapper.Map<PartViewModel, Part>(partViewModel);

                part.created_at = DateTime.Now;
                part.created_by = Session["id"].ToString().ToInt();

                db.Parts.Add(part);
                db.SaveChanges();
            }
            else
            {

                Part oldpart = db.Parts.Find(partViewModel.id);

                oldpart.part = partViewModel.part;
                oldpart.active = partViewModel.active;
                oldpart.updated_by = Session["id"].ToString().ToInt();
                oldpart.updated_at = DateTime.Now;

                db.SaveChanges();
            }

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult deletePart(int id)
        {
            Part deletePart = db.Parts.Find(id);
            deletePart.active = (int)RowStatus.INACTIVE;
            deletePart.deleted_by = Session["id"].ToString().ToInt();
            deletePart.deleted_at = DateTime.Now;
            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }
    }
}