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
    public class CustomerSpecialStatusController : Controller
    {
        private UsersContext db = new UsersContext();

        //
        // GET: /CustomerSpecialStatus/

        public ActionResult Index()
        {
            var tabcustomerspecialstatus = db.tabCustomerSpecialStatus.Include(c => c.Customers);
            return View(tabcustomerspecialstatus.ToList());
        }

        //
        // GET: /CustomerSpecialStatus/Details/5

        public ActionResult Details(int id = 0)
        {
            CustomerSpecialStatus customerspecialstatus = db.tabCustomerSpecialStatus.Find(id);
            if (customerspecialstatus == null)
            {
                return HttpNotFound();
            }
            return View(customerspecialstatus);
        }

        //
        // GET: /CustomerSpecialStatus/Create

        public ActionResult Create()
        {
            ViewBag.CustomerId = new SelectList(db.tabCustomers, "CustomerId", "FirstName");
            return View();
        }

        //
        // POST: /CustomerSpecialStatus/Create

        [HttpPost]
        public ActionResult Create(CustomerSpecialStatus customerspecialstatus)
        {
            if (ModelState.IsValid)
            {
                db.tabCustomerSpecialStatus.Add(customerspecialstatus);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CustomerId = new SelectList(db.tabCustomers, "CustomerId", "FirstName", customerspecialstatus.CustomerId);
            return View(customerspecialstatus);
        }

        //
        // GET: /CustomerSpecialStatus/Edit/5

        public ActionResult Edit(int id = 0)
        {
            CustomerSpecialStatus customerspecialstatus = db.tabCustomerSpecialStatus.Find(id);
            if (customerspecialstatus == null)
            {
                return HttpNotFound();
            }
            ViewBag.CustomerId = new SelectList(db.tabCustomers, "CustomerId", "FirstName", customerspecialstatus.CustomerId);
            return View(customerspecialstatus);
        }

        //
        // POST: /CustomerSpecialStatus/Edit/5

        [HttpPost]
        public ActionResult Edit(CustomerSpecialStatus customerspecialstatus)
        {
            if (ModelState.IsValid)
            {
                db.Entry(customerspecialstatus).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CustomerId = new SelectList(db.tabCustomers, "CustomerId", "FirstName", customerspecialstatus.CustomerId);
            return View(customerspecialstatus);
        }

        //
        // GET: /CustomerSpecialStatus/Delete/5

        public ActionResult Delete(int id = 0)
        {
            CustomerSpecialStatus customerspecialstatus = db.tabCustomerSpecialStatus.Find(id);
            if (customerspecialstatus == null)
            {
                return HttpNotFound();
            }
            return View(customerspecialstatus);
        }

        //
        // POST: /CustomerSpecialStatus/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            CustomerSpecialStatus customerspecialstatus = db.tabCustomerSpecialStatus.Find(id);
            db.tabCustomerSpecialStatus.Remove(customerspecialstatus);
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