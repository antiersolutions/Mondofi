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
    public class WeekDayController : Controller
    {
        private UsersContext db = new UsersContext();

        //
        // GET: /WeekDay/

        public ActionResult Index()
        {
            return View(db.tabWeekDays.ToList());
        }

        //
        // GET: /WeekDay/Details/5

        public ActionResult Details(int id = 0)
        {
            WeekDays weekdays = db.tabWeekDays.Find(id);
            if (weekdays == null)
            {
                return HttpNotFound();
            }
            return View(weekdays);
        }

        //
        // GET: /WeekDay/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /WeekDay/Create

        [HttpPost]
        public ActionResult Create(WeekDays weekdays)
        {
            if (ModelState.IsValid)
            {
                db.tabWeekDays.Add(weekdays);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(weekdays);
        }

        //
        // GET: /WeekDay/Edit/5

        public ActionResult Edit(int id = 0)
        {
            WeekDays weekdays = db.tabWeekDays.Find(id);
            if (weekdays == null)
            {
                return HttpNotFound();
            }
            return View(weekdays);
        }

        //
        // POST: /WeekDay/Edit/5

        [HttpPost]
        public ActionResult Edit(WeekDays weekdays)
        {
            if (ModelState.IsValid)
            {
                db.Entry(weekdays).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(weekdays);
        }

        //
        // GET: /WeekDay/Delete/5

        public ActionResult Delete(int id = 0)
        {
            WeekDays weekdays = db.tabWeekDays.Find(id);
            if (weekdays == null)
            {
                return HttpNotFound();
            }
            return View(weekdays);
        }

        //
        // POST: /WeekDay/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            WeekDays weekdays = db.tabWeekDays.Find(id);
            db.tabWeekDays.Remove(weekdays);
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