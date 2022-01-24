using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HRMS.Models;
using HRMS.ViewModels;
using HRMS.Auth;
using HRMS.Enum;
using System.IO;
using HRMS.Helpers;

namespace HRMS.Controllers
{
    [CustomAuthenticationFilter]
    public class UserController : Controller
    {
        HRMSDBContext db = new HRMSDBContext();
        // GET: User
        public ActionResult Index(int? branch_id)
        {
            User currentUser = Session["user"] as User;
            if (!(isA.SuperAdmin() || (isA.BranchAdmin() && branch_id == currentUser.branch_id) || (isA.BranchAdmin() && branch_id == null)))
                return RedirectToAction("Index", "Dashboard");

            if (Request.IsAjaxRequest())
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();
                var search_id_type = Request.Form.GetValues("columns[0][search][value]")[0];
                var search_nationality_id = Request.Form.GetValues("columns[1][search][value]")[0];
                var search_job_id = Request.Form.GetValues("columns[2][search][value]")[0];
                var search_gender = Request.Form.GetValues("columns[3][search][value]")[0];
                var search_type = Request.Form.GetValues("columns[4][search][value]")[0];
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                // Getting all data    
                var userData = (from user in db.Users
                                join idtype in db.IDTypes on user.id_type equals idtype.id
                                join nationality in db.Nationalities on user.nationality_id equals nationality.id
                                join branch in db.Branches on user.branch_id equals branch.id
                                join department in db.Departments on user.department_id equals department.id
                                join job in db.Jobs on user.job_id equals job.id
                                select new UserViewModel
                                {
                                    id = user.id,
                                    code = user.code,
                                    user_name = user.user_name,
                                    full_name = user.first_name + " " + user.middle_name + " " + user.last_name,
                                    first_name = user.first_name,
                                    middle_name = user.middle_name,
                                    last_name = user.last_name,
                                    password = user.password,
                                    id_type = user.id_type,
                                    id_type_name = idtype.name,
                                    id_number = user.id_number,
                                    birth_date = user.birth_date,
                                    phone = user.phone,
                                    address = user.address,
                                    nationality_id = user.nationality_id,
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
                                }).Where(s => s.active == (int)RowStatus.ACTIVE);

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    userData = userData.Where(
                        m => m.full_name.ToLower().Contains(searchValue.ToLower()) ||
                        m.code.ToLower().Contains(searchValue.ToLower()) ||
                        m.phone.ToLower().Contains(searchValue.ToLower()) ||
                        m.address.ToLower().Contains(searchValue.ToLower()) ||
                        m.id.ToString().ToLower().Contains(searchValue.ToLower()) ||
                        m.nationality_name.ToLower().Contains(searchValue.ToLower()) ||
                        m.branch_name.ToLower().Contains(searchValue.ToLower()) ||
                        m.notes.ToLower().Contains(searchValue.ToLower())
                     );
                }

                if (isA.SuperAdmin())
                {
                    if (branch_id != null)
                    {
                        userData = userData.Where(u => u.branch_id == branch_id);
                    }
                }
                else
                {
                    if (isA.BranchAdmin() && (branch_id == currentUser.branch_id || branch_id == null))
                    {
                        userData = userData.Where(u => u.branch_id == currentUser.branch_id);
                    }
                    else
                    {
                        userData = userData.Where(u => u.branch_id == -1);
                    }
                }

                if (!string.IsNullOrEmpty(search_id_type))
                {
                    int search_id_type_int = int.Parse(search_id_type);
                    userData = userData.Where(s => s.id_type == search_id_type_int);
                }

                if (!string.IsNullOrEmpty(search_nationality_id))
                {
                    int search_nationality_id_int = int.Parse(search_nationality_id);
                    userData = userData.Where(s => s.nationality_id == search_nationality_id_int);
                }

                if (!string.IsNullOrEmpty(search_job_id))
                {
                    int search_job_id_int = int.Parse(search_job_id);
                    userData = userData.Where(s => s.job_id == search_job_id_int);

                }
                if (!string.IsNullOrEmpty(search_gender))
                {
                    int search_gender_int = int.Parse(search_gender);
                    userData = userData.Where(s => s.gender == search_gender_int);
                }

                if (!string.IsNullOrEmpty(search_type))
                {
                    int search_type_int = int.Parse(search_type);
                    userData = userData.Where(s => s.type == search_type_int);
                }

                //total number of rows count     
                var displayResult = userData.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = userData.Count();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = displayResult

                }, JsonRequestBehavior.AllowGet);

            }
            ViewBag.IDTypes = db.IDTypes.Where(p => p.active == (int)RowStatus.ACTIVE).Select(p => new { p.id, p.name }).ToList();
            ViewBag.Nationalities = db.Nationalities.Where(p => p.active == (int)RowStatus.ACTIVE).Select(p => new { p.id, p.name }).ToList();
            ViewBag.Branches = db.Branches.Where(p => p.active == (int)RowStatus.ACTIVE).Select(p => new { p.id, p.name }).ToList();
            ViewBag.Departments = db.Departments.Where(p => p.active == (int)RowStatus.ACTIVE).Select(p => new { p.id, p.name }).ToList();
            ViewBag.Jobs = db.Jobs.Where(p => p.active == (int)RowStatus.ACTIVE).Select(p => new { p.id, p.name }).ToList();

            ViewBag.branchId = branch_id;
            if (branch_id != null)
                ViewBag.branchName = db.Branches.Where(b => b.id == branch_id).FirstOrDefault().name;
            return View();
        }

        [HttpPost]
        public JsonResult saveUser(UserViewModel userVM)
        {
            if (userVM.id == 0)
            {

                User user = AutoMapper.Mapper.Map<UserViewModel, User>(userVM);
                if (HRMS.Auth.isA.BranchAdmin())
                {
                    User currentUser = Session["user"] as User;
                    user.branch_id = currentUser.branch_id;
                }
                user.created_at = DateTime.Now;
                user.created_by = Session["id"].ToString().ToInt();

                if (userVM.image != null)
                {
                    Guid guid = Guid.NewGuid();
                    var InputFileName = Path.GetFileName(userVM.image.FileName);
                    var ServerSavePath = Path.Combine(Server.MapPath("~/Uploads/Profile/") + guid.ToString() + "_Profile" + Path.GetExtension(userVM.image.FileName));
                    userVM.image.SaveAs(ServerSavePath);
                    user.image = "/Uploads/Profile/" + guid.ToString() + "_Profile" + Path.GetExtension(userVM.image.FileName);
                }
                db.Users.Add(user);
                db.SaveChanges();
            }
            else
            {

                User oldUser = db.Users.Find(userVM.id);

                oldUser.code = userVM.code;
                oldUser.user_name = userVM.user_name;
                oldUser.password = userVM.password;
                oldUser.first_name = userVM.first_name;
                oldUser.middle_name = userVM.middle_name;
                oldUser.last_name = userVM.last_name;
                oldUser.id_type = userVM.id_type;
                oldUser.id_number = userVM.id_number;
                oldUser.birth_date = userVM.birth_date;
                oldUser.phone = userVM.phone;
                oldUser.address = userVM.address;
                oldUser.nationality_id = userVM.nationality_id;

                if (HRMS.Auth.isA.SuperAdmin())
                    oldUser.branch_id = userVM.branch_id;

                oldUser.department_id = userVM.department_id;
                oldUser.job_id = userVM.job_id;
                oldUser.gender = userVM.gender;
                oldUser.hiring_date = userVM.hiring_date;
                oldUser.notes = userVM.notes;
                oldUser.type = userVM.type;
                oldUser.active = userVM.active;



                if (userVM.image != null)
                {
                    //if (oldUser.image != null)
                    //    System.IO.File.Delete(oldUser.image);

                    Guid guid = Guid.NewGuid();
                    var InputFileName = Path.GetFileName(userVM.image.FileName);
                    var ServerSavePath = Path.Combine(Server.MapPath("~/Uploads/Profile/") + guid.ToString() + "_Profile" + Path.GetExtension(userVM.image.FileName));
                    userVM.image.SaveAs(ServerSavePath);
                    oldUser.image = "/Uploads/Profile/" + guid.ToString() + "_Profile" + Path.GetExtension(userVM.image.FileName);
                }
                oldUser.updated_at = DateTime.Now;
                oldUser.updated_by = Session["id"].ToString().ToInt();
                db.SaveChanges();
            }

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult deleteUser(int id)
        {
            User deleteUser = db.Users.Find(id);
            deleteUser.active = (int)RowStatus.INACTIVE;
            deleteUser.deleted_by = Session["id"].ToString().ToInt();

            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult checkUsernameAvailability(string user_name, int id = 0)
        {
            if (id != 0)
            {
                var oldUsername = db.Users.Find(id).user_name;
                if (oldUsername == user_name)
                    return Json(new { message = "Valid Username", is_valid = true }, JsonRequestBehavior.AllowGet);

            }
            var checkAvailabilty = db.Users.Any(s => s.user_name == user_name);
            if (checkAvailabilty)
            {
                return Json(new { message = "Username Already Taken", is_valid = false }, JsonRequestBehavior.AllowGet);

            }

            return Json(new { message = "Valid Username", is_valid = true }, JsonRequestBehavior.AllowGet);
        }
    }
}