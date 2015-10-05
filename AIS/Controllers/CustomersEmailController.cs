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
    public class CustomersEmailController : Controller
    {
        private UsersContext db = new UsersContext();

        //
        // GET: /CustomersEmail/

        public ActionResult Index()
        {
            var tabcustomersemails = db.tabCustomersEmails.Include(c => c.Customers);
            return View(tabcustomersemails.ToList());
        }

        //
        // GET: /CustomersEmail/Details/5

        public ActionResult Details(int id = 0)
        {
            CustomersEmails customersemails = db.tabCustomersEmails.Find(id);
            if (customersemails == null)
            {
                return HttpNotFound();
            }
            return View(customersemails);
        }

        //
        // GET: /CustomersEmail/Create

        public ActionResult Create()
        {
            ViewBag.CustomerId = new SelectList(db.tabCustomers, "CustomerId", "FirstName");
            return View();
        }

        //
        // POST: /CustomersEmail/Create

        [HttpPost]
        public ActionResult Create(CustomersEmails customersemails)
        {
            if (ModelState.IsValid)
            {
                db.tabCustomersEmails.Add(customersemails);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CustomerId = new SelectList(db.tabCustomers, "CustomerId", "FirstName", customersemails.CustomerId);
            return View(customersemails);
        }

        //
        // GET: /CustomersEmail/Edit/5

        public ActionResult Edit(int id = 0)
        {
            CustomersEmails customersemails = db.tabCustomersEmails.Find(id);
            if (customersemails == null)
            {
                return HttpNotFound();
            }
            ViewBag.CustomerId = new SelectList(db.tabCustomers, "CustomerId", "FirstName", customersemails.CustomerId);
            return View(customersemails);
        }

        //
        // POST: /CustomersEmail/Edit/5

        [HttpPost]
        public ActionResult Edit(CustomersEmails customersemails)
        {
            if (ModelState.IsValid)
            {
                db.Entry(customersemails).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CustomerId = new SelectList(db.tabCustomers, "CustomerId", "FirstName", customersemails.CustomerId);
            return View(customersemails);
        }

        //
        // GET: /CustomersEmail/Delete/5

        public ActionResult Delete(int id = 0)
        {
            CustomersEmails customersemails = db.tabCustomersEmails.Find(id);
            if (customersemails == null)
            {
                return HttpNotFound();
            }
            return View(customersemails);
        }

        //
        // POST: /CustomersEmail/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            CustomersEmails customersemails = db.tabCustomersEmails.Find(id);
            db.tabCustomersEmails.Remove(customersemails);
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