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
    public class SpecialStatusController : Controller
    {
        private UsersContext db = new UsersContext();

        //
        // GET: /SpecialStatus/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetAllStatusLists()
        {
            var model = (db.tabSpecialStatus.ToList());

            var searchData = (from c in model.ToList()
                              group c by c.Status.Substring(0, 1)
                                  into cgroup
                                  select new AlphabeticalMapping<SpecialStatus>()
                                  {
                                      FirstLetter = cgroup.Key,
                                      Items = cgroup.ToList()
                                  }).OrderBy(mapping => mapping.FirstLetter);

            return PartialView("_rightSpecialStatusPartialView", searchData.ToList());
        }
        public ActionResult SearchList(string sTxt)
        {
            try
            {
                sTxt = sTxt.Trim();
                var sList = db.tabSpecialStatus.AsQueryable();
                if (!string.IsNullOrEmpty(sTxt))
                {
                    sList = sList.Where(p => p.Status.Contains(sTxt));
                }
                var searchData = (from c in sList.ToList()
                                  group c by c.Status.Substring(0, 1)
                                      into cgroup
                                      select new AlphabeticalMapping<SpecialStatus>()
                                      {
                                          FirstLetter = cgroup.Key,
                                          Items = cgroup.ToList()
                                      }).OrderBy(mapping => mapping.FirstLetter);
                return PartialView("_rightSpecialStatusPartialView", searchData.OrderBy(x => x.FirstLetter).ToList());
            }
            catch (Exception)
            {

                throw;
            }
        }

        //
        // GET: /SpecialStatus/Details/5

        public ActionResult Details(int id = 0)
        {
            SpecialStatus specialstatus = db.tabSpecialStatus.Find(id);
            if (specialstatus == null)
            {
                return HttpNotFound();
            }
            return PartialView(specialstatus);
        }

        //
        // GET: /SpecialStatus/Create

        public ActionResult Create()
        {
            return PartialView();
        }

        //
        // POST: /SpecialStatus/Create

        [HttpPost]
        public ActionResult Create(SpecialStatus model)
        {
            try
            {
                    var sObj = new SpecialStatus()
                    {
                        Status = model.Status
                    };
                    db.tabSpecialStatus.Add(sObj);
                    db.SaveChanges();
                    return PartialView("Create");
                   
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //
        // GET: /SpecialStatus/Edit/5

        public ActionResult Edit(int id = 0)
        {
            SpecialStatus specialstatus = db.tabSpecialStatus.Find(id);
            if (specialstatus == null)
            {
                return HttpNotFound();
            }
            return PartialView(specialstatus);
        }

        //
        // POST: /SpecialStatus/Edit/5

        [HttpPost]
        public ActionResult Edit(SpecialStatus model)
        {
            if (ModelState.IsValid)
            {
                SpecialStatus sObj = db.tabSpecialStatus.Find(model.SpecialStatusId);

                sObj.Status = model.Status;
                db.Entry(sObj).State = EntityState.Modified;
                db.SaveChanges();
                return PartialView("Create");
            }
            return View();
        }

        //
        // GET: /SpecialStatus/Delete/5

        public ActionResult Delete(int id = 0)
        {
            SpecialStatus specialstatus = db.tabSpecialStatus.Find(id);
            if (specialstatus == null)
            {
                return HttpNotFound();
            }
            return View(specialstatus);
        }

        //
        // POST: /SpecialStatus/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            SpecialStatus specialstatus = db.tabSpecialStatus.Find(id);
            db.tabSpecialStatus.Remove(specialstatus);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult IsExist(string Status, Int64? SpecialStatusId)
        {
            return Json(!db.tabSpecialStatus.Any(p => (p.Status.Trim() == Status.Trim()) && (SpecialStatusId == null ? true : (p.SpecialStatusId != SpecialStatusId))), JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}