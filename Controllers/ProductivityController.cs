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
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace HRMS.Controllers
{
    [CustomAuthenticationFilter]
    public class ProductivityController : Controller
    {
        HRMSDBContext db = new HRMSDBContext();
        // GET: Productivity
        public ActionResult Index(int? branch_id)
        {
            User currentUser = Session["user"] as User;
            if (!(isA.SuperAdmin() || isA.TeamLeader() || isA.BranchAdmin()))
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
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                // Getting all data    
                var productivityData = (from user in db.Users
                                        join userProject in db.UserProjects on user.id equals userProject.user_id
                                        join pro in db.Projects on userProject.project_id equals pro.id into pr
                                        from project in pr.DefaultIfEmpty()
                                        join branchProject in db.BranchProjects on project.id equals branchProject.project_id
                                        join are in db.Areas on userProject.area_id equals are.id into ar
                                        from area in ar.DefaultIfEmpty()
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
                                            branch_id_branch_project = branchProject.branch_id,
                                            productivity_type = userProject.productivity_type,
                                            productivity_work_place = userProject.productivity_work_place,
                                            part_id = userProject.part_id,
                                            equipment_quantity = userProject.equipment_quantity,
                                            mvoh = userProject.mvoh,
                                            lvoh = userProject.lvoh,
                                            mvug = userProject.mvug,
                                            lvug = userProject.lvug,
                                            note = userProject.note,
                                            status = userProject.status,
                                            cost = userProject.cost,
                                            team_leader_id = user.team_leader_id,
                                            area_id = userProject.area_id,
                                            area_name = area.name
                                        });

               

                if(HRMS.Auth.isA.TeamLeader())
                {
                    productivityData = productivityData.Where(p => p.team_leader_id == currentUser.id && p.branch_id == currentUser.branch_id && p.branch_id == p.branch_id_branch_project);
                }

                if (HRMS.Auth.isA.SuperAdmin())
                {
                    if (branch_id != null)
                    {
                        productivityData = productivityData.Where(p => p.branch_id == branch_id && p.branch_id == p.branch_id_branch_project);
                    }
                }

                if (HRMS.Auth.isA.BranchAdmin())
                {
                    productivityData = productivityData.Where(p => p.branch_id == currentUser.branch_id && p.branch_id == p.branch_id_branch_project);
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

                //var clonedProductivityData = productivityData.ToList();
                int? Hours = 0;
                int? Projects = 0;
                int? Employees = 0;
                double? Cost = 0;

               
                Hours = productivityData.Select(c => c.no_of_numbers).ToList().Sum();
                Projects = productivityData.Select(c => c.project_id).Distinct().ToList().Count();
                Employees = productivityData.Select(c => c.user_id).Distinct().ToList().Count();
                Cost = productivityData.Select(c => c.cost).ToList().Sum();
             
                //total number of rows count     
                var displayResult = productivityData.OrderByDescending(u => u.id).Skip(skip)
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
                    Cost = Cost

                }, JsonRequestBehavior.AllowGet);

            }
            
            ViewBag.branchId = branch_id;
            if (branch_id != null)
            {
                ViewBag.branchName = db.Branches.Where(b => b.id == branch_id).FirstOrDefault().name;
                List<int?> branchProjects = db.BranchProjects.Where(b => b.branch_id == currentUser.branch_id).Select(b => b.project_id).ToList();
                ViewBag.Projects = db.Projects.Where(p => branchProjects.Contains(p.id)).Select(p=>new { p.id,p.name}).ToList();
            }
            else
            {
                ViewBag.branchName = "Company";
                ViewBag.Projects = db.Projects.Select(p => new { p.id, p.name }).ToList();
            }
            return View();
        }

        public ActionResult Missing(int? branch_id)
        {
            User currentUser = Session["user"] as User;
            if (!(isA.SuperAdmin() || isA.TeamLeader() || isA.BranchAdmin()))
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
                        List<int?> missingProductivityUsers = db.UserProjects.Where(us => ((DateTime)us.created_at).Year == date.Year && ((DateTime)us.created_at).Month == date.Month && ((DateTime)us.created_at).Day == date.Day).Select(us => us.user_id).ToList();
                        productivityData = productivityData.Where(s => !missingProductivityUsers.Contains(s.id));
                    }
                }
                else
                {
                    productivityData = productivityData.Where(s => s.id == -1);
                }

                if(isA.SuperAdmin())
                {
                    if (branch_id != null)
                    {
                        productivityData = productivityData.Where(s => s.branch_id == branch_id);
                    }
                }

                if(isA.BranchAdmin() || isA.TeamLeader())
                {
                    productivityData = productivityData.Where(s => s.branch_id == currentUser.branch_id);
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

            ViewBag.branchId = branch_id;
            if (branch_id != null)
            {
                ViewBag.branchName = db.Branches.Where(b => b.id == branch_id).FirstOrDefault().name;
                List<int?> branchProjects = db.BranchProjects.Where(b => b.branch_id == currentUser.branch_id).Select(b => b.project_id).ToList();
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
            if (!(isA.Employee() || isA.TeamLeader()))
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
                                           equipment_quantity = userProject.equipment_quantity,
                                           mvoh = userProject.mvoh,
                                           lvoh = userProject.lvoh,
                                           mvug = userProject.mvug,
                                           lvug = userProject.lvug,
                                           note = userProject.note,
                                           status = userProject.status,
                                           area_id = userProject.area_id,
                                           area_name = area.name
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
            return View();
        }

        [HttpPost]
        public JsonResult saveProductivity(UserProjectViewModel userProjectViewModel)
        {
            User currentUser = Session["user"] as User;
            if (userProjectViewModel.id == 0)
            {
                UserProject userProject = AutoMapper.Mapper.Map<UserProjectViewModel, UserProject>(userProjectViewModel);
                userProject.user_id = currentUser.id;
                userProject.created_at = DateTime.Now;
                userProject.created_by = Session["id"].ToString().ToInt();
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
                oldUserProject.equipment_quantity = userProjectViewModel.equipment_quantity;
                oldUserProject.mvoh = userProjectViewModel.mvoh;
                oldUserProject.lvoh = userProjectViewModel.lvoh;
                oldUserProject.mvug = userProjectViewModel.mvug;
                oldUserProject.lvug = userProjectViewModel.lvug;
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
            approveUserProject.cost = currentUser.last_hour_price * approveUserProject.no_of_numbers;

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
        public void GenerateProductivityReport(UserProjectViewModel userProjectViewModel)
        {
            User currentUser = Session["user"] as User;


            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Productivity Report");

            System.Drawing.Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#000000");
            Sheet.Cells["A1:M1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells["A1:M1"].Style.Fill.BackgroundColor.SetColor(colFromHex);
            System.Drawing.Color text = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            Sheet.Cells["A1:M1"].Style.Font.Color.SetColor(text);

            Sheet.Cells["A1"].Value = "Employee Name";
            Sheet.Cells["B1"].Value = "Working Date";
            Sheet.Cells["C1"].Value = "Project";
            Sheet.Cells["D1"].Value = "Area";
            Sheet.Cells["E1"].Value = "Hours";
            Sheet.Cells["F1"].Value = "Productivity Type";
            Sheet.Cells["G1"].Value = "Part ID";
            Sheet.Cells["H1"].Value = "Equipment Quantity";
            Sheet.Cells["I1"].Value = "MVOH";
            Sheet.Cells["J1"].Value = "LVOH";
            Sheet.Cells["K1"].Value = "MVUG";
            Sheet.Cells["L1"].Value = "LVUG";
            Sheet.Cells["M1"].Value = "Status";

            var productivityData = (from user in db.Users
                                    join userProject in db.UserProjects on user.id equals userProject.user_id
                                    join pro in db.Projects on userProject.project_id equals pro.id into pr
                                    from project in pr.DefaultIfEmpty()
                                    join branchProject in db.BranchProjects on project.id equals branchProject.project_id
                                    join are in db.Areas on userProject.area_id equals are.id into ar
                                    from area in ar.DefaultIfEmpty()
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
                                        branch_id_branch_project = branchProject.branch_id,
                                        productivity_type = userProject.productivity_type,
                                        productivity_work_place = userProject.productivity_work_place,
                                        part_id = userProject.part_id,
                                        equipment_quantity = userProject.equipment_quantity,
                                        mvoh = userProject.mvoh,
                                        lvoh = userProject.lvoh,
                                        mvug = userProject.mvug,
                                        lvug = userProject.lvug,
                                        note = userProject.note,
                                        status = userProject.status,
                                        cost = userProject.cost,
                                        team_leader_id = user.team_leader_id,
                                        area_id = userProject.area_id,
                                        area_name = area.name
                                    });



            if (HRMS.Auth.isA.TeamLeader())
            {
                productivityData = productivityData.Where(p => p.team_leader_id == currentUser.id && p.branch_id == currentUser.branch_id && p.branch_id == p.branch_id_branch_project);
            }

            if (HRMS.Auth.isA.SuperAdmin())
            {
                if (userProjectViewModel.branch_id != null)
                {
                    productivityData = productivityData.Where(p => p.branch_id == userProjectViewModel.branch_id && p.branch_id == p.branch_id_branch_project);
                }
            }

            if (HRMS.Auth.isA.BranchAdmin())
            {
                productivityData = productivityData.Where(p => p.branch_id == currentUser.branch_id && p.branch_id == p.branch_id_branch_project);
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

            List<UserProjectViewModel> productivityResult = productivityData.ToList();

            int row = 2;
            foreach (var item in productivityResult)
            {

                Sheet.Cells[string.Format("A{0}", row)].Value = item.user_name;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.working_date.ToString().Split(' ')[0];
                Sheet.Cells[string.Format("C{0}", row)].Value = item.project_name;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.area_name;
                Sheet.Cells[string.Format("E{0}", row)].Value = item.no_of_numbers;
                Sheet.Cells[string.Format("F{0}", row)].Value = item.productivity_type == 1?"Normal": "OverTime";
                Sheet.Cells[string.Format("G{0}", row)].Value = item.part_id;
                Sheet.Cells[string.Format("H{0}", row)].Value = item.equipment_quantity;
                Sheet.Cells[string.Format("I{0}", row)].Value = item.mvoh;
                Sheet.Cells[string.Format("J{0}", row)].Value = item.lvoh;
                Sheet.Cells[string.Format("K{0}", row)].Value = item.mvug;
                Sheet.Cells[string.Format("L{0}", row)].Value = item.lvug;
                Sheet.Cells[string.Format("M{0}", row)].Value = item.status == 1?"Pending":item.status==2? "Approved": "Rejected";
                

                row++;
            }

            row++;
            colFromHex = System.Drawing.ColorTranslator.FromHtml("#000000");
            Sheet.Cells[string.Format("A{0},B{1}", row, row)].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells[string.Format("A{0},B{1}", row, row)].Style.Fill.BackgroundColor.SetColor(colFromHex);
            text = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            Sheet.Cells[string.Format("A{0},B{1}", row, row)].Style.Font.Color.SetColor(text);

            Sheet.Cells[string.Format("A{0}", row)].Value = "Totals";

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
                    List<int?> missingProductivityUsers = db.UserProjects.Where(us => ((DateTime)us.created_at).Year == date.Year && ((DateTime)us.created_at).Month == date.Month && ((DateTime)us.created_at).Day == date.Day).Select(us => us.user_id).ToList();
                    productivityData = productivityData.Where(s => !missingProductivityUsers.Contains(s.id));
                }
            }
            else
            {
                productivityData = productivityData.Where(s => s.id == -1);
            }

            if (isA.SuperAdmin())
            {
                if (userProjectViewModel.branch_id != null)
                {
                    productivityData = productivityData.Where(s => s.branch_id == userProjectViewModel.branch_id);
                }
            }

            if (isA.BranchAdmin() || isA.TeamLeader())
            {
                productivityData = productivityData.Where(s => s.branch_id == currentUser.branch_id);
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
                Sheet.Cells[string.Format("G{0}", row)].Value = item.type == 1? "Super Admin": item.type == 2?"Branch Admin":item.type == 3?"Employee":"Super Admin";
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
    }
}