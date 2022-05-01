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
    public class AreaController : BaseController
    {
        HRMSDBContext db = new HRMSDBContext();
        // GET: Department
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
                var areaData = (from area in db.Areas
                                      join project in db.Projects on area.project_id equals project.id
                                      select new AreaViewModel
                                      {
                                          id = area.id,
                                          name = area.name,
                                          project_id = project.id,
                                          project_name = project.name,
                                          mvoh = area.mvoh,
                                          lvoh = area.lvoh,
                                          mvug = area.mvug,
                                          lvug = area.lvug,
                                          active = area.active,
                                          created_at = area.created_at
                                      }).Where(n => n.active == (int)RowStatus.ACTIVE);

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    areaData = areaData.Where(m => m.name.ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()));
                }

                if (!isA.SuperAdmin())
                {
                    areaData = areaData.Where(n => n.id == -1);
                }
                //total number of rows count     
                var displayResult = areaData.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = areaData.Count();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = displayResult

                }, JsonRequestBehavior.AllowGet);

            }
            ViewBag.Projects = db.Projects.Where(p => p.active == (int)RowStatus.ACTIVE).Select(p => new { p.id, p.name }).ToList();
            return View();
        }
        [HttpPost]
        public JsonResult saveArea(AreaViewModel areaViewModel)
        {
            User user = Session["user"] as User;

            if (areaViewModel.id == 0)
            {
                Area area = AutoMapper.Mapper.Map<AreaViewModel, Area>(areaViewModel);

                area.created_at = DateTime.Now;
                area.created_by = user.id;

                db.Areas.Add(area);
            }
            else
            {
                Area area = db.Areas.Find(areaViewModel.id);
                area.name = areaViewModel.name;
                area.active = areaViewModel.active;
                area.mvoh = areaViewModel.mvoh;
                area.lvoh = areaViewModel.lvoh;
                area.lvug = areaViewModel.lvug;
                area.mvug = areaViewModel.mvug;
                area.updated_at = DateTime.Now;
                area.updated_by = user.id;

            }

            db.SaveChanges();
            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult deleteArea(int id)
        {
            Area deleteArea = db.Areas.Find(id);
            deleteArea.active = (int)RowStatus.INACTIVE;
            deleteArea.deleted_at = DateTime.Now;
            deleteArea.deleted_by = Session["id"].ToString().ToInt();

            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }
    }
}