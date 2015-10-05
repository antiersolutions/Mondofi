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
    public class FoodMenuShiftController : Controller
    {
        private UsersContext db = new UsersContext();

        //
        // GET: /FoodMenuShift/

        public ActionResult Index()
        {
            return View(db.tabFoodMenuShift.ToList());
        }

        //
        // GET: /FoodMenuShift/Details/5

        public ActionResult Details(int id = 0)
        {
            FoodMenuShift foodmenushift = db.tabFoodMenuShift.Find(id);
            if (foodmenushift == null)
            {
                return HttpNotFound();
            }
            return View(foodmenushift);
        }

        //
        // GET: /FoodMenuShift/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /FoodMenuShift/Create

        [HttpPost]
        public ActionResult Create(FoodMenuShift foodmenushift)
        {
            if (ModelState.IsValid)
            {
                db.tabFoodMenuShift.Add(foodmenushift);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(foodmenushift);
        }

        //
        // GET: /FoodMenuShift/Edit/5

        public ActionResult Edit(int id = 0)
        {
            FoodMenuShift foodmenushift = db.tabFoodMenuShift.Find(id);
            if (foodmenushift == null)
            {
                return HttpNotFound();
            }
            return View(foodmenushift);
        }

        //
        // POST: /FoodMenuShift/Edit/5

        [HttpPost]
        public ActionResult Edit(FoodMenuShift foodmenushift)
        {
            if (ModelState.IsValid)
            {
                db.Entry(foodmenushift).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(foodmenushift);
        }

        //
        // GET: /FoodMenuShift/Delete/5

        public ActionResult Delete(int id = 0)
        {
            FoodMenuShift foodmenushift = db.tabFoodMenuShift.Find(id);
            if (foodmenushift == null)
            {
                return HttpNotFound();
            }
            return View(foodmenushift);
        }

        //
        // POST: /FoodMenuShift/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            FoodMenuShift foodmenushift = db.tabFoodMenuShift.Find(id);
            db.tabFoodMenuShift.Remove(foodmenushift);
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