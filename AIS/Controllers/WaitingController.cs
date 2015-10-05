using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AIS.Models;
using System.Globalization;
using System.Threading;
using AISModels;
using System.Data;
using AIS.Extensions;
using AIS.Helpers.Caching;
using WebMarkupMin.Core.Minifiers;
using System.Data.Entity;

namespace AIS.Controllers
{
    [Authorize]
    public class WaitingController : Controller
    {
        private UsersContext db = new UsersContext();
        private ICacheManager cache = new MemoryCacheManager();

        #region Waiting methods

        public ActionResult SaveWaiting(WaitingVM model)
        {
            try
            {
                bool isFakeMobileNo = long.Parse(model.MobileNumber) == 0L;

                if (!string.IsNullOrEmpty(model.Email))
                {
                    model.Email = model.Email.Trim();
                }

                model.MobileNumber = model.MobileNumber.Trim();

                var customer = db.tabCustomers.Where(c => !isFakeMobileNo && c.PhoneNumbers.Any(cn => cn.PhoneNumbers.Contains(model.MobileNumber))).FirstOrDefault();


                if (customer != null)
                {
                    var waiting = new Waiting()
                    {
                        Covers = model.Covers,
                        CustomerId = customer.CustomerId,
                        WaitingDate = model.WaitDate,
                        Notes = model.Notes,
                        CreatedOn = DateTime.UtcNow
                    };

                    db.tabWaitings.Add(waiting);
                }
                else
                {
                    var cust = new Customers()
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        DateCreated = DateTime.UtcNow,
                        DateOfBirth = DateTime.UtcNow,
                        Address1 = "1",
                        Address2 = "2",
                        Anniversary = DateTime.UtcNow,
                    };

                    db.tabCustomers.Add(cust);

                    if (!string.IsNullOrEmpty(model.Email))
                    {
                        var cemail = new CustomersEmails()
                        {
                            CustomerId = cust.CustomerId,
                            Email = model.Email,
                            EmailTypeId = 1
                        };
                        db.tabCustomersEmails.Add(cemail);
                    }


                    var cphone = new CustomersPhoneNumbers()
                    {
                        CustomerId = cust.CustomerId,
                        PhoneNumbers = model.MobileNumber,
                        PhoneTypeId = 1
                    };

                    db.tabCustomersPhoneNumbers.Add(cphone);

                    var waiting = new Waiting()
                    {
                        Covers = model.Covers,
                        CustomerId = cust.CustomerId,
                        WaitingDate = model.WaitDate,
                        Notes = model.Notes,
                        CreatedOn = DateTime.UtcNow
                    };

                    db.tabWaitings.Add(waiting);

                }

                db.SaveChanges();

                ClearWaitingCache();

                return Json(new
                {
                    Status = ResponseStatus.Success,
                    Message = "Waitlist updated successfully.",
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new
                {
                    Status = ResponseStatus.Fail,
                    Message = "Failed to add waitlist booking, please try later..."
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult UpdateWaiting(WaitingVM model)
        {
            try
            {
                var waiting = db.tabWaitings.Find(model.WaitingId);
                waiting.Covers = model.Covers;
                waiting.Notes = model.Notes;

                if (!string.IsNullOrEmpty(model.GuestNote))
                {
                    waiting.Customer.Notes = model.GuestNote;
                }

                db.SaveChanges();

                ClearWaitingCache();

                return Json(new
                {
                    Status = ResponseStatus.Success,
                    Message = "Waitlist updated successfully.",
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new
                {
                    Status = ResponseStatus.Fail,
                    Message = "Failed to update waitlist booking, please try later..."
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult DeleteWaiting(Int64 WaitingId)
        {
            try
            {
                var waiting = db.tabWaitings.Find(WaitingId);

                db.Entry(waiting).State = EntityState.Deleted;
                db.SaveChanges();

                return Json(new
                {
                    Status = ResponseStatus.Success,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new
                {
                    Status = ResponseStatus.Fail,
                    Message = "Failed to delete this waitlist booking, please try later..."
                }, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                ClearWaitingCache();
            }
        }

        #endregion

        #region Floor Plan waiting methods

        public ActionResult GetAddToWaiting(WaitingVM model)
        {
            var coverList = new List<object>();

            for (int i = 1; i <= 16; i++)
            {
                coverList.Add(new { Value = i, Text = i + " Cover" });
            }

            ViewBag.CoverList = coverList;
            model.MobileNumber = "0000000000";

            return PartialView("AddToWaitListPartial", model);
        }

        public PartialViewResult GetAllWaitingList(DateTime WaitDate)
        {
            var waitList = GetWaitListItems(WaitDate);

            var clientDate = DateTime.UtcNow.ToDefaultTimeZone(User.Identity.GetDatabaseName()).Date;
            ViewBag.WaitCount = db.GetWaitingByDate(clientDate).Where(w => w.ReservationId == 0).Count();

            return PartialView("WaitingListPartial", waitList);
        }

        #endregion

        #region private methods

        private IList<WaitListItemVM> GetWaitListItems(DateTime date)
        {
            var waitList = db.GetWaitingByDate(date).Where(w => w.ReservationId == 0);
            var coverList = new List<object>();
            for (int i = 1; i <= 30; i++)
            {
                coverList.Add(new { Value = i, Text = i + " Cover" });
            }

            ViewBag.CoverList = coverList;
            var htmlMinifier = new HtmlMinifier();
            return waitList.Select(w => new WaitListItemVM
            {
                Waiting = w,
                HTMLString = cache.Get<string>(string.Format(CacheKeys.WAITING_RIGHT_LIST_ITEM,User.Identity.GetDatabaseName(), w.WaitingId), () =>
                {
                    ModelState.Clear();
                    return htmlMinifier.Minify(this.RenderPartialViewToString("~/Views/Waiting/WaitingListItemPartial.cshtml", w),
                        generateStatistics: false).MinifiedContent;
                })
            }).ToList();
        }

        private void ClearWaitingCache()
        {
            //cache.RemoveByPattern(CacheKeys.WAITING_PATTERN);
            cache.RemoveByPattern(string.Format(CacheKeys.WAITING_COMPANY_PATTERN, User.Identity.GetDatabaseName()));
        }

        #endregion

        #region Controller overridden methods

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);

            CultureInfo cultureInfo = CultureInfo.GetCultureInfo("en-US");

            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        #endregion
    }
}
