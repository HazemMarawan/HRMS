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
                var search_productivity_type = Request.Form.GetValues("columns[1][search][value]")[0];
                var search_work_place = Request.Form.GetValues("columns[2][search][value]")[0];
                var from_date = Request.Form.GetValues("columns[3][search][value]")[0];
                var to_date = Request.Form.GetValues("columns[4][search][value]")[0];
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                // Getting all data    
                var productivityData = (from user in db.Users
                                        join userProject in db.UserProjects on user.id equals userProject.user_id
                                        join project in db.Projects on userProject.project_id equals project.id
                                        join branchProject in db.BranchProjects on project.id equals branchProject.project_id
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
                                            team_leader_id = user.team_leader_id
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
                                       join project in db.Projects on userProject.project_id equals project.id
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
                                           status = userProject.status
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
    }
}