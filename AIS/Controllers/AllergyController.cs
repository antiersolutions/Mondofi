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
    public class AllergyController : Controller
    {
        private UsersContext db = new UsersContext();

        //
        // GET: /Allergy/
        //[HttpGet]
        public ActionResult GetAllAllergies()
        {
            var model = (db.tabAllergies.ToList());

            var searchData = (from c in model.ToList()
                              group c by c.Allergy.Substring(0, 1)
                                  into cgroup
                                  select new AlphabeticalMapping<Allergies>()
                                  {
                                      FirstLetter = cgroup.Key,
                                      Items = cgroup.ToList()
                                  }).OrderBy(mapping => mapping.FirstLetter);


            return PartialView("_rightAllergyPartialView", searchData.ToList());
        }

        public ActionResult SearchList(string sTxt)
        {
            try
            {
                var sList = db.tabAllergies.AsQueryable();

                if (!string.IsNullOrEmpty(sTxt))
                {
                    sTxt = sTxt.Trim();
                    sList = sList.Where(p => p.Allergy.Contains(sTxt));
                }

                var searchData = (from c in sList.ToList()
                                  group c by c.Allergy.Substring(0, 1)
                                      into cgroup
                                      select new AlphabeticalMapping<Allergies>()
                                      {
                                          FirstLetter = cgroup.Key,
                                          Items = cgroup.ToList()
                                      }).OrderBy(mapping => mapping.FirstLetter);

                return PartialView("_rightAllergyPartialView", searchData.OrderBy(x => x.FirstLetter).ToList());
            }
            catch (Exception)
            {

                throw;
            }
        }

        //
        // GET: /Allergy/Details/5

        public ActionResult Details(int id = 0)
        {
            Allergies allergies = db.tabAllergies.Find(id);
            if (allergies == null)
            {
                return HttpNotFound();
            }
            return PartialView(allergies);
        }

        //
        // GET: /Allergy/Create

        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Create()
        {
            return PartialView();
        }

        //
        // POST: /Allergy/Create

        [HttpPost]
        public ActionResult Create(Allergies model)
        {
            try
            {
                var obj = new Allergies()
                {
                    Allergy = model.Allergy
                };

                db.tabAllergies.Add(obj);
                db.SaveChanges();
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //
        // GET: /Allergy/Edit/5

        public ActionResult Edit(int id)
        {
            Allergies allergies = db.tabAllergies.Find(id);
            if (allergies == null)
            {
                return HttpNotFound();
            }
            return PartialView(allergies);
        }

        //
        // POST: /Allergy/Edit/5

        [HttpPost]
        public ActionResult Edit(Allergies model)
        {
            try
            {
                Allergies allergies = db.tabAllergies.Find(model.AllergyId);

                allergies.Allergy = model.Allergy;

                db.Entry(allergies).State = EntityState.Modified;
                db.SaveChanges();
                return PartialView("Create");
            }
            catch (Exception)
            {

                throw;
            }

        }

        //
        // GET: /Allergy/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Allergies allergies = db.tabAllergies.Find(id);
            if (allergies == null)
            {
                return HttpNotFound();
            }
            return View(allergies);
        }

        //
        // POST: /Allergy/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Allergies allergies = db.tabAllergies.Find(id);
            db.tabAllergies.Remove(allergies);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult IsExist(string Allergy, Int64? AllergyId)
        {
            return Json(!db.tabAllergies.Any(p => (p.Allergy.Trim() == Allergy.Trim()) && (AllergyId == null ? true : (p.AllergyId != AllergyId))), JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

    }
}