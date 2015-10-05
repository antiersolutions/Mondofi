using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AISModels;
using AIS.Models;

namespace AIS.Controllers
{
    [Authorize]
    public class MenuShiftHourController : Controller
    {
        private UsersContext db = new UsersContext();

        //
        // GET: /MenuShiftHour/

        //public ActionResult Index()
        //{
        //    return View(db.tabMenuShiftHours.ToList());
        //}

        ////
        //// GET: /MenuShiftHour/Details/5

        //public ActionResult Details(int id = 0)
        //{
        //    MenuShiftHours menushifthours = db.tabMenuShiftHours.Find(id);
        //    if (menushifthours == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(menushifthours);
        //}

        //
        // GET: /MenuShiftHour/Create

        public ActionResult Create()
        {
            ViewBag.WeekDays = db.tabWeekDays.ToList();
            ViewBag.FoodMenuShifts = db.tabFoodMenuShift.ToList();

            return View();
        }

        public JsonResult GetShiftRecords()
        {
            bool isSucess = false;
            var data = db.tabMenuShiftHours.ToList();
            var newdata = data.Select(p => new MenuShiftHours
            {
                //ShiftHourId = p.ShiftHourId,
                //DayId = p.DayId,
                //FoodMenuShiftId = p.FoodMenuShiftId,
                OpenAt = p.OpenAt,
                CloseAt = p.CloseAt
            }).ToList();
            isSucess = true;
            return Json(new {shitData= newdata, IsSucess = isSucess}, JsonRequestBehavior.AllowGet);
        }
        //
        // POST: /MenuShiftHour/Create

        [HttpPost]
        public JsonResult Create(List<WeekDayShift> model)
        {
            using (var context = new UsersContext())
            {
                // delete existing records
                context.Database.ExecuteSqlCommand(@"truncate table [AIS].[dbo].[MenuShiftHours]");
            }
            bool isSucess = false;
            if (ModelState.IsValid)
            {
                foreach (var day in model)
                {
                    var breakfast = new MenuShiftHours
                    {
                        DayId = day.DayId,
                        FoodMenuShiftId = 1,
                        OpenAt = day.BreakfastOpen,
                        CloseAt = day.BreakfastClose
                    };

                    db.tabMenuShiftHours.Add(breakfast);

                    var brunch = new MenuShiftHours
                    {
                        DayId = day.DayId,
                        FoodMenuShiftId = 2,
                        OpenAt = day.BrunchOpen,
                        CloseAt = day.BrunchClose
                    };

                    db.tabMenuShiftHours.Add(brunch);

                    var lunch = new MenuShiftHours
                    {
                        DayId = day.DayId,
                        FoodMenuShiftId = 3,
                        OpenAt = day.LunchOpen,
                        CloseAt = day.LunchClose
                    };

                    db.tabMenuShiftHours.Add(lunch);

                    var dinner = new MenuShiftHours
                    {
                        DayId = day.DayId,
                        FoodMenuShiftId = 4,
                        OpenAt = day.DinnerOpen,
                        CloseAt = day.DinnerClose
                    };
                    db.tabMenuShiftHours.Add(dinner);
                }

                db.SaveChanges();
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

        //
        // GET: /MenuShiftHour/Edit/5
        [HttpGet]
        public ActionResult Edit()
        {
            ViewBag.WeekDays = db.tabWeekDays.ToList();
            ViewBag.FoodMenuShifts = db.tabFoodMenuShift.ToList();
            return View();
        }

        //
        // POST: /MenuShiftHour/Edit/5

        [HttpPost]
        public JsonResult Update(List<WeekDayShift> model)
        {

            using (var context = new UsersContext())
            {
                // delete existing records
                context.Database.ExecuteSqlCommand(@"truncate table [AIS].[dbo].[MenuShiftHours]");
            }

            bool isSucess = false;
            if (ModelState.IsValid)
            {
                foreach (var day in model)
                {
                    var breakfast = new MenuShiftHours
                    {
                        DayId = day.DayId,
                        FoodMenuShiftId = 1,
                        OpenAt = day.BreakfastOpen,
                        CloseAt = day.BreakfastClose
                    };

                    db.tabMenuShiftHours.Add(breakfast);

                    var brunch = new MenuShiftHours
                    {
                        DayId = day.DayId,
                        FoodMenuShiftId = 2,
                        OpenAt = day.BrunchOpen,
                        CloseAt = day.BrunchClose
                    };

                    db.tabMenuShiftHours.Add(brunch);

                    var lunch = new MenuShiftHours
                    {
                        DayId = day.DayId,
                        FoodMenuShiftId = 3,
                        OpenAt = day.LunchOpen,
                        CloseAt = day.LunchClose
                    };

                    db.tabMenuShiftHours.Add(lunch);

                    var dinner = new MenuShiftHours
                    {
                        DayId = day.DayId,
                        FoodMenuShiftId = 4,
                        OpenAt = day.DinnerOpen,
                        CloseAt = day.DinnerClose
                    };

                    db.tabMenuShiftHours.Add(dinner);

                }

                db.SaveChanges();
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

        //
        // GET: /MenuShiftHour/Delete/5

        public ActionResult Delete(int id = 0)
        {
            MenuShiftHours menushifthours = db.tabMenuShiftHours.Find(id);
            if (menushifthours == null)
            {
                return HttpNotFound();
            }
            return View(menushifthours);
        }

        //
        // POST: /MenuShiftHour/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            MenuShiftHours menushifthours = db.tabMenuShiftHours.Find(id);
            db.tabMenuShiftHours.Remove(menushifthours);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}