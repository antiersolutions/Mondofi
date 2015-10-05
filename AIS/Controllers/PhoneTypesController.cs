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
    public class PhoneTypesController : Controller
    {
        
        private UsersContext db = new UsersContext();

        //
        // GET: /PhoneTypes/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetAllPhoneTypeLists()
        {
            var model = (db.tabPhoneTypes.ToList());

            var searchData = (from c in model.ToList()
                              group c by c.PhoneType.Substring(0, 1)
                                  into cgroup
                                  select new AlphabeticalMapping<PhoneTypes>()
                                  {
                                      FirstLetter = cgroup.Key,
                                      Items = cgroup.ToList()
                                  }).OrderBy(mapping => mapping.FirstLetter);
            return PartialView("_rightPhoneTypePartialView", searchData.ToList());

        }
        public ActionResult SearchList(string pTxt)
        {
            try
            {
                var sList = db.tabPhoneTypes.AsQueryable();

                if (!string.IsNullOrEmpty(pTxt))
                {
                    pTxt = pTxt.Trim();
                    sList = sList.Where(p => p.PhoneType.Contains(pTxt));
                }

                var searchData = (from c in sList.ToList()
                                  group c by c.PhoneType.Substring(0, 1)
                                      into cgroup
                                      select new AlphabeticalMapping<PhoneTypes>()
                                      {
                                          FirstLetter = cgroup.Key,
                                          Items = cgroup.ToList()
                                      }).OrderBy(mapping => mapping.FirstLetter);
                return PartialView("_rightPhoneTypePartialView", searchData.OrderBy(x => x.FirstLetter).ToList());
            }
            catch (Exception)
            {

                throw;
            }
        }
        //
        // GET: /PhoneTypes/Details/5

        public ActionResult Details(int id = 0)
        {
            PhoneTypes phonetypes = db.tabPhoneTypes.Find(id);
            if (phonetypes == null)
            {
                return HttpNotFound();
            }
            return PartialView(phonetypes);
        }

        //
        // GET: /PhoneTypes/Create

        public ActionResult Create()
        {
            return PartialView();
        }

        //
        // POST: /PhoneTypes/Create

        [HttpPost]
        public ActionResult Create(PhoneTypes phonetypes)
        {
            try
            {
                    var sObj = new PhoneTypes()
                    {
                        PhoneType = phonetypes.PhoneType
                    };
                    db.tabPhoneTypes.Add(sObj);
                    db.SaveChanges();
                    return PartialView("Create");
                  
            }
            catch (Exception ex)
            {
                throw ex;
            } 
        }
        //
        // GET: /PhoneTypes/Edit/5

        public ActionResult Edit(int id = 0)
        {
            PhoneTypes phonetypes = db.tabPhoneTypes.Find(id);
            if (phonetypes == null)
            {
                return HttpNotFound();
            }
            return PartialView(phonetypes);
        }

        //
        // POST: /PhoneTypes/Edit/5

        [HttpPost]
        public ActionResult Edit(PhoneTypes phonetypes)
        {
            if (ModelState.IsValid)
            {
                PhoneTypes pObj = db.tabPhoneTypes.Find(phonetypes.PhoneTypeId);
                pObj.PhoneType = phonetypes.PhoneType;
                db.Entry(pObj).State = EntityState.Modified;
                db.SaveChanges();
                return PartialView("Create");
            }
            return PartialView(phonetypes);
        }

        //
        // GET: /PhoneTypes/Delete/5

        public ActionResult Delete(int id = 0)
        {
            PhoneTypes phonetypes = db.tabPhoneTypes.Find(id);
            if (phonetypes == null)
            {
                return HttpNotFound();
            }
            return View(phonetypes);
        }

        //
        // POST: /PhoneTypes/Delete/5

        public ActionResult IsExist(string PhoneType, Int64? PhoneTypeId)
        {
            return Json(!db.tabPhoneTypes.Any(p => (p.PhoneType.Trim() == PhoneType.Trim()) && (PhoneTypeId == null ? true : (p.PhoneTypeId != PhoneTypeId))), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            PhoneTypes phonetypes = db.tabPhoneTypes.Find(id);
            db.tabPhoneTypes.Remove(phonetypes);
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