using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using AIS.Models;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using AIS.EmailSender;
using System.Data.Entity;
using AISModels;
using System.Text.RegularExpressions;



namespace AIS.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        SAASContext db = new SAASContext();
        MakeEmails send = new MakeEmails();
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        private ApplicationSignInManager _signInManager;

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set { _signInManager = value; }
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {

            if (ModelState.IsValid)
            {
                var user = db.Users.Where(c => c.Email == model.Email && c.IsDelete == false).FirstOrDefault();
                if (user == null || user.IsDelete == true)
                {
                    ModelState.AddModelError("", "Invalid login attempt..");
                    if (Request.IsAjaxRequest())
                    {
                        return PartialView("~/Views/Mondofi/_SignInModal.cshtml", model);
                    }
                    return View(model);
                }
                if (user.Approved == false)
                {
                    ModelState.AddModelError("", "Your Account is not Approved");
                    if (Request.IsAjaxRequest())
                    {
                        return PartialView("~/Views/Mondofi/_SignInModal.cshtml", model);
                    }
                    return View(model);
                }
                string databasename = user.RestaurantName;
                databasename = databasename.Replace(" ", "");
                UsersContext _db = new UsersContext(databasename);


                using (var companyUserManger = ApplicationUserManager.Create(databasename))
                using (var companySignInManager = ApplicationSignInManager.Create(databasename))
                {
                    var companyUser = companyUserManger.FindByName(model.Email);
                    if (companyUser == null || companyUser.IsDelete == true)
                    {
                        ModelState.AddModelError("", "Invalid login attempt.");
                        if (Request.IsAjaxRequest())
                        {
                            return PartialView("~/Views/Mondofi/_SignInModal.cshtml", model);
                        }
                        return View(model);
                    }

                    //Add this to check if the email was confirmed.
                    if (!companyUserManger.IsEmailConfirmed(companyUser.Id))
                    {
                        ModelState.AddModelError("", "You need to confirm your email.");
                        if (Request.IsAjaxRequest())
                        {
                            return PartialView("~/Views/Mondofi/_SignInModal.cshtml", model);
                        }
                        return View(model);
                    }

                    var result = companySignInManager.PasswordSignIn(model.Email, model.Password, model.RememberMe, shouldLockout: false);
                      
                    var roles = companyUserManger.GetRoles(companyUser.Id);
                      
                    switch (result)
                    {
                        case SignInStatus.Success:
                            if (Request.IsAjaxRequest())
                            {
                                return Json("Success", JsonRequestBehavior.AllowGet);
                            }
                            if (Request.Browser.IsMobileDevice)
                            {
                                return RedirectToAction("Index", "Book");
                            }
                            if (roles.Contains("SuperAdmin"))
                            {
                                return RedirectToAction("Index", "Setting");
                            }
                            if (roles.Contains("Admin"))
                            {
                                return RedirectToAction("Index", "Setting");
                            }
                            else
                            {
                                return RedirectToAction("FloorPlan", "FloorPlan");
                            }

                        case SignInStatus.LockedOut:
                            return View("Lockout");
                        case SignInStatus.RequiresVerification:
                            return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                        case SignInStatus.Failure:
                        default:
                            ModelState.AddModelError("", "Invalid login attempt");
                            if (Request.IsAjaxRequest())
                            {
                                return PartialView("~/Views/Mondofi/_SignInModal.cshtml", model);
                            }
                            return View(model);
                    }
                }
            }
            return View(model);

        }
        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            var user = await UserManager.FindByIdAsync(await SignInManager.GetVerifiedUserIdAsync());
            if (user != null)
            {
                var code = await UserManager.GenerateTwoFactorTokenAsync(user.Id, provider);
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {

            ViewBag.Countries = new SelectList(db.tabCountry.ToList(), "CountryId", "CountryName");

            return View();
        }

        [AllowAnonymous]
        public ActionResult ThankYouRegister()
        {

            return View();
        }

        [AllowAnonymous]
        public ActionResult NotRegister()
        {

            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {

            if (model.TermAndCondition == false)
            {
                //ViewBag.Countries = new SelectList(db.tabCountry.ToList(), "CountryId", "CountryName");
                ModelState.AddModelError("", "Select Terms & Conditions.");
                return View(model);
            }

            if (ModelState.IsValid)
            {
              
                var userget = UserManager.FindByName(model.UserName);
                if (userget != null)
                {
                    if (userget.UserName == model.UserName)
                    {
                        //ViewBag.Countries = new SelectList(db.tabCountry.ToList(), "CountryId", "CountryName");
                        ModelState.AddModelError("", "Email already exists.");
                        return View(model);
                    }
                }

                var RestaurantName = db.Users.Where(c => c.RestaurantName.Trim() == model.RestaurantName.Trim()).FirstOrDefault();
                if (RestaurantName != null)
                {

                    if (RestaurantName.RestaurantName.Trim() == RestaurantName.RestaurantName.Trim())
                    {
                        //ViewBag.Countries = new SelectList(db.tabCountry.ToList(), "CountryId", "CountryName");
                        ModelState.AddModelError("", "Venue Name already exists.");
                        return View(model);
                    }
                }

                Address address = new Address();
                address.City = model.Address.City;
                address.State = model.Address.State;
                address.CountryId = model.Address.CountryId;
                address.PostalCode = model.Address.PostalCode;
                db.tabAddress.Add(address);
                db.SaveChanges();

                var result = UserManager.Create(new UserProfile
                {
                    Email = model.UserName,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UserName = model.UserName,
                    PhoneNumber = model.phone,
                    Notes = model.Notes,
                    AddressId = address.AddressId,
                    RestaurantName = model.RestaurantName,
                    VenueName = model.VenueName,
                    TermAndCondition = model.TermAndCondition,
                    Approved = false,
                    IsDelete=false

                }, model.Password);
                if (result.Succeeded)
                {

                    var getUser = UserManager.FindByName(model.UserName);
                    var roleresult = UserManager.AddToRole(getUser.Id, "user");
                    try
                    {
                        send.SendEmailToAdmin(model.UserName, model);
                        send.SendEmailToUserWaitForApprovel(model.UserName, model.FirstName);
                    }
                    catch (Exception)
                    {
                        var getuser = UserManager.FindById(getUser.Id);
                        var logins = getuser.Logins;

                        foreach (var login in logins.ToList())
                        {
                            UserManager.RemoveLogin(login.UserId, new UserLoginInfo(login.LoginProvider, login.ProviderKey));
                        }

                        var rolesForUser = UserManager.GetRoles(getUser.Id);

                        if (rolesForUser.Count() > 0)
                        {
                            foreach (var item in rolesForUser.ToList())
                            {
                                // item should be the name of the role
                                var getresult = UserManager.RemoveFromRole(getuser.Id, item);
                            }
                        }
                        try
                        {
                            UserManager.Delete(getuser);
                        }
                        catch (Exception)
                        {

                            throw;
                        }

                        return RedirectToAction("NotRegister", "Account");

                    }

                    return RedirectToAction("ThankYouRegister", "Account");

                }
                AddErrors(result);
            }
            //ViewBag.Countries = new SelectList(db.tabCountry.ToList(), "CountryId", "CountryName");
            // If we got this far, something failed, redisplay form
            return View(model);
        }


        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(long userId, string code)
        {
            if (userId == 0L || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        [AllowAnonymous]
        public async Task<ActionResult> ValidEmail(string UserName)
        {
            if (UserName == null)
            {
                return Json(UserName == null ? false : true, JsonRequestBehavior.AllowGet);
            }
            bool isEmail = Regex.IsMatch(UserName, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
            if (isEmail)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);

        }




        ////
        //// GET: /Account/ForgotPassword
        //[AllowAnonymous]
        //public ActionResult ForgotPassword()
        //{
        //    return View();
        //}

        ////
        //// POST: /Account/ForgotPassword
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = await UserManager.FindByNameAsync(model.Email);
        //        if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
        //        {
        //            // Don't reveal that the user does not exist or is not confirmed
        //            return View("ForgotPasswordConfirmation");
        //        }

        //        // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
        //        // Send an email with this link
        //        // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
        //        // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
        //        // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
        //        // return RedirectToAction("ForgotPasswordConfirmation", "Account");
        //    }

        //    // If we got this far, something failed, redisplay form
        //    return View(model);
        //}

        //
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            EmailSender.EmailSender sendEmail = new EmailSender.EmailSender();
            string subject = "Forgot Password";
            string CC = (string)ConfigurationManager.AppSettings["adminEmail"];
            string fromEmail = (string)ConfigurationManager.AppSettings["Email_To"];
            string template = string.Empty;
            SAASContext SAASdb = new SAASContext();
            var user = SAASdb.Users.Where(c => c.UserName == model.UserName).FirstOrDefault();
            using (StreamReader reader = new StreamReader(System.Web.HttpContext.Current.Server.MapPath("~/EmailTemplates/ForgotPassword.html")))
            {
                template = reader.ReadToEnd();
            }

            var companyUserManger = ApplicationUserManager.Create(user.RestaurantName);
            var getuser = companyUserManger.FindByName(user.Email);
            var resetToken = companyUserManger.GeneratePasswordResetToken(getuser.Id);

            string sitepath = "https://www.mondofi.com/Account/ResetPassword?email=" + model.UserName + "&code=" + resetToken;

            template = template.Replace("{verifyUrl}", sitepath);

            sendEmail.SendEmail(subject, template, model.UserName, null, fromEmail, null);
            return RedirectToAction("EmailSend", "Account");
        }


        [AllowAnonymous]
        public ActionResult EmailSend()
        {

            return View();
        }




        [AllowAnonymous]
        public async Task<ActionResult> IsEmailExist(string UserName)
        {
            SAASContext SAASdb = new SAASContext();
            var user = db.Users.Where(c => c.UserName == UserName && c.IsDelete == false).FirstOrDefault();
            if (user == null)
            {

                return Json(true, JsonRequestBehavior.AllowGet);
            }
            //var isuser = Membership.GetUser(UserName);
            //return Json(isuser == null ? false : true, JsonRequestBehavior.AllowGet);

            return Json(false, JsonRequestBehavior.AllowGet);

        }

        [AllowAnonymous]
        public async Task<ActionResult> IsEmailExistForgot(string UserName)
        {
            SAASContext SAASdb = new SAASContext();
            var user = db.Users.Where(c => c.UserName == UserName && c.IsDelete == false).FirstOrDefault();
            if (user == null)
            {

                return Json(user == null ? false : true, JsonRequestBehavior.AllowGet);
            }
            if (user.IsDelete == true)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            var companyUserManger = ApplicationUserManager.Create(user.RestaurantName);

            var getuser = companyUserManger.FindByEmail(UserName);
            if (getuser.IsDelete == true)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            return Json(getuser == null ? false : true, JsonRequestBehavior.AllowGet);

        }



        [AllowAnonymous]
        public async Task<ActionResult> IsVenueExist(string RestaurantName)
        {
            SAASContext SAASdb = new SAASContext();
            var user = db.Users.Where(c => c.RestaurantName == RestaurantName).FirstOrDefault();
            if (user == null)
            {

                return Json(true, JsonRequestBehavior.AllowGet);
            }
            return Json(false, JsonRequestBehavior.AllowGet);

        }


        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string email, string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var code = model.Code.Replace(" ", "+");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);

            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var companyUserManger = ApplicationUserManager.Create(user.RestaurantName);
            var getuser = await companyUserManger.FindByNameAsync(model.Email);
            var result = await companyUserManger.ResetPasswordAsync(getuser.Id, code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == 0L)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new UserProfile { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}