using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AIS.Models;
using AISModels;
using System.Globalization;
using System.Threading;
using AIS.Helpers.Caching;
using  Microsoft.AspNet.Identity;

namespace AIS.Controllers
{
    //[Authorize(Roles = "SuperAdmin,Admin")]
    public class SettingController : Controller
    {
        private UsersContext db = new UsersContext();
        private ICacheManager cache = new MemoryCacheManager();

        //
        // GET: /ShiftHour/

        public ActionResult Index()
        {
            var companyUserManger = ApplicationUserManager.Create(db.Database.Connection.Database);
             var roles = companyUserManger.GetRoles(User.Identity.GetUserId<long>());

             if (roles.Contains("SuperAdmin") || roles.Contains("Admin"))
             {
                var logoSetting = db.tabSettings.Where(s => s.Name.Contains("Logo")).Single();
                ViewBag.Phone = db.tabSettings.Where(s => s.Name.Contains("Phone")).Single();
                ViewBag.OnlineResosL = db.tabSettings.Where(s => s.Name.Contains("OnlineResosL")).Single();
                ViewBag.Address = db.tabSettings.Where(s => s.Name.Contains("Address")).Single();
                ViewBag.ReplyToEmail = db.tabSettings.Where(s => s.Name.Contains("ReplyToEmail")).Single();
                ViewBag.BCCEmail = db.tabSettings.Where(s => s.Name.Contains("BCCEmail")).Single();
                ViewBag.Salutation = db.tabSettings.Where(s => s.Name.Contains("Salutation")).Single();
                ViewBag.timeSet = db.tabSettings.Where(s => s.Name.Contains("SetTime")).Single().Value;
                return View(logoSetting);
            }
            else
            {
                return RedirectToAction("FloorPlan", "FloorPlan"); 
            }
           
        }

        public ActionResult UpdateTimeSet(string time)
        {
            if (time!=null)
            {
                var timeSet = db.tabSettings.Where(s => s.Name.Contains("SetTime")).Single();
                timeSet.Value = time;
                db.Entry(timeSet).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                ClearSettingCache("SetTime");
            }
          
            return Json("Ok", JsonRequestBehavior.AllowGet);
        }


        public ActionResult UpdatePhone(string phone)
        {
            var phoneNumber = db.tabSettings.Where(s => s.Name.Contains("Phone")).Single();
            phoneNumber.Value = phone;
            db.Entry(phoneNumber).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            ClearSettingCache("Phone");

            return Json("Ok", JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateTabMatrix(string val)
        {
            var tabVal = db.tabSettings.Where(s => s.Name.Contains("TabVal")).Single();
            tabVal.Value = val;
            db.Entry(tabVal).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            ClearSettingCache("TabVal");

            return Json("Ok", JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateReplyToEmail(string ReplyToEmail)
        {
            var replyToEmail = db.tabSettings.Where(s => s.Name.Contains("ReplyToEmail")).Single();
            replyToEmail.Value = ReplyToEmail;
            db.Entry(replyToEmail).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            ClearSettingCache("ReplyToEmail");
            return Json("Ok", JsonRequestBehavior.AllowGet);
        }


        [ValidateInput(false)]
        public ActionResult UpdateSalutation(string Salutation)
        {
            var salutation = db.tabSettings.Where(s => s.Name.Contains("Salutation")).Single();
            salutation.Value = Salutation;
            db.Entry(salutation).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            ClearSettingCache("Salutation");
            return Json("Ok", JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateBCCEmail(string BCCEmail)
        {
            var bCCEmail = db.tabSettings.Where(s => s.Name.Contains("BCCEmail")).Single();
            bCCEmail.Value = BCCEmail;
            db.Entry(bCCEmail).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            ClearSettingCache("BCCEmail");
            return Json("Ok", JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateAddress(string address)
        {
            var getAddress = db.tabSettings.Where(s => s.Name.Contains("Address")).Single();
            getAddress.Value = address;
            db.Entry(getAddress).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            ClearSettingCache("Address");
            return Json("Ok", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Update(string timezone)
        {
            if (ModelState.IsValid)
            {
                var timezoneSetting = db.tabSettings.Where(s => s.Name.Contains("TimeZone")).Single();
                timezoneSetting.Value = timezone;
                db.Entry(timezoneSetting).State = System.Data.Entity.EntityState.Modified;

                db.SaveChanges();
                //ClearSettingCache("TimeZone");
                cache.Remove(string.Format(CacheKeys.FOOD_MENUSHIFT_SHIFTHOURS, User.Identity.GetDatabaseName()));
                UpdateShiftHoursCache();

            }
            return Json("ok", JsonRequestBehavior.AllowGet);

        }

        private void UpdateShiftHoursCache()
        {
            cache.RemoveByPattern(string.Format(CacheKeys.SETTING_BY_NAME_COMPANY_PATTERN, User.Identity.GetDatabaseName()));
            //cache.RemoveByPattern(CacheKeys.FOOD_MENUSHIFT_PATTERN);
            cache.RemoveByPattern(string.Format(CacheKeys.FOOD_MENUSHIFT_COMPANY_PATTERN, User.Identity.GetDatabaseName()));
            // update cache
            db.GetFoodMenuShifts();
            //db.GetMenuShiftHours();
        }

        private void ClearSettingCache(string settingName)
        {
            cache.RemoveByPattern(string.Format(CacheKeys.SETTING_BY_NAME_KEY, User.Identity.GetDatabaseName(), settingName));
        }

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);

            CultureInfo cultureInfo = CultureInfo.GetCultureInfo("en-IN");

            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
