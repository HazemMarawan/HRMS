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
                var search_project_id = Request.Form.GetValues("columns[0][search][value]")[0];
                var search_area_id = Request.Form.GetValues("columns[1][search][value]")[0];
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
                                          mvoh = part.mvoh,
                                          lvoh= part.lvoh,
                                          mvug = part.mvug,
                                          lvug = part.lvug,
                                          equipment_quantity = part.equipment_quantity,
                                          active = part.active,
                                          branch_id = user.branch_id,
                                          created_at = part.created_at
                                      }).Where(n => n.active == (int)RowStatus.ACTIVE && n.branch_id == currentUser.branch_id);

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    partData = partData.Where(m => m.part.ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()));
                }
                
                if(!String.IsNullOrEmpty(search_area_id))
                {
                    int search_area_id_int = int.Parse(search_area_id);
                    partData = partData.Where(m =>m.area_id == search_area_id_int);

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

            List<int?> branchProjectIds = db.BranchProjects.Where(br => br.branch_id == currentUser.branch_id).Select(br => br.project_id).ToList();
            ViewBag.Projects = db.Projects.Where(a => a.active == (int)RowStatus.ACTIVE && branchProjectIds.Contains(a.id)).Select(a => new { a.id, a.name }).ToList();
            return View();
        }
        [HttpPost]
        public JsonResult savePart(PartViewModel partViewModel)
        {

            if (partViewModel.id == 0)
            {
                Part part = AutoMapper.Mapper.Map<PartViewModel, Part>(partViewModel);

                part.active = (int)RowStatus.ACTIVE;
                part.created_at = DateTime.Now;
                part.created_by = Session["id"].ToString().ToInt();

                db.Parts.Add(part);
                db.SaveChanges();
            }
            else
            {

                Part oldpart = db.Parts.Find(partViewModel.id);

                oldpart.part = partViewModel.part;
                oldpart.area_id = partViewModel.area_id;
                oldpart.lvoh = partViewModel.lvoh;
                oldpart.mvoh = partViewModel.mvoh;
                oldpart.lvug = partViewModel.lvug;
                oldpart.mvug = partViewModel.mvug;
                oldpart.equipment_quantity = partViewModel.equipment_quantity;
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

        public ActionResult PartsByArea(int? id)
        {
            User user = Session["user"] as User;
            if (isA.Employee())
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
                var areaData = (from part in db.Parts
                                select new PartViewModel
                                {
                                    id = part.id,
                                    part = part.part,
                                    area_id = part.area_id,
                                    mvoh = part.mvoh,
                                    lvoh = part.lvoh,
                                    mvug = part.mvug,
                                    lvug = part.lvug,
                                    mvoh_sum = db.UserProjects.Where(up => up.part_id_fk == part.id && up.status == (int)ProductivityStatus.Approved).Select(up => up.mvoh).Sum(),
                                    lvoh_sum = db.UserProjects.Where(up => up.part_id_fk == part.id && up.status == (int)ProductivityStatus.Approved).Select(up => up.lvoh).Sum(),
                                    mvug_sum = db.UserProjects.Where(up => up.part_id_fk == part.id && up.status == (int)ProductivityStatus.Approved).Select(up => up.mvug).Sum(),
                                    lvug_sum = db.UserProjects.Where(up => up.part_id_fk == part.id && up.status == (int)ProductivityStatus.Approved).Select(up => up.lvug).Sum(),
                                    created_at = part.created_at,
                                    active = part.active,
                                }).Where(n => n.active == (int)RowStatus.ACTIVE && n.area_id == id);




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
            ViewBag.AreaId = id;
            if (id != null)
                ViewBag.AreaName = db.Areas.Find(id).name;
            return View();
        }
    }
}