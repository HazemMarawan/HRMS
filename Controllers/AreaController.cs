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
using System.Configuration;
using System.Data.SqlClient;
using OfficeOpenXml.Style;
using OfficeOpenXml;

namespace HRMS.Controllers
{
    [CustomAuthenticationFilter]
    public class AreaController : BaseController
    {
        HRMSDBContext db = new HRMSDBContext();
        // GET: Department
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
                var areaData = (from area in db.Areas
                                      join project in db.Projects on area.project_id equals project.id
                                      select new AreaViewModel
                                      {
                                          id = area.id,
                                          name = area.name,
                                          project_id = project.id,
                                          project_name = project.name,
                                          mvoh = area.mvoh,
                                          lvoh = area.lvoh,
                                          mvug = area.mvug,
                                          lvug = area.lvug,
                                          active = area.active,
                                          created_at = area.created_at
                                      }).Where(n => n.active == (int)RowStatus.ACTIVE);

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    areaData = areaData.Where(m => m.name.ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()));
                }

                if (!isA.SuperAdmin())
                {
                    areaData = areaData.Where(n => n.id == -1);
                }
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
            ViewBag.Projects = db.Projects.Where(p => p.active == (int)RowStatus.ACTIVE).Select(p => new { p.id, p.name }).ToList();
            return View();
        }
        [HttpPost]
        public JsonResult saveArea(AreaViewModel areaViewModel)
        {
            User user = Session["user"] as User;

            if (areaViewModel.id == 0)
            {
                Area area = AutoMapper.Mapper.Map<AreaViewModel, Area>(areaViewModel);

                area.created_at = DateTime.Now;
                area.created_by = user.id;

                db.Areas.Add(area);
            }
            else
            {
                Area area = db.Areas.Find(areaViewModel.id);
                area.name = areaViewModel.name;
                area.active = areaViewModel.active;
                area.mvoh = areaViewModel.mvoh;
                area.lvoh = areaViewModel.lvoh;
                area.lvug = areaViewModel.lvug;
                area.mvug = areaViewModel.mvug;
                area.updated_at = DateTime.Now;
                area.updated_by = user.id;

            }

            db.SaveChanges();
            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult deleteArea(int id)
        {
            Area deleteArea = db.Areas.Find(id);
            deleteArea.active = (int)RowStatus.INACTIVE;
            deleteArea.deleted_at = DateTime.Now;
            deleteArea.deleted_by = Session["id"].ToString().ToInt();

            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public void GenerateAreaReport(UserProjectViewModel userProjectViewModel)
        {
            User currentUser = Session["user"] as User;

            ExcelPackage Ep = new ExcelPackage();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Area Report");

            System.Drawing.Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#000000");
            System.Drawing.Color redColor = System.Drawing.ColorTranslator.FromHtml("#FF0000");
            System.Drawing.Color warningColor = System.Drawing.ColorTranslator.FromHtml("#FFA000");
            System.Drawing.Color greenColor = System.Drawing.ColorTranslator.FromHtml("#00FF00");
            Sheet.Cells["A5:L5"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells["A5:L5"].Style.Fill.BackgroundColor.SetColor(colFromHex);
            System.Drawing.Color text = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            Sheet.Cells["A5:L5"].Style.Font.Color.SetColor(text);

            Sheet.Cells["A1"].Value = "Area";
            Sheet.Cells["B1"].Value = db.Areas.Find(userProjectViewModel.area_id).name;

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
	                        where  userProjects.area_id = " + userProjectViewModel.area_id + @"
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
	                    where  userProjects.area_id = " + userProjectViewModel.area_id + @" and branch_id = " + currentUser.branch_id + @"
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
            Sheet.Cells[string.Format("B{0}", row)].Value = projectReport.Select(pr => pr.no_of_numbers).Sum();


            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + DateTime.Now.ToString() + "Report.xlsx");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();
        }

    }
}