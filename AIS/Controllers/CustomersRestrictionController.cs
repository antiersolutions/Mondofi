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
    public class CustomersRestrictionController : Controller
    {
        private UsersContext db = new UsersContext();

        //
        // GET: /CustomersRestriction/

        public ActionResult Index()
        {
            var tabcustomersrestrictions = db.tabCustomersRestrictions.Include(c => c.Customers).Include(c => c.Restrictions);
            return View(tabcustomersrestrictions.ToList());
        }

        //
        // GET: /CustomersRestriction/Details/5

        public ActionResult Details(int id = 0)
        {
            CustomersRestrictions customersrestrictions = db.tabCustomersRestrictions.Find(id);
            if (customersrestrictions == null)
            {
                return HttpNotFound();
            }
            return View(customersrestrictions);
        }

        //
        // GET: /CustomersRestriction/Create

        public ActionResult Create()
        {
            ViewBag.CustomerId = new SelectList(db.tabCustomers, "CustomerId", "FirstName");
            ViewBag.RestrictionId = new SelectList(db.tabRestrictions, "RestrictionId", "Restriction");
            return View();
        }

        //
        // POST: /CustomersRestriction/Create

        [HttpPost]
        public ActionResult Create(CustomersRestrictions customersrestrictions)
        {
            if (ModelState.IsValid)
            {
                db.tabCustomersRestrictions.Add(customersrestrictions);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CustomerId = new SelectList(db.tabCustomers, "CustomerId", "FirstName", customersrestrictions.CustomerId);
            ViewBag.RestrictionId = new SelectList(db.tabRestrictions, "RestrictionId", "Restriction", customersrestrictions.RestrictionId);
            return View(customersrestrictions);
        }

        //
        // GET: /CustomersRestriction/Edit/5

        public ActionResult Edit(int id = 0)
        {
            CustomersRestrictions customersrestrictions = db.tabCustomersRestrictions.Find(id);
            if (customersrestrictions == null)
            {
                return HttpNotFound();
            }
            ViewBag.CustomerId = new SelectList(db.tabCustomers, "CustomerId", "FirstName", customersrestrictions.CustomerId);
            ViewBag.RestrictionId = new SelectList(db.tabRestrictions, "RestrictionId", "Restriction", customersrestrictions.RestrictionId);
            return View(customersrestrictions);
        }

        //
        // POST: /CustomersRestriction/Edit/5

        [HttpPost]
        public ActionResult Edit(CustomersRestrictions customersrestrictions)
        {
            if (ModelState.IsValid)
            {
                db.Entry(customersrestrictions).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CustomerId = new SelectList(db.tabCustomers, "CustomerId", "FirstName", customersrestrictions.CustomerId);
            ViewBag.RestrictionId = new SelectList(db.tabRestrictions, "RestrictionId", "Restriction", customersrestrictions.RestrictionId);
            return View(customersrestrictions);
        }

        //
        // GET: /CustomersRestriction/Delete/5

        public ActionResult Delete(int id = 0)
        {
            CustomersRestrictions customersrestrictions = db.tabCustomersRestrictions.Find(id);
            if (customersrestrictions == null)
            {
                return HttpNotFound();
            }
            return View(customersrestrictions);
        }

        //
        // POST: /CustomersRestriction/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            CustomersRestrictions customersrestrictions = db.tabCustomersRestrictions.Find(id);
            db.tabCustomersRestrictions.Remove(customersrestrictions);
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