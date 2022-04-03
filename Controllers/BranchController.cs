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
    public class BranchController : BaseController
    {
        HRMSDBContext db = new HRMSDBContext();
        // GET: Branch
        public ActionResult Index()
        {
            User user = Session["user"] as User;
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
                var branchData = (from branch in db.Branches
                                  select new BranchViewModel
                                  {
                                      id = branch.id,
                                      name = branch.name,
                                      active = branch.active,
                                      created_at = branch.created_at,
                                      projects = (from project in db.Projects
                                                  join branchProject in db.BranchProjects on project.id equals branchProject.project_id
                                                  select new BranchProjectViewModel
                                                  {
                                                      id = project.id,
                                                      project_name = project.name,
                                                      branch_id = branchProject.branch_id
                                                  }).Where(p => p.branch_id == branch.id).ToList()
                                  }).Where(n => n.active == (int)RowStatus.ACTIVE);

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    branchData = branchData.Where(m => m.name.ToLower().Contains(searchValue.ToLower()) || m.id.ToString().ToLower().Contains(searchValue.ToLower()));
                }
                if (!isA.SuperAdmin())
                {
                    branchData = branchData.Where(n => n.id == -1);
                }
                //total number of rows count     
                var displayResult = branchData.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = branchData.Count();

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
        public JsonResult saveBranch(BranchViewModel branchViewModel)
        {

            if (branchViewModel.id == 0)
            {
                Branch branch = AutoMapper.Mapper.Map<BranchViewModel, Branch>(branchViewModel);

                branch.created_at = DateTime.Now;
                branch.created_by = Session["id"].ToString().ToInt();

                db.Branches.Add(branch);
                db.SaveChanges();
            }
            else
            {

                Branch oldBranch = db.Branches.Find(branchViewModel.id);

                oldBranch.name = branchViewModel.name;
                oldBranch.active = branchViewModel.active;
                oldBranch.updated_by = Session["id"].ToString().ToInt();
                oldBranch.updated_at = DateTime.Now;

                db.SaveChanges();
            }

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult deleteBranch(int id)
        {
            Branch deleteBranch = db.Branches.Find(id);
            deleteBranch.active = (int)RowStatus.INACTIVE;
            deleteBranch.deleted_at = DateTime.Now;
            deleteBranch.deleted_by = Session["id"].ToString().ToInt();

            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult saveBranchProject(BranchProjectViewModel branchProjectViewModel)
        {
            BranchProject branchProject = AutoMapper.Mapper.Map<BranchProjectViewModel, BranchProject>(branchProjectViewModel);
            branchProject.created_by = Session["id"].ToString().ToInt();
            branchProject.created_at = DateTime.Now;

            db.BranchProjects.Add(branchProject);
            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult getProjects(int id)
        {
            List<int?> selectedProjects = db.BranchProjects.Where(s => s.branch_id == id).Select(s => s.project_id).ToList();

            List<ProjectViewModel> projects = db.Projects.Where(p => !selectedProjects.Contains(p.id)).Select(p => new ProjectViewModel
            {
                id = p.id,
                name = p.name
            }).ToList();


            db.SaveChanges();

            return Json(new { projects = projects }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult Show()
        {
            if (!(isA.SuperAdmin() || isA.BranchAdmin()))
                return RedirectToAction("Index", "Dashboard");

            var branchData = (from branch in db.Branches
                              select new BranchViewModel
                              {
                                  id = branch.id,
                                  name = branch.name,
                                  active = branch.active,
                                  created_at = branch.created_at
                              }).Where(n => n.active == (int)RowStatus.ACTIVE);

            if (isA.BranchAdmin())
            {
                User currentUser = Session["user"] as User;
                branchData = branchData.Where(b => b.id == currentUser.branch_id);
            }
            List<BranchViewModel> branches = branchData.ToList();
            return View(branches);
        }

        [HttpGet]
        public ActionResult Details(int id)
        {
            ViewBag.branch_id = id;
            ViewBag.branch_name = db.Branches.Find(id).name;
            return View();
        }
    }
}