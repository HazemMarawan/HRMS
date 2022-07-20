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

namespace HRMS.Controllers
{
    [CustomAuthenticationFilter]
    public class TaskManagementController : Controller
    {
        HRMSDBContext db = new HRMSDBContext();
        // GET: ProjectType
        public ActionResult Index(int? branch_id)
        {
            if(isA.SuperAdmin())
                return RedirectToAction("Index", "Dashboard");

            User currentUser = Session["user"] as User;

            TaskManagementViewModel taskManagementViewModel = new TaskManagementViewModel();
            taskManagementViewModel.allTasks = (from taskClassification in db.TaskClassifications
                                                     join userTask in db.UserTasks on taskClassification.id equals userTask.task_classification_id
                                                     join user in db.Users on userTask.user_id equals user.id
                                                     join from_user in db.Users on userTask.created_by equals from_user.id
                                                     select new UserTaskViewModel
                                                     {
                                                         id = userTask.id,
                                                         name = userTask.name,
                                                         task_classification_id = userTask.task_classification_id,
                                                         task_classification_name = taskClassification.name,
                                                         description = userTask.description,
                                                         full_name = user.full_name,
                                                         from_full_name = from_user.full_name,
                                                         is_favourite_by_owner = userTask.is_favourite_by_owner,
                                                         is_favourite_by_assignee = userTask.is_favourite_by_assignee,
                                                         is_favourite = ((userTask.is_favourite_by_owner == 1 && userTask.created_by == currentUser.id) || userTask.is_favourite_by_assignee == 1)?1:0,
                                                         active = userTask.active,
                                                         status = userTask.status,
                                                         user_id = userTask.user_id,
                                                         created_by = userTask.created_by,
                                                         created_at = userTask.created_at,
                                                         own_status = userTask.created_by == currentUser.id?1:0
                                                     }).Where(s=>(s.created_by == currentUser.id || s.user_id == currentUser.id) && s.active == (int)RowStatus.ACTIVE).ToList();

            //taskManagementViewModel.myTasks = (from taskClassification in db.TaskClassifications
            //                                         join userTask in db.UserTasks on taskClassification.id equals userTask.task_classification_id
            //                                         join user in db.Users on userTask.user_id equals user.id
            //                                         select new UserTaskViewModel
            //                                         {
            //                                             id = userTask.id,
            //                                             task_classification_name = taskClassification.name,
            //                                             description = userTask.description,
            //                                             full_name = user.full_name,
            //                                             is_favourite_by_owner = userTask.is_favourite_by_owner,
            //                                             is_favourite_by_assignee = userTask.is_favourite_by_assignee,
            //                                             active = userTask.active,
            //                                             user_id = userTask.user_id,
            //                                             created_by = userTask.created_by,
            //                                             created_at = userTask.created_at
            //                                         }).Where(s => s.user_id == currentUser.id && s.active == (int)RowStatus.ACTIVE).ToList();
            ViewBag.TaskClassifications = db.TaskClassifications.Where(s => s.active == (int)RowStatus.ACTIVE).Select(s => new { s.id, s.name }).ToList();
            ViewBag.Users = db.Users.Where(u => u.active == (int)RowStatus.ACTIVE && u.branch_id == currentUser.branch_id && u.id != currentUser.id).Select(u => new
            {
                u.id,
                u.full_name
            }).ToList();
            return View(taskManagementViewModel);
        }
        [HttpPost]
        public JsonResult saveUserTask(UserTaskViewModel userTaskViewModel)
        {

            if (userTaskViewModel.id == 0)
            {
                UserTask userTask = AutoMapper.Mapper.Map<UserTaskViewModel, UserTask>(userTaskViewModel);

                userTask.status = (int)TaskManagementStatus.New;
                userTask.active = (int)RowStatus.ACTIVE;
                userTask.created_at = DateTime.Now; ;
                userTask.created_by = Session["id"].ToString().ToInt();

                db.UserTasks.Add(userTask);
                db.SaveChanges();
            }
            else
            {

                UserTask oldUserTask = db.UserTasks.Find(userTaskViewModel.id);

                oldUserTask.name = userTaskViewModel.name;
                oldUserTask.description = userTaskViewModel.description;
                oldUserTask.user_id = userTaskViewModel.user_id;
                oldUserTask.task_classification_id = userTaskViewModel.task_classification_id;
                oldUserTask.updated_by = Session["id"].ToString().ToInt();
                oldUserTask.updated_at = DateTime.Now; ;

                db.SaveChanges();
            }

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult deleteUserTask(int id)
        {
            UserTask deleteUserTask = db.UserTasks.Find(id);
            deleteUserTask.active = (int)RowStatus.INACTIVE;
            deleteUserTask.deleted_by = Session["id"].ToString().ToInt();
            deleteUserTask.deleted_at = DateTime.Now; ;
            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult addToFavorites(int id)
        {
            User CurrentUser = Session["user"] as User;
            UserTask addToFavoritesTask = db.UserTasks.Find(id);
            
            if(addToFavoritesTask.created_by == CurrentUser.id)
                addToFavoritesTask.is_favourite_by_owner = addToFavoritesTask.is_favourite_by_owner == 1? 0 : 1;

            else
                addToFavoritesTask.is_favourite_by_assignee = addToFavoritesTask.is_favourite_by_assignee ==1 ? 0 : 1;

            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult changeStatus(UserTaskViewModel userTaskViewModel)
        {
            UserTask changedUserTask = db.UserTasks.Find(userTaskViewModel.id);
            changedUserTask.status = userTaskViewModel.status;
            
            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }
    }
}