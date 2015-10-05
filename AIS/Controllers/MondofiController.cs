using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AIS.Controllers
{
   
    public class MondofiController : Controller
    {
        public ActionResult Index()
        {
            //if (Request.Browser.IsMobileDevice)
            //{
            //    return RedirectToAction("Index", "Book");
            //}

            //ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";
            return View();

            //return View();
        }



        public ActionResult Price()
        {
            //if (Request.Browser.IsMobileDevice)
            //{
            //    return RedirectToAction("Index", "Book");
            //}

            //ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";
            return View();

            //return View();
        }

        public ActionResult Index2()
        {
            //if (Request.Browser.IsMobileDevice)
            //{
            //    return RedirectToAction("Index", "Book");
            //}

            //ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";
            return View();

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
