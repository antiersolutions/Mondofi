using AIS.Filters;
using AIS.Helpers;
using AIS.Models;
using AISModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AIS.Helpers.Caching;

namespace AIS.Controllers
{
    [Authorize]
    public class HistoryController : Controller
    {
        private UsersContext db = new UsersContext();

        public ActionResult Index()
        {
            ViewBag.shiftDdl = new SelectList(db.tabFoodMenuShift, "FoodMenuShiftId", "MenuShift");
            return View();
        }

        public ActionResult HistoryList(DateTime startDate, DateTime endDate, Int64? shiftId, string name,
            string auditAction, string sortColumn = "Time", string sortby = "desc")
        {
            //ViewBag.statusDDl = new SelectList(db.Status, "StatusId", "StatusName");
            ViewBag.statusData = db.Status.ToList();
            ViewBag.SearchKey = string.Empty;
            ViewBag.SortColumn = sortColumn;
            ViewBag.SortBy = sortby;
            endDate = endDate.AddDays(1);

            var rec = db.GetReservationByDateRange(startDate, endDate, true);
            rec = rec.Where(r => r.ReservationAudits.Any()).ToList();

            if (shiftId.HasValue)
            {
                rec = rec.Where(p => p.FoodMenuShiftId == shiftId.Value).ToList();
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                ViewBag.SearchKey = name;
                name = name.Trim().ToLower();
                long pnum = 0;

                if (long.TryParse(name, out pnum))
                {
                    rec = rec.Where(r => r.Customers.PhoneNumbers.Any(pn => pn.PhoneNumbers.ToLower().Contains(name))
                        || r.ReservationId == pnum).ToList();
                }
                else
                {
                    rec = rec.Where(r => r.Customers.FirstName.ToLower().Contains(name)
                        || r.Customers.LastName.ToLower().Contains(name)
                        || (r.ReservationAudits.Any()
                                && (r.ReservationAudits.Any(a => (a.PinUserId.HasValue ? (a.PinUser.FirstName.Contains(name) || a.PinUser.LastName.Contains(name)) : false)
                                    || (a.LoginUser.FirstName.Contains(name) || a.LoginUser.LastName.Contains(name)))))
                        || (r.Customers.Emails.Any() && r.Customers.Emails.Any(e => e.Email.StartsWith(name)))).ToList();
                }
            }

            if (!string.IsNullOrWhiteSpace(auditAction))
            {
                rec = rec.Where(r => r.ReservationAudits.Last().Action == auditAction).ToList();
            }

            var orderByColumn = sortColumn + " " + sortby;
            orderByColumn = orderByColumn.Trim().ToLower();

            switch (orderByColumn)
            {
                case "date":
                    rec = rec.OrderBy(r => r.ReservationDate).ToList();
                    break;
                case "date desc":
                    rec = rec.OrderByDescending(r => r.ReservationDate).ToList();
                    break;
                case "time":
                    rec = rec.OrderBy(r => r.TimeForm).ToList();
                    break;
                case "time desc":
                    rec = rec.OrderByDescending(r => r.TimeForm).ToList();
                    break;
                case "covers":
                    rec = rec.OrderBy(r => r.Covers).ToList();
                    break;
                case "covers desc":
                    rec = rec.OrderByDescending(r => r.Covers).ToList();
                    break;
                case "guest last name":
                    rec = rec.OrderBy(r => r.Customers.LastName).ToList();
                    break;
                case "guest last name desc":
                    rec = rec.OrderByDescending(r => r.Customers.LastName).ToList();
                    break;
                case "guest first name":
                    rec = rec.OrderBy(r => r.Customers.FirstName).ToList();
                    break;
                case "guest first name desc":
                    rec = rec.OrderByDescending(r => r.Customers.FirstName).ToList();
                    break;
                case "guest ph.no.":
                    rec = rec.OrderBy(r => r.Customers.PhoneNumbers.FirstOrDefault().PhoneNumbers).ToList();
                    break;
                case "guest ph.no. desc":
                    rec = rec.OrderByDescending(r => r.Customers.PhoneNumbers.FirstOrDefault().PhoneNumbers).ToList();
                    break;
                case "guest email":
                    rec = rec.OrderBy(r => (r.Customers.Emails.FirstOrDefault() ?? new CustomersEmails()).Email).ToList();
                    break;
                case "guest email desc":
                    rec = rec.OrderByDescending(r => (r.Customers.Emails.FirstOrDefault() ?? new CustomersEmails()).Email).ToList();
                    break;
                case "table":
                    rec = rec.OrderBy(r => r.FloorTableId > 0 ? r.FloorTable.TableName : r.MergedFloorTable.TableName, new AlphaNumericComparer()).ToList();
                    break;
                case "table desc":
                    rec = rec.OrderByDescending(r => r.FloorTableId > 0 ? r.FloorTable.TableName : r.MergedFloorTable.TableName, new AlphaNumericComparer()).ToList();
                    break;
                case "notes":
                    rec = rec.OrderBy(r => r.ReservationNote).ToList();
                    break;
                case "notes desc":
                    rec = rec.OrderByDescending(r => r.ReservationNote).ToList();
                    break;
                case "action":
                    rec = rec.OrderBy(r => r.ReservationAudits.LastOrDefault().Action).ToList();
                    break;
                case "action desc":
                    rec = rec.OrderByDescending(r => r.ReservationAudits.LastOrDefault().Action).ToList();
                    break;
                case "action date":
                case "action time":
                    rec = rec.OrderBy(r => r.ReservationAudits.LastOrDefault().CreatedOn).ToList();
                    break;
                case "action date desc":
                case "action time desc":
                    rec = rec.OrderByDescending(r => r.ReservationAudits.LastOrDefault().CreatedOn).ToList();
                    break;
                case "performed by":
                    rec = rec.OrderBy(r =>
                    {
                        var user = r.ReservationAudits.LastOrDefault().PinUser ?? r.ReservationAudits.LastOrDefault().LoginUser;
                        return user.FirstName + user.LastName;
                    }).ToList();
                    break;
                case "performed by desc":
                    rec = rec.OrderByDescending(r =>
                    {
                        var user = r.ReservationAudits.LastOrDefault().PinUser ?? r.ReservationAudits.LastOrDefault().LoginUser;
                        return user.FirstName + user.LastName;
                    }).ToList();
                    break;
                case "reservation id":
                    rec = rec.OrderBy(r => r.ReservationId).ToList();
                    break;
                case "reservation id desc":
                    rec = rec.OrderByDescending(r => r.ReservationId).ToList();
                    break;
                default:
                    rec = rec.OrderBy(r => r.ReservationId).ToList();
                    break;
            }



            return PartialView(rec);
        }
    }
}
