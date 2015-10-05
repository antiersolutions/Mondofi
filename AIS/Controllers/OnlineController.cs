using AIS.Models;
using AISModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AIS.OnlineExtensions;
using AIS.Helpers;
using AIS.Helpers.Caching;
using AIS.Filters;
using System.Globalization;
using AIS.Models.TableAvailablity;
using System.Web.Security;
using System.Threading;
using AIS.Helpers.Email;
using System.Net.Mail;
using System.Data.SqlClient;
using System.Data;

namespace AIS.Controllers
{
    [AllowAnonymous]
    [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
    [Route("{company}/Online/{action}")]
    public class OnlineController : Controller
    {
        private const int defaultDuration = 90;
        private UsersContext _db;
        private readonly ICacheManager _cache;
        private readonly WorkflowMessageService _wfmService;

        public UsersContext context
        {
            get
            {
                if (_db == null)
                {
                    //using (var con = new SqlConnection("Data Source=DOTNET15; Integrated Security=True;").)
                    //{
                    string DataBaseName;
                    try
                    {
                        DataBaseName = RouteData.Values["company"].ToString();
                    }
                    catch (Exception)
                    {
                        DataBaseName = Request["company"].ToString();

                    }


                    string databasenameRemovesapse = DataBaseName;
                    databasenameRemovesapse = databasenameRemovesapse.Replace(" ", "");
                    //con.Open();
                    //DataTable databases = con.GetSchema("Databases");
                    //foreach (DataRow database in databases.Rows)
                    //{

                    //    String databaseName = database.Field<String>("database_name");
                    //    if (DataBaseName == databaseName)
                    //    {
                    _db = new UsersContext(databasenameRemovesapse);
                    //        }
                    //        else
                    //        {

                    //        }
                    //    }
                    //}


                }

                return _db;
            }
        }

        public string CompanyName
        {
            get
            {
                string databasenameRemovesapse;
                try
                {
                    databasenameRemovesapse = RouteData.Values["company"].ToString();
                }
                catch (Exception)
                {
                    databasenameRemovesapse = Request["company"].ToString();
                }

                databasenameRemovesapse = databasenameRemovesapse.Replace(" ", "");
                return databasenameRemovesapse;
            }
        }

        public OnlineController()
        {

            //var fcompany = RouteData.Values["company"].ToString();
            //_db = new UsersContext(company);
            _cache = new MemoryCacheManager();
            _wfmService = new WorkflowMessageService();
        }

        #region Online Actions

        public ActionResult Index()
        {
            //var users = context.Users.ToList();

            BindTimeList(DateTime.UtcNow.Date);
            ViewBag.date = DateTime.UtcNow.Date;
            return View();
        }

        [HttpPost]
        public ActionResult Index(DateTime date, string time, int covers)
        {

            this.BindTimeList(date);
            ViewBag.date = date;
            ViewBag.time = time;
            ViewBag.covers = covers;

            //return View(this.GetMatchedAvailabilities(date, time, covers));
            return View(this.GetMatchedAvailabilities20150519(date, time, covers));
        }

        public ActionResult UpdateTime(DateTime date)
        {
            #region Get time list
            var clientTime = DateTime.UtcNow.ToDefaultTimeZone(CompanyName); //.ToClientTime();
            clientTime = clientTime.AddMinutes(15 - (clientTime.TimeOfDay.Minutes % 15)).AddSeconds(-1 * clientTime.TimeOfDay.Seconds).AddHours(2);

            var day = date.DayOfWeek.ToString().Trim();
            var dId = context.GetWeekDays().Single(p => p.DayName.Contains(day)).DayId;

            var ttime = context.GetMenuShiftHours().Where(p => p.DayId == dId).AsEnumerable();
            var minOpenAt = ttime.Where(p => p.OpenAt != null).Min(p => Convert.ToDateTime(p.OpenAt));
            var maxCloseAt = ttime.Where(p => p.CloseAt != null).Max(p => Convert.ToDateTime(p.CloseAt).AddDays(Convert.ToInt32(p.IsNext)));

            var openTime = Convert.ToDateTime(minOpenAt);
            var closeTime = Convert.ToDateTime(maxCloseAt);

            if (date == clientTime.Date && openTime <= clientTime)
            {
                openTime = date.AddHours(clientTime.TimeOfDay.Hours).AddMinutes(clientTime.TimeOfDay.Minutes);
            }

            if (date == clientTime.Date && closeTime < clientTime)
            {
                closeTime = date.AddHours(clientTime.TimeOfDay.Hours).AddMinutes(clientTime.TimeOfDay.Minutes);
            }

            var TimeList = new List<SelectListItem>();

            var aa = context.GetMenuShiftHours().AsEnumerable().Where(p => p.DayId == dId);

            while (openTime < closeTime)
            {
                var startTime = openTime;
                openTime = openTime.AddMinutes(15);

                int tShiftId = 0;

                var openTM = new DateTime();
                var closeTM = new DateTime();

                var timeShift = aa.Where(s => (DateTime.TryParse(s.OpenAt, out openTM) && DateTime.TryParse(s.CloseAt, out closeTM)) &&
                    startTime.Date.Add(openTM.TimeOfDay) <= startTime &&
                    startTime.Date.Add(closeTM.TimeOfDay).AddDays(s.IsNext.Value) >= startTime).FirstOrDefault();

                if (timeShift != null)
                {
                    tShiftId = timeShift.FoodMenuShiftId;
                }

                TimeList.Add(new SelectListItem
                {
                    Text = startTime.ToString("hh:mm tt"),
                    Value = new DateTime().Add(startTime.TimeOfDay).ToString("ddMMyyyyhhmmtt") + " - " + new DateTime().Add(openTime.TimeOfDay).ToString("ddMMyyyyhhmmtt") + " - " + tShiftId
                });
            }
            #endregion

            return Json(TimeList, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Reserve(DateTime date, string time, int covers)
        {
            //if (Request.UrlReferrer.LocalPath.ToLower().Contains("fail"))
            //    return RedirectToAction("Index");

            var startTime = date.Add(DateTime.ParseExact(time.Trim(), "h:mm tt", CultureInfo.InvariantCulture).TimeOfDay);
            var endTime = startTime.AddMinutes(defaultDuration);

            var array = new string[] { "Sofa", "Chair", "SofaTable", "Wall", "SolidWall", "GlassWall", "BarTable", "Fence", "Pillar" };
            var tableDB = context.tabFloorTables.Where(p => p.IsDeleted == false).ToList()
                .Where(p => !array.Contains(p.TableName.Split('-')[0])
                 && p.MaxCover >= covers).ToList();

            var day = date.DayOfWeek.ToString();

            var dId = context.GetWeekDays().Single(p => p.DayName.Contains(day)).DayId;
            var availList = context.tabTableAvailabilities
                .Include("TableAvailabilityFloorTables")
                .Include("TableAvailabilityWeekDays")
                .Where(ta => ta.StartDate <= date && date <= ta.EndDate
                && ta.TableAvailabilityWeekDays.Any(taw => taw.DayId == dId)).ToList();

            ViewBag.BlockId = Guid.Empty;

            var exactMatchedDBTables = tableDB.Where(t => availList.CheckAvailStatus(date, startTime, endTime, t, 1)
                && !AnyReservation(t.FloorTableId, startTime, endTime)).ToList();

            if (exactMatchedDBTables.Count == 0)
                return View();

            var firstAvailTable = exactMatchedDBTables.First();

            int tShiftId = 0;

            var openTM = new DateTime();
            var closeTM = new DateTime();

            var aa = context.GetMenuShiftHours().AsEnumerable().Where(p => p.DayId == dId);
            var timeShift = aa.Where(s => (DateTime.TryParse(s.OpenAt, out openTM) && DateTime.TryParse(s.CloseAt, out closeTM)) &&
                startTime.Date.Add(openTM.TimeOfDay) <= startTime &&
                startTime.Date.Add(closeTM.TimeOfDay).AddDays(s.IsNext.Value) >= startTime).FirstOrDefault();

            if (timeShift != null)
            {
                tShiftId = timeShift.FoodMenuShiftId;
            }

            var model = new ReservationVM
            {
                resDate = date,
                time = time,
                Covers = covers,
                tableIdd = firstAvailTable.FloorTableId.ToString(),
                TablePositionLeft = firstAvailTable.TLeft,
                TablePositionTop = firstAvailTable.TTop,
                ShiftId = tShiftId,
                //Duration = "15MIN"
                Duration = "0MIN".AddMinutesToDuration(defaultDuration)
            };

            var blockTime = new FloorTableBlock
            {
                FloorTableBlockId = Guid.NewGuid(),
                BlockDate = date,
                BlockFrom = startTime,
                BlockTo = endTime,
                FloorTableId = firstAvailTable.FloorTableId
            };

            context.tabFloorTableBlocks.Add(blockTime);
            context.SaveChanges();

            ViewBag.BlockId = blockTime.FloorTableBlockId;

            this.ClearReservationCache(context.Database.Connection.Database);

            return View(model);
        }

        [HttpPost]
        public ActionResult Reserve(ReservationVM model)
        {
            Reservation reservation = null;
                     
            try
            {
                var onlineUserName = context.Users.Where(c => c.Roles.Any(ur => ur.RoleId == context.Roles.Where(r => r.Name == "Online").FirstOrDefault().Id)).Single().UserName;
                var onlineUser = context.Users.Where(u => u.UserName.Contains(onlineUserName)).First();

                FloorTableServer server = null;
                var fTblId = Convert.ToInt64(model.tableIdd);

                var flrTbl = context.tabFloorTables.Find(fTblId);
                model.FloorPlanId = flrTbl.FloorPlanId;
                model.MergeTableId = 0;
                server = flrTbl.FloorTableServer;

                double time = 0;
                if (!string.IsNullOrEmpty(model.Email))
                {
                    model.Email = model.Email.Trim();
                }

                model.MobileNumber = model.MobileNumber.Trim();
                // model.ShiftId = model.ShiftId;

                var startTime = model.resDate.Add(DateTime.ParseExact(model.time.Trim(), "h:mm tt", CultureInfo.InvariantCulture).TimeOfDay);

                bool isFakeMobileNo = long.Parse(model.MobileNumber) == 0L;

                var customer = context.tabCustomers.Where(c => !isFakeMobileNo && c.PhoneNumbers.Any(cn => cn.PhoneNumbers.Contains(model.MobileNumber))).FirstOrDefault();

                int tShiftId = 0;

                var openTM = new DateTime();
                var closeTM = new DateTime();

                var day = model.resDate.DayOfWeek.ToString();
                var dId = context.GetWeekDays().Single(p => p.DayName.Contains(day)).DayId;

                var aa = context.GetMenuShiftHours().AsEnumerable().Where(p => p.DayId == dId);
                var timeShift = aa.Where(s => (DateTime.TryParse(s.OpenAt, out openTM) && DateTime.TryParse(s.CloseAt, out closeTM)) &&
                    startTime.Date.Add(openTM.TimeOfDay) <= startTime &&
                    startTime.Date.Add(closeTM.TimeOfDay).AddDays(s.IsNext.Value) >= startTime).FirstOrDefault();

                if (timeShift != null)
                {
                    tShiftId = timeShift.FoodMenuShiftId;
                }

                model.ShiftId = tShiftId;

                model.Status = (!string.IsNullOrEmpty(model.Status)) ? model.Status : ReservationStatus.Online_Booking.ToString();

                #region Old Customer
                //if (customer != null)
                //{
                if (customer != null &&
                   StringComparer.OrdinalIgnoreCase.Equals(customer.FirstName.Trim(), model.FirstName.Trim()) &&
                   StringComparer.OrdinalIgnoreCase.Equals(customer.FirstName.Trim(), model.FirstName.Trim()))
                {
                    reservation = new Reservation()
                    {
                        FloorPlanId = model.FloorPlanId,
                        Covers = model.Covers,
                        CustomerId = customer.CustomerId,
                        FoodMenuShiftId = model.ShiftId,
                        ReservationDate = model.resDate,
                        TimeForm = startTime,
                        TimeTo = startTime.AddMinutes(model.Duration.GetMinutesFromDuration()),
                        FloorTableId = fTblId,
                        MergedFloorTableId = model.MergeTableId.Value,
                        StatusId = Convert.ToInt64(model.Status),
                        UserId = onlineUser.Id,
                        TablePositionLeft = model.TablePositionLeft,
                        TablePositionTop = model.TablePositionTop,
                        Duration = model.Duration,
                        ReservationNote = model.ReservationNote,
                        CreatedOn = DateTime.UtcNow,
                        UpdatedBy = onlineUser.Id,
                        UpdatedOn = DateTime.UtcNow
                    };

                    if (server != null && server.ServerId != null)
                    {
                        reservation.ReservationServer = new ReservationServer() { ServerId = server.ServerId.Value };
                    }

                    context.tabReservations.Add(reservation);

                    if (!string.IsNullOrEmpty(model.GuestNote))
                    {
                        customer.Notes = model.GuestNote;
                    }

                    if (!string.IsNullOrEmpty(model.Email))
                    {
                        if (customer.Emails == null || (customer.Emails != null && !customer.Emails.Any(ce => ce.Email.Contains(model.Email))))
                        {
                            var cemail = new CustomersEmails()
                            {
                                CustomerId = customer.CustomerId,
                                Email = model.Email,
                                EmailTypeId = 1
                            };
                            context.tabCustomersEmails.Add(cemail);
                        }
                    }

                    time = reservation.TimeForm.TimeOfDay.TotalMinutes;

                    context.LogAddReservation(reservation, onlineUser, null);
                }
                #endregion
                #region new customer
                else
                {
                    var cust = new Customers()
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        DateCreated = DateTime.UtcNow, //.ToClientTime(),
                        DateOfBirth = DateTime.UtcNow, //.ToClientTime(),
                        Address1 = "1",
                        Address2 = "2",
                        Anniversary = DateTime.UtcNow //.ToClientTime(),
                    };

                    context.tabCustomers.Add(cust);

                    if (!string.IsNullOrEmpty(model.Email))
                    {
                        var cemail = new CustomersEmails()
                        {
                            CustomerId = cust.CustomerId,
                            Email = model.Email,
                            EmailTypeId = 1
                        };
                        context.tabCustomersEmails.Add(cemail);
                    }


                    var cphone = new CustomersPhoneNumbers()
                    {
                        CustomerId = cust.CustomerId,
                        PhoneNumbers = model.MobileNumber,
                        PhoneTypeId = 1
                    };

                    context.tabCustomersPhoneNumbers.Add(cphone);

                    reservation = new Reservation()
                    {
                        FloorPlanId = model.FloorPlanId,
                        Covers = model.Covers,
                        CustomerId = cust.CustomerId,
                        FoodMenuShiftId = model.ShiftId,
                        ReservationDate = model.resDate,
                        TimeForm = startTime,
                        TimeTo = startTime.AddMinutes(model.Duration.GetMinutesFromDuration()),
                        FloorTableId = fTblId,
                        MergedFloorTableId = model.MergeTableId.Value,
                        StatusId = Convert.ToInt64(model.Status),
                        UserId = onlineUser.Id,
                        TablePositionLeft = model.TablePositionLeft,
                        TablePositionTop = model.TablePositionTop,
                        Duration = model.Duration,
                        ReservationNote = model.ReservationNote,
                        CreatedOn = DateTime.UtcNow,
                        UpdatedBy = onlineUser.Id,
                        UpdatedOn = DateTime.UtcNow
                    };

                    if (server != null && server.ServerId != null)
                    {
                        reservation.ReservationServer = new ReservationServer() { ServerId = server.ServerId.Value };
                    }

                    context.tabReservations.Add(reservation);

                    if (!string.IsNullOrEmpty(model.GuestNote))
                    {
                        cust.Notes = model.GuestNote;
                    }

                    time = reservation.TimeForm.TimeOfDay.TotalMinutes;

                    context.LogAddReservation(reservation, onlineUser, null);
                }

                #endregion

                context.SaveChanges();

                _wfmService.SendCustomerBookingSuccess(this.Url, reservation,context);

                return Redirect(this.Url.EncodedUrl("ReserveSuccess", "Online", new { id = reservation.ReservationId, company = context.Database.Connection.Database }));
            }
            catch (SmtpException)
            {
                return Redirect(this.Url.EncodedUrl("ReserveSuccess", "Online", new { id = reservation.ReservationId ,company=context.Database.Connection.Database}));
            }
            catch (Exception)
            {
                return RedirectToAction("ReserveFail", new { id = 2, company = context.Database.Connection.Database });
            }
            finally
            {
                this.ClearReservationCache(context.Database.Connection.Database);
            }
        }

        public ActionResult Edit(long id, string company)
        {
            var clientTime = DateTime.UtcNow.ToDefaultTimeZone(company); //.ToClientTime();
            ViewBag.company = company;
            //UsersContext _context = new UsersContext(company);
            var reservation = context.tabReservations.SingleOrDefault(r => !r.IsDeleted && r.ReservationId == id);

            if (reservation == null || reservation.StatusId == ReservationStatus.Cancelled)
            {
                return RedirectToAction("ReserveFail", new { id = 4, company = context.Database.Connection.Database });
            }

            if (!IsOnlineUser(reservation.UserId))
            {
                return RedirectToAction("ReserveFail", new { id = 6, company = context.Database.Connection.Database });
            }

            if (reservation.ReservationDate < clientTime.Date || clientTime > reservation.TimeForm.AddHours(-2))
            {
                return RedirectToAction("ReserveFail", new { id = 5, company = context.Database.Connection.Database });
            }

            this.BindTimeList(reservation.ReservationDate);
            var time = new DateTime().Add(reservation.TimeForm.TimeOfDay).ToString("ddMMyyyyhhmmtt") +
                " - " +
                new DateTime().Add(reservation.TimeForm.AddMinutes(15).TimeOfDay).ToString("ddMMyyyyhhmmtt") + " - " + reservation.FoodMenuShiftId;

            //var availableReservations = this.GetMatchedAvailabilities(reservation.ReservationDate, time, reservation.Covers);
            var availableReservations = this.GetMatchedAvailabilities20150519(reservation.ReservationDate, time, reservation.Covers, reservation);
            availableReservations.ExactMatch = availableReservations.ExactMatch ?? new TimeSlotAvailabilities
            {
                StartTime = reservation.TimeForm,
                EndTime = reservation.TimeForm.AddMinutes(reservation.Duration.GetMinutesFromDuration())
            };

            availableReservations.ExactMatch.AvailableTables.Add(reservation.FloorTable);

            var username = (reservation.Customers.FirstName + " " + ((reservation.Customers.LastName.Length > 1) ? reservation.Customers.LastName.Remove(1) : reservation.Customers.LastName));
            ViewBag.UserName = username;
            ViewBag.IsCurrentReservation = true;
            ViewBag.CurrentReservation = reservation;
            ViewBag.date = reservation.ReservationDate;
            ViewBag.time = time;
            ViewBag.covers = reservation.Covers;

            return View(availableReservations);
        }

        [HttpPost]
        public ActionResult Edit(long id, DateTime date, string time, int covers)
        {
            ViewBag.company = CompanyName;
            var clientTime = DateTime.UtcNow.ToDefaultTimeZone(CompanyName); //.ToClientTime();
            bool IsCurrentReservation = false;
            var reservation = context.tabReservations.SingleOrDefault(r => !r.IsDeleted && r.ReservationId == id);

            if (reservation == null || reservation.StatusId == ReservationStatus.Cancelled)
            {
                return RedirectToAction("ReserveFail", new { id = 4, company = context.Database.Connection.Database });
            }

            if (!IsOnlineUser(reservation.UserId))
            {
                return RedirectToAction("ReserveFail", new { id = 6, company = context.Database.Connection.Database });
            }

            if (reservation.ReservationDate < clientTime.Date || clientTime > reservation.TimeForm.AddHours(-2))
            {
                return RedirectToAction("ReserveFail", new { id = 5, company = context.Database.Connection.Database });
            }

            this.BindTimeList(date);
            var resTime = new DateTime().Add(reservation.TimeForm.TimeOfDay).ToString("ddMMyyyyhhmmtt") +
               " - " +
               new DateTime().Add(reservation.TimeForm.AddMinutes(15).TimeOfDay).ToString("ddMMyyyyhhmmtt") + " - " + reservation.FoodMenuShiftId;
            //var availableReservations = this.GetMatchedAvailabilities(date, time, covers);
            var availableReservations = this.GetMatchedAvailabilities20150519(date, time, covers, reservation);

            if (date == reservation.ReservationDate && time == resTime && covers == reservation.Covers)
            {
                availableReservations.ExactMatch = availableReservations.ExactMatch ?? new TimeSlotAvailabilities
                {
                    StartTime = reservation.TimeForm,
                    EndTime = reservation.TimeForm.AddMinutes(reservation.Duration.GetMinutesFromDuration())
                };

                availableReservations.ExactMatch.AvailableTables.Add(reservation.FloorTable);
                IsCurrentReservation = true;
            }

            var username = (reservation.Customers.FirstName + " " + ((reservation.Customers.LastName.Length > 1) ? reservation.Customers.LastName.Remove(1) : reservation.Customers.LastName));
            ViewBag.UserName = username;
            ViewBag.IsCurrentReservation = IsCurrentReservation;
            ViewBag.CurrentReservation = reservation;
            ViewBag.date = date;
            ViewBag.time = time;
            ViewBag.covers = covers;

            return View(availableReservations);
        }

        [HttpPost]
        public ActionResult Update(ReservationVM model)
        {
            var resDb = context.tabReservations.SingleOrDefault(r => !r.IsDeleted && r.ReservationId == model.ReservationId);

            try
            {
                var clientTime = DateTime.UtcNow.ToDefaultTimeZone(CompanyName); //.ToClientTime();

                if (resDb == null || resDb.StatusId == ReservationStatus.Cancelled)
                {
                    return RedirectToAction("ReserveFail", new { id = 4 , company = context.Database.Connection.Database});
                }

                if (!IsOnlineUser(resDb.UserId))
                {
                    return RedirectToAction("ReserveFail", new { id = 6, company = context.Database.Connection.Database });
                }

                if (resDb.ReservationDate < clientTime.Date || clientTime > resDb.TimeForm.AddHours(-2))
                {
                    return RedirectToAction("ReserveFail", new { id = 5, company = context.Database.Connection.Database });
                }

                var onlineUserName = context.Users.Where(c => c.Roles.Any(ur => ur.RoleId == context.Roles.Where(r => r.Name == "Online").FirstOrDefault().Id)).Single().UserName;
                var onlineUser = context.Users.Where(u => u.UserName.Contains(onlineUserName)).First();
                var fTblId = Convert.ToInt64(model.tableIdd);
                var floorTbl = context.tabFloorTables.Find(fTblId);

                model.FloorPlanId = floorTbl.FloorPlanId;
                model.MergeTableId = 0;

                var startTime = model.resDate.Add(DateTime.ParseExact(model.time.Trim(), "h:mm tt", CultureInfo.InvariantCulture).TimeOfDay);
                var endTime = startTime.AddMinutes(15);

                int tShiftId = 0;

                var openTM = new DateTime();
                var closeTM = new DateTime();

                var day = model.resDate.DayOfWeek.ToString();
                var dId = context.GetWeekDays().Single(p => p.DayName.Contains(day)).DayId;

                var aa = context.GetMenuShiftHours().AsEnumerable().Where(p => p.DayId == dId);
                var timeShift = aa.Where(s => (DateTime.TryParse(s.OpenAt, out openTM) && DateTime.TryParse(s.CloseAt, out closeTM)) &&
                    startTime.Date.Add(openTM.TimeOfDay) <= startTime &&
                    startTime.Date.Add(closeTM.TimeOfDay).AddDays(s.IsNext.Value) >= startTime).FirstOrDefault();

                if (timeShift != null)
                {
                    tShiftId = timeShift.FoodMenuShiftId;
                }

                model.ShiftId = tShiftId;

                resDb.Covers = model.Covers;
                resDb.FoodMenuShiftId = model.ShiftId;
                resDb.ReservationDate = model.resDate;
                resDb.TimeForm = startTime;
                resDb.TimeTo = startTime.AddMinutes(defaultDuration);
                resDb.Duration = "0MIN".AddMinutesToDuration(defaultDuration);
                resDb.FloorTableId = fTblId;
                resDb.TablePositionLeft = floorTbl.TLeft;
                resDb.TablePositionTop = floorTbl.TTop;
                //resDb.ReservationNote = model.ReservationNote;
                resDb.UpdatedBy = onlineUser.Id;
                resDb.UpdatedOn = DateTime.UtcNow;

                resDb.FloorPlanId = model.FloorPlanId;

                context.Entry(resDb).State = System.Data.Entity.EntityState.Modified;
                context.LogEditReservation(resDb, onlineUser, null);
                context.SaveChanges();

                _wfmService.SendCustomerBookingSuccess(this.Url, resDb,context);

                return Redirect(this.Url.EncodedUrl("ReserveSuccess", "Online", new { id = resDb.ReservationId, company = context.Database.Connection.Database }));
            }
            catch (SmtpException)
            {
                return Redirect(this.Url.EncodedUrl("ReserveSuccess", "Online", new { id = resDb.ReservationId, company = context.Database.Connection.Database }));
            }
            catch (Exception)
            {
                return RedirectToAction("ReserveFail", new { id = 2, company = context.Database.Connection.Database });
            }
            finally
            {
                this.ClearReservationCache(context.Database.Connection.Database);
            }
        }

        [HttpPost]
        public ActionResult Cancel(long id)
        {
            var clientTime = DateTime.UtcNow.ToDefaultTimeZone(CompanyName); //.ToClientTime();
            var reservation = context.tabReservations.SingleOrDefault(r => !r.IsDeleted && r.ReservationId == id);

            if (reservation == null || reservation.StatusId == ReservationStatus.Cancelled)
            {
                return RedirectToAction("ReserveFail", new { id = 4, company = context.Database.Connection.Database });
            }

            if (!IsOnlineUser(reservation.UserId))
            {
                return RedirectToAction("ReserveFail", new { id = 6, company = context.Database.Connection.Database });
            }

            if (reservation.ReservationDate < clientTime.Date || clientTime > reservation.TimeForm)
            {
                return RedirectToAction("ReserveFail", new { id = 5, company = context.Database.Connection.Database });
            }

            var onlineUserName = context.Users.Where(c => c.Roles.Any(ur => ur.RoleId == context.Roles.Where(r => r.Name == "Online").FirstOrDefault().Id)).Single().UserName;
            var onlineUser = context.Users.Where(u => u.UserName.Contains(onlineUserName)).First();
            reservation.StatusId = ReservationStatus.Cancelled;

            context.Entry(reservation).State = System.Data.Entity.EntityState.Modified;
            context.LogEditReservation(reservation, onlineUser, null);
            context.SaveChanges();

            return View(reservation);
        }

        public ActionResult ReserveFail(FailureType id, string company)
        {
            string message = string.Empty;
            ViewBag.company = company;

            switch (id)
            {
                case FailureType.TimeOut:
                    message = "The time limit for completing this reservation request has expired.";
                    break;
                case FailureType.Exception:
                    message = "An error occured while processing your request. Please try again later.";
                    break;
                case FailureType.AlreadyBooked:
                    message = "Sorry, this reservation is no longer available. Please click <a href='/online'>Search Again</a> to choose another available reservation.";
                    break;
                case FailureType.ReservationNotFound:
                    message = "Sorry, No reservation found.";
                    break;
                case FailureType.ReservationLocked:
                    message = "Sorry, You cannot edit this reservation.";
                    break;
                case FailureType.NotOnlineReservation:
                    message = "This reservation is not created online.";
                    break;
                default:
                    break;
            }

            return View(model: message);
        }

        [EncryptedActionParameter]
        public ActionResult ReserveSuccess(long id,string company)
        {
            var clientTime = DateTime.UtcNow.ToDefaultTimeZone(company); //.ToClientTime();
            UsersContext _context = new UsersContext(company);
            var reservation = _context.tabReservations.SingleOrDefault(r => !r.IsDeleted && r.ReservationId == id);

            if (reservation == null || reservation.StatusId == ReservationStatus.Cancelled)
            {
                return RedirectToAction("ReserveFail", new { id = 4, company = context.Database.Connection.Database });
            }

            if (!IsOnlineUser15062015(reservation.UserId, _context))
            {
                return RedirectToAction("ReserveFail", new { id = 6, company = context.Database.Connection.Database });
            }

            if (reservation.ReservationDate < clientTime.Date || clientTime > reservation.TimeForm.AddHours(-2))
            {
                return RedirectToAction("ReserveFail", new { id = 5, company = context.Database.Connection.Database });
            }
            ViewBag.company = company;
            return View(reservation);
        }

        [HttpPost]
        public void ReleaseTable(Guid id)
        {
            var tableBlock = context.tabFloorTableBlocks.Find(id);
            context.tabFloorTableBlocks.Remove(tableBlock);
            context.SaveChanges();
            this.ClearReservationCache(context.Database.Connection.Database);
        }

        #endregion

        #region private methods

        private bool IsOnlineUser(long userId)
        {
            var onlineUsers = context.Users.Where(c => c.Roles.Any(ur => ur.RoleId == context.Roles.Where(r => r.Name == "Online").FirstOrDefault().Id)).Single().UserName;
            var user = context.Users.Find(userId);

            return onlineUsers.Contains(user.UserName);
        }

        private bool IsOnlineUser15062015(long userId, UsersContext context)
        {
            var onlineUsers = context.Users.Where(c => c.Roles.Any(ur => ur.RoleId == context.Roles.Where(r => r.Name == "Online").FirstOrDefault().Id)).Single().UserName;
            var user = context.Users.Find(userId);

            return onlineUsers.Contains(user.UserName);
        }

        private void ClearReservationCache(string companyName)
        {
            //cache.RemoveByPattern(CacheKeys.RESERVATION_BY_DATE_PATTREN);
            _cache.RemoveByPattern(string.Format(CacheKeys.RESERVATION_BY_DATE_COMPANY_PATTREN, companyName));
            //cache.RemoveByPattern(CacheKeys.FILTERED_RESERVATION_PATTREN);
            _cache.RemoveByPattern(string.Format(CacheKeys.FILTERED_RESERVATION_COMPANY_PATTREN, companyName));
            //cache.RemoveByPattern(CacheKeys.FLOOR_TABLES_SCREEN_PATTREN);
            _cache.RemoveByPattern(string.Format(CacheKeys.FLOOR_TABLES_SCREEN_COMPANY_PATTREN, companyName));
        }

        private void BindTimeList(DateTime date)
        {
            var clientTime = DateTime.UtcNow.ToDefaultTimeZone(CompanyName);   //.ToClientTime();
            clientTime = clientTime.AddMinutes(15 - (clientTime.TimeOfDay.Minutes % 15)).AddSeconds(-1 * clientTime.TimeOfDay.Seconds).AddHours(2);

            int dId;
            DateTime openTime;
            DateTime closeTime;

            GetOpenAndCloseTime(date, out dId, out openTime, out closeTime);

            // close must be 90 min less than origional close time.
            closeTime = closeTime.AddMinutes(-1 * defaultDuration);


            if (date == clientTime.Date && openTime <= clientTime)
            {
                openTime = date.AddHours(clientTime.TimeOfDay.Hours).AddMinutes(clientTime.TimeOfDay.Minutes);
            }

            if (date == clientTime.Date && closeTime < clientTime)
            {
                closeTime = date.AddHours(clientTime.TimeOfDay.Hours).AddMinutes(clientTime.TimeOfDay.Minutes);
            }

            var TimeList = new List<object>();

            var aa = context.GetMenuShiftHours().AsEnumerable().Where(p => p.DayId == dId);

            while (openTime <= closeTime)
            {
                var startTime = openTime;
                openTime = openTime.AddMinutes(15);

                int tShiftId = 0;

                var openTM = new DateTime();
                var closeTM = new DateTime();

                var timeShift = aa.Where(s => (DateTime.TryParse(s.OpenAt, out openTM) && DateTime.TryParse(s.CloseAt, out closeTM)) &&
                    startTime.Date.Add(openTM.TimeOfDay) <= startTime &&
                    startTime.Date.Add(closeTM.TimeOfDay).AddDays(s.IsNext.Value) >= startTime).FirstOrDefault();

                if (timeShift != null)
                {
                    tShiftId = timeShift.FoodMenuShiftId;
                }

                TimeList.Add(new
                {
                    Text = startTime.ToString("hh:mm tt"),
                    Value = new DateTime().Add(startTime.TimeOfDay).ToString("ddMMyyyyhhmmtt") + " - " + new DateTime().Add(openTime.TimeOfDay).ToString("ddMMyyyyhhmmtt") + " - " + tShiftId
                });
            }

            ViewBag.TimeList = TimeList;

            var coverList = new List<object>();

            for (int i = 1; i <= 6; i++)
            {
                coverList.Add(new { Value = i, Text = i + " Cover" });
            }

            ViewBag.CoverList = coverList;
        }

        private void GetOpenAndCloseTime(DateTime date, out int dId, out DateTime openTime, out DateTime closeTime)
        {
            var day = date.DayOfWeek.ToString().Trim();
            dId = context.GetWeekDays().Single(p => p.DayName.Contains(day)).DayId;
            var dayId = dId;

            var ttime = context.GetMenuShiftHours().Where(p => p.DayId == dayId).AsEnumerable();
            var minOpenAt = ttime.Where(p => p.OpenAt != null).Min(p => Convert.ToDateTime(p.OpenAt));
            var maxCloseAt = ttime.Where(p => p.CloseAt != null).Max(p => Convert.ToDateTime(p.CloseAt).AddDays(Convert.ToInt32(p.IsNext)));

            openTime = Convert.ToDateTime(minOpenAt);
            closeTime = Convert.ToDateTime(maxCloseAt);
        }

        //private OnlineAvailTables GetMatchedAvailabilities(DateTime date, string time, int covers)
        //{
        //    var tt = time.Split('-');
        //    var startTime = date.Add(DateTime.ParseExact(tt[0].Trim(), "ddMMyyyyhhmmtt", CultureInfo.InvariantCulture).TimeOfDay);
        //    var endTime = startTime.AddMinutes(15);

        //    var array = new string[] { "Sofa", "Chair", "SofaTable", "Wall", "SolidWall", "GlassWall", "BarTable", "Fence", "Pillar" };
        //    var tableDB = context.tabFloorTables.Where(p => p.IsDeleted == false).ToList()
        //        .Where(p => !array.Contains(p.TableName.Split('-')[0])
        //         && p.MaxCover >= covers).ToList();

        //    var day = date.DayOfWeek.ToString();

        //    var dId = context.GetWeekDays().Single(p => p.DayName.Contains(day)).DayId;
        //    var availList = context.tabTableAvailabilities
        //        .Include("TableAvailabilityFloorTables")
        //        .Include("TableAvailabilityWeekDays")
        //        .Where(ta => ta.StartDate <= date && date <= ta.EndDate
        //        && ta.TableAvailabilityWeekDays.Any(taw => taw.DayId == dId)).ToList();

        //    var model = new OnlineAvailTables();

        //    var exactMatchedDBTables = tableDB.Where(t => availList.CheckAvailStatus(date, startTime, endTime, t, 1)
        //        && !AnyReservation(t.FloorTableId, startTime, endTime)).ToList();

        //    if (exactMatchedDBTables.Count > 0)
        //    {
        //        model.ExactMatch = new TimeSlotAvailabilities
        //        {
        //            StartTime = startTime,
        //            EndTime = endTime,
        //            AvailableTables = exactMatchedDBTables
        //        };
        //    }

        //    var clientTime = DateTime.UtcNow.ToClientTime();
        //    clientTime = clientTime.AddMinutes(15 - (clientTime.TimeOfDay.Minutes % 15)).AddHours(2);

        //    var dayStartTime = (date == clientTime.Date) ? date.AddHours(clientTime.TimeOfDay.Hours).AddMinutes(clientTime.TimeOfDay.Minutes) : date;
        //    var dayEndTime = date.AddDays(1).AddMinutes(-15);

        //    while (dayStartTime <= dayEndTime)
        //    {
        //        if (dayStartTime != startTime)
        //        {
        //            var otherAvailabilities = tableDB.Where(t => availList.CheckAvailStatus(date, dayStartTime, dayStartTime.AddMinutes(15), t, 1)
        //                && !AnyReservation(t.FloorTableId, dayStartTime, dayStartTime.AddMinutes(15))).ToList();

        //            if (otherAvailabilities.Count > 0)
        //            {
        //                model.OtherMatches.Add(new TimeSlotAvailabilities
        //                {
        //                    StartTime = dayStartTime,
        //                    EndTime = dayStartTime.AddMinutes(15),
        //                    AvailableTables = otherAvailabilities
        //                });
        //            }
        //        }

        //        dayStartTime = dayStartTime.AddMinutes(15);
        //    }

        //    return model;
        //}

        //private OnlineAvailTables GetMatchedAvailabilities20150415(DateTime date, string time, int covers)
        //{
        //    var tt = time.Split('-');
        //    var startTime = date.Add(DateTime.ParseExact(tt[0].Trim(), "ddMMyyyyhhmmtt", CultureInfo.InvariantCulture).TimeOfDay);
        //    var endTime = startTime.AddMinutes(defaultDuration);

        //    var array = new string[] { "Sofa", "Chair", "SofaTable", "Wall", "SolidWall", "GlassWall", "BarTable", "Fence", "Pillar" };
        //    var tableDB = context.tabFloorTables.Where(p => p.IsDeleted == false).ToList()
        //        .Where(p => !array.Contains(p.TableName.Split('-')[0])
        //         && p.MaxCover >= covers).ToList();

        //    int dId;
        //    DateTime openTime;
        //    DateTime closeTime;

        //    GetOpenAndCloseTime(date, out dId, out openTime, out closeTime);

        //    var availList = context.tabTableAvailabilities
        //        .Include("TableAvailabilityFloorTables")
        //        .Include("TableAvailabilityWeekDays")
        //        .Where(ta => ta.StartDate <= date && date <= ta.EndDate
        //        && ta.TableAvailabilityWeekDays.Any(taw => taw.DayId == dId)).ToList();

        //    var model = new OnlineAvailTables();

        //    var exactMatchedDBTables = tableDB.Where(t => availList.CheckAvailStatus(date, startTime, endTime, t, 1)
        //        && !AnyReservation(t.FloorTableId, startTime)).ToList();

        //    if (exactMatchedDBTables.Count > 0)
        //    {
        //        model.ExactMatch = new TimeSlotAvailabilities
        //        {
        //            StartTime = startTime,
        //            EndTime = endTime,
        //            AvailableTables = exactMatchedDBTables
        //        };
        //    }

        //    var clientTime = DateTime.UtcNow.ToClientTime();
        //    clientTime = clientTime.AddMinutes(15 - (clientTime.TimeOfDay.Minutes % 15)).AddHours(2);

        //    var dayStartTime = (date == clientTime.Date) ? date.AddHours(clientTime.TimeOfDay.Hours).AddMinutes(clientTime.TimeOfDay.Minutes) : date;
        //    //var dayEndTime = date.AddDays(1).AddMinutes(-15);

        //    var otherStartTime = startTime.AddMinutes(-30);
        //    otherStartTime = otherStartTime >= dayStartTime ? otherStartTime : dayStartTime;

        //    var otherEndTime = startTime.AddMinutes(+30);

        //    if (otherEndTime > date.Add(closeTime.TimeOfDay).AddDays((closeTime.Day - openTime.Day)).AddMinutes(-1 * defaultDuration))
        //        otherEndTime = date.Add(closeTime.TimeOfDay).AddDays((closeTime.Day - openTime.Day)).AddMinutes(-1 * defaultDuration);

        //    while (otherStartTime <= otherEndTime)
        //    {
        //        var slotEndTime = otherStartTime.AddMinutes(defaultDuration);

        //        var otherAvailabilities = tableDB.Where(t => availList.CheckAvailStatus(date, otherStartTime, slotEndTime, t, 1)
        //            && !AnyReservation(t.FloorTableId, otherStartTime)).ToList();

        //        if (otherAvailabilities.Count > 0)
        //        {
        //            model.OtherMatches.Add(new TimeSlotAvailabilities
        //            {
        //                StartTime = otherStartTime,
        //                EndTime = slotEndTime,
        //                AvailableTables = otherAvailabilities
        //            });
        //        }

        //        otherStartTime = otherStartTime.AddMinutes(15);
        //    }

        //    return model;
        //}

        private OnlineAvailTables GetMatchedAvailabilities20150519(DateTime date, string time, int covers, Reservation reservation = null)
        {
            int dId;
            DateTime openTime;
            DateTime closeTime;
            var model = new OnlineAvailTables();

            var tt = time.Split('-');
            var startTime = date.Add(DateTime.ParseExact(tt[0].Trim(), "ddMMyyyyhhmmtt", CultureInfo.InvariantCulture).TimeOfDay);
            var endTime = startTime.AddMinutes(defaultDuration);

            var array = new string[] { "Sofa", "Chair", "SofaTable", "Wall", "SolidWall", "GlassWall", "BarTable", "Fence", "Pillar" };
            var tableDB = context.tabFloorTables.Where(p => p.IsDeleted == false).ToList()
                .Where(p => !array.Contains(p.TableName.Split('-')[0])
                 && p.MaxCover >= covers && covers >= p.MinCover).ToList();

            GetOpenAndCloseTime(date, out dId, out openTime, out closeTime);

            var availList = context.tabTableAvailabilities
                .Include("TableAvailabilityFloorTables")
                .Include("TableAvailabilityWeekDays")
                .Where(ta => ta.StartDate <= date && date <= ta.EndDate
                && ta.TableAvailabilityWeekDays.Any(taw => taw.DayId == dId)).ToList();

            var clientTime = DateTime.UtcNow.ToDefaultTimeZone(CompanyName); //.ToClientTime();
            clientTime = clientTime.AddMinutes(15 - (clientTime.TimeOfDay.Minutes % 15)).AddHours(2);

            var dayStartTime = (date == clientTime.Date) ? date.AddHours(clientTime.TimeOfDay.Hours).AddMinutes(clientTime.TimeOfDay.Minutes) : date;
            //var dayEndTime = date.AddDays(1).AddMinutes(-15);

            var otherStartTime = startTime.AddMinutes(-30);
            otherStartTime = otherStartTime >= dayStartTime ? otherStartTime : dayStartTime;

            var otherEndTime = startTime.AddMinutes(+30);

            if (otherEndTime > date.Add(closeTime.TimeOfDay).AddDays((closeTime.Day - openTime.Day)).AddMinutes(-1 * defaultDuration))
                otherEndTime = date.Add(closeTime.TimeOfDay).AddDays((closeTime.Day - openTime.Day)).AddMinutes(-1 * defaultDuration);

            while (otherStartTime <= otherEndTime)
            {
                var slotEndTime = otherStartTime.AddMinutes(defaultDuration);

                var otherAvailabilities = tableDB.Where(t => availList.CheckAvailStatus(date, otherStartTime, slotEndTime, t, 1)
                   && !AnyReservation(t.FloorTableId, otherStartTime, slotEndTime, reservation)).OrderBy(t => t.MinCover).ThenBy(t => t.MaxCover).ToList();

                // testing
                //var testResult = otherAvailabilities.Select(t => new { Min = t.MinCover, Max = t.MaxCover }).ToList();


                if (startTime == otherStartTime && endTime == slotEndTime)
                {
                    if (otherAvailabilities.Count > 0)
                    {
                        model.ExactMatch = new TimeSlotAvailabilities
                        {
                            StartTime = startTime,
                            EndTime = endTime,
                            AvailableTables = otherAvailabilities
                        };
                    }
                }

                if (otherAvailabilities.Count > 0)
                {
                    model.OtherMatches.Add(new TimeSlotAvailabilities
                    {
                        StartTime = otherStartTime,
                        EndTime = slotEndTime,
                        AvailableTables = otherAvailabilities
                    });
                }

                otherStartTime = otherStartTime.AddMinutes(15);
            }

            return model;
        }

        private bool AnyReservation(long tableId, DateTime startTime, DateTime endTime, Reservation reservation = null)
        {
            var reservations = context.GetReservationByDate(startTime.Date);

            if (reservation != null)
                reservations = reservations.Where(r => r.ReservationId != reservation.ReservationId).ToList();

            return reservations.Any(r => !r.IsDeleted
                && (r.FloorTableId == 0 ? r.MergedFloorTable.OrigionalTables.Any(t => t.FloorTableId == tableId) : r.FloorTableId == tableId)
                && r.ReservationDate == startTime.Date
                //&& (r.TimeForm <= startTime && startTime < r.TimeTo));
                && ((r.TimeForm <= startTime && r.TimeTo >= endTime)
                || (r.TimeForm >= startTime && r.TimeTo <= endTime)
                || (r.TimeForm < startTime && r.TimeTo > startTime)
                || (r.TimeForm < endTime && r.TimeTo > endTime)));
        }

        #endregion

        public enum FailureType
        {
            TimeOut = 1,
            Exception = 2,
            AlreadyBooked = 3,
            ReservationNotFound = 4,
            ReservationLocked = 5,
            NotOnlineReservation = 6
        }

        public class ReservationFailure
        {
            public FailureType FailureType { get; set; }
            public string Reason { get; set; }
        }
    }
}
