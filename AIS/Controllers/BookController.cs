using AIS.Filters;
using AIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AIS.Helpers.Caching;

namespace AIS.Controllers
{
    [Authorize]
    public class BookController : Controller
    {
        private UsersContext db = new UsersContext();
        //
        // GET: /Book/
        //[AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        public PartialViewResult GetAllReservationList(DateTime date)
        {

            var reservations = db.GetReservationByDate(date);

            return PartialView("~/Views/Book/ReservationListPartial.cshtml", reservations);

        }
    }
}
