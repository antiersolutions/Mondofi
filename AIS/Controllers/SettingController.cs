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

namespace AIS.Controllers
{
    [Authorize]
    public class SettingController : Controller
    {
        private UsersContext db = new UsersContext();
        private ICacheManager cache = new MemoryCacheManager();

        //
        // GET: /ShiftHour/

        public ActionResult Index()
        {
            var logoSetting = db.tabSettings.Where(s => s.Name.Contains("Logo")).Single();
            ViewBag.Phone = db.tabSettings.Where(s => s.Name.Contains("Phone")).Single();
            ViewBag.Address = db.tabSettings.Where(s => s.Name.Contains("Address")).Single();

            return View(logoSetting);
        }

        public ActionResult UpdatePhone(string phone)
        {
            var phoneNumber = db.tabSettings.Where(s => s.Name.Contains("Phone")).Single();
            phoneNumber.Value = phone;
            db.Entry(phoneNumber).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return Json("Ok", JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateAddress(string address)
        {
            var getAddress = db.tabSettings.Where(s => s.Name.Contains("Address")).Single();
            getAddress.Value = address;
            db.Entry(getAddress).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return Json("Ok", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Update(List<WeekDayShift> hours, string timezone)
        {
            using (var context = new UsersContext())
            {
                // delete existing records
                context.Database.ExecuteSqlCommand(@"truncate table MenuShiftHours");
            }

            bool isSucess = false;
            if (ModelState.IsValid)
            {
                foreach (var day in hours)
                {
                    var breakfast = new MenuShiftHours
                    {
                        DayId = day.DayId,
                        FoodMenuShiftId = 1,
                        OpenAt = day.BreakfastOpen == null ? null : Convert.ToDateTime(day.BreakfastOpen, CultureInfo.CurrentCulture).ToString("hh:mm tt"),
                        CloseAt = day.BreakfastClose == null ? null : Convert.ToDateTime(day.BreakfastClose, CultureInfo.CurrentCulture).ToString("hh:mm tt"),
                        IsNext = day.BreakfastClose == null ? 0 : Convert.ToDateTime(day.BreakfastClose, CultureInfo.CurrentCulture).Date == DateTime.Now.AddDays(1).Date ? 1 : 0
                    };

                    db.tabMenuShiftHours.Add(breakfast);

                    var brunch = new MenuShiftHours
                    {
                        DayId = day.DayId,
                        FoodMenuShiftId = 2,
                        OpenAt = day.BrunchOpen == null ? null : Convert.ToDateTime(day.BrunchOpen, CultureInfo.CurrentCulture).ToString("hh:mm tt"),
                        CloseAt = day.BrunchClose == null ? null : Convert.ToDateTime(day.BrunchClose, CultureInfo.CurrentCulture).ToString("hh:mm tt"),
                        IsNext = day.BrunchClose == null ? 0 : Convert.ToDateTime(day.BrunchClose, CultureInfo.CurrentCulture).Date == DateTime.Now.AddDays(1).Date ? 1 : 0
                    };

                    db.tabMenuShiftHours.Add(brunch);

                    var lunch = new MenuShiftHours
                    {
                        DayId = day.DayId,
                        FoodMenuShiftId = 3,
                        OpenAt = day.LunchOpen == null ? null : Convert.ToDateTime(day.LunchOpen, CultureInfo.CurrentCulture).ToString("hh:mm tt"),
                        CloseAt = day.LunchClose == null ? null : Convert.ToDateTime(day.LunchClose, CultureInfo.CurrentCulture).ToString("hh:mm tt"),
                        IsNext = day.LunchClose == null ? 0 : Convert.ToDateTime(day.LunchClose, CultureInfo.CurrentCulture).Date == DateTime.Now.AddDays(1).Date ? 1 : 0
                    };

                    db.tabMenuShiftHours.Add(lunch);

                    var dinner = new MenuShiftHours
                    {
                        DayId = day.DayId,
                        FoodMenuShiftId = 4,
                        OpenAt = day.DinnerOpen == null ? null : Convert.ToDateTime(day.DinnerOpen, CultureInfo.CurrentCulture).ToString("hh:mm tt"),
                        CloseAt = day.DinnerClose == null ? null : Convert.ToDateTime(day.DinnerClose, CultureInfo.CurrentCulture).ToString("hh:mm tt"),
                        IsNext = day.DinnerClose == null ? 0 : Convert.ToDateTime(day.DinnerClose, CultureInfo.CurrentCulture).Date == DateTime.Now.AddDays(1).Date ? 1 : 0
                    };
                    db.tabMenuShiftHours.Add(dinner);

                }

                var timezoneSetting = db.tabSettings.Where(s => s.Name.Contains("TimeZone")).Single();
                timezoneSetting.Value = timezone;
                db.Entry(timezoneSetting).State = System.Data.Entity.EntityState.Modified;

                db.SaveChanges();

                UpdateShiftHoursCache();

                isSucess = true;
                return Json(new
                {
                    IsSucess = isSucess,
                });
            }
            return Json(new
            {
                IsSucess = isSucess,
            });

        }

        private void UpdateShiftHoursCache()
        {
            cache.RemoveByPattern(string.Format(CacheKeys.SETTING_BY_NAME_COMPANY_PATTERN, User.Identity.GetDatabaseName()));
            //cache.RemoveByPattern(CacheKeys.FOOD_MENUSHIFT_PATTERN);
            cache.RemoveByPattern(string.Format(CacheKeys.FOOD_MENUSHIFT_COMPANY_PATTERN, User.Identity.GetDatabaseName()));
            // update cache
            db.GetFoodMenuShifts();
            db.GetMenuShiftHours();
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
