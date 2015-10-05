using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AIS.Models;
using System.Globalization;
using AIS.Extensions;
using AISModels;
using AIS.Helpers.Caching;
using Microsoft.AspNet.Identity;

namespace AIS.Controllers
{
    [Authorize]
    public class CalendarController : Controller
    {
        private UsersContext db = new UsersContext();

        public ActionResult Weeks()
        {
            ViewBag.shiftDdl = new SelectList(db.tabFoodMenuShift, "FoodMenuShiftId", "MenuShift");
            return View();
        }

        public ActionResult WeekList(DateTime date, Int64? shiftId, string name)
        {
            ViewBag.date = date;
            ViewBag.shiftId = shiftId;
            ViewBag.searchText = name ?? string.Empty;

            var reservations = db.tabReservations.Include("Customers").Where(r => !r.IsDeleted).AsQueryable();
            if (shiftId.HasValue)
            {
                reservations = reservations.Where(p => p.FoodMenuShiftId == shiftId.Value);
            }

            if (!string.IsNullOrEmpty(name))
            {
                name = name.Trim().ToLower();
                long pnum = 0;
                if (long.TryParse(name, out pnum))
                {
                    reservations = reservations.Where(r => r.Customers.PhoneNumbers.Any(pn => pn.PhoneNumbers.ToLower().Contains(name)));
                }
                else
                {
                    reservations = reservations.Where(r => r.Customers.FirstName.ToLower().Contains(name) || r.Customers.LastName.ToLower().Contains(name));
                }
            }

            var shift = db.tabMenuShiftHours.ToList();

            var daysInWeek = date.GetDaysInWeek(CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek);

            var startTimeOfWeek = string.Empty;
            var endTimeOfWeek = string.Empty;

            db.GetStartEndTimeList(daysInWeek.First().ToString("dddd, dd MMM, yy"), daysInWeek.Last().ToString("dddd, dd MMM, yy"), out startTimeOfWeek, out endTimeOfWeek);

            var st = DateTime.Parse(startTimeOfWeek);//var st = DateTime.Parse("4:00:00");
            st = st.AddMinutes(-(st.TimeOfDay.Minutes));

            var et = DateTime.Parse(endTimeOfWeek);
            et = et.AddMinutes(-(et.TimeOfDay.Minutes));
            et = et.IsNextDay() ? et.AddDays(1) : et.AddMinutes(-(et.TimeOfDay.Minutes)); //var et = st.AddHours(23);

            var listTime = new List<TimeData>();
            listTime.Add(new TimeData()
            {
                time = st.TimeOfDay
            });

            while (st < et)
            {
                st = st.AddHours(1);
                listTime.Add(new TimeData() { time = st.TimeOfDay });
            }

            var weekData = new WeekVM();
            weekData.TimeData = listTime;

            foreach (var item in weekData.TimeData)
            {
                item.wDay = new List<wDay>();

                foreach (DateTime d in daysInWeek)
                {
                    var resForDay = reservations.Where(r => r.ReservationDate == d).ToList();

                    var timeNow = d.Date.AddTicks(item.time.Ticks);
                    var timeafterAnHour = timeNow.AddHours(1);
                    var wday = new wDay() { day = d };
                    wday.Reservations = new List<Reservation>();

                    //resForDay = resForDay.Where(r => timeNow <= (r.TimeForm.IsNextDay() ? r.TimeForm.AddDays(1) : r.TimeForm) &&
                    //    timeafterAnHour >= (r.TimeTo.IsNextDay() ? r.TimeTo.AddDays(1) : r.TimeTo)).ToList();

                    resForDay = resForDay.Where(r =>
                            (r.TimeForm.IsNextDay() ? r.TimeForm.AddDays(1) : r.TimeForm) >= timeNow
                            && (r.TimeForm.IsNextDay() ? r.TimeForm.AddDays(1) : r.TimeForm) < timeafterAnHour)
                            .OrderBy(r => r.TimeForm)
                            .ToList();

                    wday.Reservations.AddRange(resForDay ?? new List<Reservation>());

                    item.wDay.Add(wday);
                }
            }

            return PartialView(weekData);
        }

        public ActionResult Days()
        {
            ViewBag.shiftDdl = new SelectList(db.tabFoodMenuShift, "FoodMenuShiftId", "MenuShift");
            return View();
        }

        public ActionResult DayList(DateTime date, Int64? shiftId, string name)
        {
            //ViewBag.statusDDl = new SelectList(db.Status, "StatusId", "StatusName");
            ViewBag.statusData = db.Status.ToList();


            var rec = db.tabReservations.Include("Customers").Where(r => !r.IsDeleted).Where(r => r.ReservationDate == date).AsQueryable();
            if (shiftId.HasValue)
            {
                rec = rec.Where(p => p.FoodMenuShiftId == shiftId.Value);
            }

            if (!string.IsNullOrEmpty(name))
            {
                name = name.Trim().ToLower();
                long pnum = 0;

                if (long.TryParse(name, out pnum))
                {
                    rec = rec.Where(r => r.Customers.PhoneNumbers.Any(pn => pn.PhoneNumbers.ToLower().Contains(name)));
                }
                else
                {
                    rec = rec.Where(r => r.Customers.FirstName.ToLower().Contains(name) || r.Customers.LastName.ToLower().Contains(name));
                }
            }

            var model = rec.OrderBy(r => r.TimeForm).ToList();

            return PartialView(model);
        }

        public ActionResult Months()
        {
            ViewBag.shiftDdl = new SelectList(db.tabFoodMenuShift, "FoodMenuShiftId", "MenuShift");
            return View();
        }

        public ActionResult MonthList(int month, int year, Int64? shiftId, string name)
        {
            ViewBag.shiftId = shiftId;
            ViewBag.searchText = name ?? string.Empty;

            var monthData = new MonthVM();
            int totalCovers;
            int totalParties;

            this.PopulateMonthVM(monthData, month, year, shiftId, name, out totalCovers, out totalParties);
            this.PopulateMonthCalenderTopOption(month, year);

            ViewBag.TotalCovers = totalCovers;
            ViewBag.TotalParties = totalParties;
            ViewBag.date = DateTime.UtcNow.ToDefaultTimeZone(User.Identity.GetDatabaseName()).Date;

            return PartialView(monthData);
        }

        private void PopulateMonthCalenderTopOption(int month, int year)
        {
            var data = new List<object>();
            ViewBag.month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month) + "/" + year;
            data.AddRange(DateTimeFormatInfo.InvariantInfo.MonthNames
                .TakeWhile(m => m != string.Empty)
                .Select((m, i) => new
                {
                    Month = i + 1 + "/" + (year - 1),
                    MonthName = m + " " + (year - 1)
                })
                .ToList());
            data.AddRange(DateTimeFormatInfo.InvariantInfo.MonthNames
                .TakeWhile(m => m != string.Empty)
                .Select((m, i) => new
                {
                    Month = i + 1 + "/" + year,
                    MonthName = m + " " + year
                })
                .ToList());
            data.AddRange(DateTimeFormatInfo.InvariantInfo.MonthNames
                .TakeWhile(m => m != string.Empty)
                .Select((m, i) => new
                {
                    Month = i + 1 + "/" + (year + 1),
                    MonthName = m + " " + (year + 1)
                })
                .ToList());

            ViewBag.SelectList = new SelectList(data, "Month", "MonthName", month + "/" + year);
        }

        public ActionResult GetShiftNote(ShiftNotes model)
        {
            ModelState.Clear();

            var dbshiftNote = db.ShiftNotes.Where(sn => sn.Date == model.Date.Date).FirstOrDefault();

            if (dbshiftNote == null)
            {
                return PartialView("ShiftNotes", model);
            }
            else
            {
                return PartialView("ShiftNotes", dbshiftNote);
            }
        }

        public ActionResult DeleteReservation(Int64 id)
        {
            try
            {
                var loginUser = db.Users.Find(User.Identity.GetUserId<long>());
                var rec = db.tabReservations.Find(id);

                //db.tabReservations.Remove(rec);
                rec.IsDeleted = true;
                db.Entry(rec).State = System.Data.Entity.EntityState.Modified;

                db.LogDeleteReservation(rec, loginUser, null);

                db.SaveChanges();
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult SaveShiftNotes(ShiftNotes model)
        {
            try
            {
                string msg = string.Empty;

                if (model.DayShiftNotesId > 0)
                {
                    db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                    msg = "Successfully Updated.";
                }
                else
                {
                    db.ShiftNotes.Add(model);
                    msg = "Successfully Saved.";
                }

                db.SaveChanges();

                return Json(new { result = true, msz = msg }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                //var ss = ex.InnerException.Message;
                return Json(new { result = false, msz = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult UpdateReservationStatus(Int64 reservationId, Int64 statusId)
        {
            try
            {
                var res = db.tabReservations.Where(r => !r.IsDeleted).SingleOrDefault(r => r.ReservationId == reservationId);
                res.StatusId = statusId;
                res.UpdatedBy = User.Identity.GetUserId<long>();
                res.UpdatedOn = DateTime.UtcNow;

                db.SaveChanges();
                return Json(new { result = true, msz = "Status update successfully.", UpdateTime = res.UpdatedOn.Value.ToClientTime().TimeAgo() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { result = false, msz = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private void PopulateMonthVM(MonthVM monthVM, int month, int year, Int64? shiftId, string name, out int totalCovers, out int totalParty)
        {
            totalCovers = 0;
            totalParty = 0;

            monthVM.Week1 = new List<wDay>();
            monthVM.Week2 = new List<wDay>();
            monthVM.Week3 = new List<wDay>();
            monthVM.Week4 = new List<wDay>();
            monthVM.Week5 = new List<wDay>();
            monthVM.Week6 = new List<wDay>();

            List<DateTime> dt = new List<DateTime>();
            dt = GetDates(year, month);

            foreach (DateTime d in dt)
            {
                var resForDay = db.GetReservationByDate(d);
                var wday = new wDay() { day = d, Reservations = new List<Reservation>() };

                if (shiftId.HasValue && shiftId.Value > 0)
                    resForDay = resForDay.Where(r => r.FoodMenuShiftId == shiftId).ToList();

                if (!string.IsNullOrEmpty(name))
                {
                    name = name.Trim().ToLower();
                    long pnum = 0;
                    if (long.TryParse(name, out pnum))
                    {
                        resForDay = resForDay.Where(r => r.Customers.PhoneNumbers.Any(pn => pn.PhoneNumbers.ToLower().Contains(name))).ToList();
                    }
                    else
                    {
                        resForDay = resForDay.Where(r => r.Customers.FirstName.ToLower().Contains(name) || r.Customers.LastName.ToLower().Contains(name)).ToList();
                    }
                }

                resForDay = resForDay.OrderBy(r => r.TimeForm).ToList();
                wday.Reservations.AddRange(resForDay ?? new List<Reservation>());
                totalCovers = totalCovers + wday.Reservations.Sum(r => r.Covers);
                totalParty = totalParty + wday.Reservations.Count();

                switch (GetWeekOfMonth(d))
                {
                    case 1:
                        monthVM.Week1.Add(wday);
                        break;
                    case 2:
                        monthVM.Week2.Add(wday);
                        break;
                    case 3:
                        monthVM.Week3.Add(wday);
                        break;
                    case 4:
                        monthVM.Week4.Add(wday);
                        break;
                    case 5:
                        monthVM.Week5.Add(wday);
                        break;
                    case 6:
                        monthVM.Week6.Add(wday);
                        break;
                };
            }

            while (monthVM.Week1.Count < 7) // not starting from sunday
            {
                wDay dy = null;
                monthVM.Week1.Insert(0, dy);
            }

            if (month == 12)
            {
                monthVM.nextMonth = (01).ToString() + "/" + (year + 1).ToString();
                monthVM.prevMonth = (month - 1).ToString() + "/" + (year).ToString();
            }
            else if (month == 1)
            {
                monthVM.nextMonth = (month + 1).ToString() + "/" + (year).ToString();
                monthVM.prevMonth = (12).ToString() + "/" + (year - 1).ToString();
            }
            else
            {
                monthVM.nextMonth = (month + 1).ToString() + "/" + (year).ToString();
                monthVM.prevMonth = (month - 1).ToString() + "/" + (year).ToString();
            }
        }

        public static List<DateTime> GetDates(int year, int month)
        {
            return Enumerable.Range(1, DateTime.DaysInMonth(year, month))  // Days: 1, 2 ... 31 etc.
                             .Select(day => new DateTime(year, month, day)) // Map each day to a date
                             .ToList();
        }

        public static int GetWeekOfMonth(DateTime date)
        {
            DateTime beginningOfMonth = new DateTime(date.Year, date.Month, 1);
            while (date.Date.AddDays(1).DayOfWeek != DayOfWeek.Sunday)//CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek)
                date = date.AddDays(1);
            return (int)Math.Truncate((double)date.Subtract(beginningOfMonth).TotalDays / 7f) + 1;
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
