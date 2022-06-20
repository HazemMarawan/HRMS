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
using System.Configuration;
using System.Data.SqlClient;

namespace HRMS.Controllers
{
    [CustomAuthenticationFilter]
    public class ProjectController : Controller
    {
        HRMSDBContext db = new HRMSDBContext();
        // GET: Project
        public ActionResult Index(int? branch_id)
        {
            User user = Session["user"] as User;
            if (!(isA.SuperAdmin() || (isA.BranchAdmin() && (user.branch_id == branch_id || branch_id == null))))
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
                var productitvityData = (from project in db.Projects
                                       join projectType in db.ProjectTypes on project.project_type_id equals projectType.id
                                       select new ProjectViewModel
                                       {
                                           id = project.id,
                                           name = project.name,
                                           project_type_id = project.project_type_id,
                                           project_type_name = projectType.name,
                                           start_date = project.start_date,
                                           end_date = project.end_date,
                                           mvoh = project.mvoh,
                                           lvoh = project.lvoh,
                                           mvug = project.mvug,
                                           lvug = project.lvug,
                                           equipment_quantity = project.equipment_quantity,
                                           created_at = project.created_at,
                                           active = project.active,
                                           areas = db.Areas.Where(a=>a.project_id == project.id && a.active == (int)RowStatus.ACTIVE).Select(a=>new AreaViewModel
                                           {
                                               id = a.id,
                                               name = a.name,
                                               active = a.active
                                           }).ToList()
                                       }).Where(n => n.active == (int)RowStatus.ACTIVE);

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    productitvityData = productitvityData.Where(m => m.name.ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()));
                }
                if(isA.SuperAdmin())
                {
                    if (branch_id != null)
                    {
                        List<int?> branchProjects = db.BranchProjects.Where(b => b.branch_id == branch_id).Select(b => b.project_id).ToList();
                        productitvityData = productitvityData.Where(p => branchProjects.Contains(p.id));
                    }
                }
                else if (isA.BranchAdmin())
                {
                    List<int?> branchProjects = db.BranchProjects.Where(b => b.branch_id == user.branch_id).Select(b => b.project_id).ToList();
                    productitvityData = productitvityData.Where(p => branchProjects.Contains(p.id));
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
            ViewBag.ProjectTypes = db.ProjectTypes.Where(p => p.active == (int)RowStatus.ACTIVE).Select(p => new { p.id, p.name }).ToList();
            if (isA.BranchAdmin())
            {
                branch_id = user.branch_id;
            }
            ViewBag.branchId = branch_id;
            if(branch_id != null)
                ViewBag.branchName = db.Branches.Where(b => b.id == branch_id).FirstOrDefault().name;
            return View();
        }
        [HttpPost]
        public JsonResult saveProject(ProjectViewModel projectViewModel)
        {

            if (projectViewModel.id == 0)
            {
                Project project = AutoMapper.Mapper.Map<ProjectViewModel, Project>(projectViewModel);

                project.created_at = DateTime.Now;
                project.created_by = Session["id"].ToString().ToInt();

                db.Projects.Add(project);
                db.SaveChanges();
            }
            else
            {

                Project oldProject = db.Projects.Find(projectViewModel.id);

                oldProject.name = projectViewModel.name;
                oldProject.project_type_id = projectViewModel.project_type_id;
                oldProject.start_date = projectViewModel.start_date;
                oldProject.end_date = projectViewModel.end_date;
                oldProject.mvoh = projectViewModel.mvoh;
                oldProject.lvoh = projectViewModel.lvoh;
                oldProject.lvug = projectViewModel.lvug;
                oldProject.mvug = projectViewModel.mvug;
                oldProject.equipment_quantity = projectViewModel.equipment_quantity;
                oldProject.active = projectViewModel.active;
                oldProject.updated_by = Session["id"].ToString().ToInt();
                oldProject.updated_at = DateTime.Now;

                db.SaveChanges();
            }

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult deleteProject(int id)
        {
            Project deleteProject = db.Projects.Find(id);
            deleteProject.active = (int)RowStatus.INACTIVE;
            deleteProject.deleted_at = DateTime.Now;
            deleteProject.deleted_by = Session["id"].ToString().ToInt();

            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }
        
        [HttpGet]
        public JsonResult getAreaByProjectId(int id)
        {
            List<AreaViewModel> areas = db.Areas.Where(a => a.project_id == id).Select(a => new AreaViewModel
            {
                id = a.id,
                name = a.name,
                active = a.active
            }).Where(a=>a.active == (int)RowStatus.ACTIVE).ToList();
            return Json(new { areas = areas }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AreasDetails(int ?id)
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
                var areaData = (from area in db.Areas
                                         select new AreaViewModel
                                         {
                                             id = area.id,
                                             name = area.name,
                                             project_id = area.project_id,
                                             mvoh = area.mvoh,
                                             lvoh = area.lvoh,
                                             mvug = area.mvug,
                                             lvug = area.lvug,
                                             mvoh_sum = db.UserProjects.Where(up=>up.area_id == area.id && up.status == (int)ProductivityStatus.Approved).Select(up=>up.mvoh).Sum(),
                                             lvoh_sum = db.UserProjects.Where(up=>up.area_id == area.id && up.status == (int)ProductivityStatus.Approved).Select(up=>up.lvoh).Sum(),
                                             mvug_sum = db.UserProjects.Where(up=>up.area_id == area.id && up.status == (int)ProductivityStatus.Approved).Select(up=>up.mvug).Sum(),
                                             lvug_sum = db.UserProjects.Where(up=>up.area_id == area.id && up.status == (int)ProductivityStatus.Approved).Select(up=>up.lvug).Sum(),
                                             created_at = area.created_at,
                                             active = area.active,
                                         }).Where(n => n.active == (int)RowStatus.ACTIVE && n.project_id == id);

              


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
            ViewBag.ProjectId = id;
            if (id != null)
                ViewBag.ProjectName = db.Projects.Find(id).name;
            return View();
        }
        [HttpPost]
        public void GenerateProjectReport(UserProjectViewModel userProjectViewModel)
        {
            User currentUser = Session["user"] as User;

            ExcelPackage Ep = new ExcelPackage();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Project Report");

            System.Drawing.Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#000000");
            System.Drawing.Color redColor = System.Drawing.ColorTranslator.FromHtml("#FF0000");
            System.Drawing.Color warningColor = System.Drawing.ColorTranslator.FromHtml("#FFA000");
            System.Drawing.Color greenColor = System.Drawing.ColorTranslator.FromHtml("#00FF00");
            Sheet.Cells["A5:L5"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells["A5:L5"].Style.Fill.BackgroundColor.SetColor(colFromHex);
            System.Drawing.Color text = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            Sheet.Cells["A5:L5"].Style.Font.Color.SetColor(text);

            Sheet.Cells["A1"].Value = "Project";
            Sheet.Cells["B1"].Value = db.Projects.Find(userProjectViewModel.project_id).name;

            if (userProjectViewModel.from_date != null)
            {
                Sheet.Cells["A2"].Value = "From";
                Sheet.Cells["B1"].Value = userProjectViewModel.from_date.ToString();
            }

            if (userProjectViewModel.to_date != null)
            {
                Sheet.Cells["A3"].Value = "To";
                Sheet.Cells["B3"].Value = userProjectViewModel.to_date.ToString();
            }

            Sheet.Cells["A5"].Value = "Task";
            Sheet.Cells["B5"].Value = "Hours";
            Sheet.Cells["C5"].Value = "Normal";
            Sheet.Cells["D5"].Value = "Overtime";
            Sheet.Cells["E5"].Value = "Compensation";
            Sheet.Cells["F5"].Value = "Employees";
            Sheet.Cells["G5"].Value = "MVOH";
            Sheet.Cells["H5"].Value = "LVOH";
            Sheet.Cells["I5"].Value = "MVUG";
            Sheet.Cells["J5"].Value = "LVUG";
            Sheet.Cells["K5"].Value = "Equipment Quantity";
            Sheet.Cells["L5"].Value = "Sub Station";

            string query = String.Empty;

            if (HRMS.Auth.isA.SuperAdmin())
            {
                query = @"select tasks.name as Task,
	                        sum(CASE WHEN userProjects.no_of_numbers is not null then userProjects.no_of_numbers else 0 end) as Hours,
                            sum(CASE WHEN UserProjects.productivity_type = 1 then 1
                            ELSE 0 end) AS Normal, 
	                        sum(CASE WHEN UserProjects.productivity_type = 2 then 1
                            ELSE 0 end) AS OverTime , 
	                        sum(CASE WHEN UserProjects.productivity_type = 3 then 1
                            ELSE 0 end) AS Compensation , 
	                        count(userProjects.user_id) as Employees,
	                        sum(CASE WHEN userProjects.mvoh is not null then userProjects.mvoh else 0 end) as MVOH,
	                        sum(CASE WHEN userProjects.lvoh is not null then userProjects.lvoh else 0 end) as LVOH,
	                        sum(CASE WHEN userProjects.mvug is not null then userProjects.mvug else 0 end) as MVUG,
	                        sum(CASE WHEN userProjects.lvug is not null then userProjects.lvug else 0 end) as LVUG,
	                        sum(CASE WHEN userProjects.equipment_quantity is not null then userProjects.equipment_quantity else 0 end) as  EquipmentQuantity,
	                        sum(CASE WHEN userProjects.substation is not null then userProjects.substation else 0 end) as  SubStation
	
	                        from userProjects
	                        inner join projects on userProjects.project_id = projects.id
	                        inner join Tasks on userProjects.task_id = tasks.id
	                        where  userProjects.project_id = "+ userProjectViewModel.project_id+@"
                            group by Tasks.id,Tasks.name";
            }

            if (HRMS.Auth.isA.BranchAdmin())
            {
                query = @"select tasks.name as Task,
	                    sum(CASE WHEN userProjects.no_of_numbers is not null then userProjects.no_of_numbers else 0 end) as Hours,
                        sum(CASE WHEN UserProjects.productivity_type = 1 then 1
                        ELSE 0 end) AS Normal, 
	                    sum(CASE WHEN UserProjects.productivity_type = 2 then 1
                        ELSE 0 end) AS OverTime , 
	                    sum(CASE WHEN UserProjects.productivity_type = 3 then 1
                        ELSE 0 end) AS Compensation , 
	                    count(userProjects.user_id) as Employees,
	                    sum(CASE WHEN userProjects.mvoh is not null then userProjects.mvoh else 0 end) as MVOH,
	                    sum(CASE WHEN userProjects.lvoh is not null then userProjects.lvoh else 0 end) as LVOH,
	                    sum(CASE WHEN userProjects.mvug is not null then userProjects.mvug else 0 end) as MVUG,
	                    sum(CASE WHEN userProjects.lvug is not null then userProjects.lvug else 0 end) as LVUG,
	                    sum(CASE WHEN userProjects.equipment_quantity is not null then userProjects.equipment_quantity else 0 end) as  EquipmentQuantity,
	                    sum(CASE WHEN userProjects.substation is not null then userProjects.substation else 0 end) as  SubStation
	
	                    from userProjects
	                    inner join projects on userProjects.project_id = projects.id
	                    inner join Tasks on userProjects.task_id = tasks.id
	                    inner join Users on userProjects.user_id = Users.id
	                    where  userProjects.project_id = "+userProjectViewModel.project_id+@" and branch_id = "+currentUser.branch_id+@"
	                    group by Tasks.id,Tasks.name";
            }


            string cs = ConfigurationManager.ConnectionStrings["HRMSDBContextADO"].ConnectionString;
            SqlConnection sql = new SqlConnection(cs);
            sql.Open();

            SqlCommand comm = new SqlCommand(query, sql);
            SqlDataReader reader = comm.ExecuteReader();

            List<UserProjectViewModel> projectReport = new List<UserProjectViewModel>();

            while (reader.Read())
            {
                UserProjectViewModel userProjectViewModel1 = new UserProjectViewModel();
                
                userProjectViewModel1.task_name = reader["Task"].ToString();
                userProjectViewModel1.no_of_numbers = Convert.ToDouble(reader["Hours"].ToString());
                userProjectViewModel1.normal = Convert.ToInt32(reader["Normal"].ToString());
                userProjectViewModel1.overtime = Convert.ToInt32(reader["OverTime"].ToString());
                userProjectViewModel1.compensation = Convert.ToInt32(reader["Compensation"].ToString());
                userProjectViewModel1.employees = Convert.ToInt32(reader["Employees"].ToString());
                userProjectViewModel1.mvoh = Convert.ToDouble(reader["MVOH"].ToString());
                userProjectViewModel1.lvoh = Convert.ToDouble(reader["LVOH"].ToString());
                userProjectViewModel1.mvug = Convert.ToDouble(reader["MVUG"].ToString());
                userProjectViewModel1.lvug = Convert.ToDouble(reader["LVUG"].ToString());
                userProjectViewModel1.equipment_quantity = Convert.ToDouble(reader["EquipmentQuantity"].ToString());
                userProjectViewModel1.substation = Convert.ToDouble(reader["SubStation"].ToString());

                projectReport.Add(userProjectViewModel1);

            }

            int row = 6;

            foreach (var item in projectReport)
            {

                Sheet.Cells[string.Format("A{0}", row)].Value = item.task_name;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.no_of_numbers;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.normal;
                Sheet.Cells[string.Format("D{0}", row)].Value = item.overtime;
                Sheet.Cells[string.Format("E{0}", row)].Value = item.compensation;
                Sheet.Cells[string.Format("F{0}", row)].Value = item.employees;
                Sheet.Cells[string.Format("G{0}", row)].Value = item.mvoh;
                Sheet.Cells[string.Format("H{0}", row)].Value = item.lvoh;
                Sheet.Cells[string.Format("I{0}", row)].Value = item.mvug;
                Sheet.Cells[string.Format("J{0}", row)].Value = item.lvug;
                Sheet.Cells[string.Format("K{0}", row)].Value = item.equipment_quantity;
                Sheet.Cells[string.Format("L{0}", row)].Value = item.substation;

                row++;
            }

            row++;
            colFromHex = System.Drawing.ColorTranslator.FromHtml("#000000");
            Sheet.Cells[string.Format("A{0},B{1}", row, row)].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells[string.Format("A{0},B{1}", row, row)].Style.Fill.BackgroundColor.SetColor(colFromHex);
            text = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            Sheet.Cells[string.Format("A{0},B{1}", row, row)].Style.Font.Color.SetColor(text);

            Sheet.Cells[string.Format("A{0}", row)].Value = "Total Hours";
            Sheet.Cells[string.Format("B{0}", row)].Value = projectReport.Select(pr=>pr.no_of_numbers).Sum();

            
            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + DateTime.Now.ToString() + "Report.xlsx");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();
        }
    }
}