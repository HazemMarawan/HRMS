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
    public class VacationTypeController : BaseController
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
                var vacationTypesData = (from vacationType in db.VacationTypes
                                        select new VacationTypeViewModel
                                        {
                                            id = vacationType.id,
                                            name = vacationType.name,
                                            must_inform_before_duration = vacationType.must_inform_before_duration,
                                            inform_before_duration = vacationType.inform_before_duration,
                                            inform_before_duration_measurement = vacationType.inform_before_duration_measurement,
                                            inform_before_duration_min_range = vacationType.inform_before_duration_min_range,
                                            inform_before_duration_2 = vacationType.inform_before_duration_2,
                                            inform_before_duration_measurement_2 = vacationType.inform_before_duration_measurement_2,
                                            need_approve = vacationType.need_approve,
                                            closed_at_specific_time = vacationType.closed_at_specific_time,
                                            closed_at = vacationType.closed_at,
                                            max_days = vacationType.max_days,
                                            include_official_vacation = vacationType.include_official_vacation,
                                            active = vacationType.active,
                                            created_at = vacationType.created_at
                                        }).Where(n => n.active == (int)RowStatus.ACTIVE);

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    vacationTypesData = vacationTypesData.Where(m => m.name.ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()));
                }

                //total number of rows count     
                var displayResult = vacationTypesData.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = vacationTypesData.Count();

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
        public JsonResult saveVacationType(VacationTypeViewModel vacationTypeViewModel)
        {

            if (vacationTypeViewModel.id == 0)
            {
                VacationType vacationType = AutoMapper.Mapper.Map<VacationTypeViewModel, VacationType>(vacationTypeViewModel);
                if (vacationTypeViewModel.must_inform_before_duration != 1)
                {
                    vacationType.inform_before_duration = null;
                    vacationType.inform_before_duration_measurement = null;
                }

                if (vacationTypeViewModel.closed_at_specific_time != 1)
                {
                    vacationType.closed_at = null;
                }

                vacationType.created_at = DateTime.Now.AddHours(-3);
                vacationType.created_by = Session["id"].ToString().ToInt();

                db.VacationTypes.Add(vacationType);
                db.SaveChanges();
            }
            else
            {

                VacationType oldVacationType = db.VacationTypes.Find(vacationTypeViewModel.id);

                oldVacationType.name = vacationTypeViewModel.name;
                oldVacationType.must_inform_before_duration = vacationTypeViewModel.must_inform_before_duration;
                if (vacationTypeViewModel.must_inform_before_duration != 1)
                {
                    oldVacationType.inform_before_duration = null;
                    oldVacationType.inform_before_duration_measurement = null;
                    oldVacationType.inform_before_duration_min_range = null;
                    oldVacationType.inform_before_duration_2 = null;
                    oldVacationType.inform_before_duration_measurement_2 = null;
                }
                else
                {
                    oldVacationType.inform_before_duration = vacationTypeViewModel.inform_before_duration;
                    oldVacationType.inform_before_duration_measurement = vacationTypeViewModel.inform_before_duration_measurement;
                    oldVacationType.inform_before_duration_min_range = vacationTypeViewModel.inform_before_duration_min_range;
                    oldVacationType.inform_before_duration_2 = vacationTypeViewModel.inform_before_duration_2;
                    oldVacationType.inform_before_duration_measurement_2 = vacationTypeViewModel.inform_before_duration_measurement_2;
                }
                oldVacationType.need_approve = vacationTypeViewModel.need_approve;
                oldVacationType.closed_at_specific_time = vacationTypeViewModel.closed_at_specific_time;
                if (vacationTypeViewModel.closed_at_specific_time != 1)
                {
                    oldVacationType.closed_at = null;
                }
                else
                {
                    oldVacationType.closed_at = vacationTypeViewModel.closed_at;
                }
                
                oldVacationType.max_days = vacationTypeViewModel.max_days;
                oldVacationType.include_official_vacation = vacationTypeViewModel.include_official_vacation;
                oldVacationType.active = vacationTypeViewModel.active;
                oldVacationType.updated_by = Session["id"].ToString().ToInt();
                oldVacationType.updated_at = DateTime.Now.AddHours(-3);

                db.SaveChanges();
            }

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult deleteVacationType(int id)
        {
            VacationType deleteVacationType = db.VacationTypes.Find(id);
            deleteVacationType.active = (int)RowStatus.INACTIVE;
            deleteVacationType.deleted_by = Session["id"].ToString().ToInt();
            deleteVacationType.deleted_at = DateTime.Now.AddHours(-3);
            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }
    }
}