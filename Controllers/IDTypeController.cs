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
    public class IDTypeController : BaseController
    {
        HRMSDBContext db = new HRMSDBContext();
        // GET: IDType
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
                var idTypeData = (from idtype in db.IDTypes
                                        select new NationalityViewModel
                                        {
                                            id = idtype.id,
                                            name = idtype.name,
                                            active = idtype.active,
                                            created_at = idtype.created_at
                                        }).Where(n => n.active == (int)RowStatus.ACTIVE);

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    idTypeData = idTypeData.Where(m => m.name.ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()));
                }
                //total number of rows count     
                var displayResult = idTypeData.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = idTypeData.Count();

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
        public JsonResult saveIDType(IDTypeViewModel iDTypeViewModel)
        {

            if (iDTypeViewModel.id == 0)
            {
                IDType idType = AutoMapper.Mapper.Map<IDTypeViewModel, IDType>(iDTypeViewModel);

                idType.created_at = DateTime.Now;;
                idType.created_by = Session["id"].ToString().ToInt();

                db.IDTypes.Add(idType);
                db.SaveChanges();
            }
            else
            {

                IDType oldIDType = db.IDTypes.Find(iDTypeViewModel.id);

                oldIDType.name = iDTypeViewModel.name;
                oldIDType.active = iDTypeViewModel.active;
                oldIDType.updated_by = Session["id"].ToString().ToInt();
                oldIDType.updated_at = DateTime.Now;;

                db.SaveChanges();
            }

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult deleteIDType(int id)
        {
            IDType deleteIDType = db.IDTypes.Find(id);
            deleteIDType.active = (int)RowStatus.INACTIVE;
            deleteIDType.deleted_by = Session["id"].ToString().ToInt();
            deleteIDType.deleted_at = DateTime.Now;;
            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }
    }
}