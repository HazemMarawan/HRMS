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
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Data;

namespace HRMS.Controllers
{
    [CustomAuthenticationFilter]
    public class VacationRequestController : BaseController
    {
        HRMSDBContext db = new HRMSDBContext();
        // GET: ProjectType
        public ActionResult Index()
        {
            if (!(isA.Employee() || isA.TeamLeader() || isA.BranchAdmin() || isA.Supervisor() || isA.ProjectManager()))
                return RedirectToAction("Index", "Dashboard");

            User currentUser = Session["user"] as User;
            if (currentUser.vacations_balance == null)
            {
                User updatedUser = db.Users.Find(currentUser.id);
                updatedUser.vacations_balance = 21;
                db.SaveChanges();
            }
            if (!db.VacationYears.Where(vy => vy.year == DateTime.Now.Year && vy.user_id == currentUser.id).Any())
            {
                VacationYear vacationYear = new VacationYear();
                vacationYear.year = DateTime.Now.Year;
                vacationYear.user_id = currentUser.id;
                vacationYear.vacation_balance = currentUser.vacations_balance;
                vacationYear.remaining = vacationYear.vacation_balance;
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
            else
            {
                VacationYear vacationYear = db.VacationYears.Where(vy => vy.year == DateTime.Now.Year && vy.user_id == currentUser.id).FirstOrDefault();
                vacationYear.vacation_balance = db.Users.Find(currentUser.id).vacations_balance;
                db.SaveChanges();
            }
        
            //DateTime myDate = DateTime.ParseExact(currentUser.hiring_date, "dd/MM/yyyy",
            //                               System.Globalization.CultureInfo.InvariantCulture);
            ViewBag.canRequest = 0;//hiring date is null
            if (currentUser.hiring_date != null)
            {
                ViewBag.canRequest = 1;//in 6 month training
                if (Convert.ToDateTime(((DateTime)currentUser.hiring_date).AddMonths(6).ToShortDateString()) < Convert.ToDateTime(DateTime.Now.ToShortDateString()))
                {
                    ViewBag.canRequest = 2;//can apply on vacation
                }
            }

            if (Request.IsAjaxRequest())
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                int? totalVacations = db.VacationRequests.Where(vr => vr.user_id == currentUser.id && vr.year == DateTime.Now.Year && vr.status != (int)ApprovementStatus.Rejected && vr.active == (int)RowStatus.ACTIVE).Select(vr => vr.days).Sum();
                int? totalPendingVacations = db.VacationRequests.Where(vr => vr.user_id == currentUser.id && vr.year == DateTime.Now.Year && vr.status != (int)ApprovementStatus.Rejected && vr.status != (int)ApprovementStatus.ApprovedBySuperAdmin && vr.active == (int)RowStatus.ACTIVE).Select(vr => vr.days).Sum();
                // Getting all data    
                var vacationYearsData = (from vacationYear in db.VacationYears
                                         select new VacationYearViewModel
                                         {
                                             id = vacationYear.id,
                                             year = vacationYear.year,
                                             user_id = vacationYear.user_id,
                                             vacation_balance = vacationYear.vacation_balance,
                                             remaining = vacationYear.remaining,
                                             actual_remaining = (from vacationType in db.VacationTypes
                                                                 join vacationRequest in db.VacationRequests on vacationType.id equals vacationRequest.vacation_type_id
                                                                 select new VacationRequestViewModel
                                                                 {
                                                                     value = vacationType.value,
                                                                     active = vacationRequest.active,
                                                                     status = vacationRequest.status,
                                                                     year = vacationRequest.year,
                                                                     user_id = vacationRequest.user_id,
                                                                     days = vacationRequest.days
                                                                 }
                                                                 ).Where(vr => vr.user_id == currentUser.id && vr.year == vacationYear.year && vr.status != (int)ApprovementStatus.Rejected && (vr.value == 1 || vr.value ==2) && vr.active == (int)RowStatus.ACTIVE).Sum(vt=>vt.days) != null?vacationYear.vacation_balance -
                                                                 (from vacationType in db.VacationTypes
                                                                  join vacationRequest in db.VacationRequests on vacationType.id equals vacationRequest.vacation_type_id
                                                                  select new VacationRequestViewModel
                                                                  {
                                                                      value = vacationType.value,
                                                                      active = vacationRequest.active,
                                                                      status = vacationRequest.status,
                                                                      year = vacationRequest.year,
                                                                      user_id = vacationRequest.user_id,
                                                                      days = vacationRequest.days
                                                                  }
                                                                 ).Where(vr => vr.user_id == currentUser.id && vr.year == vacationYear.year && vr.status != (int)ApprovementStatus.Rejected && (vr.value == 1 || vr.value == 2) && vr.active == (int)RowStatus.ACTIVE).Sum(vt => vt.days):vacationYear.vacation_balance,
                                             pending = (from vacationType in db.VacationTypes
                                                        join vacationRequest in db.VacationRequests on vacationType.id equals vacationRequest.vacation_type_id
                                                        select new VacationRequestViewModel
                                                        {
                                                            value = vacationType.value,
                                                            active = vacationRequest.active,
                                                            status = vacationRequest.status,
                                                            year = vacationRequest.year,
                                                            user_id = vacationRequest.user_id,
                                                            days = vacationRequest.days
                                                        }
                                                                 ).Where(vr => vr.user_id == currentUser.id && vr.year == vacationYear.year && vr.status != (int)ApprovementStatus.Rejected && vr.status != (int)ApprovementStatus.ApprovedBySuperAdmin && (vr.value == 1 || vr.value == 2) && vr.active == (int)RowStatus.ACTIVE).Sum(vt => vt.days) != null ? (from vacationType in db.VacationTypes
                                                                                                                                                                                                                                                                                                     join vacationRequest in db.VacationRequests on vacationType.id equals vacationRequest.vacation_type_id
                                                                                                                                                                                                                                                                                                                                                                 select new VacationRequestViewModel
                                                                                                                                                                                                                                                                                                                                                                 {
                                                                                                                                                                                                                                                                                                                                                                     value = vacationType.value,
                                                                                                                                                                                                                                                                                                                                                                     active = vacationRequest.active,
                                                                                                                                                                                                                                                                                                                                                                     status = vacationRequest.status,
                                                                                                                                                                                                                                                                                                                                                                     year = vacationRequest.year,
                                                                                                                                                                                                                                                                                                                                                                     user_id = vacationRequest.user_id,
                                                                                                                                                                                                                                                                                                                                                                     days = vacationRequest.days
                                                                                                                                                                                                                                                                                                                                                                 }
                                                                 ).Where(vr => vr.user_id == currentUser.id && vr.year == vacationYear.year && vr.status != (int)ApprovementStatus.Rejected && vr.status != (int)ApprovementStatus.ApprovedBySuperAdmin && (vr.value == 1 || vr.value == 2) && vr.active == (int)RowStatus.ACTIVE).Sum(vt => vt.days): 0,
                                             pending_not_affect = (from vacationType in db.VacationTypes
                                                        join vacationRequest in db.VacationRequests on vacationType.id equals vacationRequest.vacation_type_id
                                                        select new
                                                        {
                                                            vacationType.value,
                                                            vacationRequest.active,
                                                            vacationRequest.status,
                                                            vacationRequest.year,
                                                            vacationRequest.user_id,
                                                            vacationRequest.days
                                                        }
                                                                 ).Where(vr => vr.user_id == currentUser.id && vr.year == vacationYear.year && vr.status != (int)ApprovementStatus.Rejected && vr.status != (int)ApprovementStatus.ApprovedBySuperAdmin && !(vr.value == 1 || vr.value == 2) && vr.active == (int)RowStatus.ACTIVE).Sum(vt => vt.days) != null ? (from vacationType in db.VacationTypes
                                                                                                                                                                                                                                                                                                                                                                 join vacationRequest in db.VacationRequests on vacationType.id equals vacationRequest.vacation_type_id
                                                                                                                                                                                                                                                                                                                                                                  select new VacationRequestViewModel
                                                                                                                                                                                                                                                                                                                                                                  {
                                                                                                                                                                                                                                                                                                                                                                      value = vacationType.value,
                                                                                                                                                                                                                                                                                                                                                                      active = vacationRequest.active,
                                                                                                                                                                                                                                                                                                                                                                      status = vacationRequest.status,
                                                                                                                                                                                                                                                                                                                                                                      year = vacationRequest.year,
                                                                                                                                                                                                                                                                                                                                                                      user_id = vacationRequest.user_id,
                                                                                                                                                                                                                                                                                                                                                                      days = vacationRequest.days
                                                                                                                                                                                                                                                                                                                                                                  }
                                                                 ).Where(vr => vr.user_id == currentUser.id && vr.year == vacationYear.year && vr.status != (int)ApprovementStatus.Rejected && vr.status != (int)ApprovementStatus.ApprovedBySuperAdmin && !(vr.value == 1 || vr.value == 2) && vr.active == (int)RowStatus.ACTIVE).Sum(vt => vt.days) : 0,
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
            User currentUser = Session["user"] as User;
            bool requestStatus = true;
            string errorReport = String.Empty;
            VacationTypeViewModel selectedVacation = db.VacationTypes.Where(vt => vt.id == vacationRequestViewModel.vacation_type_id).Select(vt => new VacationTypeViewModel
            {
                id = vt.id,
                name = vt.name,
                must_inform_before_duration = vt.must_inform_before_duration,
                inform_before_duration_min_range = vt.inform_before_duration_min_range,
                inform_before_duration = vt.inform_before_duration,
                inform_before_duration_measurement = vt.inform_before_duration_measurement,
                inform_before_duration_2 = vt.inform_before_duration_2,
                inform_before_duration_measurement_2 = vt.inform_before_duration_measurement_2,
                need_approve = vt.need_approve,
                closed_at_specific_time = vt.closed_at_specific_time,
                closed_at = vt.closed_at,
                max_days = vt.max_days,
                include_official_vacation = vt.include_official_vacation,
                active = vt.active,
                created_at = vt.created_at
            }).FirstOrDefault();

            if (Convert.ToDateTime(((DateTime)vacationRequestViewModel.vacation_to).ToShortDateString()) >= Convert.ToDateTime(((DateTime)vacationRequestViewModel.vacation_from).ToShortDateString()))
            {
                double? currentVacationDays = (int?)Convert.ToDateTime(((DateTime)vacationRequestViewModel.vacation_to).ToShortDateString()).Date.Subtract(Convert.ToDateTime(((DateTime)vacationRequestViewModel.vacation_from).ToShortDateString())).TotalDays + 1;

                VacationYearViewModel userVacationYear = db.VacationYears.Where(vy => vy.user_id == currentUser.id && vy.year == DateTime.Now.Year).Select(vy => new VacationYearViewModel { vacation_balance = vy.vacation_balance, remaining = vy.remaining }).FirstOrDefault();

                int? totalVacations = (from vacationType in db.VacationTypes
                                       join vacationRequest in db.VacationRequests on vacationType.id equals vacationRequest.vacation_type_id
                                       select new VacationRequestViewModel
                                       {
                                           value = vacationType.value,
                                           active = vacationRequest.active,
                                           status = vacationRequest.status,
                                           year = vacationRequest.year,
                                           user_id = vacationRequest.user_id,
                                           days = vacationRequest.days
                                       }).Where(vr => vr.user_id == currentUser.id && vr.year == DateTime.Now.Year && (vr.value == 1 || vr.value == 2) && vr.status != (int)ApprovementStatus.Rejected && vr.active == (int)RowStatus.ACTIVE).Select(vr => vr.days).Sum();
                totalVacations = totalVacations == null ? 0 : totalVacations;

                int? actualRemaining = userVacationYear.vacation_balance - totalVacations;

                if (currentVacationDays <= actualRemaining || !(selectedVacation.value == 1 || selectedVacation.value == 2))
                {

                    if (vacationRequestViewModel.id == 0)
                    {

                        if (selectedVacation.must_inform_before_duration == 1)
                        {
                            if (currentVacationDays <= selectedVacation.inform_before_duration_min_range)
                            {
                                if (selectedVacation.inform_before_duration_measurement == 1)
                                {
                                    double Days = Convert.ToDateTime(((DateTime)vacationRequestViewModel.vacation_from).ToShortDateString()).Date.Subtract(Convert.ToDateTime(DateTime.Now.ToShortDateString())).TotalDays;

                                    if (Days < selectedVacation.inform_before_duration)
                                    {
                                        requestStatus = false;
                                        errorReport += "Must Inform " + selectedVacation.inform_before_duration.ToString() + "Days Before<br>";
                                    }
                                }
                                else
                                {
                                    double Hours = Convert.ToDateTime(((DateTime)vacationRequestViewModel.vacation_from).ToShortDateString()).Date.Subtract(Convert.ToDateTime(DateTime.Now.ToShortDateString())).TotalHours;
                                    if (Hours < (int)selectedVacation.inform_before_duration)
                                    {
                                        requestStatus = false;
                                        errorReport += "Must Inform " + selectedVacation.inform_before_duration.ToString() + "Hours Before<br>";
                                    }
                                }
                            }
                            else 
                            {
                                if(selectedVacation.inform_before_duration_2 != null && selectedVacation.inform_before_duration_measurement_2 != null)
                                {
                                    if (selectedVacation.inform_before_duration_measurement_2 == 1)
                                    {
                                        double Days = Convert.ToDateTime(((DateTime)vacationRequestViewModel.vacation_from).ToShortDateString()).Date.Subtract(Convert.ToDateTime(DateTime.Now.ToShortDateString())).TotalDays;

                                        if (Days < selectedVacation.inform_before_duration_2)
                                        {
                                            requestStatus = false;
                                            errorReport += "Must Inform " + selectedVacation.inform_before_duration_2.ToString() + "Days Before<br>";
                                        }
                                    }
                                    else
                                    {
                                        double Hours = Convert.ToDateTime(((DateTime)vacationRequestViewModel.vacation_from).ToShortDateString()).Date.Subtract(Convert.ToDateTime(DateTime.Now.ToShortDateString())).TotalHours;
                                        if (Hours < (int)selectedVacation.inform_before_duration_2)
                                        {
                                            requestStatus = false;
                                            errorReport += "Must Inform " + selectedVacation.inform_before_duration_2.ToString() + "Hours Before<br>";
                                        }
                                    }
                                }
                                else
                                {
                                    requestStatus = false;
                                    errorReport += "Must Inform " + selectedVacation.inform_before_duration.ToString() + (selectedVacation.inform_before_duration_measurement == 1?"Days":"Hours")+ " Before<br>";
                                }
                            }
                        }

                        if (selectedVacation.max_days != null)
                        {
                            int? selectedVacationCounter = db.VacationRequests.Where(vr => vr.user_id == currentUser.id && vr.year == DateTime.Now.Year && vr.vacation_type_id == selectedVacation.id && vr.status != (int)ApprovementStatus.Rejected && vr.active == (int)RowStatus.ACTIVE).Select(vr => vr.days).Sum();
                            selectedVacationCounter = selectedVacationCounter == null ? 0 : selectedVacationCounter;
                            int? selectedVacationRemaining = selectedVacation.max_days - selectedVacationCounter;
                            if (currentVacationDays > selectedVacationRemaining)
                            {
                                requestStatus = false;
                                errorReport += "Max Days " + selectedVacation.max_days.ToString() + "per Year<br>";
                            }
                        }

                        if (selectedVacation.closed_at_specific_time == 1)
                        {

                            DateTime currentDateTime = DateTime.Now;
                            TimeSpan currentTime = new TimeSpan(currentDateTime.Hour, currentDateTime.Minute, currentDateTime.Second);
                            if (currentTime > selectedVacation.closed_at)
                            {
                                requestStatus = false;
                                errorReport += "This Vacation can be applied before " + selectedVacation.closed_at.ToString() + "<br>";
                            }

                        }
                        if (requestStatus == true)
                        {
                            VacationRequest vacationRequest = AutoMapper.Mapper.Map<VacationRequestViewModel, VacationRequest>(vacationRequestViewModel);
                            vacationRequest.user_id = Session["id"].ToString().ToInt();
                            vacationRequest.days = (int?)currentVacationDays;
                            vacationRequest.year = ((DateTime)vacationRequestViewModel.vacation_from).Year;
                            vacationRequest.created_at = DateTime.Now;
                            vacationRequest.created_by = Session["id"].ToString().ToInt();
                            if (selectedVacation.need_approve == 1)
                            {
                                if (isA.Employee())
                                    vacationRequest.status = (int)ApprovementStatus.PendingApprove;
                                if (isA.TeamLeader())
                                    vacationRequest.status = (int)ApprovementStatus.ApprovedByTeamLeader;
                                if (isA.BranchAdmin())
                                    vacationRequest.status = (int)ApprovementStatus.ApprovedByBranchAdmin;
                                if (isA.Supervisor())
                                    vacationRequest.status = (int)ApprovementStatus.ApprovedBySupervisor;
                            }
                            else
                                vacationRequest.status = (int)ApprovementStatus.ApprovedBySuperAdmin;
                            vacationRequest.active = (int)RowStatus.ACTIVE;
                            db.VacationRequests.Add(vacationRequest);
                            db.SaveChanges();

                            return Json(new { message = "done", success = true }, JsonRequestBehavior.AllowGet);
                        }
                        else

                            return Json(new { message = errorReport, success = false }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        if (selectedVacation.must_inform_before_duration == 1)
                        {
                            if (currentVacationDays <= selectedVacation.inform_before_duration_min_range)
                            {
                                if (selectedVacation.inform_before_duration_measurement == 1)
                                {
                                    double Days = Convert.ToDateTime(((DateTime)vacationRequestViewModel.vacation_from).ToShortDateString()).Date.Subtract(Convert.ToDateTime(DateTime.Now.ToShortDateString())).TotalDays;

                                    if (Days < selectedVacation.inform_before_duration)
                                    {
                                        requestStatus = false;
                                        errorReport += "Must Inform " + selectedVacation.inform_before_duration.ToString() + "Days Before<br>";
                                    }
                                }
                                else
                                {
                                    double Hours = Convert.ToDateTime(((DateTime)vacationRequestViewModel.vacation_from).ToShortDateString()).Date.Subtract(Convert.ToDateTime(DateTime.Now.ToShortDateString())).TotalHours;
                                    if (Hours < (int)selectedVacation.inform_before_duration)
                                    {
                                        requestStatus = false;
                                        errorReport += "Must Inform " + selectedVacation.inform_before_duration.ToString() + "Hours Before<br>";
                                    }
                                }
                            }
                            else
                            {
                                if (selectedVacation.inform_before_duration_2 != null && selectedVacation.inform_before_duration_measurement_2 != null)
                                {
                                    if (selectedVacation.inform_before_duration_measurement_2 == 1)
                                    {
                                        double Days = Convert.ToDateTime(((DateTime)vacationRequestViewModel.vacation_from).ToShortDateString()).Date.Subtract(Convert.ToDateTime(DateTime.Now.ToShortDateString())).TotalDays;

                                        if (Days < selectedVacation.inform_before_duration_2)
                                        {
                                            requestStatus = false;
                                            errorReport += "Must Inform " + selectedVacation.inform_before_duration_2.ToString() + "Days Before<br>";
                                        }
                                    }
                                    else
                                    {
                                        double Hours = Convert.ToDateTime(((DateTime)vacationRequestViewModel.vacation_from).ToShortDateString()).Date.Subtract(Convert.ToDateTime(DateTime.Now.ToShortDateString())).TotalHours;
                                        if (Hours < (int)selectedVacation.inform_before_duration_2)
                                        {
                                            requestStatus = false;
                                            errorReport += "Must Inform " + selectedVacation.inform_before_duration_2.ToString() + "Hours Before<br>";
                                        }
                                    }
                                }
                                else
                                {
                                    requestStatus = false;
                                    errorReport += "Must Inform " + selectedVacation.inform_before_duration.ToString() + (selectedVacation.inform_before_duration_measurement == 1 ? "Days" : "Hours") + " Before<br>";
                                }
                            }
                        }


                        if (selectedVacation.max_days != null)
                        {
                            int? selectedVacationCounter = db.VacationRequests.Where(vr => vr.user_id == currentUser.id && vr.year == DateTime.Now.Year && vr.vacation_type_id == selectedVacation.id && vr.status != (int)ApprovementStatus.Rejected && vr.status != (int)ApprovementStatus.ApprovedBySuperAdmin && vr.active == (int)RowStatus.ACTIVE).Select(vr => vr.days).Sum();
                            selectedVacationCounter = selectedVacationCounter == null ? 0 : selectedVacationCounter;
                            int? selectedVacationRemaining = selectedVacation.max_days - selectedVacationCounter;
                            if (currentVacationDays > selectedVacationRemaining)
                            {
                                requestStatus = false;
                                errorReport += "Max Days " + selectedVacation.max_days.ToString() + "per Year<br>";
                            }
                        }

                        if (selectedVacation.closed_at_specific_time == 1)
                        {

                            DateTime currentDateTime = DateTime.Now;
                            TimeSpan currentTime = new TimeSpan(currentDateTime.Hour, currentDateTime.Minute, currentDateTime.Second);
                            if (currentTime > selectedVacation.closed_at)
                            {
                                requestStatus = false;
                                errorReport += "This Vacation can be applied before " + selectedVacation.closed_at.ToString() + "<br>";
                            }

                        }
                        if (requestStatus == true)
                        {
                            VacationRequest oldVacationRequest = db.VacationRequests.Find(vacationRequestViewModel.id);

                            oldVacationRequest.vacation_type_id = vacationRequestViewModel.vacation_type_id;
                            oldVacationRequest.vacation_from = vacationRequestViewModel.vacation_from;
                            oldVacationRequest.vacation_to = vacationRequestViewModel.vacation_to;
                            oldVacationRequest.days = (int?)currentVacationDays;
                            oldVacationRequest.updated_by = Session["id"].ToString().ToInt();
                            oldVacationRequest.updated_at = DateTime.Now;
                            if (selectedVacation.need_approve == 1)
                            {
                                if (isA.Employee())
                                    oldVacationRequest.status = (int)ApprovementStatus.PendingApprove;
                                if (isA.TeamLeader())
                                    oldVacationRequest.status = (int)ApprovementStatus.ApprovedByTeamLeader;
                                if (isA.BranchAdmin())
                                    oldVacationRequest.status = (int)ApprovementStatus.ApprovedByBranchAdmin;
                            }
                            else
                                oldVacationRequest.status = (int)ApprovementStatus.ApprovedBySuperAdmin;
                            db.SaveChanges();

                            return Json(new { message = "done", success = true }, JsonRequestBehavior.AllowGet);

                        }
                        else
                        {
                            return Json(new { message = errorReport, success = false }, JsonRequestBehavior.AllowGet);
                        }
                    }

                }
                else
                {
                    return Json(new { message = "Annual: " + userVacationYear.vacation_balance + "<br> Remaining: " + actualRemaining, success = false }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { message = "To Date must be greater than or equal From Date", success = false }, JsonRequestBehavior.AllowGet);

            }
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

        public ActionResult History(int year)
        {
            if (!(isA.Employee() || isA.TeamLeader() || isA.BranchAdmin() || isA.Supervisor()))
                return RedirectToAction("Index", "Dashboard");
            User currentUser = Session["user"] as User;
            if (Request.IsAjaxRequest())
            {

                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                // Getting all data    
                var productitvityData = (from vacationRequest in db.VacationRequests
                                         join vacationType in db.VacationTypes on vacationRequest.vacation_type_id equals vacationType.id
                                         select new VacationRequestViewModel
                                         {
                                             id = vacationRequest.id,
                                             user_id = vacationRequest.user_id,
                                             year = vacationRequest.year,
                                             vacation_type_id = vacationRequest.vacation_type_id,
                                             vacation_name = vacationType.name,
                                             vacation_from = vacationRequest.vacation_from,
                                             vacation_to = vacationRequest.vacation_to,
                                             days = vacationRequest.days,
                                             status = vacationRequest.status,
                                             active = vacationRequest.active
                                         }).Where(n => n.active == (int)RowStatus.ACTIVE && n.user_id == currentUser.id && n.year == year);

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    productitvityData = productitvityData.Where(m => m.vacation_name.ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()));
                }

                //total number of rows count     
                var displayResult = productitvityData.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = productitvityData.Count();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = displayResult

                }, JsonRequestBehavior.AllowGet);

            }
            ViewBag.year = year;
            ViewBag.currentYear = DateTime.Now.Year;
            ViewBag.VacationTypes = db.VacationTypes.Select(v => new { v.id, v.name }).ToList();
            return View();
        }

        public ActionResult Approval(int? branch_id)
        {
            User currentUser = Session["user"] as User;

            if (!(isA.TeamLeader() 
                || (isA.BranchAdmin() && (currentUser.branch_id == branch_id || branch_id == null)) 
                || isA.SuperAdmin()
                || isA.Supervisor()))
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
                var vacationsRequestsData = (from vacationRequest in db.VacationRequests
                                         join vacationType in db.VacationTypes on vacationRequest.vacation_type_id equals vacationType.id
                                         join user in db.Users on vacationRequest.user_id equals user.id
                                         join superAd in db.Users on vacationRequest.approved_by_super_admin equals superAd.id into sa
                                         from superAdmin in sa.DefaultIfEmpty()
                                         join branchAd in db.Users on vacationRequest.approved_by_branch_admin equals branchAd.id into ba
                                         from branchAdmin in ba.DefaultIfEmpty()
                                         join teamLead in db.Users on vacationRequest.approved_by_team_leader equals teamLead.id into tl
                                         from teamLeader in tl.DefaultIfEmpty()
                                         join super in db.Users on vacationRequest.approved_by_supervisor equals super.id into tm
                                         from supervisor in tm.DefaultIfEmpty()
                                         select new VacationRequestViewModel
                                         {
                                             id = vacationRequest.id,
                                             user_id = vacationRequest.user_id,
                                             branch_id = user.branch_id,
                                             team_leader_id = user.team_leader_id,
                                             user_type = user.type,
                                             full_name = user.full_name,
                                             year = vacationRequest.year,
                                             vacation_type_id = vacationRequest.vacation_type_id,
                                             vacation_name = vacationType.name,
                                             vacation_from = vacationRequest.vacation_from,
                                             vacation_to = vacationRequest.vacation_to,
                                             days = vacationRequest.days,
                                             status = vacationRequest.status,
                                             active = vacationRequest.active,
                                             created_at = vacationRequest.created_at,
                                             approved_by_super_admin_name = superAdmin.full_name,
                                             approved_by_branch_admin_name = branchAdmin.full_name,
                                             approved_by_team_leader_name = teamLeader.full_name,
                                             approved_by_supervisor_name = supervisor.full_name,
                                             approved_by_super_admin_at = vacationRequest.approved_by_super_admin_at,
                                             approved_by_branch_admin_at = vacationRequest.approved_by_branch_admin_at,
                                             approved_by_team_leader_at = vacationRequest.approved_by_team_leader_at,
                                             approved_by_supervisor_at = vacationRequest.approved_by_supervisor_at,
                                         }).Where(n => n.active == (int)RowStatus.ACTIVE);

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    vacationsRequestsData = vacationsRequestsData.Where(m => m.vacation_name.ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()));
                }

                if (isA.SuperAdmin())
                {
                    vacationsRequestsData = vacationsRequestsData.Where(p => p.user_id != currentUser.id && p.status == (int)ApprovementStatus.ApprovedByBranchAdmin && (p.user_type == (int)UserRole.Employee || p.user_type == (int)UserRole.TeamLeader || p.user_type == (int)UserRole.Supervisor || p.user_type == (int)UserRole.BranchAdmin));
                    if (branch_id != null)
                    {
                        vacationsRequestsData = vacationsRequestsData.Where(p => p.branch_id == branch_id);
                    }
                }

                if (isA.BranchAdmin())
                {
                    vacationsRequestsData = vacationsRequestsData.Where(p => p.branch_id == currentUser.branch_id && p.status == (int)ApprovementStatus.ApprovedBySupervisor && p.user_id != currentUser.id && (p.user_type == (int)UserRole.Employee || p.user_type == (int)UserRole.TeamLeader || p.user_type == (int)UserRole.Supervisor));

                }

                if (isA.TeamLeader())
                {
                    vacationsRequestsData = vacationsRequestsData.Where(p => p.team_leader_id == currentUser.id && p.status == (int)ApprovementStatus.PendingApprove && p.user_id != currentUser.id && p.user_type == (int)UserRole.Employee);

                }

                if (isA.Supervisor())
                {
                    vacationsRequestsData = vacationsRequestsData.Where(p => p.branch_id == currentUser.branch_id && p.status == (int)ApprovementStatus.ApprovedByTeamLeader && p.user_id != currentUser.id && (p.user_type == (int)UserRole.Employee || p.user_type == (int)UserRole.TeamLeader));

                }

                //total number of rows count     
                var displayResult = vacationsRequestsData.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = vacationsRequestsData.Count();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = displayResult

                }, JsonRequestBehavior.AllowGet);

            }

            if (isA.BranchAdmin() || isA.TeamLeader())
            {
                branch_id = currentUser.branch_id;
            }
            ViewBag.branchId = branch_id;
            if (branch_id != null)
            {
                ViewBag.branchName = db.Branches.Where(b => b.id == branch_id).FirstOrDefault().name;
            }
            else
            {
                ViewBag.branchName = "Company";
            }
            return View();
        }

        [HttpGet]
        public JsonResult acceptVacation(int? id)
        {
            VacationRequest vacationRequest = db.VacationRequests.Find(id);
            if (isA.SuperAdmin())
            {
                vacationRequest.approved_by_super_admin = Session["id"].ToString().ToInt();
                vacationRequest.approved_by_super_admin_at = DateTime.Now;
                vacationRequest.status = (int)ApprovementStatus.ApprovedBySuperAdmin;
            }

            if (isA.BranchAdmin())
            {
                vacationRequest.approved_by_branch_admin = Session["id"].ToString().ToInt();
                vacationRequest.approved_by_branch_admin_at = DateTime.Now;
                vacationRequest.status = (int)ApprovementStatus.ApprovedByBranchAdmin;
            }

            if (isA.TeamLeader())
            {
                vacationRequest.approved_by_team_leader = Session["id"].ToString().ToInt();
                vacationRequest.approved_by_team_leader_at = DateTime.Now;
                vacationRequest.status = (int)ApprovementStatus.ApprovedByTeamLeader;
            }

            if (isA.Supervisor())
            {
                vacationRequest.approved_by_team_leader = Session["id"].ToString().ToInt();
                vacationRequest.approved_by_team_leader_at = DateTime.Now;
                vacationRequest.status = (int)ApprovementStatus.ApprovedBySupervisor;
            }

            db.SaveChanges();
            if (isA.SuperAdmin())
            {
                VacationTypeViewModel vacationTypeView = db.VacationTypes.Where(vt => vt.id == vacationRequest.vacation_type_id).Select(vt => new VacationTypeViewModel
                {
                    value = vt.value
                }).FirstOrDefault();
                VacationYear vacationYear = db.VacationYears.Where(vy => vy.year == vacationRequest.year && vy.user_id == vacationRequest.user_id).FirstOrDefault();
                if (vacationTypeView.value == 1)
                    vacationYear.a3tyady_vacation_counter = vacationYear.a3tyady_vacation_counter != null ? vacationYear.a3tyady_vacation_counter + vacationRequest.days : vacationRequest.days;

                else if (vacationTypeView.value == 2)
                    vacationYear.arda_vacation_counter = vacationYear.arda_vacation_counter != null ? vacationYear.arda_vacation_counter + vacationRequest.days : vacationRequest.days;

                else if (vacationTypeView.value == 3)
                    vacationYear.medical_vacation_counter = vacationYear.medical_vacation_counter != null ? vacationYear.medical_vacation_counter + vacationRequest.days : vacationRequest.days;

                else if (vacationTypeView.value == 4)
                    vacationYear.married_vacation_counter = vacationYear.married_vacation_counter != null ? vacationYear.married_vacation_counter + vacationRequest.days : vacationRequest.days;

                else if (vacationTypeView.value == 5)
                    vacationYear.work_from_home_vacation_counter = vacationYear.work_from_home_vacation_counter != null ? vacationYear.work_from_home_vacation_counter + vacationRequest.days : vacationRequest.days;

                else if (vacationTypeView.value == 6)
                    vacationYear.death_vacation_counter = vacationYear.death_vacation_counter != null ? vacationYear.death_vacation_counter + vacationRequest.days : vacationRequest.days;

                db.SaveChanges();
            }
            return Json(new { msg = "done" }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult rejectVacation(int? id)
        {
            VacationRequest vacationRequest = db.VacationRequests.Find(id);
            vacationRequest.rejected_by = Session["id"].ToString().ToInt();
            vacationRequest.rejected_by_at = DateTime.Now;
            vacationRequest.status = (int)ApprovementStatus.Rejected;
            db.SaveChanges();

            return Json(new { msg = "done" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult InitialBalance()
        {
            if (!isA.BranchAdmin())
                return RedirectToAction("Index", "Dashboard");

            return View();
        }
        public void ExportEmployeeBalanceSheet()
        {
            User currentUser = Session["user"] as User;

            ExcelPackage Ep = new ExcelPackage();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Employee Balance Sheet" + DateTime.Now.Month + "-" + DateTime.Now.Year);

            System.Drawing.Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#000000");
            System.Drawing.Color redColor = System.Drawing.ColorTranslator.FromHtml("#FF0000");
            System.Drawing.Color warningColor = System.Drawing.ColorTranslator.FromHtml("#FFA000");
            System.Drawing.Color greenColor = System.Drawing.ColorTranslator.FromHtml("#00FF00");
            Sheet.Cells["A1:H1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells["A1:H1"].Style.Fill.BackgroundColor.SetColor(colFromHex);
            System.Drawing.Color text = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            Sheet.Cells["A1:H1"].Style.Font.Color.SetColor(text);

            Sheet.Cells["A1"].Value = "م";
            Sheet.Cells["B1"].Value = "اسم الموظف";
            Sheet.Cells["C1"].Value = "أعتيادي";
            Sheet.Cells["D1"].Value = "عرضة";
            Sheet.Cells["E1"].Value = "مرضي";
            Sheet.Cells["F1"].Value = "زواج";
            Sheet.Cells["G1"].Value = "عمل من المنزل";
            Sheet.Cells["H1"].Value = "وفاه";
           

            var userData = (from user in db.Users
                            join idtype in db.IDTypes on user.id_type equals idtype.id
                            join nationality in db.Nationalities on user.nationality_id equals nationality.id
                            join bran in db.Branches on user.branch_id equals bran.id into br
                            from branch in br.DefaultIfEmpty()
                            join department in db.Departments on user.department_id equals department.id
                            join job in db.Jobs on user.job_id equals job.id
                            select new UserViewModel
                            {
                                id = user.id,
                                code = user.code,
                                attendance_code = user.attendance_code,
                                user_name = user.user_name,
                                full_name = user.full_name,
                                first_name = user.first_name,
                                middle_name = user.middle_name,
                                last_name = user.last_name,
                                password = user.password,
                                id_type = user.id_type,
                                id_type_name = idtype.name,
                                id_number = user.id_number,
                                birth_date = user.birth_date,
                                last_salary = user.last_salary,
                                last_hour_price = user.last_hour_price,
                                last_over_time_price = user.last_over_time_price,
                                required_productivity = user.required_productivity,
                                phone = user.phone,
                                address = user.address,
                                nationality_id = user.nationality_id,
                                team_leader_id = user.team_leader_id,
                                nationality_name = nationality.name,
                                branch_id = user.branch_id,
                                branch_name = branch.name,
                                department_id = user.department_id,
                                department_name = department.name,
                                job_id = user.job_id,
                                job_name = job.name,
                                gender = user.gender,
                                hiring_date = user.hiring_date,
                                vacations_balance = user.vacations_balance,
                                imagePath = user.image,
                                notes = user.notes,
                                type = user.type,
                                active = user.active
                            }).Where(s => s.active == (int)RowStatus.ACTIVE && (s.type == (int)UserRole.Employee || s.type == (int)UserRole.TeamLeader || s.type == (int)UserRole.Supervisor || s.type == (int)UserRole.BranchAdmin) && s.branch_id == currentUser.branch_id);

            List<UserViewModel> employees = userData.ToList();

            int row = 2;
            foreach (var item in employees)
            {

                Sheet.Cells[string.Format("A{0}", row)].Value = item.id;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.full_name;
                Sheet.Cells[string.Format("C{0}", row)].Value = "0";
                Sheet.Cells[string.Format("D{0}", row)].Value = "0";
                Sheet.Cells[string.Format("E{0}", row)].Value = "0";
                Sheet.Cells[string.Format("F{0}", row)].Value = "0";
                Sheet.Cells[string.Format("G{0}", row)].Value = "0";
                Sheet.Cells[string.Format("H{0}", row)].Value = "0";


                row++;
            }

            row++;

            Sheet.Cells["A:AZ"].AutoFitColumns();

            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + DateTime.Now.ToString() + "Report.xlsx");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();
        }

        public JsonResult ImportEmployeeBalanceSheet(GeneralUploadViewModel generalUploadViewModel)
        {
            User currentUser = Session["user"] as User;

            Guid guid = Guid.NewGuid();
            var InputFileName = Path.GetFileName(generalUploadViewModel.file.FileName);
            var ServerSavePath = Path.Combine(Server.MapPath("~/Uploads/EmployeeBalanceSheet/") + guid.ToString() + "_EBS" + Path.GetExtension(generalUploadViewModel.file.FileName));
            generalUploadViewModel.file.SaveAs(ServerSavePath);

            
            //Save the uploaded Excel file.

            //Open the Excel file in Read Mode using OpenXml.
            using (SpreadsheetDocument doc = SpreadsheetDocument.Open(ServerSavePath, false))
            {
                //Read the first Sheet from Excel file.
                Sheet sheet = doc.WorkbookPart.Workbook.Sheets.GetFirstChild<Sheet>();

                //Get the Worksheet instance.
                Worksheet worksheet = (doc.WorkbookPart.GetPartById(sheet.Id.Value) as WorksheetPart).Worksheet;

                //Fetch all the rows present in the Worksheet.
                IEnumerable<Row> rows = worksheet.GetFirstChild<SheetData>().Descendants<Row>();

                //Create a new DataTable.
                DataTable dt = new DataTable();

                //Loop through the Worksheet rows.
                foreach (Row row in rows)
                {
                    //Use the first row to add columns to DataTable.
                    if (row.RowIndex.Value == 1)
                    {
                        foreach (Cell cell in row.Descendants<Cell>())
                        {
                            dt.Columns.Add(GetValue(doc, cell));
                        }
                    }
                    else
                    {
                        //Add rows to DataTable.
                        dt.Rows.Add();
                        int i = 0;
                        foreach (Cell cell in row.Descendants<Cell>())
                        {
                            dt.Rows[dt.Rows.Count - 1][i] = GetValue(doc, cell);
                            i++;
                        }
                    }
                }
                List<VacationRequest> importedVacationRequests = new List<VacationRequest>();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    int currentImportedUserId = dt.Rows[i][0].ToString().ToInt();

                    User currentImportedUser = db.Users.Find(currentImportedUserId);
                    if (currentUser.vacations_balance == null)
                    {
                        currentImportedUser.vacations_balance = 21;
                        db.SaveChanges();
                    }

                    VacationYear vacationYear = new VacationYear();
                    if (!db.VacationYears.Where(vy => vy.year == DateTime.Now.Year && vy.user_id == currentImportedUserId).Any())
                    {
                        vacationYear.year = DateTime.Now.Year;
                        vacationYear.user_id = currentImportedUserId;
                        vacationYear.vacation_balance = currentImportedUser.vacations_balance;
                        vacationYear.remaining = vacationYear.vacation_balance;
                        vacationYear.a3tyady_vacation_counter = 0;
                        vacationYear.arda_vacation_counter = 0;
                        vacationYear.medical_vacation_counter = 0;
                        vacationYear.married_vacation_counter = 0;
                        vacationYear.work_from_home_vacation_counter = 0;
                        vacationYear.death_vacation_counter = 0;
                        vacationYear.active = 1;
                        vacationYear.created_by = currentUser.id;
                        vacationYear.created_at = DateTime.Now;

                        db.VacationYears.Add(vacationYear);
                        db.SaveChanges();
                    }
                    else
                    {
                        vacationYear = db.VacationYears.Where(vy => vy.year == DateTime.Now.Year && vy.user_id == currentImportedUserId).FirstOrDefault();
                        vacationYear.vacation_balance = db.Users.Find(currentUser.id).vacations_balance;
                        db.SaveChanges();
                    }

                    if (dt.Rows[i][2].ToString().ToInt() != 0)
                    {
                        VacationRequest vacationRequest = new VacationRequest();
                        vacationRequest.id = 0;
                        vacationRequest.user_id = dt.Rows[i][0].ToString().ToInt();
                        vacationRequest.vacation_type_id = 3;
                        vacationRequest.year = DateTime.Now.Year;
                        vacationRequest.days = dt.Rows[i][2].ToString().ToInt();
                        vacationRequest.status = (int)ApprovementStatus.ApprovedBySuperAdmin;
                        vacationRequest.active = (int)RowStatus.ACTIVE;
                        vacationRequest.created_by = currentUser.id;
                        vacationRequest.created_at = DateTime.Now;

                        vacationYear.a3tyady_vacation_counter = vacationYear.a3tyady_vacation_counter != null ? vacationYear.a3tyady_vacation_counter + dt.Rows[i][2].ToString().ToInt() : dt.Rows[i][2].ToString().ToInt();
                        db.SaveChanges();

                        importedVacationRequests.Add(vacationRequest);

                    }

                    if (dt.Rows[i][3].ToString().ToInt() != 0)
                    {
                        VacationRequest vacationRequest = new VacationRequest();
                        vacationRequest.id = 0;
                        vacationRequest.user_id = dt.Rows[i][0].ToString().ToInt();
                        vacationRequest.vacation_type_id = 4;
                        vacationRequest.year = DateTime.Now.Year;
                        vacationRequest.days = dt.Rows[i][3].ToString().ToInt();
                        vacationRequest.status = (int)ApprovementStatus.ApprovedBySuperAdmin;
                        vacationRequest.active = (int)RowStatus.ACTIVE;
                        vacationRequest.created_by = currentUser.id;
                        vacationRequest.created_at = DateTime.Now;

                        vacationYear.arda_vacation_counter = vacationYear.arda_vacation_counter != null ? vacationYear.arda_vacation_counter + dt.Rows[i][3].ToString().ToInt() : dt.Rows[i][3].ToString().ToInt();
                        db.SaveChanges();

                        importedVacationRequests.Add(vacationRequest);

                    }

                    if (dt.Rows[i][4].ToString().ToInt() != 0)
                    {
                        VacationRequest vacationRequest = new VacationRequest();
                        vacationRequest.id = 0;
                        vacationRequest.user_id = dt.Rows[i][0].ToString().ToInt();
                        vacationRequest.vacation_type_id = 5;
                        vacationRequest.year = DateTime.Now.Year;
                        vacationRequest.days = dt.Rows[i][4].ToString().ToInt();
                        vacationRequest.status = (int)ApprovementStatus.ApprovedBySuperAdmin;
                        vacationRequest.active = (int)RowStatus.ACTIVE;
                        vacationRequest.created_by = currentUser.id;
                        vacationRequest.created_at = DateTime.Now;

                        vacationYear.medical_vacation_counter = vacationYear.medical_vacation_counter != null ? vacationYear.medical_vacation_counter + dt.Rows[i][4].ToString().ToInt() : dt.Rows[i][4].ToString().ToInt();
                        db.SaveChanges();

                        importedVacationRequests.Add(vacationRequest);

                        
                    }

                    if (dt.Rows[i][5].ToString().ToInt() != 0)
                    {
                        VacationRequest vacationRequest = new VacationRequest();
                        vacationRequest.id = 0;
                        vacationRequest.user_id = dt.Rows[i][0].ToString().ToInt();
                        vacationRequest.vacation_type_id = 6;
                        vacationRequest.year = DateTime.Now.Year;
                        vacationRequest.days = dt.Rows[i][5].ToString().ToInt();
                        vacationRequest.status = (int)ApprovementStatus.ApprovedBySuperAdmin;
                        vacationRequest.active = (int)RowStatus.ACTIVE;
                        vacationRequest.created_by = currentUser.id;
                        vacationRequest.created_at = DateTime.Now;

                        vacationYear.married_vacation_counter = vacationYear.married_vacation_counter != null ? vacationYear.married_vacation_counter + dt.Rows[i][5].ToString().ToInt() : dt.Rows[i][5].ToString().ToInt();
                        db.SaveChanges();

                        importedVacationRequests.Add(vacationRequest);
                    }

                    if (dt.Rows[i][6].ToString().ToInt() != 0)
                    {
                        VacationRequest vacationRequest = new VacationRequest();
                        vacationRequest.id = 0;
                        vacationRequest.user_id = dt.Rows[i][0].ToString().ToInt();
                        vacationRequest.vacation_type_id = 7;
                        vacationRequest.year = DateTime.Now.Year;
                        vacationRequest.days = dt.Rows[i][6].ToString().ToInt();
                        vacationRequest.status = (int)ApprovementStatus.ApprovedBySuperAdmin;
                        vacationRequest.active = (int)RowStatus.ACTIVE;
                        vacationRequest.created_by = currentUser.id;
                        vacationRequest.created_at = DateTime.Now;

                        vacationYear.work_from_home_vacation_counter = vacationYear.work_from_home_vacation_counter != null ? vacationYear.work_from_home_vacation_counter + dt.Rows[i][6].ToString().ToInt() : dt.Rows[i][6].ToString().ToInt();
                        db.SaveChanges();

                        importedVacationRequests.Add(vacationRequest);
                    }

                    if (dt.Rows[i][7].ToString().ToInt() != 0)
                    {
                        VacationRequest vacationRequest = new VacationRequest();
                        vacationRequest.id = 0;
                        vacationRequest.user_id = dt.Rows[i][0].ToString().ToInt();
                        vacationRequest.vacation_type_id = 8;
                        vacationRequest.year = DateTime.Now.Year;
                        vacationRequest.days = dt.Rows[i][6].ToString().ToInt();
                        vacationRequest.status = (int)ApprovementStatus.ApprovedBySuperAdmin;
                        vacationRequest.active = (int)RowStatus.ACTIVE;
                        vacationRequest.created_by = currentUser.id;
                        vacationRequest.created_at = DateTime.Now;

                        vacationYear.death_vacation_counter = vacationYear.death_vacation_counter != null ? vacationYear.death_vacation_counter + dt.Rows[i][7].ToString().ToInt() : dt.Rows[i][7].ToString().ToInt();
                        db.SaveChanges();

                        importedVacationRequests.Add(vacationRequest);
                    }

                }
                db.VacationRequests.AddRange(importedVacationRequests);
                db.SaveChanges();
            }

            return Json(new { msg = "done" }, JsonRequestBehavior.AllowGet);
        }

        private string GetValue(SpreadsheetDocument doc, Cell cell)
        {
            string value = cell.CellValue.InnerText;
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return doc.WorkbookPart.SharedStringTablePart.SharedStringTable.ChildElements.GetItem(int.Parse(value)).InnerText;
            }
            return value;
        }
    }
}