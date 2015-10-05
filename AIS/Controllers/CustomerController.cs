using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AISModels;
using AIS.Models;
using System.Web.Script.Serialization;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Globalization;
using Newtonsoft.Json;
using AIS.Helpers.Caching;

namespace AIS.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private UsersContext db = new UsersContext();
        private ICacheManager cache = new MemoryCacheManager();

        //
        // GET: /Customer/
        public ActionResult Index(Int64 id = 0)
        {
            //var tabcustomers = db.tabCustomers.Include(c => c.Cities);
            //return View(tabcustomers.ToList());
            ViewBag.Id = id;
            return View();
        }

        [HttpGet]
        public ActionResult GetAllCustomers()
        {
            var list = db.tabCustomers.ToList();
            return PartialView("_rightSideCustomerPartialView", list);
        }

        [HttpGet]
        public ActionResult Search(string sTxt)
        {
            var searchTxt = sTxt.Trim();
            var sList = db.tabCustomers.AsQueryable();
            if (!string.IsNullOrEmpty(searchTxt))
            {
                sList = sList.Where(p => p.FirstName.Contains(searchTxt) || p.LastName.Contains(searchTxt) || p.PhoneNumbers.Any(ph => ph.PhoneNumbers.Contains(searchTxt)));
            }

            var searchData = (from c in sList.ToList()
                              group c by c.LastName.Substring(0, 1)
                                  into cgroup
                                  select new AlphabeticalMapping<Customers>()
                                  {
                                      FirstLetter = cgroup.Key,
                                      Items = cgroup.ToList()
                                  }).OrderBy(mapping => mapping.FirstLetter);
            return PartialView("_rightSideCustomerPartialView", searchData.OrderBy(u => u.FirstLetter).ToList());
        }

        //
        // GET: /Customer/Details/5
        [HttpGet]
        public ActionResult Details(int? id)
        {
            Customers customers = db.tabCustomers.Find(id);
            if (customers == null)
            {
                return HttpNotFound();
            }
            return PartialView("Details", customers);
        }

        //
        // GET: /Customer/Create
        public ActionResult Create()
        {
            var phonelists = db.tabPhoneTypes.ToList();
            ViewBag.PhoneTypes = new SelectList(phonelists, "PhoneTypeId", "PhoneType");
            var emailLists = db.tabEmailTypes.ToList();
            ViewBag.EmailTypes = new SelectList(emailLists, "EmailTypeId", "EmailType");
            var allergylists = db.tabAllergies.ToList();
            ViewBag.Allergy = allergylists;
            var specialstatus = db.tabSpecialStatus.ToList();
            ViewBag.SpecialStatus = specialstatus;
            var restrictlist = db.tabRestrictions.ToList();
            ViewBag.Restrictions = restrictlist;

            var cities = db.tabCities.ToList();
            ViewBag.cities = JsonConvert.SerializeObject(cities);
            return View();
        }

        //
        // GET: /Customer/Edit/5
        [HttpPost]
        public ActionResult Create(CustomerViewModel model)
        {
            //if(ModelState.IsValid)
            //{
            try
            {
                var customerObj = new Customers();
                customerObj.DateCreated = DateTime.Now;
                customerObj.FirstName = model.FirstName;
                customerObj.LastName = model.Lastname;
                customerObj.DateOfBirth = model.DateOfBirth;
                customerObj.Anniversary = DateTime.UtcNow;
                customerObj.Address1 = model.Address1;
                customerObj.Address2 = model.Address2;
                customerObj.Notes = model.Notes;
                customerObj.PhotoPath = model.PhotoPath;


                var c = db.tabCities.Where(p => p.City.ToLower().Trim() == model.CityName.ToLower().Trim()).FirstOrDefault();
                if (c != null)
                {
                    customerObj.CityId = c.CityId;
                }
                else
                {
                    customerObj.CityName = model.CityName;
                }


                db.tabCustomers.Add(customerObj);
                db.SaveChanges();

                var cid = customerObj.CustomerId;


                model.PhoneNumbers = model.PhoneNumbers.Replace("/", "\\/");
                List<CustomersPhoneNumbers> pNumbers = (new JavaScriptSerializer()).Deserialize<List<CustomersPhoneNumbers>>(model.PhoneNumbers);

                foreach (var phone in pNumbers)
                {
                    phone.CustomerId = cid;
                    //phone.PhoneNumbers = model.PhoneNumbers;
                    db.tabCustomersPhoneNumbers.Add(phone);
                }
                model.Emails = model.Emails.Replace("/", "\\/");
                List<CustomersEmails> emails = (new JavaScriptSerializer()).Deserialize<List<CustomersEmails>>(model.Emails);

                foreach (var email in emails)
                {
                    if (email.Email != null && email.Email != string.Empty)
                    {
                        email.CustomerId = cid;
                        //email.Email = model.Emails;
                        db.tabCustomersEmails.Add(email);
                    }

                }

                if (model.allergyList != null)
                {
                    for (int i = 0; i < model.allergyList.Count(); i++)
                    {
                        var alist = new CustomersAllergies();
                        alist.AllergyId = Convert.ToInt32(model.allergyList[i]);
                        alist.CustomerId = cid;
                        db.tabCustomersAllergies.Add(alist);
                    }
                }

                if (model.specialList != null)
                {
                    for (int i = 0; i < model.specialList.Count(); i++)
                    {
                        var slist = new CustomerSpecialStatus();
                        slist.SpecialStatusId = Convert.ToInt32(model.specialList[i]);
                        slist.CustomerId = cid;
                        db.tabCustomerSpecialStatus.Add(slist);
                    }
                }


                if (model.restrictionList != null)
                {
                    for (int i = 0; i < model.restrictionList.Count(); i++)
                    {
                        var rlist = new CustomersRestrictions();
                        rlist.RestrictionId = Convert.ToInt32(model.restrictionList[i]);
                        rlist.CustomerId = cid;
                        db.tabCustomersRestrictions.Add(rlist);
                    }
                }

                db.SaveChanges();
                return RedirectToAction("Index", new { id = customerObj.CustomerId });
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
            }
            finally
            {
                this.ClearReservationCache();
                this.ClearWaitingCache();
            }

            return RedirectToAction("Create");

        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            Customers customers = db.tabCustomers.Find(id);

            if (customers == null)
            {
                return HttpNotFound();
            }
            var model = new CustomerViewModel();
            model.CustomerId = customers.CustomerId;
            model.PhotoPath = customers.PhotoPath;
            model.Lastname = customers.LastName;
            model.FirstName = customers.FirstName;
            model.Notes = customers.Notes;
            model.DateOfBirth = customers.DateOfBirth;
            model.Anniversary = customers.Anniversary;
            model.Address1 = customers.Address1;
            model.Address2 = customers.Address2;


            ViewBag.PhoneTypes = db.tabPhoneTypes.ToList();
            ViewBag.PhoneNumbers = db.tabCustomersPhoneNumbers.Where(p => p.CustomerId == id).ToList();

            ViewBag.EmailTypes = db.tabEmailTypes.ToList();
            ViewBag.Email = db.tabCustomersEmails.Where(p => p.CustomerId == id).ToList();

            var spList = db.tabSpecialStatus.Select(ss => new SpecialStatusViewModel()
            {
                SpecialStatusId = ss.SpecialStatusId,
                Specialstatus = ss.Status,
                Ischecked = false
            }).ToList();

            foreach (var item in customers.SpecialStatus.ToList())
            {
                if (spList.Any(sp => sp.SpecialStatusId == item.SpecialStatusId))
                {
                    spList.Where(sp => sp.SpecialStatusId == item.SpecialStatusId).Single().Ischecked = true;
                }
            }

            var AgList = db.tabAllergies.Select(ca => new AllergyViewModel()
            {
                AllergyId = ca.AllergyId,
                Allergy = ca.Allergy,
                Ischecked = false
            }).ToList();

            foreach (var item in customers.Allergies.ToList())
            {
                if (AgList.Any(al => al.AllergyId == item.AllergyId))
                {
                    AgList.Where(al => al.AllergyId == item.AllergyId).Single().Ischecked = true;
                }
            }

            var RsList = db.tabRestrictions.Select(rl => new RestrictionViewModel()
            {
                RestrictionId = rl.RestrictionId,
                Restriction = rl.Restriction
            }).ToList();
            foreach (var item in customers.Restrictions.ToList())
            {
                if (RsList.Any(rlist => rlist.RestrictionId == item.RestrictionId))
                {
                    RsList.Where(rlist => rlist.RestrictionId == item.RestrictionId).Single().Ischecked = true;
                }
            }
            ViewBag.SpecialStatus = spList;
            ViewBag.Allergy = AgList;
            ViewBag.Restrictions = RsList;

            var cities = db.tabCities.ToList();
            ViewBag.cities = JsonConvert.SerializeObject(cities);

            model.CityName = customers.CityId == null ? customers.CityName : customers.Cities.City;

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(CustomerViewModel model)
        {
            var user = db.tabCustomers.Find(model.CustomerId);

            try
            {
                user.PhotoPath = model.PhotoPath;
                user.LastName = model.Lastname;
                user.FirstName = model.FirstName;
                user.DateOfBirth = model.DateOfBirth;
                user.Anniversary = model.Anniversary;
                user.Address1 = model.Address1;
                user.Address2 = model.Address2;
                user.Notes = model.Notes;

                var c = db.tabCities.Where(p => p.City.ToLower().Trim() == model.CityName.ToLower().Trim()).FirstOrDefault();
                if (c != null)
                {
                    user.CityId = c.CityId;
                    user.CityName = null;
                }
                else
                {
                    user.CityName = model.CityName;
                    user.CityId = null;
                }


                db.Database.ExecuteSqlCommand("DELETE from CustomersPhoneNumbers where CustomerId = {0}", user.CustomerId);
                model.PhoneNumbers = model.PhoneNumbers.Replace("/", "\\/");
                List<CustomersPhoneNumbers> pNumbers = (new JavaScriptSerializer()).Deserialize<List<CustomersPhoneNumbers>>(model.PhoneNumbers);

                foreach (var phone in pNumbers)
                {
                    phone.CustomerId = user.CustomerId;
                    db.tabCustomersPhoneNumbers.Add(phone);
                }

                db.Database.ExecuteSqlCommand("DELETE from CustomersEmails where CustomerId = {0}", user.CustomerId);
                model.Emails = model.Emails.Replace("/", "\\/");
                List<CustomersEmails> emails = (new JavaScriptSerializer()).Deserialize<List<CustomersEmails>>(model.Emails);

                foreach (var email in emails)
                {
                    if (email.Email != null && email.Email != string.Empty)
                    {
                        email.CustomerId = user.CustomerId;
                        db.tabCustomersEmails.Add(email);
                    }
                }

                db.Entry(user).State = EntityState.Modified;

                db.Database.ExecuteSqlCommand("DELETE from CustomerSpecialStatus where CustomerId = {0}", user.CustomerId);

                if (model.specialList != null)
                {
                    for (int i = 0; i < model.specialList.Count(); i++)
                    {
                        var stList = new CustomerSpecialStatus();
                        stList.SpecialStatusId = Convert.ToInt32(model.specialList[i]);
                        stList.CustomerId = user.CustomerId;
                        db.tabCustomerSpecialStatus.Add(stList);
                    }
                }

                db.Database.ExecuteSqlCommand("DELETE from CustomersAllergies where CustomerId = {0}", user.CustomerId);

                if (model.allergyList != null)
                {
                    for (int i = 0; i < model.allergyList.Count(); i++)
                    {
                        var algyList = new CustomersAllergies();
                        algyList.AllergyId = Convert.ToInt32(model.allergyList[i]);
                        algyList.CustomerId = user.CustomerId;
                        db.tabCustomersAllergies.Add(algyList);
                    }
                }

                db.Database.ExecuteSqlCommand("DELETE from CustomersRestrictions where CustomerId = {0}", user.CustomerId);

                if (model.restrictionList != null)
                {
                    for (int i = 0; i < model.restrictionList.Count(); i++)
                    {
                        var reslist = new CustomersRestrictions();
                        reslist.RestrictionId = Convert.ToInt32(model.restrictionList[i]);
                        reslist.CustomerId = user.CustomerId;
                        db.tabCustomersRestrictions.Add(reslist);
                    }
                }

                db.SaveChanges();

                return RedirectToAction("Index", new { id = user.CustomerId });
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                this.ClearReservationCache();
                this.ClearWaitingCache();
            }
        }


        //
        // GET: /Customer/Delete/5
        public ActionResult Delete(long id = 0)
        {
            Customers customers = db.tabCustomers.Find(id);
            if (customers == null)
            {
                return HttpNotFound();
            }
            return View(customers);
        }

        //
        // POST: /Customer/Delete/5
        [HttpPost]
        public ActionResult DeleteConfirmed(int? id)
        {
            try
            {
                db.Database.ExecuteSqlCommand("DELETE from CustomersPhoneNumbers where CustomerId = {0}", id);
                db.Database.ExecuteSqlCommand("DELETE from CustomersEmails where CustomerId = {0}", id);
                db.Database.ExecuteSqlCommand("DELETE from CustomerSpecialStatus where CustomerId = {0}", id);
                db.Database.ExecuteSqlCommand("DELETE from CustomersAllergies where CustomerId = {0}", id);
                db.Database.ExecuteSqlCommand("DELETE from CustomersRestrictions where CustomerId = {0}", id);
                db.Database.ExecuteSqlCommand("DELETE from Reservation where CustomerId = {0}", id);
                db.Database.ExecuteSqlCommand("DELETE from Customers where CustomerId = {0}", id);
                db.SaveChanges();

                this.ClearReservationCache();
                this.ClearWaitingCache();

                return Json(true);
            }
            catch (Exception)
            {
                return Json(false);
            }

            //Customers customers = db.tabCustomers.Find(id);
            //db.tabCustomers.Remove(customers);
            //db.SaveChanges();
            //return RedirectToAction("Index");
        }

        private void ClearWaitingCache()
        {
            //cache.RemoveByPattern(CacheKeys.WAITING_PATTERN);
            cache.RemoveByPattern(string.Format(CacheKeys.WAITING_COMPANY_PATTERN,User.Identity.GetDatabaseName()));
        }

        private void ClearReservationCache()
        {
            //cache.RemoveByPattern(CacheKeys.RESERVATION_BY_DATE_PATTREN);
            cache.RemoveByPattern(string.Format(CacheKeys.RESERVATION_BY_DATE_COMPANY_PATTREN, User.Identity.GetDatabaseName()));
            //cache.RemoveByPattern(CacheKeys.FILTERED_RESERVATION_PATTREN);
            cache.RemoveByPattern(string.Format(CacheKeys.FILTERED_RESERVATION_COMPANY_PATTREN, User.Identity.GetDatabaseName()));
            //cache.RemoveByPattern(CacheKeys.FLOOR_TABLES_SCREEN_PATTREN);
            cache.RemoveByPattern(string.Format(CacheKeys.FLOOR_TABLES_SCREEN_COMPANY_PATTREN,db.Database.Connection.Database));
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}