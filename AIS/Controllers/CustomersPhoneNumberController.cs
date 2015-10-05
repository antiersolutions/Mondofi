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
    public class CustomersPhoneNumberController : Controller
    {
        private UsersContext db = new UsersContext();

        //
        // GET: /CustomersPhoneNumber/

        public ActionResult Index()
        {
            var tabcustomersphonenumbers = db.tabCustomersPhoneNumbers.Include(c => c.Customers).Include(c => c.PhoneTypes);
            return View(tabcustomersphonenumbers.ToList());
        }

        //
        // GET: /CustomersPhoneNumber/Details/5

        public ActionResult Details(int id = 0)
        {
            CustomersPhoneNumbers customersphonenumbers = db.tabCustomersPhoneNumbers.Find(id);
            if (customersphonenumbers == null)
            {
                return HttpNotFound();
            }
            return View(customersphonenumbers);
        }

        //
        // GET: /CustomersPhoneNumber/Create

        public ActionResult Create()
        {
            ViewBag.CustomerId = new SelectList(db.tabCustomers, "CustomerId", "FirstName");
            ViewBag.PhoneTypeId = new SelectList(db.tabPhoneTypes, "PhoneTypeId", "PhoneType");
            return View();
        }

        //
        // POST: /CustomersPhoneNumber/Create

        [HttpPost]
        public ActionResult Create(CustomersPhoneNumbers customersphonenumbers)
        {
            if (ModelState.IsValid)
            {
                db.tabCustomersPhoneNumbers.Add(customersphonenumbers);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CustomerId = new SelectList(db.tabCustomers, "CustomerId", "FirstName", customersphonenumbers.CustomerId);
            ViewBag.PhoneTypeId = new SelectList(db.tabPhoneTypes, "PhoneTypeId", "PhoneType", customersphonenumbers.PhoneTypeId);
            return View(customersphonenumbers);
        }

        //
        // GET: /CustomersPhoneNumber/Edit/5

        public ActionResult Edit(int id = 0)
        {
            CustomersPhoneNumbers customersphonenumbers = db.tabCustomersPhoneNumbers.Find(id);
            if (customersphonenumbers == null)
            {
                return HttpNotFound();
            }
            ViewBag.CustomerId = new SelectList(db.tabCustomers, "CustomerId", "FirstName", customersphonenumbers.CustomerId);
            ViewBag.PhoneTypeId = new SelectList(db.tabPhoneTypes, "PhoneTypeId", "PhoneType", customersphonenumbers.PhoneTypeId);
            return View(customersphonenumbers);
        }

        //
        // POST: /CustomersPhoneNumber/Edit/5

        [HttpPost]
        public ActionResult Edit(CustomersPhoneNumbers customersphonenumbers)
        {
            if (ModelState.IsValid)
            {
                db.Entry(customersphonenumbers).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CustomerId = new SelectList(db.tabCustomers, "CustomerId", "FirstName", customersphonenumbers.CustomerId);
            ViewBag.PhoneTypeId = new SelectList(db.tabPhoneTypes, "PhoneTypeId", "PhoneType", customersphonenumbers.PhoneTypeId);
            return View(customersphonenumbers);
        }

        //
        // GET: /CustomersPhoneNumber/Delete/5

        public ActionResult Delete(int id = 0)
        {
            CustomersPhoneNumbers customersphonenumbers = db.tabCustomersPhoneNumbers.Find(id);
            if (customersphonenumbers == null)
            {
                return HttpNotFound();
            }
            return View(customersphonenumbers);
        }

        //
        // POST: /CustomersPhoneNumber/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            CustomersPhoneNumbers customersphonenumbers = db.tabCustomersPhoneNumbers.Find(id);
            db.tabCustomersPhoneNumbers.Remove(customersphonenumbers);
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