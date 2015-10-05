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
    public class RestrictionController : Controller
    {
        private UsersContext db = new UsersContext();

        //
        // GET: /Restriction/

        public ActionResult GetAllRestrictionList()
        {
            var model = (db.tabRestrictions.ToList());

            var searchData = (from c in model.ToList()
                              group c by c.Restriction.Substring(0, 1)
                                  into cgroup
                                  select new AlphabeticalMapping<Restrictions>()
                                  {
                                      FirstLetter = cgroup.Key,
                                      Items = cgroup.ToList()
                                  }).OrderBy(mapping => mapping.FirstLetter);
            return PartialView("_rightRestrictionPartialView", searchData.ToList());
        }

        public ActionResult Index()
        {
            return View();
        }


        public ActionResult SearchList(string rTxt)
        {
            try
            {

                var sList = db.tabRestrictions.AsQueryable();

                if (!string.IsNullOrEmpty(rTxt))
                {
                    rTxt = rTxt.Trim();
                    sList = sList.Where(p => p.Restriction.Contains(rTxt));
                }

                var searchData = (from c in sList.ToList()
                                  group c by c.Restriction.Substring(0, 1)
                                      into cgroup
                                      select new AlphabeticalMapping<Restrictions>()
                                      {
                                          FirstLetter = cgroup.Key,
                                          Items = cgroup.ToList()
                                      }).OrderBy(mapping => mapping.FirstLetter);
                return PartialView("_rightRestrictionPartialView", searchData.OrderBy(x => x.FirstLetter).ToList());
            }
            catch (Exception)
            {

                throw;
            }
        }
        //
        // GET: /Restriction/Details/5

        public ActionResult Details(int id = 0)
        {
            Restrictions restrictions = db.tabRestrictions.Find(id);
            if (restrictions == null)
            {
                return HttpNotFound();
            }
            return PartialView(restrictions);
        }

        //
        // GET: /Restriction/Create

        public ActionResult Create()
        {
            return PartialView();
        }

        //
        // POST: /Restriction/Create

        [HttpPost]
        public ActionResult Create(Restrictions model)
        {
            try
            {
                if (model.RestrictionId == 0)
                {
                    var obj = new Restrictions()
                    {
                        Restriction = model.Restriction
                    };
                    db.tabRestrictions.Add(obj);
                    db.SaveChanges();
                    return PartialView("Create");
                }
                else
                {
                    Restrictions rObj = db.tabRestrictions.Find(model.RestrictionId);
                    rObj.Restriction = model.Restriction;
                    db.Entry(rObj).State = EntityState.Modified;
                    db.SaveChanges();
                    return PartialView("Create");
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        //
        // GET: /Restriction/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Restrictions restrictions = db.tabRestrictions.Find(id);
            if (restrictions == null)
            {
                return HttpNotFound();
            }
            return PartialView(restrictions);
        }

        //
        // POST: /Restriction/Edit/5

        [HttpPost]
        public ActionResult Edit(Restrictions re)
        {
            if (ModelState.IsValid)
            {
                var r = db.tabRestrictions.Find(re.RestrictionId);

                r.Restriction = re.Restriction;

                db.Entry(r).State = EntityState.Modified;
                db.SaveChanges();
            }
            return PartialView();
        }

        //
        // GET: /Restriction/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Restrictions restrictions = db.tabRestrictions.Find(id);
            if (restrictions == null)
            {
                return HttpNotFound();
            }
            return View(restrictions);
        }

        //
        // POST: /Restriction/Delete/5
        public ActionResult IsExist(string Restriction, Int64? RestrictionId)
        {
            return Json(!db.tabRestrictions.Any(p => (p.Restriction.Trim() == Restriction.Trim()) && (RestrictionId == null ? true : (p.RestrictionId != RestrictionId))), JsonRequestBehavior.AllowGet);
        }


        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Restrictions restrictions = db.tabRestrictions.Find(id);
            db.tabRestrictions.Remove(restrictions);
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