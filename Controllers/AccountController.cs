using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HRMS.ViewModels;
using HRMS.Models;
using HRMS.Enums;
using HRMS.Auth;

namespace HRMS.Controllers
{
    public class AccountController : BaseController
    {
        HRMSDBContext db = new HRMSDBContext();
        // GET: Account
        public ActionResult Login()
        {
            ViewBag.errorMsg = TempData["errorMsg"];
            return View();
        }
        [HttpPost]
        public ActionResult Login(UserViewModel userViewModel)
        {
            User user = db.Users.Where(u => u.user_name == userViewModel.user_name && u.password == userViewModel.password).FirstOrDefault();
            if(user != null)
            {
                if (user.active == (int)RowStatus.ACTIVE)
                {
                    Session["id"] = user.id;
                    Session["user_name"] = user.user_name;
                    Session["image"] = user.image;
                    Session["type"] = user.type;
                    Session["user"] = user;
                    Session["job_title"] = db.Jobs.Find(user.job_id).name ;
                    if(user.required_productivity == 1)
                        Session["required_productivity"] = 1;
                    else
                        Session["required_productivity"] = 0;

                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    TempData["errorMsg"] = "User is not Active";
                    return RedirectToAction("Login");
                }
            }
            else
            {
                TempData["errorMsg"] = "Invalid Username or Password";
                return RedirectToAction("Login");
            }
        }
        [HttpGet]
        public ActionResult Logout()
        {
            Session.Abandon();
            Session.Clear();
            return RedirectToAction("Login");
        }
        [CustomAuthenticationFilter]
        public JsonResult changePassword(ChangePasswordViewModel changePasswordViewModel)
        {
            User currentUSer = Session["user"] as User;
            if (currentUSer.password == changePasswordViewModel.old_password)
            {
                if(changePasswordViewModel.new_password == changePasswordViewModel.new_password_confirm)
                {
                    User changedUser = db.Users.Find(currentUSer.id);
                    changedUser.password = changePasswordViewModel.new_password;
                    db.SaveChanges();
                    return Json(new { message = "done", success = true }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return Json(new { message = "Password doesn't match", success = false }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { message="Wrong Old Password", success =false }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}