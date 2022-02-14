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
    public class AssetController : Controller
    {
        HRMSDBContext db = new HRMSDBContext();
        // GET: ProjectType
        public ActionResult Index(int? branch_id)
        {
            User currentUser = Session["user"] as User;
            if (!(isA.SuperAdmin()
                || (isA.BranchAdmin() && (currentUser.branch_id == branch_id || branch_id == null))
                ))
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
                var assetsData = (from asset in db.Assets
                                  join user in db.Users on asset.created_by equals user.id
                                  select new AssetViewModel
                                  {
                                      id = asset.id,
                                      name = asset.name,
                                      code = asset.code,
                                      notes = asset.notes,
                                      branch_id = user.branch_id,
                                      active = asset.active,
                                      created_at = asset.created_at
                                  }).Where(n => n.active == (int)RowStatus.ACTIVE);

                //Search    
                if (!string.IsNullOrEmpty(searchValue))
                {
                    assetsData = assetsData.Where(m => m.name.ToLower().Contains(searchValue.ToLower())
                    || m.id.ToString().ToLower().Contains(searchValue.ToLower())
                    || m.code.ToString().ToLower().Contains(searchValue.ToLower())
                    || m.notes.ToString().ToLower().Contains(searchValue.ToLower())
                    );
                }

                if(isA.SuperAdmin())
                {
                    if(branch_id != null)
                    {
                        assetsData = assetsData.Where(a => a.branch_id == branch_id);
                    }
                }

                if (isA.BranchAdmin())
                {
                    assetsData = assetsData.Where(a => a.branch_id == currentUser.branch_id);
               
                }
                //total number of rows count     
                var displayResult = assetsData.OrderByDescending(u => u.id).Skip(skip)
                     .Take(pageSize).ToList();
                var totalRecords = assetsData.Count();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = displayResult

                }, JsonRequestBehavior.AllowGet);

            }
            if (isA.BranchAdmin())
                ViewBag.branch_id = currentUser.branch_id;
            else
                ViewBag.branch_id = branch_id;

            if (branch_id != null)
                ViewBag.branch_name = db.Branches.Find(branch_id).name;
            else
                ViewBag.branch_name = "Company";

            return View();
        }
        [HttpPost]
        public JsonResult saveAsset(AssetViewModel assetViewModel)
        {

            if (assetViewModel.id == 0)
            {
                Asset asset = AutoMapper.Mapper.Map<AssetViewModel, Asset>(assetViewModel);

                asset.active = (int)RowStatus.ACTIVE;
                asset.created_at = DateTime.Now;
                asset.created_by = Session["id"].ToString().ToInt();

                db.Assets.Add(asset);
                db.SaveChanges();
            }
            else
            {

                Asset oldAsset = db.Assets.Find(assetViewModel.id);

                oldAsset.name = assetViewModel.name;
                oldAsset.code = assetViewModel.code;
                oldAsset.notes = assetViewModel.notes;
                oldAsset.active = assetViewModel.active;
                oldAsset.updated_by = Session["id"].ToString().ToInt();
                oldAsset.updated_at = DateTime.Now;

                db.SaveChanges();
            }

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult deleteAsset(int id)
        {
            Asset deleteAsset = db.Assets.Find(id);
            deleteAsset.active = (int)RowStatus.INACTIVE;
            deleteAsset.deleted_by = Session["id"].ToString().ToInt();
            deleteAsset.deleted_at = DateTime.Now;
            db.SaveChanges();

            return Json(new { message = "done" }, JsonRequestBehavior.AllowGet);
        }
    }
}