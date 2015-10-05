//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.Entity;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;
//using AIS.Models;
//using WebMatrix.WebData;
//using System.Web.Security;

//namespace AIS.Controllers
//{
//    public class GuestController : Controller
//    {
//        private UsersContext db = new UsersContext();

//        //
//        // GET: /User/

//        public ActionResult Index()
//        {
//            return View(db.Users.ToList());
//        }

//        //
//        // GET: /User/Details/5

//        public ActionResult Details(int id = 0)
//        {
//            UserProfile userprofile = db.Users.Find(id);
//            if (userprofile == null)
//            {
//                return HttpNotFound();
//            }
//            return View(userprofile);
//        }

//        //
//        // GET: /User/Create

//        public ActionResult Create()
//        {
//            return View();
//        }

//        //
//        // POST: /User/Create

//        [HttpPost]
//        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
//        public ActionResult Create(UserRegisterViewModel model)
//        {
//            if (ModelState.IsValid)
//            {
//                WebSecurity.CreateUserAndAccount(model.UserName, model.Password);
//                var userprofile = Membership.GetUser(model.UserName);

//                db.Entry(userprofile).State = EntityState.Modified;
//                db.SaveChanges();
//                return RedirectToAction("Index");
//            }

//            return PartialView(userprofile);
//        }

//        //
//        // GET: /User/Edit/5

//        public ActionResult Edit(int id = 0)
//        {
//            UserProfile userprofile = db.Users.Find(id);
//            if (userprofile == null)
//            {
//                return HttpNotFound();
//            }
//            return View(userprofile);
//        }

//        //
//        // POST: /User/Edit/5

//        [HttpPost]
//        public ActionResult Edit(UserProfile userprofile)
//        {
//            if (ModelState.IsValid)
//            {
//                db.Entry(userprofile).State = EntityState.Modified;
//                db.SaveChanges();
//                return RedirectToAction("Index");
//            }
//            return View(userprofile);
//        }

//        //
//        // GET: /User/Delete/5

//        public ActionResult Delete(int id = 0)
//        {
//            UserProfile userprofile = db.Users.Find(id);
//            if (userprofile == null)
//            {
//                return HttpNotFound();
//            }
//            return View(userprofile);
//        }

//        //
//        // POST: /User/Delete/5

//        [HttpPost, ActionName("Delete")]
//        public ActionResult DeleteConfirmed(int id)
//        {
//            UserProfile userprofile = db.Users.Find(id);
//            db.Users.Remove(userprofile);
//            db.SaveChanges();
//            return RedirectToAction("Index");
//        }

//        protected override void Dispose(bool disposing)
//        {
//            db.Dispose();
//            base.Dispose(disposing);
//        }
//    }
//}