using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AIS.Models;
using System.Globalization;
using System.Data;
using System.Threading;
using AIS.Extensions;
using AISModels;
using AIS.Helpers.Caching;
using WebMarkupMin.Core.Minifiers;
using AIS.Helpers.Fakes;
using System.Web.Routing;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using System.Data.Entity;

namespace AIS.Controllers
{
    [Authorize]
    public class ReservationController : Controller
    {
        private UsersContext db = new UsersContext();
        private ICacheManager cache = new MemoryCacheManager();

        #region Reservation Update

        [HttpPost]
        public ActionResult UpdateReservation(ReservationVM model)
        {
            try
            {
                var loginUser = db.Users.Find(User.Identity.GetUserId<long>());
                var fTblId = Convert.ToInt64(model.tableIdd);

                if (fTblId == 0 && model.MergeTableId == 0)
                {
                    return Json(new
                    {
                        Status = ResponseStatus.Fail,
                        Message = "Please select a table."
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (fTblId > 0)
                    {
                        model.FloorPlanId = db.tabFloorTables.Find(fTblId).FloorPlanId;
                        model.MergeTableId = 0;
                    }
                    else
                    {
                        model.FloorPlanId = db.tabMergedFloorTables.Find(model.MergeTableId.Value).FloorPlanId;
                    }
                }

                if (!string.IsNullOrEmpty(model.time))
                {
                    var tt = model.time.Split('-');

                    var startTime = model.resDate.Add(DateTime.ParseExact(tt[0].Trim(), "ddMMyyyyhhmmtt", CultureInfo.InvariantCulture).TimeOfDay);
                    var endTime = model.resDate.Add(DateTime.ParseExact(tt[1].Trim(), "ddMMyyyyhhmmtt", CultureInfo.InvariantCulture).TimeOfDay);

                    var sftId = Convert.ToInt32(tt[2]);

                    var resDb = db.tabReservations.SingleOrDefault(r => !r.IsDeleted && r.ReservationId == model.ReservationId);

                    resDb.Covers = model.Covers;
                    resDb.Duration = model.Duration;
                    resDb.FoodMenuShiftId = sftId;
                    resDb.ReservationDate = model.resDate;
                    resDb.StatusId = Convert.ToInt32(model.Status);
                    resDb.FoodMenuShiftId = model.ShiftId;
                    resDb.TimeForm = startTime;
                    resDb.TimeTo = startTime.AddMinutes(model.Duration.GetMinutesFromDuration());
                    resDb.FloorTableId = fTblId;
                    resDb.MergedFloorTableId = model.MergeTableId.Value;
                    resDb.ReservationNote = model.ReservationNote;
                    resDb.UpdatedBy = User.Identity.GetUserId<long>();
                    resDb.UpdatedOn = DateTime.UtcNow;

                    if (!string.IsNullOrEmpty(model.TablePositionTop) && !string.IsNullOrEmpty(model.TablePositionLeft))
                    {
                        resDb.TablePositionTop = model.TablePositionTop;
                        resDb.TablePositionLeft = model.TablePositionLeft;
                    }

                    if (!string.IsNullOrEmpty(model.GuestNote))
                    {
                        resDb.Customers.Notes = model.GuestNote;
                    }

                    //var floorId = db.tabFloorTables.Find(resDb.FloorTableId).FloorPlanId;
                    //resDb.FloorPlanId = floorId;

                    resDb.FloorPlanId = model.FloorPlanId;

                    db.Entry(resDb).State = System.Data.Entity.EntityState.Modified;
                    db.LogEditReservation(resDb, loginUser, null);
                    db.SaveChanges();

                    var listItem = GetReservationListItem(resDb.ReservationId);

                    return Json(new
                    {
                        Status = ResponseStatus.Success,
                        Message = "Reservation updated successfully...",
                        ListItem = listItem,
                        Time = resDb.TimeForm.TimeOfDay.TotalMinutes
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    throw new ArgumentNullException("Time");
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Status = ResponseStatus.Fail,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                this.ClearReservationCache(model.resDate);
            }
        }

        [HttpPost]
        public ActionResult UpdateReservationWithPIN(ReservationVM model)
        {
            Reservation resDb = null;
            try
            {
                var loginUser = db.Users.Find(User.Identity.GetUserId<long>());
                UserProfile pinUser = null;

                if ((model.PIN.HasValue || loginUser.EnablePIN) && !model.MobileNumber.Contains("9999999999"))
                {
                    pinUser = db.Users.Where(u => u.UserCode == model.PIN.Value).FirstOrDefault();

                    if (pinUser == null)
                    {
                        return Json(new
                        {
                            Status = ResponseStatus.Fail,
                            Message = "Please enter a valid user PIN."
                        }, JsonRequestBehavior.AllowGet);
                    }
                }

                var fTblId = Convert.ToInt64(model.tableIdd);

                if (fTblId == 0 && model.MergeTableId == 0)
                {
                    return Json(new
                    {
                        Status = ResponseStatus.Fail,
                        Message = "Please select a table."
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (fTblId > 0)
                    {
                        model.FloorPlanId = db.tabFloorTables.Find(fTblId).FloorPlanId;
                        model.MergeTableId = 0;
                    }
                    else
                    {
                        model.FloorPlanId = db.tabMergedFloorTables.Find(model.MergeTableId.Value).FloorPlanId;
                    }
                }

                if (!string.IsNullOrEmpty(model.time))
                {
                    var tt = model.time.Split('-');

                    var startTime = model.resDate.Add(DateTime.ParseExact(tt[0].Trim(), "ddMMyyyyhhmmtt", CultureInfo.InvariantCulture).TimeOfDay);
                    var endTime = model.resDate.Add(DateTime.ParseExact(tt[1].Trim(), "ddMMyyyyhhmmtt", CultureInfo.InvariantCulture).TimeOfDay);

                    var sftId = Convert.ToInt32(tt[2]);

                    resDb = db.tabReservations.SingleOrDefault(r => !r.IsDeleted && r.ReservationId == model.ReservationId);

                    resDb.Covers = model.Covers;
                    resDb.Duration = model.Duration;
                    resDb.FoodMenuShiftId = sftId;
                    resDb.ReservationDate = model.resDate;
                    resDb.StatusId = Convert.ToInt32(model.Status);
                    resDb.FoodMenuShiftId = model.ShiftId;
                    resDb.TimeForm = startTime;
                    resDb.TimeTo = startTime.AddMinutes(model.Duration.GetMinutesFromDuration());
                    resDb.FloorTableId = fTblId;
                    resDb.MergedFloorTableId = model.MergeTableId.Value;
                    resDb.ReservationNote = model.ReservationNote;
                    resDb.UpdatedBy = User.Identity.GetUserId<long>();
                    resDb.UpdatedOn = DateTime.UtcNow;

                    if (!string.IsNullOrEmpty(model.TablePositionTop) && !string.IsNullOrEmpty(model.TablePositionLeft))
                    {
                        resDb.TablePositionTop = model.TablePositionTop;
                        resDb.TablePositionLeft = model.TablePositionLeft;
                    }

                    if (!string.IsNullOrEmpty(model.GuestNote))
                    {
                        resDb.Customers.Notes = model.GuestNote;
                    }

                    //var floorId = db.tabFloorTables.Find(resDb.FloorTableId).FloorPlanId;
                    //resDb.FloorPlanId = floorId;

                    resDb.FloorPlanId = model.FloorPlanId;

                    db.Entry(resDb).State = System.Data.Entity.EntityState.Modified;

                    db.LogEditReservation(resDb, loginUser, pinUser);

                    db.SaveChanges();

                    var listItem = GetReservationListItem(resDb.ReservationId);

                    return Json(new
                    {
                        Status = ResponseStatus.Success,
                        Message = "Reservation updated successfully...",
                        ListItem = listItem,
                        Time = resDb.TimeForm.TimeOfDay.TotalMinutes
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    throw new ArgumentNullException("Time");
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Status = ResponseStatus.Fail,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                this.ClearReservationCache(resDb != null ? (DateTime?)resDb.ReservationDate : null);
            }
        }

        [HttpPost]
        public JsonResult UpdateReservationStatus(Int64 ReservationId, Int64 StatusId)
        {
            var loginUser = db.Users.Find(User.Identity.GetUserId<long>());
            var res = db.tabReservations.SingleOrDefault(r => !r.IsDeleted && r.ReservationId == ReservationId);
            res.StatusId = StatusId;
            res.UpdatedBy = User.Identity.GetUserId<long>();
            res.UpdatedOn = DateTime.UtcNow;

            db.Entry(res).State = System.Data.Entity.EntityState.Modified;
            db.LogEditReservation(res, loginUser, null);
            db.SaveChanges();

            this.ClearReservationCache(res.ReservationDate);

            return Json(res.Status, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateRervationEditOptions(ReservationEditOptionsVM model)
        {
            ViewBag.StatusList = db.Status;
            ViewBag.ShiftList = db.tabFoodMenuShift;
            ViewBag.DurationList = new List<string>() { "15MIN", "30MIN", "45MIN", "1HR", "1HR 30MIN", "2HR", "2HR 30MIN", "3HR", "3HR 30MIN", "4HR" };

            var coverList = new List<object>();

            for (int i = 1; i <= 16; i++)
            {
                coverList.Add(new { Value = i, Text = i + " Cover" });
            }

            ViewBag.CoverList = coverList;

            // code for getting time  list
            var day = model.Date.DayOfWeek;

            int sId = model.ShiftId;

            var dId = db.tabWeekDays.AsEnumerable().Single(p => p.DayName.Trim() == day.ToString().Trim()).DayId;

            var openTime = new DateTime();
            var closeTime = new DateTime();
            if (sId != 0)
            {
                var ttime = db.tabMenuShiftHours.Single(p => p.DayId == dId && p.FoodMenuShiftId == sId);

                openTime = Convert.ToDateTime(ttime.OpenAt);
                closeTime = Convert.ToDateTime(ttime.CloseAt).AddDays(Convert.ToInt32(ttime.IsNext));
            }
            else
            {
                var ttime = db.tabMenuShiftHours.Where(p => p.DayId == dId).AsEnumerable();

                openTime = Convert.ToDateTime(ttime.Min(p => Convert.ToDateTime(p.OpenAt)));
                closeTime = Convert.ToDateTime(ttime.Max(p => Convert.ToDateTime(p.CloseAt).AddDays(Convert.ToInt32(p.IsNext))));
            }

            var op = openTime;
            var cl = closeTime;

            var TimeList = new List<object>();

            var aa = db.tabMenuShiftHours.AsEnumerable().Where(p => p.DayId == dId);

            while (op < cl)
            {
                var startTime = op;
                op = op.AddMinutes(15);

                int tShiftId = 0;
                var timeShift = aa.Where(s => Convert.ToDateTime(s.OpenAt) <= startTime && Convert.ToDateTime(s.CloseAt).AddDays(s.IsNext.Value) >= startTime).FirstOrDefault();
                if (timeShift != null)
                {
                    tShiftId = timeShift.FoodMenuShiftId;
                }

                TimeList.Add(new
                {
                    Text = startTime.ToString("hh:mm tt"),
                    Value = new DateTime().Add(startTime.TimeOfDay).ToString("ddMMyyyyhhmmtt") + " - " + new DateTime().Add(op.TimeOfDay).ToString("ddMMyyyyhhmmtt") + " - " + tShiftId
                });
            }

            ViewBag.TimeList = TimeList;

            //  end of code for getting time  list

            // code to get tables list

            var tt = model.Time.Split('-');

            var startTm = model.Date.Add(DateTime.ParseExact(tt[0].Trim(), "ddMMyyyyhhmmtt", CultureInfo.InvariantCulture).TimeOfDay);
            var endTime = new DateTime();
            if (string.IsNullOrEmpty(model.Duration))
            {
                endTime = model.Date.Add(DateTime.ParseExact(tt[1].Trim(), "ddMMyyyyhhmmtt", CultureInfo.InvariantCulture).TimeOfDay);


            }
            else
            {
                if (model.Duration.ToUpper().Contains("MIN") == true && model.Duration.ToUpper().Contains("HR"))
                {
                    var newTime = model.Duration.Split(' ');
                    var hr = Convert.ToInt32(newTime[0].Replace("HR", ""));
                    var min = Convert.ToInt32(newTime[1].Replace("MIN", ""));
                    endTime = startTm.AddHours(hr).AddMinutes(min);
                }
                else
                {
                    if (model.Duration.ToUpper().Contains("MIN"))
                    {
                        var newTime = model.Duration.Split(' ');
                        var min = Convert.ToInt32(newTime[0].Replace("MIN", ""));
                        endTime = startTm.AddMinutes(min);
                    }
                    else
                    {
                        var newTime = model.Duration.Split(' ');
                        var hr = Convert.ToInt32(newTime[0].Replace("HR", ""));
                        endTime = startTm.AddHours(hr);
                    }
                }


            }

            var reservation = db.tabReservations.SingleOrDefault(r => !r.IsDeleted && r.ReservationId == model.ReservationId);
            var floorPlan = db.tabFloorPlans.Include("FloorTables").Where(f => f.FloorPlanId == reservation.FloorPlanId).Single();
            //var sid = Db.tabFoodMenuShift.Single(p => p.MenuShift == shift).FoodMenuShiftId;


            var array = new string[] { "Sofa", "Chair", "SofaTable", "Wall", "SolidWall", "GlassWall", "BarTable", "Fence", "Pillar" };

            var table = floorPlan.FloorTables.Where(t => t.IsDeleted == false).AsEnumerable().Where(t => !array.Contains(t.TableName.Split('-')[0]));

            if (!string.IsNullOrEmpty(model.Duration))
            {
                table = table.Where(t => !t.Reservations.Any(r => !r.IsDeleted && r.ReservationDate == model.Date && (r.TimeForm >= startTm && r.TimeForm < endTime)) || !t.Reservations.Any(r => r.ReservationDate == model.Date && (r.TimeForm > startTm && r.TimeForm < endTime) && r.FloorTableId == reservation.FloorTableId));
            }
            else
            {
                table = table.Where(t => !t.Reservations.Any(r => !r.IsDeleted && r.ReservationDate == model.Date && ((!string.IsNullOrEmpty(r.Duration) && (startTm > r.TimeForm)) ? (startTm < r.TimeForm.AddMinutes(GetMinutesFromDuration(r.Duration))) : (r.TimeForm == startTm))) || t.FloorTableId == reservation.FloorTableId);
            }

            ViewBag.TableList = table
                .Select(p => new
                {
                    Id = p.FloorTableId,
                    Name = p.TableName
                }).ToList();

            // end of code to get tables list

            return PartialView("~/Views/Floor/ReservationEditPopupOptionsPartial.cshtml", model);
        }

        public ActionResult QuickUpdateReservationTable(Int64 resId, Int64 tableId, string top, string left)
        {
            Reservation res = null;
            try
            {
                var loginUser = db.Users.Find(User.Identity.GetUserId<long>());
                var tables = new List<FloorTable>();
                res = db.tabReservations.Single(r => !r.IsDeleted && r.ReservationId == resId);

                var model = new ReservationVM
                {
                    ReservationId = res.ReservationId,
                    resDate = res.ReservationDate,
                    Duration = res.Duration,
                    Covers = res.Covers,
                    time = new DateTime().Add(res.TimeForm.TimeOfDay).ToString("ddMMyyyyhhmmtt") +
                               " - " +
                               new DateTime().Add(res.TimeForm.AddMinutes(15).TimeOfDay).ToString("ddMMyyyyhhmmtt") +
                               " - " +
                               res.FoodMenuShiftId
                };

                IList<Int64> upcomingTableIds;
                IList<Int64> smallTableIds;
                tables = db.GetAvailableFloorTables(model, out upcomingTableIds, out smallTableIds, true, false).ToList();

                if (!tables.Any(t => t.FloorTableId == tableId))
                {
                    return Json(new
                    {
                        Status = ResponseStatus.Fail,
                        Message = "Sorry, this table is not available for update. Please select another table."
                    }, JsonRequestBehavior.AllowGet);
                }

                res.FloorTableId = tableId;

                if (!string.IsNullOrEmpty(top) && !string.IsNullOrEmpty(left))
                {
                    res.TablePositionTop = top;
                    res.TablePositionLeft = left;
                }

                db.Entry(res).State = EntityState.Modified;
                db.LogEditReservation(res, loginUser, null);
                db.SaveChanges();

                return Json(new
                        {
                            Status = ResponseStatus.Success,
                            Message = "Reservation updated successfully...",
                        }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Status = ResponseStatus.Fail,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                this.ClearReservationCache(res != null ? (DateTime?)res.ReservationDate : null);
            }
        }

        [HttpPost]
        public JsonResult DeleteReservation(Int64 ReservationId)
        {
            Reservation res = null;
            try
            {
                var loginUser = db.Users.Find(User.Identity.GetUserId<long>());
                res = db.tabReservations.Find(ReservationId);
                var time = res.TimeForm.TimeOfDay.TotalMinutes;

                //db.Entry(res).State = EntityState.Deleted;
                res.IsDeleted = true;
                db.Entry(res).State = EntityState.Modified;
                db.LogDeleteReservation(res, loginUser, null);
                db.SaveChanges();

                return Json(new
                {
                    Status = ResponseStatus.Success,
                    Time = time
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new
                {
                    Status = ResponseStatus.Fail,
                    Message = "Failed to delete this reservation, please try later..."
                }, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                this.ClearReservationCache(res != null ? (DateTime?)res.ReservationDate : null);
            }
        }

        [HttpPost]
        public JsonResult DeleteReservationWithPIN(Int64 ReservationId, int? PIN)
        {
            Reservation res = null;
            try
            {
                var loginUser = db.Users.Find(User.Identity.GetUserId<long>());
                UserProfile pinUser = null;

                res = db.tabReservations.Find(ReservationId);

                if ((PIN.HasValue || loginUser.EnablePIN) && !res.Customers.PhoneNumbers.Any(p => p.PhoneNumbers.Contains("9999999999")))
                {
                    pinUser = db.Users.Where(u => u.UserCode == PIN.Value).FirstOrDefault();

                    if (pinUser == null)
                    {
                        return Json(new
                        {
                            Status = ResponseStatus.Fail,
                            Message = "Please enter a valid user PIN."
                        }, JsonRequestBehavior.AllowGet);
                    }
                }

                var time = res.TimeForm.TimeOfDay.TotalMinutes;

                res.IsDeleted = true;
                db.Entry(res).State = EntityState.Modified;

                db.LogDeleteReservation(res, loginUser, pinUser);

                db.SaveChanges();

                return Json(new
                {
                    Status = ResponseStatus.Success,
                    Time = time
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new
                {
                    Status = ResponseStatus.Fail,
                    Message = "Failed to delete this reservation, please try later..."
                }, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                this.ClearReservationCache(res != null ? (DateTime?)res.ReservationDate : null);
            }
        }

        public string GetReservationListItem(Int64 ReservtaionId)
        {
            var res = db.tabReservations.SingleOrDefault(r => !r.IsDeleted && r.ReservationId == ReservtaionId);
            //ViewBag.PopUpPartial = this.RenderPartialViewToString("~/Views/Floor/ReservationEditPopUpPartial.cshtml", res);

            return this.RenderPartialViewToString("~/Views/FloorPlan/ReservationListItemPartial.cshtml", res);
        }

        //[AllowAnonymous]
        public JsonResult GetCustomerDetailIfExist(string phoneNumber)
        {
            var customer = db.tabCustomers.Where(c => c.PhoneNumbers.Any(cn => cn.PhoneNumbers.Equals(phoneNumber))).FirstOrDefault();

            if (customer != null)
            {
                return Json(
                    new
                    {
                        FirstName = customer.FirstName,
                        LastName = customer.LastName,
                        Status = ResponseStatus.Success,
                        Message = "Customer already exist with this phone number."
                    }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(
                    new
                    {
                        Status = ResponseStatus.Fail
                    }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Floor Plan reservation methods

        [AllowAnonymous]
        public ActionResult SaveReservation(ReservationVM model)
        {
            try
            {
                var loginUser = db.Users.Find(User.Identity.GetUserId<long>());
                FloorTableServer server = null;
                Waiting waiting = null;

                var fTblId = Convert.ToInt64(model.tableIdd);

                if (fTblId == 0 && model.MergeTableId == 0)
                {
                    return Json(new
                    {
                        Status = ResponseStatus.Fail,
                        Message = "Please select a table."
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (fTblId > 0)
                    {
                        var flrTbl = db.tabFloorTables.Find(fTblId);
                        model.FloorPlanId = flrTbl.FloorPlanId;
                        model.MergeTableId = 0;
                        server = flrTbl.FloorTableServer;
                    }
                    else
                    {
                        model.FloorPlanId = db.tabMergedFloorTables.Find(model.MergeTableId.Value).FloorPlanId;
                    }
                }

                double time = 0;
                long resId = 0;
                if (!string.IsNullOrEmpty(model.Email))
                {
                    model.Email = model.Email.Trim();
                }

                model.MobileNumber = model.MobileNumber.Trim();
                // model.ShiftId = model.ShiftId;

                var tt = model.time.Split('-');

                var startTime = model.resDate.Add(DateTime.ParseExact(tt[0].Trim(), "ddMMyyyyhhmmtt", CultureInfo.InvariantCulture).TimeOfDay);
                var endTime = model.resDate.Add(DateTime.ParseExact(tt[1].Trim(), "ddMMyyyyhhmmtt", CultureInfo.InvariantCulture).TimeOfDay);

                bool isFakeMobileNo = long.Parse(model.MobileNumber) == 0L;
               
                if (model.WaitingId > 0)
                {
                    waiting = db.tabWaitings.Find(model.WaitingId);
                }
              
                var customer = db.tabCustomers.Where(c => !isFakeMobileNo && c.PhoneNumbers.Any(cn => cn.PhoneNumbers.Contains(model.MobileNumber))).FirstOrDefault();

                if (isFakeMobileNo && waiting != null)
                {
                    customer = waiting.Customer;
                    customer.FirstName = model.FirstName;
                    customer.LastName = model.LastName;
                }
                
                model.ShiftId = Convert.ToInt32(tt[2]);

                model.Status = (!string.IsNullOrEmpty(model.Status)) ? model.Status : ReservationStatus.Not_confirmed.ToString();

                if (customer != null)
                {
                    var reservation = new Reservation()
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
                        UserId = User.Identity.GetUserId<long>(),
                        TablePositionLeft = model.TablePositionLeft,
                        TablePositionTop = model.TablePositionTop,
                        Duration = model.Duration,
                        ReservationNote = model.ReservationNote,
                        CreatedOn = DateTime.UtcNow,
                        UpdatedBy = User.Identity.GetUserId<long>(),
                        UpdatedOn = DateTime.UtcNow
                    };

                    if (server != null && server.ServerId != null)
                    {
                        reservation.ReservationServer = new ReservationServer() { ServerId = server.ServerId.Value };
                    }

                    db.tabReservations.Add(reservation);

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
                            db.tabCustomersEmails.Add(cemail);
                        }
                    }

                    time = reservation.TimeForm.TimeOfDay.TotalMinutes;

                    db.LogAddReservation(reservation, loginUser, null);
                }
                else
                {
                    var cust = new Customers()
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        DateCreated = DateTime.UtcNow.ToDefaultTimeZone(User.Identity.GetDatabaseName()),
                        DateOfBirth = DateTime.UtcNow.ToDefaultTimeZone(User.Identity.GetDatabaseName()),
                        Address1 = "1",
                        Address2 = "2",
                        Anniversary = DateTime.UtcNow.ToDefaultTimeZone(User.Identity.GetDatabaseName()),
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

                    var reservation = new Reservation()
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
                        UserId = User.Identity.GetUserId<long>(),
                        TablePositionLeft = model.TablePositionLeft,
                        TablePositionTop = model.TablePositionTop,
                        Duration = model.Duration,
                        ReservationNote = model.ReservationNote,
                        CreatedOn = DateTime.UtcNow,
                        UpdatedBy = User.Identity.GetUserId<long>(),
                        UpdatedOn = DateTime.UtcNow
                    };

                    if (server != null && server.ServerId != null)
                    {
                        reservation.ReservationServer = new ReservationServer() { ServerId = server.ServerId.Value };
                    }

                    db.tabReservations.Add(reservation);

                    if (!string.IsNullOrEmpty(model.GuestNote))
                    {
                        cust.Notes = model.GuestNote;
                    }

                    time = reservation.TimeForm.TimeOfDay.TotalMinutes;

                    db.LogAddReservation(reservation, loginUser, null);
                }

                if (waiting != null)
                {
                    waiting.ReservationId = resId;
                    db.Entry(waiting).State = EntityState.Modified;

                    ClearWaitingCache();
                }


                db.SaveChanges();

                return Json(new
                 {
                     Status = ResponseStatus.Success,
                     Message = "Reservation saved successfully.",
                     Time = time
                 }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new
                {
                    Status = ResponseStatus.Fail,
                    Message = "Failed to save reservation, please try later..."
                }, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                this.ClearReservationCache(model.resDate);
            }
        }

        [AllowAnonymous]
        public ActionResult SaveReservationWithPIN(ReservationVM model)
        {
            try
            {
                var loginUser = db.Users.Find(User.Identity.GetUserId<long>());
                UserProfile pinUser = null;
                Waiting waiting = null;

                if ((model.PIN.HasValue || loginUser.EnablePIN) && !model.MobileNumber.Contains("9999999999"))
                {
                    pinUser = db.Users.Where(u => u.UserCode == model.PIN.Value).FirstOrDefault();

                    if (pinUser == null)
                    {
                        return Json(new
                        {
                            Status = ResponseStatus.Fail,
                            Message = "Please enter a valid user PIN."
                        }, JsonRequestBehavior.AllowGet);
                    }
                }

                FloorTableServer server = null;
                var fTblId = Convert.ToInt64(model.tableIdd);

                if (fTblId == 0 && model.MergeTableId == 0)
                {
                    return Json(new
                    {
                        Status = ResponseStatus.Fail,
                        Message = "Please select a table."
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (fTblId > 0)
                    {
                        var flrTbl = db.tabFloorTables.Find(fTblId);
                        model.FloorPlanId = flrTbl.FloorPlanId;
                        model.MergeTableId = 0;
                        server = flrTbl.FloorTableServer;
                    }
                    else
                    {
                        model.FloorPlanId = db.tabMergedFloorTables.Find(model.MergeTableId.Value).FloorPlanId;
                    }
                }

                double time = 0;
                long resId = 0;
                if (!string.IsNullOrEmpty(model.Email))
                {
                    model.Email = model.Email.Trim();
                }

                model.MobileNumber = model.MobileNumber.Trim();
                // model.ShiftId = model.ShiftId;

                var tt = model.time.Split('-');

                var startTime = model.resDate.Add(DateTime.ParseExact(tt[0].Trim(), "ddMMyyyyhhmmtt", CultureInfo.InvariantCulture).TimeOfDay);
                var endTime = model.resDate.Add(DateTime.ParseExact(tt[1].Trim(), "ddMMyyyyhhmmtt", CultureInfo.InvariantCulture).TimeOfDay);

                bool isFakeMobileNo = long.Parse(model.MobileNumber) == 0L;

                if (model.WaitingId > 0)
                {
                    waiting = db.tabWaitings.Find(model.WaitingId);
                }

                var customer = db.tabCustomers.Where(c => !isFakeMobileNo && c.PhoneNumbers.Any(cn => cn.PhoneNumbers.Contains(model.MobileNumber))).FirstOrDefault();

                if (isFakeMobileNo && waiting != null)
                {   
                    customer = waiting.Customer;
                    customer.FirstName = model.FirstName;
                    customer.LastName = model.LastName;
                }

                model.ShiftId = Convert.ToInt32(tt[2]);

                model.Status = (!string.IsNullOrEmpty(model.Status)) ? model.Status : ReservationStatus.Not_confirmed.ToString();

                if (customer != null)
                {
                    var reservation = new Reservation()
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
                        UserId = User.Identity.GetUserId<long>(),
                        TablePositionLeft = model.TablePositionLeft,
                        TablePositionTop = model.TablePositionTop,
                        Duration = model.Duration,
                        ReservationNote = model.ReservationNote,
                        CreatedOn = DateTime.UtcNow,
                        UpdatedBy = User.Identity.GetUserId<long>(),
                        UpdatedOn = DateTime.UtcNow
                    };

                    if (server != null && server.ServerId != null)
                    {
                        reservation.ReservationServer = new ReservationServer() { ServerId = server.ServerId.Value };
                    }

                    db.tabReservations.Add(reservation);

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
                            db.tabCustomersEmails.Add(cemail);
                        }
                    }

                    time = reservation.TimeForm.TimeOfDay.TotalMinutes;

                    db.LogAddReservation(reservation, loginUser, pinUser);
                }
                else
                {
                    var cust = new Customers()
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        DateCreated = DateTime.UtcNow.ToDefaultTimeZone(User.Identity.GetDatabaseName()),
                        DateOfBirth = DateTime.UtcNow.ToDefaultTimeZone(User.Identity.GetDatabaseName()),
                        Address1 = "1",
                        Address2 = "2",
                        Anniversary = DateTime.UtcNow.ToDefaultTimeZone(User.Identity.GetDatabaseName()),
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

                    var reservation = new Reservation()
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
                        UserId = User.Identity.GetUserId<long>(),
                        TablePositionLeft = model.TablePositionLeft,
                        TablePositionTop = model.TablePositionTop,
                        Duration = model.Duration,
                        ReservationNote = model.ReservationNote,
                        CreatedOn = DateTime.UtcNow,
                        UpdatedBy = User.Identity.GetUserId<long>(),
                        UpdatedOn = DateTime.UtcNow
                    };

                    if (server != null && server.ServerId != null)
                    {
                        reservation.ReservationServer = new ReservationServer() { ServerId = server.ServerId.Value };
                    }

                    db.tabReservations.Add(reservation);

                    if (!string.IsNullOrEmpty(model.GuestNote))
                    {
                        cust.Notes = model.GuestNote;
                    }

                    time = reservation.TimeForm.TimeOfDay.TotalMinutes;

                    db.LogAddReservation(reservation, loginUser, pinUser);
                }

                if (waiting != null)
                {
                    waiting.ReservationId = resId;
                    db.Entry(waiting).State = EntityState.Modified;

                    ClearWaitingCache();
                }

                db.SaveChanges();

                return Json(new
                {
                    Status = ResponseStatus.Success,
                    Message = "Reservation saved successfully.",
                    Time = time
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new
                {
                    Status = ResponseStatus.Fail,
                    Message = "Failed to save reservation, please try later..."
                }, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                this.ClearReservationCache(model.resDate);
            }
        }

        #endregion

        #region private methods

        private void ClearWaitingCache()
        {
            //cache.RemoveByPattern(CacheKeys.WAITING_PATTERN);
            cache.RemoveByPattern(string.Format(CacheKeys.WAITING_COMPANY_PATTERN, User.Identity.GetDatabaseName()));
        }

        private void ClearReservationCache(DateTime? date = null)
        {
            //cache.RemoveByPattern(CacheKeys.RESERVATION_BY_DATE_PATTREN);
            cache.RemoveByPattern(string.Format(CacheKeys.RESERVATION_BY_DATE_COMPANY_PATTREN, User.Identity.GetDatabaseName()));
            //cache.RemoveByPattern(CacheKeys.FILTERED_RESERVATION_PATTREN);
            cache.RemoveByPattern(string.Format(CacheKeys.FILTERED_RESERVATION_COMPANY_PATTREN, User.Identity.GetDatabaseName()));
            //cache.RemoveByPattern(CacheKeys.FLOOR_TABLES_SCREEN_PATTREN);
            cache.RemoveByPattern(string.Format(CacheKeys.FLOOR_TABLES_SCREEN_COMPANY_PATTREN, User.Identity.GetDatabaseName()));
           
            cache.Remove(string.Format(CacheKeys.FLOOR_TABLES_SCREEN_PATTREN_TableAvailability, User.Identity.GetDatabaseName()));
            cache.Remove(string.Format(CacheKeys.FLOOR_TABLES_ONLY, User.Identity.GetDatabaseName()));

            if (date.HasValue)
            {
                // put reservation list into cache for performance
                //CacheOutReservationList(date.Value);
            }
        }

        private int GetMinutesFromDuration(string duration)
        {
            var minutes = 0;

            if (duration.ToUpper().Contains("MIN") == true && duration.ToUpper().Contains("HR"))
            {
                var newTime = duration.Split(' ');
                var hr = Convert.ToInt32(newTime[0].Replace("HR", ""));
                var min = Convert.ToInt32(newTime[1].Replace("MIN", ""));
                minutes = (hr * 60) + (min);
            }
            else
            {
                if (duration.ToUpper().Contains("MIN"))
                {
                    var newTime = duration.Split(' ');
                    var min = Convert.ToInt32(newTime[0].Replace("MIN", ""));
                    minutes = (min);
                }
                else
                {
                    var newTime = duration.Split(' ');
                    var hr = Convert.ToInt32(newTime[0].Replace("HR", ""));
                    minutes = (hr * 60);
                }
            }

            return minutes;
        }

        private void CacheOutReservationList(DateTime date)
        {
            var resList = db.GetReservationByDate(date);
            var htmlMinifier = new HtmlMinifier();

            foreach (var res in resList)
            {

                var HTMLString = cache.Get<string>(string.Format(CacheKeys.RESERVATION_RIGHT_LIST_ITEM,db.Database.Connection.Database, res.ReservationId), () =>
                {
                    ModelState.Clear();
                    var partialViewString = this.RenderPartialViewToString("~/Views/FloorPlan/ReservationListItemPartial.cshtml", res);
                    return htmlMinifier.Minify(partialViewString,
                        generateStatistics: false).MinifiedContent;
                });
            }
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
