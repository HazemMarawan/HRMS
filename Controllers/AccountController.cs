using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HRMS.ViewModels;
using HRMS.Models;
using HRMS.Enum;

namespace HRMS.Controllers
{
    public class AccountController : Controller
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
    }
}