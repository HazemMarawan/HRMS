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
    public class NationalityController : BaseController
    {
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
                var nationalityData = (from nationality in db.Nationalities
                                select new NationalityViewModel
                                {
                                    id = nationality.id,
                                    name = nationality.name,
                                    active = nationality.active,
                                    created_at = nationality.created_at
                                }).Where(n=>n.active == (int)RowStatus.ACTIVE);

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    nationalityData = nationalityData.Where(m => m.name.ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()));
                }

                

                //total number of rows count     
                var displayResult = nationalityData.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = nationalityData.Count();

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
        public JsonResult saveNationality(NationalityViewModel nationalityViewModel)
        {

            if (nationalityViewModel.id == 0)
            {
                Nationality nationality = AutoMapper.Mapper.Map<NationalityViewModel, Nationality>(nationalityViewModel);

                nationality.created_at = DateTime.Now;;
                nationality.created_by = Session["id"].ToString().ToInt();

                db.Nationalities.Add(nationality);
                db.SaveChanges();
            }
            else
            {

                Nationality oldNationality = db.Nationalities.Find(nationalityViewModel.id);

                oldNationality.name = nationalityViewModel.name;
                oldNationality.active = nationalityViewModel.active;
                oldNationality.updated_by = Session["id"].ToString().ToInt();
                oldNationality.updated_at = DateTime.Now;;

                db.SaveChanges();
            }

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult deleteNationality(int id)
        {
            Nationality deleteNationality = db.Nationalities.Find(id);
            deleteNationality.active = (int)RowStatus.INACTIVE;
            deleteNationality.deleted_at = DateTime.Now;;
            deleteNationality.deleted_by = Session["id"].ToString().ToInt();

            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }
    }
}