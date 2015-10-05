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
    public class DesignationController : Controller
    {
        private UsersContext db = new UsersContext();

        //
        // GET: /Designation/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetAllDesignationLists()
        {
            var model=(db.tabDesignations.ToList());

            var searchData = (from c in model.ToList()
                              group c by c.Desgination.Substring(0, 1)
                                  into cgroup
                                  select new AlphabeticalMapping<Designations>()
                                  {
                                      FirstLetter = cgroup.Key,
                                      Items = cgroup.ToList()
                                  }).OrderBy(mapping => mapping.FirstLetter);


            return PartialView("_rightDesignationPartialView", searchData.ToList());

        }
        [HttpGet]
        public ActionResult SearchList(string dTxt)
        {
            try
            {
                var sList = db.tabDesignations.AsQueryable();
                if (!string.IsNullOrEmpty(dTxt))
                {
                    dTxt = dTxt.Trim();
                    sList = sList.Where(p => p.Desgination.Contains(dTxt));
                }

                var searchData = (from c in sList.ToList()
                                  group c by c.Desgination.Substring(0, 1)
                                      into cgroup
                                      select new AlphabeticalMapping<Designations>()
                                      {
                                          FirstLetter = cgroup.Key,
                                          Items = cgroup.ToList()
                                      }).OrderBy(mapping => mapping.FirstLetter);

                return PartialView("_rightDesignationPartialView", searchData.ToList());
            }
            catch (Exception)
            {

                throw;
            }
        }

        //
        // GET: /Designation/Details/5

        public ActionResult Details(int id = 0)
        {
            Designations designations = db.tabDesignations.Find(id);
            if (designations == null)
            {
                return HttpNotFound();
            }
            return PartialView(designations);
        }

        //
        // GET: /Designation/Create

        public ActionResult Create()
        {
            return PartialView();
        }

        //
        // POST: /Designation/Create

        [HttpPost]
        public ActionResult Create(Designations designations)
        {
            try
            {
                if (designations.DesignationId == 0)
                {
                    db.tabDesignations.Add(designations);
                    db.SaveChanges();
                   // return PartialView("Create");
                    return RedirectToAction("Create");
                }
                else
                {
                    Designations dObj = db.tabDesignations.Find(designations.DesignationId);
                    dObj.DesignationId = designations.DesignationId;
                    dObj.Desgination = designations.Desgination;
                    db.Entry(dObj).State = EntityState.Modified;
                    db.SaveChanges();
                    //return PartialView("Create");
                    return RedirectToAction("Create");
                }
            }
            catch (Exception ex)
            {
                
                throw ex;
            }
            //if (ModelState.IsValid)
            //{
            //    db.tabDesignations.Add(designations);
            //    db.SaveChanges();
            //    return RedirectToAction("Index");
            //}

            //return View(designations);
        }

        //
        // GET: /Designation/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Designations designations = db.tabDesignations.Find(id);
            if (designations == null)
            {
                return HttpNotFound();
            }
            return PartialView(designations);
        }

        //
        // POST: /Designation/Edit/5

        [HttpPost]
        public ActionResult Edit(Designations designations)
        {
            if (ModelState.IsValid)
            {
                db.Entry(designations).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(designations);
        }

        //
        // GET: /Designation/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Designations designations = db.tabDesignations.Find(id);
            if (designations == null)
            {
                return HttpNotFound();
            }
            return View(designations);
        }

        //
        // POST: /Designation/Delete/5

        public ActionResult IsExist(string Desgination, Int64? DesignationId)
        {
            return Json(!db.tabDesignations.Any(p => (p.Desgination.Trim() == Desgination.Trim()) && (DesignationId == null ? true : (p.DesignationId != DesignationId))), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Designations designations = db.tabDesignations.Find(id);
            db.tabDesignations.Remove(designations);
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