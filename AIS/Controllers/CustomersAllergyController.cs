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
    public class CustomersAllergyController : Controller
    {
        private UsersContext db = new UsersContext();

        //
        // GET: /CustomersAllergy/

        public ActionResult Index()
        {
            var tabcustomersallergies = db.tabCustomersAllergies.Include(c => c.Customers);
            return View(tabcustomersallergies.ToList());
        }

        //
        // GET: /CustomersAllergy/Details/5

        public ActionResult Details(int id = 0)
        {
            CustomersAllergies customersallergies = db.tabCustomersAllergies.Find(id);
            if (customersallergies == null)
            {
                return HttpNotFound();
            }
            return View(customersallergies);
        }

        //
        // GET: /CustomersAllergy/Create

        public ActionResult Create()
        {
            ViewBag.CustomerId = new SelectList(db.tabCustomers, "CustomerId", "FirstName");
            return View();
        }

        //
        // POST: /CustomersAllergy/Create

        [HttpPost]
        public ActionResult Create(CustomersAllergies customersallergies)
        {
            if (ModelState.IsValid)
            {
                db.tabCustomersAllergies.Add(customersallergies);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CustomerId = new SelectList(db.tabCustomers, "CustomerId", "FirstName", customersallergies.CustomerId);
            return View(customersallergies);
        }

        //
        // GET: /CustomersAllergy/Edit/5

        public ActionResult Edit(int id = 0)
        {
            CustomersAllergies customersallergies = db.tabCustomersAllergies.Find(id);
            if (customersallergies == null)
            {
                return HttpNotFound();
            }
            ViewBag.CustomerId = new SelectList(db.tabCustomers, "CustomerId", "FirstName", customersallergies.CustomerId);
            return View(customersallergies);
        }

        //
        // POST: /CustomersAllergy/Edit/5

        [HttpPost]
        public ActionResult Edit(CustomersAllergies customersallergies)
        {
            if (ModelState.IsValid)
            {
                db.Entry(customersallergies).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CustomerId = new SelectList(db.tabCustomers, "CustomerId", "FirstName", customersallergies.CustomerId);
            return View(customersallergies);
        }

        //
        // GET: /CustomersAllergy/Delete/5

        public ActionResult Delete(int id = 0)
        {
            CustomersAllergies customersallergies = db.tabCustomersAllergies.Find(id);
            if (customersallergies == null)
            {
                return HttpNotFound();
            }
            return View(customersallergies);
        }

        //
        // POST: /CustomersAllergy/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            CustomersAllergies customersallergies = db.tabCustomersAllergies.Find(id);
            db.tabCustomersAllergies.Remove(customersallergies);
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