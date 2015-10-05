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
    public class CityController : Controller
    {
        private UsersContext db = new UsersContext();

        //
        // GET: /City/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetAllCitiesList()
        {
            var model = (db.tabCities.ToList());

            var searchData = (from c in model.ToList()
                              group c by c.City.Substring(0, 1)
                                  into cgroup
                                  select new AlphabeticalMapping<Cities>()
                                  {
                                      FirstLetter = cgroup.Key,
                                      Items = cgroup.ToList()
                                  }).OrderBy(mapping => mapping.FirstLetter);

            return PartialView("_rightlCityPartialView", searchData.ToList());
        }
        public ActionResult SearchList(string cTxt)
        {
            try
            {

                var sList = db.tabCities.AsQueryable();

                if (!string.IsNullOrEmpty(cTxt))
                {
                    cTxt = cTxt.Trim();

                    sList = sList.Where(p => p.City.Contains(cTxt));
                }

                var searchData = (from c in sList.ToList()
                                  group c by c.City.Substring(0, 1)
                                      into cgroup
                                      select new AlphabeticalMapping<Cities>()
                                      {
                                          FirstLetter = cgroup.Key,
                                          Items = cgroup.ToList()
                                      }).OrderBy(mapping => mapping.FirstLetter);

                return PartialView("_rightlCityPartialView", searchData.OrderBy(x => x.FirstLetter).ToList());
            }
            catch (Exception)
            {

                throw;
            }
        }
        //
        // GET: /City/Details/5

        public ActionResult Details(int id = 0)
        {
            Cities cities = db.tabCities.Find(id);
            if (cities == null)
            {
                return HttpNotFound();
            }
            return PartialView(cities);
        }

        //
        // GET: /City/Create

        public ActionResult Create()
        {
            return PartialView();
        }

        //
        // POST: /City/Create

        [HttpPost]
        public ActionResult Create(Cities model)
        {
            try
            {
                var cObj = new Cities()
                {
                    City = model.City
                };
                db.tabCities.Add(cObj);
                db.SaveChanges();
                return PartialView("Create");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //
        // GET: /City/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Cities cities = db.tabCities.Find(id);
            if (cities == null)
            {
                return HttpNotFound();
            }
            return PartialView(cities);
        }

        //
        // POST: /City/Edit/5

        [HttpPost]
        public ActionResult Edit(Cities model)
        {
            if (ModelState.IsValid)
            {
                Cities cObj = db.tabCities.Find(model.CityId);

                cObj.City = model.City;
                db.Entry(cObj).State = EntityState.Modified;
                db.SaveChanges();
                return PartialView("Create");
            }
            return View();
        }

        //
        // GET: /City/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Cities cities = db.tabCities.Find(id);
            if (cities == null)
            {
                return HttpNotFound();
            }
            return View(cities);
        }

        //
        // POST: /City/Delete/5

        public ActionResult IsExist(string City, Int64? CityId)
        {
            return Json(!db.tabCities.Any(p => (p.City.Trim() == City.Trim()) && (CityId == null ? true : (p.CityId != CityId))), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Cities cities = db.tabCities.Find(id);
            db.tabCities.Remove(cities);
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