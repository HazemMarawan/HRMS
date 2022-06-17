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
    public class PartController : BaseController
    {
        HRMSDBContext db = new HRMSDBContext();
        // GET: Department
        public ActionResult Index()
        {
            User currentUser = Session["user"] as User;
            if (!isA.TeamLeader())
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
                                join area in db.Areas on part.area_id equals area.id
                                join user in db.Users on part.created_by equals user.id
                                select new PartViewModel
                                      {
                                          id = part.id,
                                          part = part.part,
                                          area_id = part.area_id,
                                          area_name = area.name,
                                          active = part.active,
                                          branch_id = user.branch_id,
                                          created_at = part.created_at
                                      }).Where(n => n.active == (int)RowStatus.ACTIVE && n.branch_id == currentUser.branch_id);

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
            ViewBag.Areas = db.Areas.Where(a => a.active == (int)RowStatus.ACTIVE).Select(a => new { a.id, a.name }).ToList();
            return View();
        }
        [HttpPost]
        public JsonResult savePart(PartViewModel partViewModel)
        {

            if (partViewModel.id == 0)
            {
                Part part = AutoMapper.Mapper.Map<PartViewModel, Part>(partViewModel);

                part.created_at = DateTime.Now.AddHours(-3);
                part.created_by = Session["id"].ToString().ToInt();

                db.Parts.Add(part);
                db.SaveChanges();
            }
            else
            {

                Part oldpart = db.Parts.Find(partViewModel.id);

                oldpart.part = partViewModel.part;
                oldpart.area_id = partViewModel.area_id;
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

        [HttpGet]
        public JsonResult getPartsByAreaId(int id)
        {
            User currentUser = Session["user"] as User;
            List<PartViewModel> parts = (from part in db.Parts
                                         join user in db.Users on part.created_by equals user.id
                                         select new PartViewModel
                                         {
                                             id = part.id,
                                             part = part.part,
                                             area_id = part.area_id,
                                             branch_id = user.branch_id,
                                             created_by = part.created_by,
                                             active = part.active
                                         }).Where(p=>p.active == (int)RowStatus.ACTIVE && p.area_id == id && p.branch_id == currentUser.branch_id).ToList();
   
            return Json(new { parts = parts }, JsonRequestBehavior.AllowGet);
        }
    }
}