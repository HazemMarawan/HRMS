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

namespace HRMS.Controllers
{
    [CustomAuthenticationFilter]
    public class DepartmentController : BaseController
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
                var departmentData = (from department in db.Departments
                                  select new DepartmentViewModel
                                  {
                                      id = department.id,
                                      name = department.name,
                                      active = department.active,
                                      created_at = department.created_at
                                  }).Where(n => n.active == (int)RowStatus.ACTIVE);

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    departmentData = departmentData.Where(m => m.name.ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()));
                }

                if (!isA.SuperAdmin())
                {
                    departmentData = departmentData.Where(n => n.id == -1);
                }
                //total number of rows count     
                var displayResult = departmentData.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = departmentData.Count();

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
        [HttpPost]
        public JsonResult saveDepartment(DepartmentViewModel departmentViewModel)
        {

            if (departmentViewModel.id == 0)
            {
                Department department = AutoMapper.Mapper.Map<DepartmentViewModel, Department>(departmentViewModel);

                department.created_at = DateTime.Now.AddHours(-3);;
                department.created_by = Session["id"].ToString().ToInt();

                db.Departments.Add(department);
                db.SaveChanges();
            }
            else
            {

                Department oldDepartment = db.Departments.Find(departmentViewModel.id);

                oldDepartment.name = departmentViewModel.name;
                oldDepartment.active = departmentViewModel.active;
                oldDepartment.updated_by = Session["id"].ToString().ToInt();
                oldDepartment.updated_at = DateTime.Now.AddHours(-3);;

                db.SaveChanges();
            }

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult deleteDepartment(int id)
        {
            Department deleteDepartment = db.Departments.Find(id);
            deleteDepartment.active = (int)RowStatus.INACTIVE;
            deleteDepartment.deleted_by = Session["id"].ToString().ToInt();
            deleteDepartment.deleted_at = DateTime.Now.AddHours(-3);;
            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }
    }
}