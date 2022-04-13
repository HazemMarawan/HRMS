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

namespace HRMS.Controllers
{
    [CustomAuthenticationFilter]
    public class ProductivityController : BaseController
    {
        HRMSDBContext db = new HRMSDBContext();
        // GET: Productivity
        public ActionResult Index(int? branch_id)
        {
            User currentUser = Session["user"] as User;
            if (!(isA.SuperAdmin() 
                || isA.TeamLeader() 
                || (isA.BranchAdmin() && (currentUser.branch_id == branch_id || branch_id == null))
                || isA.TechnicalManager()
                || isA.ProjectManager()))
                return RedirectToAction("Index", "Dashboard");

            if (Request.IsAjaxRequest())
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();
                var search_project_id = Request.Form.GetValues("columns[0][search][value]")[0];
                var search_area_id = Request.Form.GetValues("columns[1][search][value]")[0];
                var search_productivity_type = Request.Form.GetValues("columns[2][search][value]")[0];
                var search_work_place = Request.Form.GetValues("columns[3][search][value]")[0];
                var from_date = Request.Form.GetValues("columns[4][search][value]")[0];
                var to_date = Request.Form.GetValues("columns[5][search][value]")[0];
                var task_id = Request.Form.GetValues("columns[6][search][value]")[0];
                var part_id = Request.Form.GetValues("columns[7][search][value]")[0];
                var search_branch_id = Request.Form.GetValues("columns[8][search][value]")[0];
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

                // Getting all data    
                var productivityData = (from user in db.Users
                                        join userProject in db.UserProjects on user.id equals userProject.user_id
                                        join pro in db.Projects on userProject.project_id equals pro.id into pr
                                        from project in pr.DefaultIfEmpty()
                                        join are in db.Areas on userProject.area_id equals are.id into ar
                                        from area in ar.DefaultIfEmpty()
                                        join ret in db.Users on userProject.returned_by equals ret.id into r
                                        from returned in r.DefaultIfEmpty()
                                        join par in db.Parts on userProject.part_id_fk equals par.id into pa
                                        from part in pa.DefaultIfEmpty()
                                        join tas in db.Tasks on userProject.task_id equals tas.id into ts
                                        from task in ts.DefaultIfEmpty()
                                        select new UserProjectViewModel
                                        {
                                            id = userProject.id,
                                            project_id = userProject.project_id,
                                            user_id = userProject.user_id,
                                            project_name = project.name,
                                            user_name = user.full_name,
                                            type = user.type,
                                            working_date = userProject.working_date,
                                            no_of_numbers = userProject.no_of_numbers,
                                            branch_id = user.branch_id,
                                            productivity_type = userProject.productivity_type,
                                            productivity_work_place = userProject.productivity_work_place,
                                            part_id = userProject.part_id,
                                            part_id_fk = userProject.part_id_fk,
                                            part_name = part.part,
                                            equipment_quantity = userProject.equipment_quantity,
                                            mvoh = userProject.mvoh,
                                            lvoh = userProject.lvoh,
                                            mvug = userProject.mvug,
                                            lvug = userProject.lvug,
                                            mvoh_target = userProject.mvoh_target,
                                            lvoh_target = userProject.lvoh_target,
                                            mvug_target = userProject.mvug_target,
                                            lvug_target = userProject.lvug_target,
                                            substation = userProject.substation,
                                            note = userProject.note,
                                            status = userProject.status,
                                            cost = userProject.cost,
                                            team_leader_id = user.team_leader_id,
                                            area_id = userProject.area_id,
                                            area_name = area.name,
                                            task_id = task.id,
                                            task_name = task.name,
                                            returned_by_name = returned.full_name,
                                            returned_at = userProject.returned_at,
                                            rejected_by_note = userProject.returned_by_note
                                        });

               
                if(HRMS.Auth.isA.TeamLeader())
                {
                    productivityData = productivityData.Where(p => p.team_leader_id == currentUser.id && p.user_id != currentUser.id && p.branch_id == currentUser.branch_id && p.type == (int)UserRole.Employee);
                }

                if (HRMS.Auth.isA.SuperAdmin())
                {
                    productivityData = productivityData.Where(p => p.user_id != currentUser.id && (p.type == (int)UserRole.Employee || p.type == (int)UserRole.TeamLeader || p.type == (int)UserRole.BranchAdmin));
                    if (branch_id != null)
                    {
                        productivityData = productivityData.Where(p => p.branch_id == branch_id);
                    }
                }

                if (HRMS.Auth.isA.BranchAdmin())
                {
                    productivityData = productivityData.Where(p => p.branch_id == currentUser.branch_id && p.user_id != currentUser.id && (p.type == (int)UserRole.Employee || p.type == (int)UserRole.TeamLeader));
                }

                if (HRMS.Auth.isA.TechnicalManager())
                {
                    productivityData = productivityData.Where(p => p.branch_id == currentUser.branch_id && p.user_id != currentUser.id && (p.type == (int)UserRole.Employee || p.type == (int)UserRole.TeamLeader));
                }

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    productivityData = productivityData.Where(
                       m => m.project_name.ToLower().Contains(searchValue.ToLower())
                    || m.id.ToString().ToLower().Contains(searchValue.ToLower())
                    || m.user_name.ToString().ToLower().Contains(searchValue.ToLower())
                    );
                }

                if (!string.IsNullOrEmpty(search_project_id))
                {
                    int search_project_id_int = int.Parse(search_project_id);
                    productivityData = productivityData.Where(s => s.project_id == search_project_id_int);
                }

                if (!string.IsNullOrEmpty(search_area_id))
                {
                    int search_area_id_int = int.Parse(search_area_id);
                    productivityData = productivityData.Where(s => s.area_id == search_area_id_int);
                }

                if (!string.IsNullOrEmpty(search_productivity_type))
                {
                    int search_productivity_type_int = int.Parse(search_productivity_type);
                    productivityData = productivityData.Where(s => s.productivity_type == search_productivity_type_int);
                }

                if (!string.IsNullOrEmpty(search_work_place))
                {
                    int search_work_place_int = int.Parse(search_work_place);
                    productivityData = productivityData.Where(s => s.productivity_work_place == search_work_place_int);
                }

                if (!string.IsNullOrEmpty(from_date))
                {
                    if (Convert.ToDateTime(from_date) != DateTime.MinValue)
                    {
                        DateTime from = Convert.ToDateTime(from_date);
                        productivityData = productivityData.Where(s => s.working_date >= from);
                    }
                }

                if (!string.IsNullOrEmpty(to_date))
                {
                    if (Convert.ToDateTime(to_date) != DateTime.MinValue)
                    {
                        DateTime to = Convert.ToDateTime(to_date);
                        productivityData = productivityData.Where(s => s.working_date <= to);
                    }
                }

                if (!string.IsNullOrEmpty(task_id))
                {
                    int task_id_int = int.Parse(task_id);
                    productivityData = productivityData.Where(s => s.task_id == task_id_int);
                }

                if (!string.IsNullOrEmpty(part_id))
                {
                    int part_id_int = int.Parse(part_id);
                    productivityData = productivityData.Where(s => s.part_id == part_id || s.part_id_fk == part_id_int);
                }

                if (!string.IsNullOrEmpty(search_branch_id))
                {
                    int search_branch_id_int = int.Parse(search_branch_id);
                    productivityData = productivityData.Where(s => s.branch_id == search_branch_id_int);
                }
                
                //var clonedProductivityData = productivityData.ToList();
                int? Hours = 0;
                int? Projects = 0;
                int? Employees = 0;
                double? Cost = 0;
                double? MVOH = 0;
                double? LVOH = 0;
                double? MVUG = 0;
                double? LVUG = 0;
                double? EquipmentQuantity = 0;
                double? Substation = 0;

                Hours = productivityData.Select(c => c.no_of_numbers).ToList().Sum();
                Projects = productivityData.Select(c => c.project_id).Distinct().ToList().Count();
                Employees = productivityData.Select(c => c.user_id).Distinct().ToList().Count();
                Cost = productivityData.Select(c => c.cost).ToList().Sum();

                MVOH = productivityData.Select(c => c.mvoh).ToList().Sum();
                LVOH = productivityData.Select(c => c.lvoh).ToList().Sum();
                MVUG = productivityData.Select(c => c.mvug).ToList().Sum();
                LVUG = productivityData.Select(c => c.lvug).ToList().Sum();
                EquipmentQuantity = productivityData.Select(c => c.equipment_quantity).ToList().Sum();
                Substation = productivityData.Select(c => c.substation).ToList().Sum();
                //Sorting    
                if ((!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir)))
                {
                    productivityData = productivityData.OrderBy(sortColumn + " " + sortColumnDir);
                }

                //total number of rows count     
                var displayResult = productivityData.Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = productivityData.Count();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = displayResult,
                    Hours = Hours,
                    Projects = Projects,
                    Employees = Employees,
                    Cost = Cost,
                    MVOH = MVOH,
                    LVOH = LVOH,
                    MVUG = MVUG,
                    LVUG = LVUG,
                    EquipmentQuantity = EquipmentQuantity,
                    Substation = Substation

                }, JsonRequestBehavior.AllowGet);

            }

            if (isA.BranchAdmin() || isA.TeamLeader() || isA.TechnicalManager())
            {
                branch_id = currentUser.branch_id;
            }
            ViewBag.branchId = branch_id;
            if (branch_id != null)
            {
                ViewBag.branchName = db.Branches.Where(b => b.id == branch_id).FirstOrDefault().name;
                List<int?> branchProjects = db.BranchProjects.Where(b => b.branch_id == branch_id).Select(b => b.project_id).ToList();
                ViewBag.Projects = db.Projects.Where(p => branchProjects.Contains(p.id)).Select(p=>new { p.id,p.name}).ToList();
            }
            else
            {
                ViewBag.branchName = "Company";
                ViewBag.Projects = db.Projects.Where(p => p.active == (int)RowStatus.ACTIVE).Select(p => new { p.id, p.name }).ToList();
                ViewBag.Branches = db.Branches.Where(b=>b.active == (int)RowStatus.ACTIVE).Select(p => new { p.id, p.name }).ToList();
            }
            ViewBag.Tasks = db.Tasks.Where(t => t.active == (int)RowStatus.ACTIVE).Select(t => new { t.id, t.name }).ToList();
            return View();
        }

        public ActionResult Missing(int? branch_id)
        {
            User currentUser = Session["user"] as User;
            if (!(isA.SuperAdmin() 
                || isA.TeamLeader() 
                || (isA.BranchAdmin() && (currentUser.branch_id == branch_id || branch_id == null))
                || isA.TechnicalManager()))
                return RedirectToAction("Index", "Dashboard");

            if (Request.IsAjaxRequest())
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();
                var search_date = Request.Form.GetValues("columns[0][search][value]")[0];
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                // Getting all data    
                var productivityData = (from user in db.Users
                                        join idtype in db.IDTypes on user.id_type equals idtype.id
                                        join nationality in db.Nationalities on user.nationality_id equals nationality.id
                                        join branch in db.Branches on user.branch_id equals branch.id
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
                                            active = user.active,
                                        }).Where(u=>u.active == (int)RowStatus.ACTIVE);

                if (!string.IsNullOrEmpty(searchValue))
                {
                    productivityData = productivityData.Where(m=>m.id.ToString().ToLower().Contains(searchValue.ToLower())
                    || m.user_name.ToString().ToLower().Contains(searchValue.ToLower())
                    );
                }

                if (!string.IsNullOrEmpty(search_date))
                {
                    if (Convert.ToDateTime(search_date) != DateTime.MinValue)
                    {
                        DateTime date = Convert.ToDateTime(search_date);
                        List<int?> missingProductivityUsers = db.UserProjects.Where(us => ((DateTime)us.working_date).Year == date.Year && ((DateTime)us.working_date).Month == date.Month && ((DateTime)us.working_date).Day == date.Day).Select(us => us.user_id).ToList();
                        productivityData = productivityData.Where(s => !missingProductivityUsers.Contains(s.id) && s.required_productivity == 1);
                    }
                }
                else
                {
                    productivityData = productivityData.Where(s => s.id == -1);
                }

                if(isA.SuperAdmin())
                {
                    productivityData = productivityData.Where(s => s.id != currentUser.id && (s.type == (int)UserRole.Employee || s.type == (int)UserRole.TeamLeader || s.type == (int)UserRole.TechnicalManager));
                    if (branch_id != null)
                    {
                        productivityData = productivityData.Where(s => s.branch_id == branch_id);
                    }
                }

                if(isA.BranchAdmin())
                {
                    productivityData = productivityData.Where(s => s.branch_id == currentUser.branch_id && s.id != currentUser.id && (s.type == (int)UserRole.Employee || s.type == (int)UserRole.TeamLeader || s.type == (int)UserRole.TechnicalManager));
                }

                if (isA.TeamLeader())
                {
                    productivityData = productivityData.Where(s => s.team_leader_id == currentUser.id && s.id != currentUser.id && s.type == (int)UserRole.Employee);
                }

                if (isA.TechnicalManager())
                {
                    productivityData = productivityData.Where(s => s.branch_id == currentUser.branch_id && s.id != currentUser.id && (s.type == (int)UserRole.Employee || s.type == (int)UserRole.TeamLeader));
                }

                //total number of rows count     
                var displayResult = productivityData.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = productivityData.Count();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = displayResult
                }, JsonRequestBehavior.AllowGet);

            }

            if (isA.BranchAdmin() || isA.TeamLeader() || isA.TechnicalManager())
            {
                branch_id = currentUser.branch_id; 
            }

            ViewBag.branchId = branch_id;

            if (branch_id != null)
            {
                ViewBag.branchName = db.Branches.Where(b => b.id == branch_id).FirstOrDefault().name;
                List<int?> branchProjects = db.BranchProjects.Where(b => b.branch_id == branch_id).Select(b => b.project_id).ToList();
                ViewBag.Projects = db.Projects.Where(p => branchProjects.Contains(p.id)).Select(p => new { p.id, p.name }).ToList();
            }
            else
            {
                ViewBag.branchName = "Company";
                ViewBag.Projects = db.Projects.Select(p => new { p.id, p.name }).ToList();
            }
            return View();
        }
        public ActionResult Employee()
        {
            if (!(isA.Employee() || isA.TeamLeader() || isA.TechnicalManager()))
                return RedirectToAction("Index", "Dashboard");

            User currentUser = Session["user"] as User;
            if (Request.IsAjaxRequest())
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();
                var from_date = Request.Form.GetValues("columns[0][search][value]")[0];
                var to_date = Request.Form.GetValues("columns[1][search][value]")[0];
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                // Getting all data    
                var productivityData = (
                                       from userProject in db.UserProjects
                                       join pro in db.Projects on userProject.project_id equals pro.id into pr
                                       from project in pr.DefaultIfEmpty()
                                       join are in db.Areas on userProject.area_id equals are.id into ar
                                       from area in ar.DefaultIfEmpty()
                                       join ret in db.Users on userProject.returned_by equals ret.id into r
                                       from returned in r.DefaultIfEmpty()
                                       join par in db.Parts on userProject.part_id_fk equals par.id into pa
                                       from part in pa.DefaultIfEmpty()
                                       join tas in db.Tasks on userProject.task_id equals tas.id into ts
                                       from task in ts.DefaultIfEmpty()
                                       select new UserProjectViewModel
                                       {
                                           id = userProject.id,
                                           user_id = userProject.user_id,
                                           project_id = userProject.project_id,
                                           project_name = project.name,
                                           working_date = userProject.working_date,
                                           no_of_numbers = userProject.no_of_numbers,
                                           productivity_type = userProject.productivity_type,
                                           productivity_work_place = userProject.productivity_work_place,
                                           part_id = userProject.part_id,
                                           part_id_fk = userProject.part_id_fk,
                                           part_name = part.part,
                                           task_id = task.id,
                                           task_name = task.name,
                                           equipment_quantity = userProject.equipment_quantity,
                                           mvoh = userProject.mvoh,
                                           lvoh = userProject.lvoh,
                                           mvug = userProject.mvug,
                                           lvug = userProject.lvug,
                                           mvoh_target = userProject.mvoh_target,
                                           lvoh_target = userProject.lvoh_target,
                                           mvug_target = userProject.mvug_target,
                                           lvug_target = userProject.lvug_target,
                                           substation = userProject.substation,
                                           note = userProject.note,
                                           status = userProject.status,
                                           area_id = userProject.area_id,
                                           area_name = area.name,
                                           returned_by_name = returned.full_name,
                                           returned_at = userProject.returned_at,
                                           rejected_by_note = userProject.returned_by_note
                                       }).Where(p => p.user_id == currentUser.id) ;

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    productivityData = productivityData.Where(m => m.project_name.ToLower().Contains(searchValue.ToLower())
                    || m.id.ToString().ToLower().Contains(searchValue.ToLower())
                    || m.user_name.ToString().ToLower().Contains(searchValue.ToLower())
                    );
                }

                if (!string.IsNullOrEmpty(from_date))
                {
                    if (Convert.ToDateTime(from_date) != DateTime.MinValue)
                    {
                        DateTime from = Convert.ToDateTime(from_date);
                        productivityData = productivityData.Where(s => s.working_date >= from);
                    }
                }

                if (!string.IsNullOrEmpty(to_date))
                {
                    if (Convert.ToDateTime(to_date) != DateTime.MinValue)
                    {
                        DateTime to = Convert.ToDateTime(to_date);
                        productivityData = productivityData.Where(s => s.working_date <= to);
                    }
                }

                //total number of rows count     
                var displayResult = productivityData.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = productivityData.Count();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = displayResult

                }, JsonRequestBehavior.AllowGet);

            }
            List<int?> branchProjects = db.BranchProjects.Where(b => b.branch_id == currentUser.branch_id).Select(b => b.project_id).ToList();
            ViewBag.Projects = db.Projects.Where(p => branchProjects.Contains(p.id));
            ViewBag.Parts = db.Parts.Where(p => p.active == (int)RowStatus.ACTIVE).Select(p => new { p.id, p.part }).ToList();
            ViewBag.Tasks = db.Tasks.Where(p => p.active == (int)RowStatus.ACTIVE).Select(p => new { p.id, p.name }).ToList();
            return View();
        }

        [HttpPost]
        public JsonResult saveProductivity(UserProjectViewModel userProjectViewModel)
        {
            User currentUser = Session["user"] as User;
            if (userProjectViewModel.id == 0)
            {
                TargetViewModel targetViewModel = db.Targets.Select(t => new TargetViewModel
                {
                    mvoh = t.mvoh,
                    lvoh = t.lvoh,
                    mvug = t.mvug,
                    lvug = t.lvug,
                    active = t.active
                }).Where(t=>t.active == (int)RowStatus.ACTIVE).FirstOrDefault();

                UserProject userProject = AutoMapper.Mapper.Map<UserProjectViewModel, UserProject>(userProjectViewModel);
                if(targetViewModel != null)
                {
                    userProject.mvoh_target = targetViewModel.mvoh;
                    userProject.lvoh_target = targetViewModel.lvoh;
                    userProject.mvug_target = targetViewModel.mvug;
                    userProject.lvug_target = targetViewModel.lvug;
                }
                else
                {
                    userProject.mvoh_target = 0;
                    userProject.lvoh_target = 0;
                    userProject.mvug_target = 0;
                    userProject.lvug_target = 0;
                }
                userProject.user_id = currentUser.id;
                userProject.created_at = DateTime.Now;
                userProject.created_by = Session["id"].ToString().ToInt();
                if(isA.TechnicalManager())
                    userProject.status = (int)ProductivityStatus.Approved;
                else
                    userProject.status = (int)ProductivityStatus.PendingApprove;

                db.UserProjects.Add(userProject);
                db.SaveChanges();
            }
            else
            {

                UserProject oldUserProject = db.UserProjects.Find(userProjectViewModel.id);

                oldUserProject.project_id = userProjectViewModel.project_id;
                oldUserProject.area_id = userProjectViewModel.area_id;
                oldUserProject.working_date = userProjectViewModel.working_date;
                oldUserProject.no_of_numbers = userProjectViewModel.no_of_numbers;
                oldUserProject.productivity_type = userProjectViewModel.productivity_type;
                oldUserProject.productivity_work_place = userProjectViewModel.productivity_work_place;
                oldUserProject.part_id = userProjectViewModel.part_id;
                oldUserProject.task_id = userProjectViewModel.task_id;
                oldUserProject.equipment_quantity = userProjectViewModel.equipment_quantity;
                oldUserProject.mvoh = userProjectViewModel.mvoh;
                oldUserProject.lvoh = userProjectViewModel.lvoh;
                oldUserProject.mvug = userProjectViewModel.mvug;
                oldUserProject.lvug = userProjectViewModel.lvug;
                oldUserProject.substation = userProjectViewModel.substation;
                oldUserProject.note = userProjectViewModel.note;

                oldUserProject.updated_by = Session["id"].ToString().ToInt();
                oldUserProject.updated_at = DateTime.Now;

                db.SaveChanges();
            }

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult deleteProductivity(int id)
        {
            UserProject deleteUserProject = db.UserProjects.Find(id);
            db.UserProjects.Remove(deleteUserProject);

            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult approveTask(int id)
        {
            
            UserProject approveUserProject = db.UserProjects.Find(id);

            UserViewModel currentUser = db.Users.Where(u=>u.id == approveUserProject.user_id).Select(u=>new UserViewModel { 
                last_hour_price = u.last_hour_price
            }).FirstOrDefault();

            approveUserProject.status = (int)ProductivityStatus.Approved;
            approveUserProject.approved_at = DateTime.Now;
            approveUserProject.approved_by = Session["id"].ToString().ToInt();
            if(approveUserProject.productivity_type == 1)
                approveUserProject.cost = currentUser.last_hour_price * approveUserProject.no_of_numbers;
            else
                approveUserProject.cost = currentUser.last_over_time_price * approveUserProject.no_of_numbers;

            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult rejectTask(int id)
        {
            UserProject approveUserProject = db.UserProjects.Find(id);
            approveUserProject.status = (int)ProductivityStatus.Rejected;
            approveUserProject.rejected_at = DateTime.Now;
            approveUserProject.rejected_by = Session["id"].ToString().ToInt();

            db.SaveChanges();
            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        } 
        
        [HttpPost]
        public JsonResult returnTask(int id, string note)
        {
            UserProject approveUserProject = db.UserProjects.Find(id);
            approveUserProject.status = (int)ProductivityStatus.Returned;
            approveUserProject.returned_by_note = note;
            approveUserProject.returned_at = DateTime.Now;
            approveUserProject.returned_by = Session["id"].ToString().ToInt();

            db.SaveChanges();
            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public void GenerateProductivityReport(UserProjectViewModel userProjectViewModel)
        {
            User currentUser = Session["user"] as User;

            ExcelPackage Ep = new ExcelPackage();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Productivity Report");

            System.Drawing.Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#000000");
            System.Drawing.Color redColor = System.Drawing.ColorTranslator.FromHtml("#FF0000");
            System.Drawing.Color warningColor = System.Drawing.ColorTranslator.FromHtml("#FFA000");
            System.Drawing.Color greenColor = System.Drawing.ColorTranslator.FromHtml("#00FF00");
            Sheet.Cells["A1:N1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells["A1:N1"].Style.Fill.BackgroundColor.SetColor(colFromHex);
            System.Drawing.Color text = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            Sheet.Cells["A1:N1"].Style.Font.Color.SetColor(text);

            Sheet.Cells["A1"].Value = "Employee Name";
            Sheet.Cells["B1"].Value = "Working Date";
            Sheet.Cells["C1"].Value = "Project";
            Sheet.Cells["D1"].Value = "Area";
            Sheet.Cells["E1"].Value = "Hours";
            Sheet.Cells["F1"].Value = "Sub Station";
            Sheet.Cells["G1"].Value = "Productivity Type";
            Sheet.Cells["H1"].Value = "Part ID";
            Sheet.Cells["I1"].Value = "Equipment Quantity";
            Sheet.Cells["J1"].Value = "MVOH";
            Sheet.Cells["K1"].Value = "LVOH";
            Sheet.Cells["L1"].Value = "MVUG";
            Sheet.Cells["M1"].Value = "LVUG";
            Sheet.Cells["N1"].Value = "Status";

            var productivityData = (from user in db.Users
                                    join userProject in db.UserProjects on user.id equals userProject.user_id
                                    join pro in db.Projects on userProject.project_id equals pro.id into pr
                                    from project in pr.DefaultIfEmpty()
                                    join are in db.Areas on userProject.area_id equals are.id into ar
                                    from area in ar.DefaultIfEmpty()
                                    join par in db.Parts on userProject.part_id_fk equals par.id into pa
                                    from part in pa.DefaultIfEmpty()
                                    select new UserProjectViewModel
                                    {
                                        id = userProject.id,
                                        project_id = userProject.project_id,
                                        user_id = userProject.user_id,
                                        project_name = project.name,
                                        user_name = user.full_name,
                                        working_date = userProject.working_date,
                                        no_of_numbers = userProject.no_of_numbers,
                                        branch_id = user.branch_id,
                                        productivity_type = userProject.productivity_type,
                                        productivity_work_place = userProject.productivity_work_place,
                                        part_id = userProject.part_id,
                                        part_id_fk = userProject.part_id_fk,
                                        part_name = part.part,
                                        substation =userProject.substation,
                                        equipment_quantity = userProject.equipment_quantity,
                                        mvoh = userProject.mvoh,
                                        lvoh = userProject.lvoh,
                                        mvug = userProject.mvug,
                                        lvug = userProject.lvug,
                                        mvoh_target = userProject.mvoh_target,
                                        lvoh_target = userProject.lvoh_target,
                                        mvug_target = userProject.mvug_target,
                                        lvug_target = userProject.lvug_target,
                                        note = userProject.note,
                                        status = userProject.status,
                                        cost = userProject.cost,
                                        team_leader_id = user.team_leader_id,
                                        area_id = userProject.area_id,
                                        area_name = area.name,
                                        type = user.type
                                    });



            if (HRMS.Auth.isA.TeamLeader())
            {
                productivityData = productivityData.Where(p => p.team_leader_id == currentUser.id && p.user_id != currentUser.id && p.branch_id == currentUser.branch_id && p.type == (int)UserRole.Employee);
            }

            if (HRMS.Auth.isA.SuperAdmin())
            {
                productivityData = productivityData.Where(p => p.user_id != currentUser.id && (p.type == (int)UserRole.Employee || p.type == (int)UserRole.TeamLeader || p.type == (int)UserRole.BranchAdmin));
                if (userProjectViewModel.branch_id != null)
                {
                    productivityData = productivityData.Where(p => p.branch_id == userProjectViewModel.branch_id);
                }
            }

            if (HRMS.Auth.isA.BranchAdmin())
            {
                productivityData = productivityData.Where(p => p.branch_id == currentUser.branch_id && p.user_id != currentUser.id && (p.type == (int)UserRole.Employee || p.type == (int)UserRole.TeamLeader));
            }

            if (HRMS.Auth.isA.TechnicalManager())
            {
                productivityData = productivityData.Where(p => p.branch_id == currentUser.branch_id && p.user_id != currentUser.id && (p.type == (int)UserRole.Employee || p.type == (int)UserRole.TeamLeader));
            }
            //Search    


            if (userProjectViewModel.project_id != null)
            {
                productivityData = productivityData.Where(s => s.project_id == userProjectViewModel.project_id);
            }

            if (userProjectViewModel.area_id != null)
            {
                productivityData = productivityData.Where(s => s.area_id == userProjectViewModel.area_id);
            }

            if (userProjectViewModel.productivity_type != null)
            {
                productivityData = productivityData.Where(s => s.productivity_type == userProjectViewModel.productivity_type);
            }

            if (userProjectViewModel.productivity_work_place != null)
            {
                productivityData = productivityData.Where(s => s.productivity_work_place == userProjectViewModel.productivity_work_place);
            }

            if (userProjectViewModel.from_date != null)
            {
                if (Convert.ToDateTime(userProjectViewModel.from_date) != DateTime.MinValue)
                {
                    DateTime from = Convert.ToDateTime(userProjectViewModel.from_date);
                    productivityData = productivityData.Where(s => s.working_date >= from);
                }
            }

            if (userProjectViewModel.to_date != null)
            {
                if (Convert.ToDateTime(userProjectViewModel.to_date) != DateTime.MinValue)
                {
                    DateTime to = Convert.ToDateTime(userProjectViewModel.to_date);
                    productivityData = productivityData.Where(s => s.working_date <= to);
                }
            }

            List<UserProjectViewModel> productivityResult = productivityData.OrderByDescending(s=>s.working_date).ToList();

            int row = 2;
            foreach (var item in productivityResult)
            {

                Sheet.Cells[string.Format("A{0}", row)].Value = item.user_name;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.working_date.ToString().Split(' ')[0];
                Sheet.Cells[string.Format("C{0}", row)].Value = item.project_name;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.area_name;
                Sheet.Cells[string.Format("E{0}", row)].Value = item.no_of_numbers;
                Sheet.Cells[string.Format("F{0}", row)].Value = item.substation;
                Sheet.Cells[string.Format("G{0}", row)].Value = item.productivity_type == 1?"Normal": "OverTime";
                Sheet.Cells[string.Format("H{0}", row)].Value = item.part_id;
                Sheet.Cells[string.Format("I{0}", row)].Value = item.equipment_quantity;

                if (item.mvoh == null || item.mvoh == 0)
                {
                    Sheet.Cells[string.Format("J{0}", row)].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Sheet.Cells[string.Format("J{0}", row)].Style.Fill.BackgroundColor.SetColor(warningColor);
                    Sheet.Cells[string.Format("J{0}", row)].Value = "-";
                }
                else if(item.mvoh_target == null || (item.mvoh_target != null && item.mvoh >= item.mvoh_target))
                {
                    Sheet.Cells[string.Format("J{0}", row)].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Sheet.Cells[string.Format("J{0}", row)].Style.Fill.BackgroundColor.SetColor(greenColor);
                    Sheet.Cells[string.Format("J{0}", row)].Value = item.mvoh;
                }
                else
                {
                    Sheet.Cells[string.Format("J{0}", row)].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Sheet.Cells[string.Format("J{0}", row)].Style.Fill.BackgroundColor.SetColor(redColor);
                    Sheet.Cells[string.Format("J{0}", row)].Value = item.mvoh;
                }

                if (item.lvoh == null || item.lvoh == 0)
                {
                    Sheet.Cells[string.Format("K{0}", row)].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Sheet.Cells[string.Format("K{0}", row)].Style.Fill.BackgroundColor.SetColor(warningColor);
                    Sheet.Cells[string.Format("K{0}", row)].Value = "-";
                }
                else if (item.lvoh_target == null || (item.lvoh_target != null && item.lvoh >= item.lvoh_target))
                {
                    Sheet.Cells[string.Format("K{0}", row)].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Sheet.Cells[string.Format("K{0}", row)].Style.Fill.BackgroundColor.SetColor(greenColor);
                    Sheet.Cells[string.Format("K{0}", row)].Value = item.lvoh;
                }
                else
                {
                    Sheet.Cells[string.Format("K{0}", row)].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Sheet.Cells[string.Format("K{0}", row)].Style.Fill.BackgroundColor.SetColor(redColor);
                    Sheet.Cells[string.Format("K{0}", row)].Value = item.lvoh;
                }

                if (item.mvug == null || item.mvug == 0)
                {
                    Sheet.Cells[string.Format("L{0}", row)].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Sheet.Cells[string.Format("L{0}", row)].Style.Fill.BackgroundColor.SetColor(warningColor);
                    Sheet.Cells[string.Format("L{0}", row)].Value = "-";
                }
                else if (item.mvug_target == null || (item.mvug_target != null && item.mvug >= item.mvug_target))
                {
                    Sheet.Cells[string.Format("L{0}", row)].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Sheet.Cells[string.Format("L{0}", row)].Style.Fill.BackgroundColor.SetColor(greenColor);
                    Sheet.Cells[string.Format("L{0}", row)].Value = item.mvug;
                }
                else
                {
                    Sheet.Cells[string.Format("L{0}", row)].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Sheet.Cells[string.Format("L{0}", row)].Style.Fill.BackgroundColor.SetColor(redColor);
                    Sheet.Cells[string.Format("L{0}", row)].Value = item.mvug;
                }


                if (item.lvug == null || item.lvug == 0)
                {
                    Sheet.Cells[string.Format("M{0}", row)].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Sheet.Cells[string.Format("M{0}", row)].Style.Fill.BackgroundColor.SetColor(warningColor);
                    Sheet.Cells[string.Format("M{0}", row)].Value = "-";
                }
                else if (item.lvug_target == null || (item.lvug_target != null && item.lvug >= item.lvug_target))
                {
                    Sheet.Cells[string.Format("M{0}", row)].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Sheet.Cells[string.Format("M{0}", row)].Style.Fill.BackgroundColor.SetColor(greenColor);
                    Sheet.Cells[string.Format("M{0}", row)].Value = item.lvug;
                }
                else
                {
                    Sheet.Cells[string.Format("M{0}", row)].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Sheet.Cells[string.Format("M{0}", row)].Style.Fill.BackgroundColor.SetColor(redColor);
                    Sheet.Cells[string.Format("M{0}", row)].Value = item.lvug;
                }

                Sheet.Cells[string.Format("N{0}", row)].Value = item.status == 1?"Pending":item.status==2? "Approved": "Rejected";
                

                row++;
            }

            row++;
            colFromHex = System.Drawing.ColorTranslator.FromHtml("#000000");
            Sheet.Cells[string.Format("A{0},B{1}", row, row)].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells[string.Format("A{0},B{1}", row, row)].Style.Fill.BackgroundColor.SetColor(colFromHex);
            text = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            Sheet.Cells[string.Format("A{0},B{1}", row, row)].Style.Font.Color.SetColor(text);

            Sheet.Cells[string.Format("A{0}", row)].Value = "Totals";

            if(isA.SuperAdmin() || isA.BranchAdmin())
            { 
                row++;
                Sheet.Cells[string.Format("A{0}", row)].Value = "Cost";
                Sheet.Cells[string.Format("B{0}", row)].Value = productivityResult.Select(p => p.cost).Sum();
            }

            row++;
            Sheet.Cells[string.Format("A{0}", row)].Value = "Equipment Quantity";
            Sheet.Cells[string.Format("B{0}", row)].Value = productivityResult.Select(p=>p.equipment_quantity).Sum();

            row++;
            Sheet.Cells[string.Format("A{0}", row)].Value = "MVOH";
            Sheet.Cells[string.Format("B{0}", row)].Value = productivityResult.Select(p => p.mvoh).Sum();

            row++;
            Sheet.Cells[string.Format("A{0}", row)].Value = "LVOH";
            Sheet.Cells[string.Format("B{0}", row)].Value = productivityResult.Select(p => p.lvoh).Sum();

            row++;
            Sheet.Cells[string.Format("A{0}", row)].Value = "MVUG";
            Sheet.Cells[string.Format("B{0}", row)].Value = productivityResult.Select(p => p.mvug).Sum();

            row++;
            Sheet.Cells[string.Format("A{0}", row)].Value = "LVUG";
            Sheet.Cells[string.Format("B{0}", row)].Value = productivityResult.Select(p => p.lvug).Sum();

            row++;
            Sheet.Cells[string.Format("A{0}", row)].Value = "Sub Station";
            Sheet.Cells[string.Format("B{0}", row)].Value = productivityResult.Select(p => p.substation).Sum();

            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + DateTime.Now.ToString() + "Report.xlsx");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();
        }
        [HttpPost]
        public void GenerateMissingProductivityReport(UserProjectViewModel userProjectViewModel)
        {
            User currentUser = Session["user"] as User;

            ExcelPackage Ep = new ExcelPackage();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Missing Productivity Report");

            System.Drawing.Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#000000");
            Sheet.Cells["A1:G1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells["A1:G1"].Style.Fill.BackgroundColor.SetColor(colFromHex);
            System.Drawing.Color text = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            Sheet.Cells["A1:G1"].Style.Font.Color.SetColor(text);

            Sheet.Cells["A1"].Value = "ID";
            Sheet.Cells["B1"].Value = "Name";
            Sheet.Cells["C1"].Value = "Code";
            Sheet.Cells["D1"].Value = "Phone";
            Sheet.Cells["E1"].Value = "Address";
            Sheet.Cells["F1"].Value = "Gender";
            Sheet.Cells["G1"].Value = "Role";
          

            var productivityData = (from user in db.Users
                                    join idtype in db.IDTypes on user.id_type equals idtype.id
                                    join nationality in db.Nationalities on user.nationality_id equals nationality.id
                                    join branch in db.Branches on user.branch_id equals branch.id
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
                                        active = user.active,
                                    }).Where(u => u.active == (int)RowStatus.ACTIVE);


            if (userProjectViewModel.from_date != null)
            {
                if (Convert.ToDateTime(userProjectViewModel.from_date) != DateTime.MinValue)
                {
                    DateTime date = Convert.ToDateTime(userProjectViewModel.from_date);
                    List<int?> missingProductivityUsers = db.UserProjects.Where(us => ((DateTime)us.working_date).Year == date.Year && ((DateTime)us.working_date).Month == date.Month && ((DateTime)us.working_date).Day == date.Day).Select(us => us.user_id).ToList();
                    productivityData = productivityData.Where(s => !missingProductivityUsers.Contains(s.id) && s.required_productivity == 1);
                }
            }
            else
            {
                productivityData = productivityData.Where(s => s.id == -1);
            }

            if (isA.SuperAdmin())
            {
                productivityData = productivityData.Where(s => s.id != currentUser.id && (s.type == (int)UserRole.Employee || s.type == (int)UserRole.TeamLeader || s.type == (int)UserRole.TechnicalManager));
                if (userProjectViewModel.branch_id != null)
                {
                    productivityData = productivityData.Where(s => s.branch_id == userProjectViewModel.branch_id);
                }
            }

            if (isA.BranchAdmin())
            {
                productivityData = productivityData.Where(s => s.branch_id == currentUser.branch_id && s.id != currentUser.id && (s.type == (int)UserRole.Employee || s.type == (int)UserRole.TeamLeader || s.type == (int)UserRole.TechnicalManager));
            }

            if (isA.TeamLeader())
            {
                productivityData = productivityData.Where(s => s.team_leader_id == currentUser.id && s.id != currentUser.id && s.type == (int)UserRole.Employee);
            }

            if (isA.TechnicalManager())
            {
                productivityData = productivityData.Where(s => s.branch_id == currentUser.branch_id && s.id != currentUser.id && (s.type == (int)UserRole.Employee || s.type == (int)UserRole.TeamLeader));
            }


            List<UserViewModel> missingProductivityResult = productivityData.ToList();

            int row = 2;
            foreach (var item in missingProductivityResult)
            {

                Sheet.Cells[string.Format("A{0}", row)].Value = item.id;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.full_name;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.code;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.phone;
                Sheet.Cells[string.Format("E{0}", row)].Value = item.address;
                Sheet.Cells[string.Format("F{0}", row)].Value = item.gender == 1?"Male":"Female";
                Sheet.Cells[string.Format("G{0}", row)].Value = item.type == 1? "Super Admin": item.type == 2?"Branch Admin":item.type == 3?"Employee":"Team Leader";
                row++;
            }

            row++;
            //colFromHex = System.Drawing.ColorTranslator.FromHtml("#000000");
            //Sheet.Cells[string.Format("A{0},B{1}", row, row)].Style.Fill.PatternType = ExcelFillStyle.Solid;
            //Sheet.Cells[string.Format("A{0},B{1}", row, row)].Style.Fill.BackgroundColor.SetColor(colFromHex);
            //text = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            //Sheet.Cells[string.Format("A{0},B{1}", row, row)].Style.Font.Color.SetColor(text);

            //Sheet.Cells[string.Format("A{0}", row)].Value = "Totals";
           
            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + DateTime.Now.ToString() + "Report.xlsx");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();
        }
        private string toEnglishNumber(string input)
        {
            string EnglishNumbers = "";

            for (int i = 0; i < input.Length; i++)
            {
                if (Char.IsDigit(input[i]))
                {
                    EnglishNumbers += char.GetNumericValue(input, i);
                }
                else
                {
                    EnglishNumbers += input[i].ToString();
                }
            }
            return EnglishNumbers;
        }

    }
}