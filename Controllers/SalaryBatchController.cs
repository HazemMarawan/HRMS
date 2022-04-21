using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HRMS.Models;
using HRMS.ViewModels;
using HRMS.Auth;
using HRMS.Enums;
using System.IO;
using HRMS.Helpers;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Data;

namespace HRMS.Controllers
{
    [CustomAuthenticationFilter]
    public class SalaryBatchController : BaseController
    {
        HRMSDBContext db = new HRMSDBContext();
        // GET: SalaryBatch
        public ActionResult Index(int? branch_id)
        {
            User currentUser = Session["user"] as User;
            if (!(isA.SuperAdmin()
                || (isA.BranchAdmin() && (currentUser.branch_id == branch_id || branch_id == null))))
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
                var salaryBatches = (from salary_batch in db.SalaryBatches
                                      join user in db.Users on salary_batch.created_by equals user.id
                                      select new SalaryBatchViewModel
                                      {
                                          id = salary_batch.id,
                                          month = salary_batch.month,
                                          year = salary_batch.year,
                                          count = salary_batch.count,
                                          total = salary_batch.total,
                                          string_created_by = user.full_name,
                                          created_by = salary_batch.created_by,
                                          created_at = salary_batch.created_at,
                                          file_path = salary_batch.file_path,
                                          branch_id = user.branch_id,
                                          notes = salary_batch.notes,
                                          type = user.type,
                                          active = salary_batch.active
                                      }).Where(n => n.active == (int)RowStatus.ACTIVE);

                if(isA.SuperAdmin())
                {
                    if (branch_id != null)
                        salaryBatches = salaryBatches.Where(sb => sb.branch_id == branch_id && (sb.type == (int)UserRole.BranchAdmin || sb.type == (int)UserRole.TeamLeader || sb.type == (int)UserRole.TechnicalManager || sb.type == (int)UserRole.Employee));
                }

                if(isA.BranchAdmin())
                    salaryBatches = salaryBatches.Where(sb => sb.branch_id == currentUser.branch_id && (sb.type == (int)UserRole.BranchAdmin || sb.type == (int)UserRole.TeamLeader || sb.type == (int)UserRole.TechnicalManager || sb.type == (int)UserRole.Employee));

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    salaryBatches = salaryBatches.Where(m => 
                        m.month.ToString().ToLower().Contains(searchValue.ToLower())
                    ||  m.year.ToString().ToLower().Contains(searchValue.ToLower()));
                }

                //total number of rows count     
                var displayResult = salaryBatches.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = salaryBatches.Count();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = displayResult

                }, JsonRequestBehavior.AllowGet);

            }

            if (isA.BranchAdmin())
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
                ViewBag.Projects = db.Projects.Select(p => new { p.id, p.name }).ToList();
            }

            return View();
        }

        public ActionResult Details(int? id)
        {
            User currentUser = Session["user"] as User;
            if (!(isA.SuperAdmin()|| isA.BranchAdmin()))
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
                var salaryBatches = (from salary_batch in db.SalaryBatches
                                     join salary_batch_detail in db.SalaryBatchDetails on salary_batch.id equals salary_batch_detail.salary_batch_id
                                     join user in db.Users on salary_batch_detail.user_id equals user.id
                                     select new SalaryBatchDetailViewModel
                                     {
                                         id = salary_batch_detail.id,
                                         salary_batch_id = salary_batch_detail.salary_batch_id,
                                         user_id = salary_batch_detail.user_id,
                                         full_name = user.full_name,
                                         bank_code = salary_batch_detail.bank_code,
                                         salary = salary_batch_detail.salary,
                                         insurance_deductions = salary_batch_detail.insurance_deductions,
                                         tax_deductions = salary_batch_detail.tax_deductions,
                                         absense_days = salary_batch_detail.absense_days,
                                         absense_deductions = salary_batch_detail.absense_deductions,
                                         gm_amount = salary_batch_detail.gm_amount,
                                         reserved_amount = salary_batch_detail.reserved_amount,
                                         addtional_hours = salary_batch_detail.addtional_hours,
                                         addtional_hours_amount = salary_batch_detail.addtional_hours_amount,
                                         total_kilos = salary_batch_detail.total_kilos,
                                         total_salary = salary_batch_detail.total_salary,
                                         notes = salary_batch_detail.notes,
                                         active = salary_batch_detail.active,

                                     }).Where(n => n.active == (int)RowStatus.ACTIVE && n.salary_batch_id == id);

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    salaryBatches = salaryBatches.Where(m =>
                        m.full_name.ToString().ToLower().Contains(searchValue.ToLower())
                    || m.bank_code.ToString().ToLower().Contains(searchValue.ToLower()));
                }

                //total number of rows count     
                var displayResult = salaryBatches.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = salaryBatches.Count();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = displayResult

                }, JsonRequestBehavior.AllowGet);

            }

            ViewBag.pageTitle = db.SalaryBatches.Find(id).notes;
            ViewBag.batchId = id;

            if (isA.BranchAdmin())
                ViewBag.branchId = currentUser.branch_id;

            if (isA.SuperAdmin())
            {
                int? upload_user_id = db.SalaryBatches.Find(id).created_by;
                ViewBag.branchId = db.Users.Find(upload_user_id).branch_id;
            }

            ViewBag.branchName = db.Branches.Find(ViewBag.branchId).name;

            return View();
        }

        public ActionResult Employee()
        {
            User currentUser = Session["user"] as User;
            if (!(isA.Employee() || isA.TeamLeader() || isA.TechnicalManager() || isA.BranchAdmin() || isA.ProjectManager()))
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
                var salaryBatches = (from salary_batch in db.SalaryBatches
                                     join salary_batch_detail in db.SalaryBatchDetails on salary_batch.id equals salary_batch_detail.salary_batch_id
                                     join user in db.Users on salary_batch_detail.user_id equals user.id
                                     select new SalaryBatchDetailViewModel
                                     {
                                         id = salary_batch_detail.id,
                                         salary_batch_id = salary_batch_detail.salary_batch_id,
                                         salary_batch_notes = salary_batch.notes,
                                         user_id = salary_batch_detail.user_id,
                                         full_name = user.full_name,
                                         bank_code = salary_batch_detail.bank_code,
                                         salary = salary_batch_detail.salary,
                                         insurance_deductions = salary_batch_detail.insurance_deductions,
                                         tax_deductions = salary_batch_detail.tax_deductions,
                                         absense_days = salary_batch_detail.absense_days,
                                         absense_deductions = salary_batch_detail.absense_deductions,
                                         gm_amount = salary_batch_detail.gm_amount,
                                         reserved_amount = salary_batch_detail.reserved_amount,
                                         addtional_hours = salary_batch_detail.addtional_hours,
                                         addtional_hours_amount = salary_batch_detail.addtional_hours_amount,
                                         total_kilos = salary_batch_detail.total_kilos,
                                         total_salary = salary_batch_detail.total_salary,
                                         notes = salary_batch_detail.notes,
                                         active = salary_batch_detail.active,

                                     }).Where(n => n.active == (int)RowStatus.ACTIVE && n.user_id == currentUser.id);

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    salaryBatches = salaryBatches.Where(m =>
                        m.full_name.ToString().ToLower().Contains(searchValue.ToLower())
                    || m.bank_code.ToString().ToLower().Contains(searchValue.ToLower()));
                }

                //total number of rows count     
                var displayResult = salaryBatches.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = salaryBatches.Count();

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

        public ActionResult Payslip(int id)
        {
            User currentUser = Session["user"] as User;
            int? user_batch_id = db.SalaryBatchDetails.Where(sb => sb.id == id).FirstOrDefault().user_id;
            
            if (!((isA.Employee() || isA.TeamLeader() || isA.TechnicalManager() || isA.BranchAdmin()) && currentUser.id == user_batch_id))
                return RedirectToAction("Index", "Dashboard");

            SalaryBatchDetailViewModel salaryBatche = (from salary_batch in db.SalaryBatches
                                 join salary_batch_detail in db.SalaryBatchDetails on salary_batch.id equals salary_batch_detail.salary_batch_id
                                 join user in db.Users on salary_batch_detail.user_id equals user.id
                                 select new SalaryBatchDetailViewModel
                                 {
                                     id = salary_batch_detail.id,
                                     salary_batch_id = salary_batch_detail.salary_batch_id,
                                     salary_batch_notes = salary_batch.notes,
                                     month = salary_batch.month,
                                     year = salary_batch.year,
                                     user_id = salary_batch_detail.user_id,
                                     full_name = user.full_name,
                                     bank_code = salary_batch_detail.bank_code,
                                     salary = salary_batch_detail.salary,
                                     insurance_deductions = salary_batch_detail.insurance_deductions,
                                     tax_deductions = salary_batch_detail.tax_deductions,
                                     absense_days = salary_batch_detail.absense_days,
                                     absense_deductions = salary_batch_detail.absense_deductions,
                                     gm_amount = salary_batch_detail.gm_amount,
                                     reserved_amount = salary_batch_detail.reserved_amount,
                                     addtional_hours = salary_batch_detail.addtional_hours,
                                     addtional_hours_amount = salary_batch_detail.addtional_hours_amount,
                                     total_kilos = salary_batch_detail.total_kilos,
                                     total_salary = salary_batch_detail.total_salary,
                                     notes = salary_batch_detail.notes,
                                     active = salary_batch_detail.active,
                                     created_at = salary_batch_detail.created_at
                                 }).Where(n => n.active == (int)RowStatus.ACTIVE && n.id == id).FirstOrDefault();
            salaryBatche.full_name = currentUser.full_name;

            var report = new Rotativa.ViewAsPdf("Payslip", salaryBatche);
            report.PageSize = Rotativa.Options.Size.A5;
            report.PageOrientation = Rotativa.Options.Orientation.Portrait;
            report.FileName = salaryBatche.full_name+"_"+salaryBatche.month.ToString()+"_"+ salaryBatche.year.ToString()+".pdf";

            return report;
        }


        public void ExportSalarySheet()
        {
            User currentUser = Session["user"] as User;

            ExcelPackage Ep = new ExcelPackage();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Salary Batch"+DateTime.Now.Month+"-"+ DateTime.Now.Year);

            System.Drawing.Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#000000");
            System.Drawing.Color redColor = System.Drawing.ColorTranslator.FromHtml("#FF0000");
            System.Drawing.Color warningColor = System.Drawing.ColorTranslator.FromHtml("#FFA000");
            System.Drawing.Color greenColor = System.Drawing.ColorTranslator.FromHtml("#00FF00");
            Sheet.Cells["A1:O1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            Sheet.Cells["A1:O1"].Style.Fill.BackgroundColor.SetColor(colFromHex);
            System.Drawing.Color text = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");
            Sheet.Cells["A1:O1"].Style.Font.Color.SetColor(text);

            Sheet.Cells["A1"].Value = "م";
            Sheet.Cells["B1"].Value = "اسم الموظف";
            Sheet.Cells["C1"].Value = "كود البنك";
            Sheet.Cells["D1"].Value = "قيمة الراتب";
            Sheet.Cells["E1"].Value = "أستقطاعات التأمينات";
            Sheet.Cells["F1"].Value = "أستقطاعات الضرائب";
            Sheet.Cells["G1"].Value = "عدد ايام الغياب";
            Sheet.Cells["H1"].Value = "الغياب بالخصم";
            Sheet.Cells["I1"].Value = "قيمة غ/م";
            Sheet.Cells["J1"].Value = "قيمة المستحق";
            Sheet.Cells["K1"].Value = "ساعات الاضافي";
            Sheet.Cells["L1"].Value = "قيمة ساعات الاضافى";
            Sheet.Cells["M1"].Value = "قيمة كيلوهات";
            Sheet.Cells["N1"].Value = "إجمالي الراتب";
            Sheet.Cells["O1"].Value = "ملحوظات";

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
                            }).Where(s => s.active == (int)RowStatus.ACTIVE && (s.type == (int)UserRole.Employee || s.type == (int)UserRole.TeamLeader || s.type == (int)UserRole.TechnicalManager || s.type == (int)UserRole.BranchAdmin) && s.branch_id == currentUser.branch_id);

            List<UserViewModel> employees = userData.ToList();

            int row = 2;
            foreach (var item in employees)
            {

                Sheet.Cells[string.Format("A{0}", row)].Value = item.id;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.full_name;
                Sheet.Cells[string.Format("C{0}", row)].Value = "";
                Sheet.Cells[string.Format("D{0}", row)].Value = "";
                Sheet.Cells[string.Format("E{0}", row)].Value = "";
                Sheet.Cells[string.Format("F{0}", row)].Value = "";
                Sheet.Cells[string.Format("G{0}", row)].Value = "";
                Sheet.Cells[string.Format("H{0}", row)].Value = "";
                Sheet.Cells[string.Format("I{0}", row)].Value = "";
                Sheet.Cells[string.Format("J{0}", row)].Value = "";
                Sheet.Cells[string.Format("K{0}", row)].Value = "";
                Sheet.Cells[string.Format("L{0}", row)].Value = "";
                Sheet.Cells[string.Format("M{0}", row)].Value = "";
                Sheet.Cells[string.Format("N{0}", row)].Value = "";
                Sheet.Cells[string.Format("O{0}", row)].Value = "";


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

        public JsonResult ImportSalarySheet(SalaryBatchViewModel salaryBatchViewModel)
        {
            User user = Session["user"] as User;
            Guid guid = Guid.NewGuid();
            var InputFileName = Path.GetFileName(salaryBatchViewModel.file.FileName);
            var ServerSavePath = Path.Combine(Server.MapPath("~/Uploads/SalaryBatches/") + guid.ToString() + "_SB" + Path.GetExtension(salaryBatchViewModel.file.FileName));
            salaryBatchViewModel.file.SaveAs(ServerSavePath);

            SalaryBatch salaryBatch = new SalaryBatch();
            salaryBatch.month = DateTime.Now.Month;
            salaryBatch.year = DateTime.Now.Year;
            salaryBatch.count = 0;
            salaryBatch.total = 0;
            salaryBatch.active = 1;
            salaryBatch.created_by = user.id;
            salaryBatch.created_at = DateTime.Now;
            salaryBatch.notes = salaryBatchViewModel.notes;
            salaryBatch.file_path = ServerSavePath;

            db.SalaryBatches.Add(salaryBatch);
            db.SaveChanges();
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
                List<SalaryBatchDetail> importedSalaryBatchDetails = new List<SalaryBatchDetail>();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    SalaryBatchDetail salaryBatchDetail = new SalaryBatchDetail();
                    salaryBatchDetail.id = 0;
                    salaryBatchDetail.salary_batch_id = salaryBatch.id;

                    salaryBatchDetail.user_id = dt.Rows[i][0].ToString().ToInt();
                    salaryBatchDetail.bank_code = dt.Rows[i][2].ToString();
                    salaryBatchDetail.salary = dt.Rows[i][3].ToString() != null ? dt.Rows[i][3].ToString().ToDouble() : 0;
                    salaryBatchDetail.insurance_deductions = dt.Rows[i][4].ToString() != null ? dt.Rows[i][4].ToString().ToDouble():0;
                    salaryBatchDetail.tax_deductions = dt.Rows[i][5].ToString() != null ?dt.Rows[i][5].ToString().ToDouble():0;
                    salaryBatchDetail.absense_days = dt.Rows[i][6].ToString() != null?dt.Rows[i][6].ToString().ToInt():0;
                    salaryBatchDetail.absense_deductions = dt.Rows[i][7].ToString() != null ? dt.Rows[i][7].ToString().ToDouble():0;
                    salaryBatchDetail.gm_amount = dt.Rows[i][8].ToString() != null?dt.Rows[i][8].ToString().ToDouble():0;
                    salaryBatchDetail.reserved_amount = dt.Rows[i][9].ToString() != null ? dt.Rows[i][9].ToString().ToDouble():0;
                    salaryBatchDetail.addtional_hours = dt.Rows[i][10].ToString() != null ? dt.Rows[i][10].ToString().ToInt():0;
                    salaryBatchDetail.addtional_hours_amount = dt.Rows[i][11].ToString() != null ? dt.Rows[i][11].ToString().ToDouble():0;
                    salaryBatchDetail.total_kilos = dt.Rows[i][12].ToString() != null ? dt.Rows[i][12].ToString().ToDouble():0;
                    salaryBatchDetail.total_salary = dt.Rows[i][13].ToString() != null ? dt.Rows[i][13].ToString().ToDouble() : 0;
                    salaryBatchDetail.notes = dt.Rows[i][14].ToString();

                    salaryBatchDetail.active = (int)RowStatus.ACTIVE;
                    salaryBatchDetail.created_at = DateTime.Now;
                    salaryBatchDetail.created_by = user.id;

                    importedSalaryBatchDetails.Add(salaryBatchDetail);
                }
                db.SalaryBatchDetails.AddRange(importedSalaryBatchDetails);
                db.SaveChanges();

                salaryBatch.count = importedSalaryBatchDetails.Count();
                salaryBatch.total = importedSalaryBatchDetails.Select(im => im.total_salary).ToList().Sum();

                db.SaveChanges();
            }

            return Json(new { msg = "done" }, JsonRequestBehavior.AllowGet);
        }

        public FileResult Download(int id)
        {
            string path = db.SalaryBatches.Find(id).file_path;
            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            string fileName = Path.GetFileName(path);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
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