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
    public class VacationRequestController : Controller
    {
        HRMSDBContext db = new HRMSDBContext();
        // GET: ProjectType
        public ActionResult Index()
        {
            if (!(isA.Employee() || isA.TeamLeader() || isA.BranchAdmin()))
                return RedirectToAction("Index", "Dashboard");

            User currentUser = Session["user"] as User;
            if (currentUser.vacations_balance == null)
            {
                currentUser.vacations_balance = 21;
                db.SaveChanges();
            }
            if (!db.VacationYears.Where(vy=>vy.year == DateTime.Now.Year && vy.user_id == currentUser.id).Any())
            {
                VacationYear vacationYear = new VacationYear();
                vacationYear.year = DateTime.Now.Year;
                vacationYear.user_id = currentUser.id;
                vacationYear.vacation_balance = currentUser.vacations_balance;
                vacationYear.a3tyady_vacation_counter = 0;
                vacationYear.arda_vacation_counter = 0;
                vacationYear.medical_vacation_counter = 0;
                vacationYear.married_vacation_counter = 0;
                vacationYear.work_from_home_vacation_counter = 0;
                vacationYear.death_vacation_counter = 0;
                vacationYear.active = 1;
                vacationYear.created_by = Session["id"].ToString().ToInt();
                vacationYear.created_at = DateTime.Now;

                db.VacationYears.Add(vacationYear);
                db.SaveChanges();
            }

            if (Request.IsAjaxRequest())
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                // Getting all data    
                var vacationYearsData = (from vacationYear in db.VacationYears
                                        select new VacationYearViewModel
                                        {
                                            id = vacationYear.id,
                                            year = vacationYear.year,
                                            user_id = vacationYear.user_id,
                                            vacation_balance = vacationYear.vacation_balance,
                                            a3tyady_vacation_counter = vacationYear.a3tyady_vacation_counter,
                                            arda_vacation_counter = vacationYear.arda_vacation_counter,
                                            medical_vacation_counter = vacationYear.medical_vacation_counter,
                                            married_vacation_counter = vacationYear.married_vacation_counter,
                                            work_from_home_vacation_counter = vacationYear.work_from_home_vacation_counter,
                                            death_vacation_counter = vacationYear.death_vacation_counter,
                                            active = vacationYear.active,
                                            created_at = vacationYear.created_at
                                        }).Where(n => n.user_id == currentUser.id && n.active == (int)RowStatus.ACTIVE);

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    vacationYearsData = vacationYearsData.Where(m =>
                    m.year.ToString().ToLower().Contains(searchValue.ToLower())
                    || m.id.ToString().ToLower().Contains(searchValue.ToLower()));
                }

                //total number of rows count     
                var displayResult = vacationYearsData.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = vacationYearsData.Count();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = displayResult

                }, JsonRequestBehavior.AllowGet);

            }
            ViewBag.VacationTypes = db.VacationTypes.Select(v => new { v.id, v.name }).ToList();
            return View();
        }
        [HttpPost]
        public JsonResult saveVacationRequest(VacationRequestViewModel vacationRequestViewModel)
        {

            if (vacationRequestViewModel.id == 0)
            {
                bool requestStatus = true;
                VacationTypeViewModel selectedVacation = db.VacationTypes.Where(vt => vt.id == vacationRequestViewModel.vacation_type_id).Select(vt => new VacationTypeViewModel
                {
                    id = vt.id,
                    name = vt.name,
                    must_inform_before_duration = vt.must_inform_before_duration,
                    inform_before_duration = vt.inform_before_duration,
                    inform_before_duration_measurement = vt.inform_before_duration_measurement,
                    need_approve = vt.need_approve,
                    closed_at_specific_time = vt.closed_at_specific_time,
                    closed_at = vt.closed_at,
                    max_days = vt.max_days,
                    include_official_vacation = vt.include_official_vacation,
                    active = vt.active,
                    created_at = vt.created_at
                }).FirstOrDefault();

                if(selectedVacation.must_inform_before_duration == 1)
                {
                    if(selectedVacation.inform_before_duration_measurement == 1)
                    {
                        int Days = ((DateTime)vacationRequestViewModel.vacation_from - DateTime.Now).Days + 1;
                        //if(Days < (int)vacationRequestViewModel.inform_before_duration)
                        //{

                        //}
                    }
                    else
                    {

                    }
                }

                VacationRequest vacationRequest = AutoMapper.Mapper.Map<VacationRequestViewModel, VacationRequest>(vacationRequestViewModel);
                
                vacationRequest.user_id = Session["id"].ToString().ToInt();
                vacationRequest.year = ((DateTime)vacationRequestViewModel.vacation_from).Year;
                vacationRequest.created_at = DateTime.Now;
                vacationRequest.created_by = Session["id"].ToString().ToInt();
                vacationRequest.status = (int)ApprovementStatus.PendingApprove;
                vacationRequest.active = (int)RowStatus.ACTIVE;
                db.VacationRequests.Add(vacationRequest);
                db.SaveChanges();
            }
            else
            {

                VacationRequest oldVacationRequest = db.VacationRequests.Find(vacationRequestViewModel.id);

                oldVacationRequest.vacation_type_id = vacationRequestViewModel.vacation_type_id;
                oldVacationRequest.vacation_from = vacationRequestViewModel.vacation_from;
                oldVacationRequest.vacation_to = vacationRequestViewModel.vacation_to;
                oldVacationRequest.updated_by = Session["id"].ToString().ToInt();
                oldVacationRequest.updated_at = DateTime.Now;

                db.SaveChanges();
            }

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult deleteVacationRequest(int id)
        {
            VacationRequest deleteVacationRequest = db.VacationRequests.Find(id);
            deleteVacationRequest.active = (int)RowStatus.INACTIVE;
            deleteVacationRequest.deleted_by = Session["id"].ToString().ToInt();
            deleteVacationRequest.deleted_at = DateTime.Now;
            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }
    }
}