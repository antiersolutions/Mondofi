using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AIS.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //if (Request.Browser.IsMobileDevice)
            //{
            //    return RedirectToAction("Index", "Book");
            //}

            //ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";
            return RedirectToAction("Index", "Setting");

            //return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
