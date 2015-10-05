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
    public class UserPhoneController : Controller
    {
        private UsersContext db = new UsersContext();

        //
        // GET: /UserPhone/

        public ActionResult GetAllUserPhoneLists()
        {
            var model=(db.tabUserPhones.ToList());
            return PartialView("_rightUserPhoneNumberPartialView", model);
        }
        public ActionResult SearchList(string pTxt)
        {
            try
            {
                var sList = db.tabUserPhones.Where(p => p.PhoneNumber.Contains(pTxt));
                return PartialView("_rightUserPhoneNumberPartialView", sList);
            }
            catch (Exception)
            {

                throw;
            }
        }
        //
        // GET: /UserPhone/Details/5

        public ActionResult Details(long id = 0)
        {
            UserPhones userphones = db.tabUserPhones.Find(id);
            if (userphones == null)
            {
                return HttpNotFound();
            }
            return View(userphones);
        }

        //
        // GET: /UserPhone/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /UserPhone/Create

        [HttpPost]
        public ActionResult Create(UserPhones userphones)
        {
            try
            {
                //Int64 userPhnid = Convert.ToInt64(userphones.UserPhoneId);
                if (userphones.UserPhoneId == 0)
                {
                    
                    db.tabUserPhones.Add(userphones);
                    db.SaveChanges();
                    return PartialView("Create");
                }
                else
                {
                    UserPhones uObj = db.tabUserPhones.Find(userphones.UserPhoneId);
                    uObj.UserPhoneId = userphones.UserPhoneId;
                    uObj.PhoneNumber = userphones.PhoneNumber;
                    db.Entry(uObj).State = EntityState.Modified;
                    db.SaveChanges();
                    return PartialView("Create");
                }
            }
            catch (Exception)
            {
                
                throw;
            }
            //if (ModelState.IsValid)
            //{
            //    db.UserPhones.Add(userphones);
            //    db.SaveChanges();
            //    return RedirectToAction("Index");
            //}

            //return View(userphones);
        }

        //
        // GET: /UserPhone/Edit/5

        public ActionResult Edit(long id = 0)
        {
            UserPhones userphones = db.tabUserPhones.Find(id);
            if (userphones == null)
            {
                return HttpNotFound();
            }
            return View(userphones);
        }

        //
        // POST: /UserPhone/Edit/5

        [HttpPost]
        public ActionResult Edit(UserPhones userphones)
        {
            if (ModelState.IsValid)
            {
                db.Entry(userphones).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(userphones);
        }

        //
        // GET: /UserPhone/Delete/5

        public ActionResult Delete(long id = 0)
        {
            UserPhones userphones = db.tabUserPhones.Find(id);
            if (userphones == null)
            {
                return HttpNotFound();
            }
            return View(userphones);
        }

        //
        // POST: /UserPhone/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {
            UserPhones userphones = db.tabUserPhones.Find(id);
            db.tabUserPhones.Remove(userphones);
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