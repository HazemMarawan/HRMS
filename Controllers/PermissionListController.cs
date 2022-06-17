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
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace HRMS.Controllers
{
    [CustomAuthenticationFilter]
    public class PermissionListController : BaseController
    {
        HRMSDBContext db = new HRMSDBContext();

        // GET: PermissionList
        public ActionResult Index(int? branch_id)
        {
            User currentUser = Session["user"] as User;
            if (!(isA.SuperAdmin() || isA.TeamLeader() || isA.Supervisor() || (isA.BranchAdmin() && (currentUser.branch_id == branch_id || branch_id==null))))
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
                var permissionData = (from perReq in db.WorkPermissionRequests
                                      join user in db.Users on perReq.user_id equals user.id

                                      join team_leader_approve in db.Users on perReq.approved_by_team_leader equals team_leader_approve.id into tla
                                      from team_leader_approved in tla.DefaultIfEmpty()

                                      join supervisor_approve in db.Users on perReq.approved_by_supervisor equals supervisor_approve.id into tecm
                                      from supervisor_approved in tecm.DefaultIfEmpty()

                                      join branch_admin_approve in db.Users on perReq.approved_by_branch_admin equals branch_admin_approve.id into baa
                                      from branch_admin_approved in baa.DefaultIfEmpty()
                                     
                                      join super_admin_approve in db.Users on perReq.approved_by_super_admin equals super_admin_approve.id into sua
                                      from super_admin_approved in sua.DefaultIfEmpty()

                                      join rejected in db.Users on perReq.rejected_by equals rejected.id into re
                                      from rejected_by in re.DefaultIfEmpty()

                                      select new WorkPermissionRequestViewModel
                                      {
                                          id = perReq.id,
                                          user_id = perReq.user_id,
                                          month = perReq.month,
                                          year = perReq.year,
                                          date = perReq.date,
                                          minutes = perReq.minutes,
                                          reason = perReq.reason,
                                          active = perReq.active,
                                          status = perReq.status,
                                          from_time = perReq.from_time,
                                          approved_by_super_admin = perReq.approved_by_super_admin,
                                          approved_by_super_admin_at = perReq.approved_by_super_admin_at,
                                          approved_by_branch_admin = perReq.approved_by_branch_admin,
                                          approved_by_branch_admin_at = perReq.approved_by_branch_admin_at,
                                          approved_by_supervisor = perReq.approved_by_supervisor,
                                          approved_by_supervisor_at = perReq.approved_by_supervisor_at,
                                          approved_by_team_leader = perReq.approved_by_team_leader,
                                          approved_by_team_leader_at = perReq.approved_by_team_leader_at,
                                          rejected_by_at = perReq.rejected_by_at,
                                          created_at = perReq.created_at,
                                          full_name = user.full_name,
                                          branch_id = user.branch_id,
                                          type = user.type,
                                          team_leader_id = user.team_leader_id,
                                          permission_count = db.WorkPermissionRequests.Where(wo => wo.year == perReq.year && wo.month == perReq.month && wo.status == (int)ApprovementStatus.ApprovedBySuperAdmin && wo.user_id == perReq.user_id).Count(),
                                          team_leader_name = team_leader_approved.full_name,
                                          supervisor_name = supervisor_approved.full_name,
                                          branch_admin_name = branch_admin_approved.full_name,
                                          super_admin_name = super_admin_approved.full_name,
                                          rejected_by_name = rejected_by.full_name,
                                          
                                      }).Where(n => n.active == (int)RowStatus.ACTIVE);

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    permissionData = permissionData.Where(m => m.full_name.ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()));
                }
                if (isA.TeamLeader())
                {
                    permissionData = permissionData.Where(t => t.team_leader_id == currentUser.id && t.type == (int)UserRole.Employee && t.user_id != currentUser.id && t.status == (int)ApprovementStatus.PendingApprove);
                }
                else if (isA.Supervisor())
                {
                    permissionData = permissionData.Where(t => t.branch_id == currentUser.branch_id && (t.type == (int)UserRole.Employee || t.type == (int)UserRole.TeamLeader) && t.user_id != currentUser.id && t.status == (int)ApprovementStatus.ApprovedByTeamLeader);
                }
                else if (isA.BranchAdmin())
                {
                    permissionData = permissionData.Where(t => t.branch_id == currentUser.branch_id && (t.type == (int)UserRole.Employee || t.type == (int)UserRole.TeamLeader || t.type == (int)UserRole.Supervisor) && t.user_id != currentUser.id);
                    //permissionData = permissionData.Where(t => t.branch_id == currentUser.branch_id && (t.type == (int)UserRole.Employee || t.type == (int)UserRole.TeamLeader || t.type == (int)UserRole.Supervisor) && t.user_id != currentUser.id && t.status == (int)ApprovementStatus.ApprovedBySupervisor);
                } 
                else if(isA.SuperAdmin())
                {
                    //permissionData = permissionData.Where(t => t.status == (int)ApprovementStatus.ApprovedByBranchAdmin);
                    if(branch_id != null)
                    {
                        permissionData = permissionData.Where(t => t.branch_id == branch_id);
                    }
                }
                else
                {
                    permissionData = permissionData.Where(t => t.id == -1);
                }

                int? totalPermissions = 0;
                int? approvedPermissions = 0;
                int? unCompletedPermissions = 0;
                int? rejectedPermissions = 0;

                totalPermissions = permissionData.ToList().Count();
                approvedPermissions = permissionData.Select(c => c.status == (int?)ApprovementStatus.ApprovedBySuperAdmin).ToList().Count();
                rejectedPermissions = permissionData.Select(c => c.status == (int?)ApprovementStatus.Rejected).ToList().Count();
                unCompletedPermissions = totalPermissions - approvedPermissions - rejectedPermissions;

                //total number of rows count     
                var displayResult = permissionData.OrderBy(u => u.status).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = permissionData.Count();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = displayResult,
                    totalPermissions = totalPermissions,
                    approvedPermissions = approvedPermissions,
                    rejectedPermissions = rejectedPermissions,
                    unCompletedPermissions = unCompletedPermissions
                }, JsonRequestBehavior.AllowGet);

            }
            ViewBag.years = db.WorkPermissionRequests.Select(y => new { id = y.id, year = y.year }).GroupBy(w => w.year).ToList();
            ViewBag.months = db.WorkPermissionRequests.Select(y => new { id = y.id, month = y.month }).GroupBy(w => w.month).ToList();
            if (isA.BranchAdmin() || isA.TeamLeader() || isA.Supervisor())
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

        public JsonResult actionToPermission(int id, int status)
        {
            User currentUser = Session["user"] as User;

            WorkPermissionRequest workPermissionRequest = db.WorkPermissionRequests.Find(id);
            if(workPermissionRequest != null)
            {
                //accepted
                if (status == 1)
                {
                    
                    if (isA.TeamLeader())
                    {
                        workPermissionRequest.status = (int)ApprovementStatus.ApprovedByTeamLeader;
                        workPermissionRequest.approved_by_team_leader = currentUser.id;
                        workPermissionRequest.approved_by_team_leader_at = DateTime.Now;
                    }
                    else if (isA.Supervisor())
                    {
                        workPermissionRequest.status = (int)ApprovementStatus.ApprovedBySupervisor;
                        workPermissionRequest.approved_by_supervisor = currentUser.id;
                        workPermissionRequest.approved_by_supervisor_at = DateTime.Now;
                    }
                    else if (isA.BranchAdmin())
                    {
                        workPermissionRequest.status = (int)ApprovementStatus.ApprovedByBranchAdmin;
                        workPermissionRequest.approved_by_branch_admin = currentUser.id;
                        workPermissionRequest.approved_by_branch_admin_at = DateTime.Now;
                    }
                    else if(isA.SuperAdmin())
                    {
                        workPermissionRequest.status = (int)ApprovementStatus.ApprovedBySuperAdmin;
                        workPermissionRequest.approved_by_super_admin = currentUser.id;
                        workPermissionRequest.approved_by_super_admin_at = DateTime.Now;
                    }
                    db.SaveChanges();

                    WorkPermissionMonthYear workPermissionMonthYear = db.WorkPermissionMonthYears.Where(w => w.year == workPermissionRequest.year && w.month == workPermissionRequest.month).FirstOrDefault();
                    if (workPermissionMonthYear == null)
                    {
                        workPermissionMonthYear = new WorkPermissionMonthYear();
                        workPermissionMonthYear.month = workPermissionRequest.month;
                        workPermissionMonthYear.year = workPermissionRequest.year;
                        workPermissionMonthYear.user_id = workPermissionRequest.user_id;
                        workPermissionMonthYear.permission_count = 1;
                        workPermissionMonthYear.active = (int?)RowStatus.ACTIVE;
                        workPermissionMonthYear.created_at = DateTime.Now;
                        workPermissionMonthYear.created_by = Session["id"].ToString().ToInt();
                        db.WorkPermissionMonthYears.Add(workPermissionMonthYear);

                    }
                    else
                    {
                        workPermissionMonthYear.permission_count += 1;
                        workPermissionMonthYear.active = (int?)RowStatus.ACTIVE;
                        workPermissionMonthYear.updated_at = DateTime.Now;
                        workPermissionMonthYear.updated_by = Session["id"].ToString().ToInt();
                    }

                    db.SaveChanges();
                }
                else
                {
                    workPermissionRequest.status = (int?)ApprovementStatus.Rejected;
                    workPermissionRequest.rejected_by = currentUser.id;
                    workPermissionRequest.rejected_by_at = DateTime.Now;
                    db.SaveChanges();
                }
            }
            
            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }


        public void PermissionsSheet(int month)
        {
            User currentUser = Session["user"] as User;

            ExcelPackage Ep = new ExcelPackage();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Permissions Report for " + month + "-" + DateTime.Now.Year);

            System.Drawing.Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#000000");
            System.Drawing.Color redColor = System.Drawing.ColorTranslator.FromHtml("#FF0000");
            System.Drawing.Color warningColor = System.Drawing.ColorTranslator.FromHtml("#FFA000");
            System.Drawing.Color greenColor = System.Drawing.ColorTranslator.FromHtml("#00FF00");
            Sheet.Cells["A1:E1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells["A1:E1"].Style.Fill.BackgroundColor.SetColor(colFromHex);
            System.Drawing.Color text = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            Sheet.Cells["A1:E1"].Style.Font.Color.SetColor(text);

            Sheet.Cells["A1"].Value = "اسم الموظف";
            Sheet.Cells["B1"].Value = "القسم";
            Sheet.Cells["C1"].Value = "الوظيفة";
            Sheet.Cells["D1"].Value = "عدد الاذونات";
            Sheet.Cells["E1"].Value = "ساعات الاذونات";
        
           

            List<int?> userPermission = db.WorkPermissionRequests.Where(th => th.month == month && th.status == (int)ApprovementStatus.ApprovedBySuperAdmin ).Select(s => s.user_id).ToList();

            var userData = (from  user in db.Users
                            join branch in db.Branches on user.branch_id equals branch.id
                            join dep in db.Departments on user.department_id equals dep.id into d
                            from department in d.DefaultIfEmpty()
                            join jo in db.Departments on user.department_id equals jo.id into j
                            from job in j.DefaultIfEmpty()
                            //group per by per.month into permissionGroup
                            //group user by user.id into userGroup
                            select new UserViewModel
                            {
                                total_permissions = db.WorkPermissionRequests.Where(th => th.month == month && th.user_id == user.id && th.status == (int)ApprovementStatus.ApprovedBySuperAdmin).Count(),
                                total_hours = db.WorkPermissionRequests.Where(th => th.month == month && th.user_id == user.id && th.status == (int)ApprovementStatus.ApprovedBySuperAdmin).Select(s => s.minutes).Sum()/60.0,
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
                            }).Where(s => s.active == (int)RowStatus.ACTIVE && userPermission.Contains(s.id) &&
                            (s.type == (int)UserRole.Employee || s.type == (int)UserRole.TeamLeader || s.type == (int)UserRole.Supervisor || s.type == (int)UserRole.BranchAdmin));
            
            if(!isA.SuperAdmin())
            {
                userData = userData.Where(s => s.branch_id == currentUser.branch_id);
            }
            List<UserViewModel> employees = userData.ToList();

            int row = 2;
            foreach (var item in employees)
            {

                Sheet.Cells[string.Format("A{0}", row)].Value = item.full_name;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.department_name;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.job_name;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.total_permissions;
                Sheet.Cells[string.Format("E{0}", row)].Value = Math.Round((double)item.total_hours,2);

                row++;
            }

            row++;

            Sheet.Cells["A:AZ"].AutoFitColumns();

            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + DateTime.Now.ToString() + "_Permission_Report.xlsx");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();
        }

    }
}