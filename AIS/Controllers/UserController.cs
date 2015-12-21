using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AIS.Models;
using System.Web.Script.Serialization;
using AISModels;
using System.Web.Security;
using AIS.Filters;
using AIS.Helpers.Caching;
using AIS.Extensions;
using System.Web.Helpers;
using System.Configuration.Provider;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace AIS.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private UsersContext db = new UsersContext();
        private SAASContext saasDb = new SAASContext();
        private ICacheManager cache = new MemoryCacheManager();

        public ApplicationUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

        //
        // GET: /User/

        public ActionResult Index(Int64 id = 0)
        {
            var companyUserManger = ApplicationUserManager.Create(db.Database.Connection.Database);
            var roles = companyUserManger.GetRoles(User.Identity.GetUserId<long>());

            if (roles.Contains("SuperAdmin") || roles.Contains("Admin"))
            {
                //var model = db.Users.ToList();
                ViewBag.Id = id;
                return View();
            }
            else
            {
                return RedirectToAction("FloorPlan", "FloorPlan");
            }
        }

        public ActionResult RightSideBarPartial(List<UserProfile> model)
        {
            model = model ?? db.Users.ToList();

            var data = model.GroupBy(u => u.FirstName);

            return PartialView(model);
        }

        public ActionResult Search(string sTxt)
        {
            try
            {
                var SuperUserName = string.Empty;
                sTxt = sTxt.Trim();
                var sList = db.Users.Where(c => c.IsDelete == false).AsQueryable();
                var onlineUserName = db.Users.Where(c => c.Roles.Any(ur => ur.RoleId == db.Roles.Where(r => r.Name == "Online").FirstOrDefault().Id)).Single().UserName;
                try
                {
                    var companyUserManger = ApplicationUserManager.Create(db.Database.Connection.Database);
                    var roles = companyUserManger.GetRoles(User.Identity.GetUserId<long>());

                    if (!roles.Contains("SuperAdmin"))
                    {
                        SuperUserName = db.Users.Where(c => c.Roles.Any(ur => ur.RoleId == db.Roles.Where(r => r.Name == "SuperAdmin").FirstOrDefault().Id)).Single().UserName;
                    }
                }
                catch (Exception)
                {


                }

                if (!string.IsNullOrEmpty(sTxt))
                {
                    sList = sList.Where(p => !onlineUserName.Contains(p.UserName)
                        && (p.FirstName.Contains(sTxt)
                        || p.LastName.Contains(sTxt)
                        || p.PhoneNumbers.Any(ph => ph.PhoneNumber.Contains(sTxt))));


                }
                if (!string.IsNullOrEmpty(SuperUserName))
                {

                    sList = sList.Where(p => !SuperUserName.Contains(p.UserName)
                           && (p.FirstName.Contains(sTxt)
                           || p.LastName.Contains(sTxt)
                           || p.PhoneNumbers.Any(ph => ph.PhoneNumber.Contains(sTxt))));
                }
                var searchData = (from c in sList.ToList()
                                  group c by c.LastName.Substring(0, 1)
                                      into cgroup
                                      select new AlphabeticalMapping<UserProfile>()
                                      {
                                          FirstLetter = cgroup.Key,
                                          Items = cgroup.ToList()
                                      }).OrderBy(mapping => mapping.FirstLetter);


                return PartialView("RightSideBarPartial", searchData.OrderBy(x => x.FirstLetter).ToList());
            }
            catch (Exception)
            {

                throw;
            }
        }
        //
        // GET: /User/Details/5

        public ActionResult Details(int id = 0)
        {
            UserProfile userprofile = db.Users.Find(id);


            if (userprofile == null)
            {
                return HttpNotFound();
            }

            return PartialView(userprofile);
        }

        //
        // GET: /User/Create

        public ActionResult Create()
        {
            var companyUserManger = ApplicationUserManager.Create(db.Database.Connection.Database);
            var roles = companyUserManger.GetRoles(User.Identity.GetUserId<long>());

            if (roles.Contains("SuperAdmin") || roles.Contains("Admin"))
            {
                ViewBag.PhoneType = new SelectList(db.tabPhoneTypes.ToList(), "PhoneTypeId", "PhoneType");
                ViewBag.Designation = new SelectList(db.tabDesignations.ToList(), "DesignationId", "Desgination");

                ViewBag.UserCode = GetUniqueCodeForUser();

                return View();
            }
            else
            {
                return RedirectToAction("FloorPlan", "FloorPlan");
            }
        }

        //
        // POST: /User/Create

        //[HttpPost]
        //public ActionResult Create(UserRegisterViewModel model)
        //{
        //    try
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            WebSecurity.CreateUserAndAccount(model.EmailAddress, model.Password);
        //            int userId = WebSecurity.GetUserId(model.EmailAddress);

        //            var user = db.Users.Find(userId);

        //            model.PhoneNumbers = model.PhoneNumbers.Replace("/", "\\/");
        //            List<UserPhones> phnNumbers = (new JavaScriptSerializer()).Deserialize<List<UserPhones>>(model.PhoneNumbers);

        //            foreach (var phone in phnNumbers)
        //            {
        //                phone.UserId = userId;
        //                db.tabUserPhones.Add(phone);
        //            }

        //            if (model.IsAdmin)
        //            {
        //                Roles.AddUserToRole(model.EmailAddress, "Admin");
        //            }

        //            user.FirstName = model.FirstName;
        //            user.LastName = model.LastName;
        //            user.PhotoPath = model.PhotoPath;
        //            user.Availability = model.Availability;
        //            user.DesignationId = model.DesignationId;
        //            user.UserCode = model.UserCode;

        //            db.Entry(user).State = EntityState.Modified;
        //            db.SaveChanges();

        //            return RedirectToAction("Index", new { id = user.Id });
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        string[] allRoles = Roles.GetRolesForUser(model.EmailAddress);
        //        if (allRoles.Count() > 0)
        //        {
        //            Roles.RemoveUserFromRoles(model.EmailAddress, allRoles);
        //        }

        //        ((SimpleMembershipProvider)Membership.Provider).DeleteAccount(model.EmailAddress);
        //        Membership.Provider.DeleteUser(model.EmailAddress, true);
        //        Membership.DeleteUser(model.EmailAddress, true);
        //    }
        //    finally
        //    {
        //        ClearStaffCache();
        //    }

        //    ViewBag.PhoneType = new SelectList(db.tabPhoneTypes.ToList(), "PhoneTypeId", "PhoneType");
        //    ViewBag.Designation = new SelectList(db.tabDesignations.ToList(), "DesignationId", "Desgination");
        //    return View(model);
        //}

        [HttpPost]
        public ActionResult Create(UserRegisterViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using (var companyUserManger = ApplicationUserManager.Create(User.Identity.GetDatabaseName()))
                    {
                        var result = companyUserManger.Create(new UserProfile
                        {
                            Email = model.EmailAddress,
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            UserName = model.EmailAddress,
                            UserCode = model.UserCode,
                            PhotoPath = model.PhotoPath,
                            Availability = model.Availability,
                            DesignationId = model.DesignationId,
                            RestaurantName = User.Identity.GetDatabaseName(),
                            VenueName = User.Identity.GetVenueName(),
                            TermAndCondition = true,
                            EmailConfirmed = true,
                            EnablePIN = model.EnablePIN,
                            Approved = false,
                            IsDelete = false



                        }, model.Password);


                        var ii = result;

                        var getUseradd = db.Users.Where(c => c.Email == model.EmailAddress && c.IsDelete == false).FirstOrDefault();

                        if (model.IsAdmin)
                        {
                            companyUserManger.AddToRole(getUseradd.Id, "Admin");
                        }
                        model.PhoneNumbers = model.PhoneNumbers.Replace("/", "\\/");

                        List<UserPhones> phnNumbers = (new JavaScriptSerializer()).Deserialize<List<UserPhones>>(model.PhoneNumbers);

                        foreach (var phone in phnNumbers)
                        {
                            phone.UserId = getUseradd.Id;

                            db.tabUserPhones.Add(phone);

                        }

                        var Mondoresult = UserManager.Create(new UserProfile
                        {
                            Email = model.EmailAddress,
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            UserName = model.EmailAddress,
                            UserCode = model.UserCode,
                            PhotoPath = model.PhotoPath,
                            Availability = model.Availability,
                            DesignationId = model.DesignationId,
                            RestaurantName = User.Identity.GetDatabaseName(),
                            VenueName = User.Identity.GetVenueName(),
                            TermAndCondition = true,
                            IsDelete = false

                        }, model.Password);


                        var Useradd = saasDb.Users.Where(c => c.Email == model.EmailAddress && c.IsDelete == false).FirstOrDefault();

                        UserManager.AddToRole(Useradd.Id, "user");

                        db.SaveChanges();

                        return RedirectToAction("Index", new { id = getUseradd.Id });
                    }

                    //return RedirectToAction("Index", new { id = user.Id });
                }
            }
            catch (Exception)
            {
                string[] allRoles = Roles.GetRolesForUser(model.EmailAddress);
                if (allRoles.Count() > 0)
                {
                    Roles.RemoveUserFromRoles(model.EmailAddress, allRoles);
                }

                //((SimpleMembershipProvider)Membership.Provider).DeleteAccount(model.EmailAddress);
                //Membership.Provider.DeleteUser(model.EmailAddress, true);
                //Membership.DeleteUser(model.EmailAddress, true);
            }
            finally
            {
                ClearStaffCache();
            }

            ViewBag.PhoneType = new SelectList(db.tabPhoneTypes.ToList(), "PhoneTypeId", "PhoneType");
            ViewBag.Designation = new SelectList(db.tabDesignations.ToList(), "DesignationId", "Desgination");
            return View(model);
        }

        //
        // GET: /User/Edit/5

        public ActionResult Edit(int id = 0)
        {
            var companyUserManger1 = ApplicationUserManager.Create(db.Database.Connection.Database);
            var roles = companyUserManger1.GetRoles(User.Identity.GetUserId<long>());

            if (roles.Contains("SuperAdmin") || roles.Contains("Admin"))
            {
                UserProfile userprofile = db.Users.Find(id);
                if (userprofile == null)
                {
                    return HttpNotFound();
                }

                var model = new UserRegisterViewModel();
                var companyUserManger = ApplicationUserManager.Create(User.Identity.GetDatabaseName());
                model.UserId = userprofile.Id;
                model.FirstName = userprofile.FirstName;
                model.LastName = userprofile.LastName;
                model.EmailAddress = userprofile.UserName;
                model.InitialEmail = userprofile.UserName;
                model.PhotoPath = userprofile.PhotoPath;
                var userrole = companyUserManger.IsInRole(userprofile.Id, "Admin");
                //var roles = companyUserManger.(userprofile.Id);

                //model.IsAdmin = (roles.Length > 0 && roles[0] == "Admin") ? true : false;
                model.IsAdmin = userrole;
                model.Availability = userprofile.Availability ?? false;
                model.DesignationId = userprofile.DesignationId;
                model.UserCode = userprofile.UserCode;
                model.EnablePIN = userprofile.EnablePIN;

                ViewBag.PhoneType = db.tabPhoneTypes.ToList();
                ViewBag.PhoneNumbers = db.tabUserPhones.Where(p => p.UserId == id).ToList();
                ViewBag.Designation = new SelectList(db.tabDesignations.ToList(), "DesignationId", "Desgination", userprofile.DesignationId);
                return View(model);
            }
            else
            {
                return RedirectToAction("FloorPlan", "FloorPlan");
            }
        }

        //
        // POST: /User/Edit/5

        //[HttpPost]
        //public ActionResult Edit(UserRegisterViewModel model)
        //{
        //    var user = db.Users.Find(model.UserId.Value);
        //    try
        //    {
        //        model.PhoneNumbers = model.PhoneNumbers.Replace("/", "\\/");
        //        List<UserPhones> phnNumbers = (new JavaScriptSerializer()).Deserialize<List<UserPhones>>(model.PhoneNumbers);

        //        var token = WebSecurity.GeneratePasswordResetToken(model.EmailAddress);

        //        if (model.IsPasswordChanged && !this.ResetPasswordWithToken(token, model.Password))
        //        {
        //            throw new Exception("Failed to update password.");
        //        }

        //        var DBPhones = user.PhoneNumbers.ToList();

        //        foreach (var phone in phnNumbers)
        //        {
        //            if (DBPhones.Any(p => p.PhoneNumber == phone.PhoneNumber))
        //            {
        //                var dbphone = DBPhones.Where(p => p.PhoneNumber == phone.PhoneNumber).Single();
        //                dbphone.PhoneTypeId = phone.PhoneTypeId;
        //                db.Entry(dbphone).State = EntityState.Modified;
        //            }
        //            else
        //            {
        //                phone.UserId = user.Id;
        //                user.PhoneNumbers.Add(phone);
        //            }
        //        }

        //        foreach (var dbphone in DBPhones)
        //        {
        //            if (!phnNumbers.Any(p => p.PhoneNumber == dbphone.PhoneNumber))
        //            {
        //                user.PhoneNumbers.Remove(dbphone);
        //            }
        //        }

        //        if (model.IsAdmin)
        //        {
        //            var roles = Roles.GetRolesForUser(model.EmailAddress);
        //            if (!roles.Contains("Admin"))
        //            {
        //                Roles.AddUserToRole(model.EmailAddress, "Admin");
        //            }
        //        }
        //        else
        //        {

        //            var roles = Roles.GetRolesForUser(model.EmailAddress);
        //            if (roles.Contains("Admin"))
        //            {
        //                Roles.RemoveUserFromRole(model.EmailAddress, "Admin");
        //            }
        //        }

        //        user.FirstName = model.FirstName;
        //        user.LastName = model.LastName;
        //        user.PhotoPath = model.PhotoPath;
        //        user.Availability = model.Availability;
        //        user.DesignationId = model.DesignationId;
        //        user.UserCode = model.UserCode;
        //        user.EnablePIN = model.EnablePIN;

        //        db.Entry(user).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index", new { id = user.Id });
        //    }
        //    catch (Exception)
        //    {
        //        ViewBag.PhoneType = db.tabPhoneTypes.ToList();
        //        ViewBag.PhoneNumbers = db.tabUserPhones.Where(p => p.UserId == user.Id).ToList();
        //        ViewBag.Designation = new SelectList(db.tabDesignations.ToList(), "DesignationId", "Desgination");
        //        return View(model);
        //    }
        //    finally
        //    {
        //        ClearStaffCache();
        //    }
        //}

        //
        // GET: /User/Delete/5

        [HttpPost]
        public ActionResult Edit(UserRegisterViewModel model)
        {
            var user = db.Users.Find(model.UserId);
            var companyUserManger = ApplicationUserManager.Create(User.Identity.GetDatabaseName());
            var userP = companyUserManger.FindByEmail(model.InitialEmail);
            try
            {

                model.PhoneNumbers = model.PhoneNumbers.Replace("/", "\\/");
                List<UserPhones> phnNumbers = (new JavaScriptSerializer()).Deserialize<List<UserPhones>>(model.PhoneNumbers);

                var resetToken = companyUserManger.GeneratePasswordResetToken(userP.Id);

                //var password =  ResetPassword(user.Id, token, model.Password);
                if (model.IsPasswordChanged)
                {

                    string hashedPassword = Crypto.HashPassword(model.Password);

                    user.PasswordHash = hashedPassword;
                    companyUserManger.Update(userP);
                    // var result = companyUserManger.ResetPassword(user.Id, resetToken, model.Password);
                    // if (!result.Succeeded)
                    // {
                    //     throw new Exception("Failed to update password.");
                    //}
                }

                var DBPhones = user.PhoneNumbers.ToList();

                foreach (var phone in phnNumbers)
                {
                    if (DBPhones.Any(p => p.PhoneNumber == phone.PhoneNumber))
                    {
                        var dbphone = DBPhones.Where(p => p.PhoneNumber == phone.PhoneNumber).Single();
                        dbphone.PhoneTypeId = phone.PhoneTypeId;
                        db.Entry(dbphone).State = System.Data.Entity.EntityState.Modified;
                    }
                    else
                    {
                        phone.UserId = user.Id;
                        //user.PhoneNumbers.Add(phone);
                        db.tabUserPhones.Add(phone);
                    }
                }

                foreach (var dbphone in DBPhones)
                {
                    if (!phnNumbers.Any(p => p.PhoneNumber == dbphone.PhoneNumber))
                    {
                        db.Entry(dbphone).State = EntityState.Deleted;
                        //user.PhoneNumbers.Remove(dbphone);
                    }
                }

                if (model.IsAdmin)
                {
                    //var roles = Roles.GetRolesForUser(model.EmailAddress);
                    //if (!roles.Contains("Admin"))
                    //{
                    //    Roles.AddUserToRole(model.EmailAddress, "Admin");
                    //}

                    companyUserManger.AddToRole(model.UserId.Value, "Admin");
                }
                else
                {

                    var roles = companyUserManger.GetRoles(model.UserId.Value);
                    if (roles.Contains("Admin"))
                    {
                        companyUserManger.RemoveFromRole(model.UserId.Value, "Admin");
                    }
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhotoPath = model.PhotoPath;
                user.Availability = model.Availability;
                user.DesignationId = model.DesignationId;
                user.UserCode = model.UserCode;
                user.EnablePIN = model.EnablePIN;


                db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", new { id = user.Id });
            }
            catch (Exception)
            {
                ViewBag.PhoneType = db.tabPhoneTypes.ToList();
                ViewBag.PhoneNumbers = db.tabUserPhones.Where(p => p.UserId == user.Id).ToList();
                ViewBag.Designation = new SelectList(db.tabDesignations.ToList(), "DesignationId", "Desgination");
                return View(model);
            }
            finally
            {
                ClearStaffCache();
            }
        }

        public ActionResult Delete(int id = 0)
        {
            UserProfile userprofile = db.Users.Find(id);
            if (userprofile == null)
            {
                return HttpNotFound();
            }
            return View(userprofile);
        }

        //
        // POST: /User/Delete/5

        [HttpPost, ActionName("Delete")]
        public async Task<JsonResult> DeleteConfirmed(long id)
        {
            var user = db.Users.Find(id);
            try
            {
                //using (var companyUserManger = ApplicationUserManager.Create(User.Identity.GetDatabaseName()))
                //{
                //var user = companyUserManger.FindById(id);
                user.UserName = user.Email + "," + Guid.NewGuid();
                user.IsDelete = true;
                db.Entry(user).State = EntityState.Modified;
                //companyUserManger.Update(user);

                //var rolesForUser = await companyUserManger.GetRolesAsync(user.Id);

                //if (rolesForUser.Count() > 0)
                //{
                //    foreach (var item in rolesForUser.ToList())
                //    {
                //        // item should be the name of the role
                //        var result = await companyUserManger.RemoveFromRoleAsync(user.Id, item);
                //    }
                //}
                //if (user.PhoneNumbers.Count > 0)
                //{
                //    //db.Database.ExecuteSqlCommand("DELETE FROM USERPHONES WHERE USERID = {0}", user.Id);
                //    var phone = db.tabUserPhones.Where(c => c.UserId == user.Id).ToList();
                //    foreach (var item in phone)
                //    {
                //        db.tabUserPhones.Remove(item);
                //        db.SaveChanges();
                //    }

                //    //
                //}
                //var users = db.Users.Where(c => c.Id == id).FirstOrDefault();
                //db.Users.Remove(users);



                ///////delete user form mondofi ////////



                //foreach (var login in logins.ToList())
                //{
                //    UserManager.RemoveLogin(login.UserId, new UserLoginInfo(login.LoginProvider, login.ProviderKey));
                //}

                //var MondorolesForUser = UserManager.GetRoles(Mondouser.Id);

                //if (MondorolesForUser.Count() > 0)
                //{
                //    foreach (var item in rolesForUser.ToList())
                //    {
                //        // item should be the name of the role
                //        var result = UserManager.RemoveFromRole(Mondouser.Id, item);
                //    }
                //}

                //UserManager.Delete(Mondouser);

                //db.SaveChanges();

                //((SimpleMembershipProvider)Membership.Provider).DeleteAccount(userprofile.UserName);
                //Membership.Provider.DeleteUser(userprofile.UserName, true);
                //Membership.DeleteUser(userprofile.UserName, true);


                //}


                var Mondouser = saasDb.Users.Where(c => c.UserName == user.Email && c.IsDelete == false).FirstOrDefault();
                Mondouser.UserName = Mondouser.Email + "," + Guid.NewGuid();
                Mondouser.IsDelete = true;
                saasDb.Entry(Mondouser).State = EntityState.Modified;

                db.SaveChanges();
                saasDb.SaveChanges();
                //UserManager.Update(Mondouser);
                //var logins = user.Logins;
                return Json(true);






            }
            catch (Exception)
            {
                return Json(false);
            }
            finally
            {
                ClearStaffCache();
            }
        }

        [HttpPost]
        public JsonResult doesEmailExist(string InitialEmail, string EmailAddress)
        {
            if (!string.IsNullOrEmpty(InitialEmail.Trim()) && InitialEmail.Equals(EmailAddress))
            {
                return Json(true);
            }
            else
            {

                SAASContext SAASdb = new SAASContext();
                var user = db.Users.Where(c => c.UserName == EmailAddress && c.IsDelete == false).FirstOrDefault();
                var mondouser = SAASdb.Users.Where(c => c.UserName == EmailAddress && c.IsDelete == false).FirstOrDefault();
                if (mondouser == null && user == null)
                {

                    return Json(true, JsonRequestBehavior.AllowGet);
                }

                return Json(false, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost]
        public JsonResult GenerateUniqueUserCode()
        {
            var uniqueCode = GetUniqueCodeForUser();
            return Json(uniqueCode);
        }

        private void ClearStaffCache()
        {
            //cache.RemoveByPattern(CacheKeys.FLOOR_TABLES_SCREEN_PATTREN);
            cache.RemoveByPattern(string.Format(CacheKeys.FLOOR_TABLES_SCREEN_COMPANY_PATTREN, User.Identity.GetDatabaseName()));
            //cache.RemoveByPattern(CacheKeys.STAFF_PATTREN);
            cache.RemoveByPattern(string.Format(CacheKeys.STAFF_COMPANY_PATTREN, User.Identity.GetDatabaseName()));
        }

        private int GetUniqueCodeForUser()
        {
            int uniqueCode = 0;

            bool isUnique = false;
            while (!isUnique)
            {
                uniqueCode = CommonExtensions.GetRandomNumber(1000, 9999);
                isUnique = !db.Users.Any(u => u.UserCode == uniqueCode);
            }

            return uniqueCode;
        }

        private bool SetPassword(long userId, string newPassword)
        {
            string hashedPassword = Crypto.HashPassword(newPassword);
            if (hashedPassword.Length > 128)
            {
                throw new ArgumentException("The membership password is too long. (Maximum length is 128 characters).");
            }

            // Update new password
            int result = db.Database.ExecuteSqlCommand(@"UPDATE " + "webpages_Membership" + " SET Password={0}, PasswordSalt={1}, PasswordChangedDate={2} WHERE UserId = {3}", hashedPassword, String.Empty /* salt column is unused */, DateTime.UtcNow, userId);
            return result > 0;
        }

        private bool ResetPasswordWithToken(string token, string newPassword)
        {
            //VerifyInitialized();
            if (string.IsNullOrEmpty(newPassword))
            {
                throw new ArgumentException("Value cannot be null or empty string.", "newPassword");
            }

            long? userId = db.Database.SqlQuery<long>(@"SELECT UserId FROM " + "webpages_Membership" + " WHERE (PasswordVerificationToken = {0} AND PasswordVerificationTokenExpirationDate > {1})", token, DateTime.UtcNow).FirstOrDefault();
            if (userId != null)
            {
                bool success = this.SetPassword(userId.Value, newPassword);
                if (success)
                {
                    // Clear the Token on success
                    int rows = db.Database.ExecuteSqlCommand(@"UPDATE " + "webpages_Membership" + " SET PasswordVerificationToken = NULL, PasswordVerificationTokenExpirationDate = NULL WHERE (UserId = {0})", userId);
                    if (rows != 1)
                    {
                        throw new ProviderException("Database operation failed.");
                    }
                }
                return success;
            }
            else
            {
                return false;
            }

        }


        private bool ResetPasswordUser(long id, string token, string newPassword)
        {

            var dta = User.Identity.GetDatabaseName();

            var companyUserManger = ApplicationUserManager.Create(dta);
            var getuser = companyUserManger.FindById(id);
            var result = companyUserManger.ResetPassword(id, token, newPassword);
            if (result.Succeeded)
            {

            }
            //string hashedPassword = Crypto.HashPassword(newPassword);

            //getuser.PasswordHash = hashedPassword;
            //companyUserManger.Update(getuser);

            return true;
        }


        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}