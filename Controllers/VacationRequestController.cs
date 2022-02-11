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
            if (!(isA.Employee() || isA.TeamLeader() || isA.BranchAdmin() || isA.TechnicalManager()))
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
                                             actual_remaining = db.VacationRequests.Where(vr => vr.user_id == currentUser.id && vr.year == vacationYear.year && vr.status != (int)ApprovementStatus.Rejected && vr.active == (int)RowStatus.ACTIVE).Select(vr => vr.days).Sum() != null ? vacationYear.vacation_balance - db.VacationRequests.Where(vr => vr.user_id == currentUser.id && vr.year == vacationYear.year && vr.status != (int)ApprovementStatus.Rejected && vr.active == (int)RowStatus.ACTIVE).Select(vr => vr.days).Sum() : vacationYear.vacation_balance,
                                             pending = db.VacationRequests.Where(vr => vr.user_id == currentUser.id && vr.year == vacationYear.year && vr.status != (int)ApprovementStatus.Rejected && vr.status != (int)ApprovementStatus.ApprovedBySuperAdmin && vr.active == (int)RowStatus.ACTIVE).Select(vr => vr.days).Sum() != null ? db.VacationRequests.Where(vr => vr.user_id == currentUser.id && vr.year == vacationYear.year && vr.status != (int)ApprovementStatus.Rejected && vr.status != (int)ApprovementStatus.ApprovedBySuperAdmin && vr.active == (int)RowStatus.ACTIVE).Select(vr => vr.days).Sum() : 0,
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
            if (Convert.ToDateTime(((DateTime)vacationRequestViewModel.vacation_to).ToShortDateString()) >= Convert.ToDateTime(((DateTime)vacationRequestViewModel.vacation_from).ToShortDateString()))
            {
                double? currentVacationDays = (int?)Convert.ToDateTime(((DateTime)vacationRequestViewModel.vacation_to).ToShortDateString()).Date.Subtract(Convert.ToDateTime(((DateTime)vacationRequestViewModel.vacation_from).ToShortDateString())).TotalDays + 1;
                VacationYearViewModel userVacationYear = db.VacationYears.Where(vy => vy.user_id == currentUser.id && vy.year == DateTime.Now.Year).Select(vy => new VacationYearViewModel { vacation_balance = vy.vacation_balance, remaining = vy.remaining }).FirstOrDefault();
                int? totalVacations = db.VacationRequests.Where(vr => vr.user_id == currentUser.id && vr.year == DateTime.Now.Year && vr.status != (int)ApprovementStatus.Rejected && vr.active == (int)RowStatus.ACTIVE).Select(vr => vr.days).Sum();
                totalVacations = totalVacations == null ? 0 : totalVacations;
                int? actualRemaining = userVacationYear.vacation_balance - totalVacations;
                if (currentVacationDays <= actualRemaining)
                {

                    if (vacationRequestViewModel.id == 0)
                    {

                        if (selectedVacation.must_inform_before_duration == 1)
                        {
                            if (selectedVacation.inform_before_duration_measurement == 1)
                            {
                                //int Days = ((DateTime)vacationRequestViewModel.vacation_from - DateTime.Now).Days + 1;
                                double Days = Convert.ToDateTime(((DateTime)vacationRequestViewModel.vacation_from).ToShortDateString()).Date.Subtract(Convert.ToDateTime(DateTime.Now.ToShortDateString())).TotalDays;
                                if (Days < (int)selectedVacation.inform_before_duration)
                                {
                                    requestStatus = false;
                                    errorReport += "Must Inform " + selectedVacation.inform_before_duration.ToString() + "Days Before<br>";
                                }
                            }
                            else
                            {
                                //double Hours = (((DateTime)vacationRequestViewModel.vacation_from - DateTime.Now).TotalHours);
                                double Hours = Convert.ToDateTime(((DateTime)vacationRequestViewModel.vacation_from).ToShortDateString()).Date.Subtract(Convert.ToDateTime(DateTime.Now.ToShortDateString())).TotalHours;
                                if (Hours < (int)selectedVacation.inform_before_duration)
                                {
                                    requestStatus = false;
                                    errorReport += "Must Inform " + selectedVacation.inform_before_duration.ToString() + "Hours Before<br>";
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
                                if (isA.TechnicalManager())
                                    vacationRequest.status = (int)ApprovementStatus.ApprovedByTechnicalManager;
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
                            if (selectedVacation.inform_before_duration_measurement == 1)
                            {
                                //int Days = ((DateTime)vacationRequestViewModel.vacation_from - DateTime.Now).Days + 1;
                                double Days = Convert.ToDateTime(((DateTime)vacationRequestViewModel.vacation_from).ToShortDateString()).Date.Subtract(Convert.ToDateTime(DateTime.Now.ToShortDateString())).TotalDays;
                                if (Days < (int)selectedVacation.inform_before_duration)
                                {
                                    requestStatus = false;
                                    errorReport += "Must Inform " + selectedVacation.inform_before_duration.ToString() + "Days Before<br>";
                                }
                            }
                            else
                            {
                                //double Hours = (((DateTime)vacationRequestViewModel.vacation_from - DateTime.Now).TotalHours);
                                double Hours = Convert.ToDateTime(((DateTime)vacationRequestViewModel.vacation_from).ToShortDateString()).Date.Subtract(Convert.ToDateTime(DateTime.Now.ToShortDateString())).TotalHours;
                                if (Hours < (int)selectedVacation.inform_before_duration)
                                {
                                    requestStatus = false;
                                    errorReport += "Must Inform " + selectedVacation.inform_before_duration.ToString() + "Hours Before<br>";
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
                    return Json(new { message = "Max: " + userVacationYear.vacation_balance + "<br> Remaining: " + actualRemaining, success = false }, JsonRequestBehavior.AllowGet);
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
            if (!(isA.Employee() || isA.TeamLeader() || isA.BranchAdmin() || isA.TechnicalManager()))
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
                || isA.TechnicalManager()))
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
                var productitvityData = (from vacationRequest in db.VacationRequests
                                         join vacationType in db.VacationTypes on vacationRequest.vacation_type_id equals vacationType.id
                                         join user in db.Users on vacationRequest.user_id equals user.id
                                         join superAd in db.Users on vacationRequest.approved_by_super_admin equals superAd.id into sa
                                         from superAdmin in sa.DefaultIfEmpty()
                                         join branchAd in db.Users on vacationRequest.approved_by_branch_admin equals branchAd.id into ba
                                         from branchAdmin in ba.DefaultIfEmpty()
                                         join teamLead in db.Users on vacationRequest.approved_by_team_leader equals teamLead.id into tl
                                         from teamLeader in tl.DefaultIfEmpty()
                                         join techManager in db.Users on vacationRequest.approved_by_technical_manager equals techManager.id into tm
                                         from technicalManager in tm.DefaultIfEmpty()
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
                                             approved_by_technical_manager_name = technicalManager.full_name,
                                             approved_by_super_admin_at = vacationRequest.approved_by_super_admin_at,
                                             approved_by_branch_admin_at = vacationRequest.approved_by_branch_admin_at,
                                             approved_by_team_leader_at = vacationRequest.approved_by_team_leader_at,
                                             approved_by_technical_manager_at = vacationRequest.approved_by_technical_manager_at,
                                         }).Where(n => n.active == (int)RowStatus.ACTIVE);

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    productitvityData = productitvityData.Where(m => m.vacation_name.ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()));
                }

                if (isA.SuperAdmin())
                {
                    productitvityData = productitvityData.Where(p => p.user_id != currentUser.id && p.status == (int)ApprovementStatus.ApprovedByBranchAdmin && (p.user_type == (int)UserRole.Employee || p.user_type == (int)UserRole.TeamLeader || p.user_type == (int)UserRole.TechnicalManager || p.user_type == (int)UserRole.BranchAdmin));
                    if (branch_id != null)
                    {
                        productitvityData = productitvityData.Where(p => p.branch_id == branch_id);
                    }
                }

                if (isA.BranchAdmin())
                {
                    productitvityData = productitvityData.Where(p => p.branch_id == currentUser.branch_id && p.status == (int)ApprovementStatus.ApprovedByTechnicalManager && p.user_id != currentUser.id && (p.user_type == (int)UserRole.Employee || p.user_type == (int)UserRole.TeamLeader || p.user_type == (int)UserRole.TechnicalManager));

                }

                if (isA.TeamLeader())
                {
                    productitvityData = productitvityData.Where(p => p.team_leader_id == currentUser.id && p.status == (int)ApprovementStatus.PendingApprove && p.user_id != currentUser.id && p.user_type == (int)UserRole.Employee);

                }

                if (isA.TechnicalManager())
                {
                    productitvityData = productitvityData.Where(p => p.branch_id == currentUser.branch_id && p.status == (int)ApprovementStatus.ApprovedByTeamLeader && p.user_id != currentUser.id && (p.user_type == (int)UserRole.Employee || p.user_type == (int)UserRole.TeamLeader));

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

            if (isA.TechnicalManager())
            {
                vacationRequest.approved_by_team_leader = Session["id"].ToString().ToInt();
                vacationRequest.approved_by_team_leader_at = DateTime.Now;
                vacationRequest.status = (int)ApprovementStatus.ApprovedByTechnicalManager;
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
    }
}