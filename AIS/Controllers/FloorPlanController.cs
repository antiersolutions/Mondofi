using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AIS.Models;
using AISModels;
using AIS.Filters;
using AIS.Extensions;
using System.Globalization;
using AIS.Helpers;
using AIS.Helpers.Caching;
using WebMarkupMin.Mvc.ActionFilters;
using WebMarkupMin.Core.Minifiers;
using Microsoft.AspNet.Identity;

namespace AIS.Controllers
{
    [Authorize]
    public class FloorPlanController : Controller
    {
        private UsersContext db = new UsersContext();
        private ICacheManager cache = new MemoryCacheManager();

        #region FloorPlan Methods

        public ActionResult FloorPlan(ReservationVM obj)
        {
            var UTCDate = DateTime.UtcNow;
            var isEditMode = false;
            var clientDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, db.GetDefaultTimeZone());

            if (obj.ReservationId != 0)
            {
                var res = db.tabReservations.Where(r => !r.IsDeleted).SingleOrDefault(r => r.ReservationId == obj.ReservationId);

                var resObj = new ReservationVM
                {
                    ReservationId = obj.ReservationId,
                    Covers = res.Covers,
                    Duration = res.Duration,
                    Email = ((res.Customers.Emails.Count() > 0) ? res.Customers.Emails.First().Email : string.Empty),
                    FirstName = res.Customers.FirstName,
                    LastName = res.Customers.LastName,
                    MobileNumber = res.Customers.PhoneNumbers.First().PhoneNumbers,
                    resDate = res.ReservationDate,
                    ShiftId = res.FoodMenuShiftId,
                    Status = res.StatusId.ToString(),
                    tableIdd = res.FloorTableId.ToString(),
                    TablePositionLeft = res.TablePositionLeft,
                    TablePositionTop = res.TablePositionTop,
                    FloorPlanId = ((res.FloorTableId > 0) ? db.tabFloorTables.Find(res.FloorTableId).FloorPlanId : db.tabMergedFloorTables.Find(res.MergedFloorTableId).FloorPlanId),
                    MergeTableId = res.MergedFloorTableId
                };

                var day = res.ReservationDate.DayOfWeek;
                var dId = db.GetWeekDays().Single(p => p.DayName.Trim() == day.ToString().Trim()).DayId;
                //var dId = db.tabWeekDays.AsEnumerable().Single(p => p.DayName.Trim() == day.ToString().Trim()).DayId;
                var aa = db.GetMenuShiftHours().Where(p => p.DayId == dId);
                //var aa = db.tabMenuShiftHours.AsEnumerable().Where(p => p.DayId == dId);
                var startTime = DateTime.Now.Date.Add(res.TimeForm.TimeOfDay);

                var open = new DateTime();
                var close = new DateTime();

                var timeShift = aa.Where(s => (DateTime.TryParse(s.OpenAt, out open) && DateTime.TryParse(s.CloseAt, out close)) &&
                    startTime.Date.Add(open.TimeOfDay) <= startTime &&
                    startTime.Date.Add(close.TimeOfDay).AddDays(s.IsNext.Value) >= startTime).FirstOrDefault();

                resObj.time = new DateTime().Add(res.TimeForm.TimeOfDay).ToString("ddMMyyyyhhmmtt") +
                           " - " +
                           new DateTime().Add(res.TimeForm.AddMinutes(15).TimeOfDay).ToString("ddMMyyyyhhmmtt") +
                           " - " +
                           timeShift.FoodMenuShiftId;

                ViewBag.ResModel = resObj;
                obj.FloorPlanId = resObj.FloorPlanId;
                isEditMode = true;

            }
            else if (obj.resDate.Year == DateTime.UtcNow.ToDefaultTimeZone(User.Identity.GetDatabaseName()).Year)
            //else if (obj.resDate.Year == DateTime.UtcNow.ToClientTime().Year)
            {
                obj.FloorPlanId = 1;
                ViewBag.ResModel = obj;
                isEditMode = true;
            }
            else
            {
                obj.FloorPlanId = 1;
            }

            ViewBag.shiftDdl = new SelectList(db.tabFoodMenuShift, "FoodMenuShiftId", "MenuShift");
            var floorPlans = db.tabFloorPlans;

            var rec = floorPlans.Include("FloorTables").Where(p => p.FloorPlanId == obj.FloorPlanId).SingleOrDefault();

            ViewBag.IsEditMode = isEditMode;
            ViewBag.maxCoverLimit = db.GetMaxFloorCovers();

            if (rec != null)
            {
                ViewBag.Floors = floorPlans;
                return View(rec);
            }
            else
            {
                return RedirectToAction("Index", "Floor");
            }
        }

        public ActionResult _FloorPlanPartial(DateTime? startTime, DateTime? endTime, string shift, Int64 FloorPlanId = 1)
        {
            var model = db.tabFloorPlans.Include("FloorTables").Include("Sections").Where(p => p.FloorPlanId == FloorPlanId).SingleOrDefault();
            //model.FloorTables = model.FloorTables.Where(p => p.IsDeleted == false).ToList();
            int Covers = 0;

            if (model != null)
            {
                this.AttachFloorItemHtmlDesign(model, cache);

                int? shiftId = null;

                if (!string.IsNullOrEmpty(shift))
                {
                    shift = shift.Trim();

                    if (shift.ToLower() == "all")
                    {
                        shiftId = 0;
                    }
                    else
                    {
                        var shiftDB = db.GetFoodMenuShifts().Where(s => s.MenuShift.Equals(shift)).FirstOrDefault();

                        if (shiftDB != null)
                        {
                            shiftId = shiftDB.FoodMenuShiftId;
                        }
                    }
                }

                var floorItems = model.FloorTables.ToList();

                db.CheckForMergedTable20141222(this, model.FloorTables, model.FloorPlanId, startTime, endTime);
                var date = startTime.HasValue ? startTime.Value.Date : DateTime.UtcNow.ToDefaultTimeZone(User.Identity.GetDatabaseName()).Date;
                //var date = startTime.HasValue ? startTime.Value.Date : DateTime.UtcNow.ToClientTime().Date;
                var resList = db.GetReservationByDate(date);
                ViewBag.statusList = db.GetStatusList();
                foreach (var item in floorItems)
                {
                    int coverCount = 0;

                    item.AssignSectionColor();
                    this.CheckReservations20141222(resList, item, startTime, endTime, shiftId, out coverCount);
                    //this.CheckReservations20150310(item, startTime, endTime, shiftId, out coverCount);

                    Covers += coverCount;
                }

                ViewBag.Covers = Covers;
                ViewBag.FloorId = FloorPlanId;

                return PartialView(model);
            }
            else
            {
                ViewBag.Covers = Covers;
                ViewBag.FloorId = FloorPlanId;

                return PartialView(new FloorPlan());
            }
        }

        //public ActionResult UpdateFloorPlan(DateTime date, string time, string shift, Int64 FloorPlanId = 1)
        //{
        //    var model = Db.tabFloorPlans.Include("FloorTables").Include("Sections").Where(p => p.FloorPlanId == FloorPlanId).SingleOrDefault();

        //    this.AttachFloorItemHtmlDesign(model);

        //    int Covers = 0;

        //    var tt = time.Split('-');

        //    var startTime = date.Date.Add(DateTime.ParseExact(tt[0].Trim(), "ddMMyyyyhhmmtt", CultureInfo.InvariantCulture).TimeOfDay);
        //    var endTime = date.Date.Add(DateTime.ParseExact(tt[1].Trim(), "ddMMyyyyhhmmtt", CultureInfo.InvariantCulture).TimeOfDay);

        //    int? shiftId = null;

        //    if (!string.IsNullOrEmpty(shift))
        //    {
        //        shift = shift.Trim();

        //        if (shift.ToLower() == "all")
        //        {
        //            shiftId = 0;
        //        }
        //        else
        //        {
        //            var shiftDB = Db.tabFoodMenuShift.Where(s => s.MenuShift.Equals(shift)).FirstOrDefault();

        //            if (shiftDB != null)
        //            {
        //                shiftId = shiftDB.FoodMenuShiftId;
        //            }
        //        }
        //    }

        //    foreach (var item in model.FloorTables)
        //    {
        //        int coverCount = 0;

        //        item.AssignSectionColor();
        //        this.CheckReservations(item, startTime, endTime, shiftId, out coverCount);

        //        Covers += coverCount;
        //    }

        //    ViewBag.Covers = Covers;

        //    return PartialView("_FloorPlanPartial", model);
        //}

        public ActionResult UpdateFloorPlanOnTimeSlide(DateTime date, string time, string shift, string duration, Int64 FloorPlanId = 1)
        {
            string key = string.Format(CacheKeys.FLOOR_TABLES_SCREEN,
                db.Database.Connection.Database,
                date.Ticks,
                time,
                shift,
                FloorPlanId);

            return cache.Get<PartialViewResult>(key, () =>
            {
                var model = db.tabFloorPlans.Include("FloorTables").Include("Sections").Where(p => p.FloorPlanId == FloorPlanId).SingleOrDefault();

                //model.FloorTables = model.FloorTables.Where(p => p.IsDeleted == false).ToList();

                this.AttachFloorItemHtmlDesign(model, cache);

                int Covers = 0;

                time = time.Trim();

                var startTime = date.Date.Add(Convert.ToDateTime(time).TimeOfDay);
                var endTime = startTime.AddMinutes(15);

                int? shiftId = null;

                if (!string.IsNullOrEmpty(shift))
                {
                    shift = shift.Trim();

                    if (shift.ToLower() == "all")
                    {
                        shiftId = 0;
                    }
                    else
                    {
                        var shftID = 0;

                        if (Int32.TryParse(shift, out shftID))
                        {
                            shiftId = shftID;
                        }
                        else
                        {
                            var shiftDB = db.tabFoodMenuShift.Where(s => s.MenuShift.Equals(shift)).FirstOrDefault();

                            if (shiftDB != null)
                            {
                                shiftId = shiftDB.FoodMenuShiftId;
                            }
                        }
                    }
                }

                db.CheckForMergedTable20141222(this, model.FloorTables, model.FloorPlanId, startTime, endTime);

                // code for getting time  list
                var day = date.DayOfWeek.ToString().Trim();
                var dId = db.GetWeekDays().Single(p => p.DayName.Contains(day)).DayId;

                var availList = db.tabTableAvailabilities
                .Include("TableAvailabilityFloorTables")
                .Include("TableAvailabilityWeekDays")
                .Where(ta => ta.StartDate <= date && date <= ta.EndDate
                && ta.TableAvailabilityWeekDays.Any(taw => taw.DayId == dId)).ToList();

                var blockList = db.GetFloorTableBlockTimeList(date);
                var resList = db.GetReservationByDate(date);
                var tables = model.FloorTables;
                ViewBag.statusList = db.GetStatusList();
                foreach (var item in tables)
                {
                    int coverCount = 0;

                    item.AssignSectionColor();
                    //this.CheckReservations20141222(item, startTime, endTime, shiftId, out coverCount);
                    //this.CheckReservations20150310(availList, blockList, item, duration, startTime, endTime, shiftId, out coverCount);
                    this.CheckReservations20150622(resList, availList, blockList, item, duration, startTime, endTime, shiftId, out coverCount);

                    Covers += coverCount;
                }

                ViewBag.Covers = Covers;
                ViewBag.FloorId = FloorPlanId;

                return PartialView("_FloorPlanPartial", model);
            });
        }

        #endregion

        #region Reservation Methods

        public PartialViewResult GetAllReservationList(GetReservationsParamVM model)
        {
            string key = string.Format(CacheKeys.FILTERED_RESERVATION,
                User.Identity.GetDatabaseName(),
                model.Date.Ticks,
                model.Time,
                model.Filter,
                model.FloorPlanId.HasValue ? model.FloorPlanId.Value.ToString() : string.Empty,
                model.ShiftId.HasValue ? model.ShiftId.Value.ToString() : string.Empty);

            return cache.Get<PartialViewResult>(key, 60, () =>
            {
                IList<ReservationListItemVM> ResList = GetFilteredReservations20150617(model);

                var allRes = db.GetReservationByDate(model.Date)
                        .Where(r => (r.FloorTableId > 0) ? (!r.FloorTable.IsDeleted) : (!r.MergedFloorTable.OrigionalTables.Any(t => t.FloorTable.IsDeleted))).ToList();

                allRes = allRes.Where(r => r.StatusId.Value != ReservationStatus.Cancelled).ToList();

                ViewBag.Covers = allRes.Sum(r => r.Covers);

                return PartialView("~/Views/FloorPlan/ReservationListPartial.cshtml", ResList);
            });
        }

        public PartialViewResult GetAllReservationList20150617(GetReservationsParamVM model)
        {
            var floorPlan = model.FloorPlanId.HasValue ? model.FloorPlanId.Value.ToString() : string.Empty;
            var shift = model.ShiftId.HasValue ? model.ShiftId.Value.ToString() : string.Empty;

            string key = string.Format(CacheKeys.FILTERED_RESERVATION,
                User.Identity.GetDatabaseName(),
                model.Date.Ticks,
                model.Time,
                model.Filter,
                floorPlan,
                shift);

            var ResListVM = cache.Get<CachedReservationItemListVM>(key, () =>
            {
                var allRes = db.GetReservationByDate(model.Date)
                        .Where(r => (r.FloorTableId > 0) ? (!r.FloorTable.IsDeleted) : (!r.MergedFloorTable.OrigionalTables.Any(t => t.FloorTable.IsDeleted))).ToList();

                allRes = allRes.Where(r => r.StatusId.Value != ReservationStatus.Cancelled).ToList();
                var covers = allRes.Sum(r => r.Covers);

                IList<ReservationListItemVM> ResList = GetFilteredReservations20150617(model);

                var currentList = new CachedReservationItemListVM
                {
                    ReservationListItems = ResList,
                    Covers = allRes.Sum(r => r.Covers)
                };

                //var day = model.Date.DayOfWeek.ToString().Trim();
                //var dId = db.GetWeekDays().Single(p => p.DayName.Contains(day)).DayId;

                //var openTime = new DateTime();
                //var closeTime = new DateTime();

                //var ttime = db.GetMenuShiftHours().Where(p => p.DayId == dId).AsEnumerable();
                //var minOpenAt = ttime.Where(p => p.OpenAt != null).Min(p => Convert.ToDateTime(p.OpenAt));
                //var maxCloseAt = ttime.Where(p => p.CloseAt != null).Max(p => Convert.ToDateTime(p.CloseAt).AddDays(Convert.ToInt32(p.IsNext)));

                //openTime = Convert.ToDateTime(minOpenAt);
                //closeTime = Convert.ToDateTime(maxCloseAt);

                //while (openTime < closeTime)
                //{
                //    var startTime = openTime;
                //    openTime = openTime.AddMinutes(15);

                //    string otherKey = string.Format(CacheKeys.FILTERED_RESERVATION,
                //    model.Date.Ticks,
                //    startTime.ToString("HH:mm"),
                //    model.Filter,
                //    floorPlan,
                //    shift);

                //    var other = cache.Get<CachedReservationItemListVM>(otherKey, () =>
                //          {
                //              IList<ReservationListItemVM> otherResList = GetFilteredReservations20150617(model);

                //              var otherList = new CachedReservationItemListVM
                //              {
                //                  ReservationListItems = ResList,
                //                  Covers = covers
                //              };

                //              return otherList;
                //          });
                //}

                return currentList;
            });

            ViewBag.Covers = ResListVM.Covers;
            return PartialView("~/Views/FloorPlan/ReservationListPartial.cshtml", ResListVM.ReservationListItems);
        }

        public JsonResult GetJSONAllReservationList(GetReservationsParamVM model)
        {
            var floorPlan = model.FloorPlanId.HasValue ? model.FloorPlanId.Value.ToString() : string.Empty;
            var shift = model.ShiftId.HasValue ? model.ShiftId.Value.ToString() : string.Empty;

            string key = string.Format(CacheKeys.FILTERED_RESERVATION,
                User.Identity.GetDatabaseName(),
                model.Date.Ticks,
                model.Time,
                model.Filter,
                floorPlan,
                shift);

            var ResListVM = cache.Get<CachedReservationItemListVM>(key, () =>
            {
                var allRes = db.GetReservationByDate(model.Date)
                        .Where(r => (r.FloorTableId > 0) ? (!r.FloorTable.IsDeleted) : (!r.MergedFloorTable.OrigionalTables.Any(t => t.FloorTable.IsDeleted))).ToList();

                allRes = allRes.Where(r => r.StatusId.Value != ReservationStatus.Cancelled).ToList();
                var covers = allRes.Sum(r => r.Covers);

                IList<ReservationListItemVM> ResList = GetFilteredReservations20150617(model);

                var currentList = new CachedReservationItemListVM
                {
                    ReservationListItems = ResList,
                    Covers = allRes.Sum(r => r.Covers)
                };

                return currentList;
            });

            //ViewBag.Covers = ResListVM.Covers;
            var liTemplate = @"<li id='resL{0}' class='{1}cover popUp' data-cover='{1}' data-time='{2}'>{3}</li>";

            var HTMLArray = new List<string>();
            HTMLArray.Add("<ul class='reslist'>");
            HTMLArray.AddRange(ResListVM.ReservationListItems
                .Select(rvm => string.Format(liTemplate,
                    rvm.Reservation.ReservationId,
                    rvm.Reservation.Covers,
                    rvm.Reservation.TimeForm.TimeOfDay.TotalMinutes,
                    rvm.HTMLString)));
            HTMLArray.Add("</ul>");

            return Json(new
            {
                HTMLArray = HTMLArray,
                Covers = ResListVM.Covers
            }, JsonRequestBehavior.AllowGet);
        }

        //public PartialViewResult GetWaitingReservationList(GetReservationsParamVM model)
        //{
        //    var ResList = this.GetFilteredReservations(model);

        //    ViewBag.Covers = 0;

        //    if (ResList != null && ResList.Count() > 0)
        //    {
        //        var coversCount = ResList.Sum(r => r.Covers);
        //        ViewBag.Covers = coversCount;
        //    }

        //    return PartialView("~/Views/FloorPlan/ReservationListPartial.cshtml", ResList.ToList());
        //}

        //public PartialViewResult GetStaffReservationList(GetReservationsParamVM model)
        //{
        //    var ResList = this.GetFilteredReservations(model);

        //    ViewBag.Covers = 0;

        //    if (ResList != null && ResList.Count() > 0)
        //    {
        //        var coversCount = ResList.Sum(r => r.Covers);
        //        ViewBag.Covers = coversCount;
        //    }

        //    return PartialView("~/Views/FloorPlan/ReservationListPartial.cshtml", ResList.ToList());
        //}

        public PartialViewResult UpdateAddReservationAdditionalOptions(ReservationVM model, bool isMerging = false)
        {
            ModelState.Clear();
            //var table = InitializeAddResOptionsNew(model, considerFloor: true, isMerging: isMerging);
            var table = InitializeAddResOptionsNew20151215(model, considerFloor: true, isMerging: isMerging);

            ViewBag.TableList = (table != null && table.Count() > 0)
                ?
                (table.Select(p => new
                {
                    Id = p.FloorTableId,
                    Name = p.TableName + "\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0(" + p.MinCover + "/" + p.MaxCover + ")"
                }).ToList<object>())
                :
                (new List<object>()
                {
                    new
                    {
                        Id = 0,
                        Name = "No Table"
                    }
                });

            // end of code to get tables list

            return PartialView("~/Views/FloorPlan/AddReservationAdditionalOptionsPartial.cshtml", model);
        }

        public PartialViewResult GetAddResAdditionalDetailPartial(ReservationVM model)
        {
            if (!string.IsNullOrEmpty(model.MobileNumber))
            {
                var phno = model.MobileNumber.Trim();
                ViewBag.AddResCustModel = db.tabCustomers.Where(c => c.PhoneNumbers.Any(cn => cn.PhoneNumbers.Equals(model.MobileNumber))).FirstOrDefault();
            }
            else
            {
                ViewBag.AddResCustModel = null;
            }

            return PartialView("~/Views/FloorPlan/AddReservationAdditionalDetailPartial.cshtml", model);
        }

        public PartialViewResult GetCustomerDetailPartial(ReservationVM model)
        {
            Customers customer = null;

            if (!string.IsNullOrEmpty(model.MobileNumber) && long.Parse(model.MobileNumber) != 0)
            {
                var phno = model.MobileNumber.Trim();
                customer = db.tabCustomers.Where(c => c.PhoneNumbers.Any(cn => cn.PhoneNumbers.Equals(model.MobileNumber))).FirstOrDefault();
            }
            else if (model.ReservationId > 0)
            {
                customer = db.tabCustomers.Where(c => c.Reservations.Any(r => r.ReservationId == model.ReservationId)).SingleOrDefault();
            }

            return PartialView("~/Views/FloorPlan/AddReservationCustomerDetailPartial.cshtml", customer);
        }

        public PartialViewResult GetCustomerVisits(ReservationVM model)
        {
            Customers customer = null;

            if (!string.IsNullOrEmpty(model.MobileNumber))
            {
                var phno = model.MobileNumber.Trim();
                customer = db.tabCustomers.Where(c => c.PhoneNumbers.Any(cn => cn.PhoneNumbers.Equals(model.MobileNumber))).FirstOrDefault();
            }

            return PartialView("~/Views/FloorPlan/AddReservationCustomerVisitsPartial.cshtml", customer);
        }

        public PartialViewResult UpdateAddReservationOptions(ReservationVM model, bool isDateChanged = false, bool considerFloor = false, bool isMerging = false, bool isMobileSource = false)
        {
            ModelState.Clear();
            //var table = InitializeAddResOptionsNew(model, isDateChanged, considerFloor, isMerging);
            var table = InitializeAddResOptionsNew20151215(model, isDateChanged, considerFloor, isMerging);
            bool anyTable = (table != null && table.Count() > 0);
            var Tables = (table != null && table.Count() > 0)
                ?
                (table.Select(p => new
                {
                    Id = p.FloorTableId,
                    Name = "L" + p.FloorPlan.FLevel + "\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0" + p.TableName + "\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0(" + p.MinCover + "/" + p.MaxCover + ")",
                    
                }).ToList<object>())
                :
                (new List<object>()
                {
                    new
                    {
                        Id = 0,
                        Name = "No Table"
                    }
                });

            ViewBag.TableList = Tables;
            ViewBag.TableCount = anyTable ? Tables.Count() : 0;

            //if (model.MergeTableId.HasValue && model.MergeTableId.Value > 0)
            //{
            //    var originalTables = db.tabMergedTableOrigionalTables.Where(mot => mot.MergedFloorTableId == model.MergeTableId.Value).Select(ot => ot.FloorTable).ToList();
            //    ViewBag.SelectedTables = originalTables;
            //}

            // end of code to get tables list
            if (!isMobileSource)
            {
                return PartialView("~/Views/FloorPlan/AddReservationOptionsPartial.cshtml", model);
            }
            else
            {
                int maxAvailCovers = db.GetMaxFloorCovers();
                var coverList = new List<object>();

                for (int i = 1; i <= maxAvailCovers; i++)
                {
                    coverList.Add(new { Value = i, Text = i + " Cover" });
                }

                ViewBag.CoverList = coverList;

                return PartialView("~/Views/Book/MobileAddReservationOptionsPartial.cshtml", model);
            }
        }

        public PartialViewResult GetAddReservtionPartial(ReservationVM model, bool isMobileSource = false, bool isWalkIn = false)
        {
            ModelState.Clear();

            DateTime resStartTime = new DateTime();

            if (model.ReservationId > 0)
            {
                var res = db.tabReservations.Where(r => !r.IsDeleted).SingleOrDefault(r => r.ReservationId == model.ReservationId);
                model.Covers = res.Covers;
                model.Duration = res.Duration;
                model.Email = ((res.Customers.Emails.Count() > 0) ? res.Customers.Emails.First().Email : string.Empty);
                model.FirstName = res.Customers.FirstName;
                model.LastName = res.Customers.LastName;
                model.MobileNumber = res.Customers.PhoneNumbers.First().PhoneNumbers;
                model.resDate = res.ReservationDate;
                model.ShiftId = res.FoodMenuShiftId;
                model.Status = res.StatusId.ToString();
                model.tableIdd = res.FloorTableId.ToString();
                model.TablePositionLeft = res.TablePositionLeft;
                model.TablePositionTop = res.TablePositionTop;
                model.ReservationNote = res.ReservationNote;
                model.MergeTableId = res.MergedFloorTableId;
                model.FloorPlanId = res.FloorTableId == 0 ? db.tabMergedFloorTables.Find(res.MergedFloorTableId).FloorPlanId : db.tabFloorTables.Find(res.FloorTableId).FloorPlanId;
                model.enableMerging = res.FloorTableId == 0;

                resStartTime = res.TimeForm;
            }

            if (model.ReservationId == 0 && !DateTime.TryParse(model.time, out resStartTime))
            {
                //var currentClientDateTime = DateTime.UtcNow.ToClientTime();
                var currentClientDateTime = DateTime.UtcNow.ToDefaultTimeZone(User.Identity.GetDatabaseName());
                currentClientDateTime = currentClientDateTime.AddMinutes(15 - (currentClientDateTime.TimeOfDay.Minutes % 15));
                resStartTime = currentClientDateTime;
                if (string.IsNullOrWhiteSpace(model.MobileNumber))
                    model.MobileNumber = "0000000000";
            }
            else if (model.ReservationId == 0)
            {
                resStartTime = resStartTime.AddMinutes(15);
                model.MobileNumber = "0000000000";
            }

            if (isWalkIn && model.ReservationId == 0)
            {
                model.MobileNumber = "9999999999";
                model.FirstName = "WalkIn";
                model.LastName = "Guest";
                model.Status = ReservationStatus.Seated.ToString();
            }

            var day = model.resDate.DayOfWeek;
            var dId = db.tabWeekDays.AsEnumerable().Single(p => p.DayName.Trim() == day.ToString().Trim()).DayId;
            var aa = db.tabMenuShiftHours.AsEnumerable().Where(p => p.DayId == dId);
            //var startTime = DateTime.UtcNow.ToClientTime().Date.Add(resStartTime.TimeOfDay);
            var startTime = DateTime.Now.Date.Add(resStartTime.TimeOfDay);

            var open = new DateTime();
            var close = new DateTime();

            var timeShift = aa.Where(s => (DateTime.TryParse(s.OpenAt, out open) && DateTime.TryParse(s.CloseAt, out close)) &&
                startTime.Date.Add(open.TimeOfDay) <= startTime &&
                startTime.Date.Add(close.TimeOfDay).AddDays(s.IsNext.Value) >= startTime).FirstOrDefault();

            if (timeShift != null)
            {
                model.time = new DateTime().Add(resStartTime.TimeOfDay).ToString("ddMMyyyyhhmmtt") +
                           " - " +
                           new DateTime().Add(resStartTime.AddMinutes(15).TimeOfDay).ToString("ddMMyyyyhhmmtt") +
                           " - " +
                           timeShift.FoodMenuShiftId;
            }
            else
            {
                model.time = null;
            }

            var user = db.Users.Find(User.Identity.GetUserId<long>());
            ViewBag.UserPin = user.EnablePIN;

            if (!isMobileSource)
            {
                return PartialView("~/Views/FloorPlan/FloorRightAddReservationPartial.cshtml", model);
            }
            else
            {
                return PartialView("~/Views/Book/MobileAddReservationPartial.cshtml", model);
            }
        }

        public PartialViewResult GetReservtionListPartial(GetReservationsParamVM model)
        {
            ViewBag.ResParam = model;

            return PartialView("~/Views/FloorPlan/FloorRightReservationTabsPartial.cshtml");
        }

        public PartialViewResult UpdateEditReservationPopUpOptions(ReservationVM model)
        {
            ModelState.Clear();
            //var table = InitializeAddResOptionsNew(model, considerFloor: true);
            var table = InitializeAddResOptionsNew20151215(model, considerFloor: true);
            bool anyTable = (table != null && table.Count() > 0);
            var Tables = (table != null && table.Count() > 0)
                ?
                (table.Select(p => new
                {
                    Id = p.FloorTableId,
                    Name = p.TableName + "\xA0\xA0\xA0\xA0\xA0\xA0\xA0\xA0(" + p.MinCover + "/" + p.MaxCover + ")"
                }).ToList<object>())
                :
                (new List<object>()
                {
                    new
                    {
                        Id = 0,
                    Name = "No Table"
                    }
                });

            ViewBag.TableList = Tables;
            ViewBag.TableCount = anyTable ? Tables.Count() : 0;
            // end of code to get tables list

            return PartialView("~/Views/FloorPlan/ReservationEditPopupOptionsPartial.cshtml", model);
        }

        public JsonResult GetEndingReservationPopUp(DateTime ResDate, int TimeInMin)
        {
            ViewBag.TimeToAddList = new List<object> 
            { 
                new { Text = "15 MIN", Value = 15 },
                new { Text = "30 MIN", Value = 30 },
                new { Text = "45 MIN", Value = 45 } 
            };

            List<long> disabledRes = null;
            var reservations = this.GetEndingReservationList(ResDate, TimeInMin, 15, out disabledRes);

            var resCount = reservations.Count();

            var data = reservations.Select(x => new CheckListVM()
            {
                PropertyName = "selectedReservations",
                Name = x.FloorTable.TableName + " (" + x.FloorTable.MinCover + "/" + x.FloorTable.MaxCover + ")",
                Value = x.ReservationId,
                IsChecked = false,
                IsDisabled = (disabledRes.Contains(x.ReservationId))
            });

            return Json(
                new
                {
                    Status = ((resCount > 0) ? ResponseStatus.Success : ResponseStatus.Fail),
                    Container = ((resCount > 0) ? this.RenderPartialViewToString("EndingReservationPartial", null) : string.Empty),
                    Reslist = ((resCount > 0) ? this.RenderPartialViewToString("~/Views/TableAvailablity/CheckboxListPartial.cshtml", data) : string.Empty)
                }, JsonRequestBehavior.AllowGet);

            //return PartialView("EndingReservationPartial");
        }

        public PartialViewResult GetEndingReservation(DateTime EndResDate, int EndTimeInMin, int ddlTimeToAdd)
        {
            List<long> disabledRes = null;
            var reservations = this.GetEndingReservationList(EndResDate, EndTimeInMin, ddlTimeToAdd, out disabledRes);

            var data = reservations.Select(x => new CheckListVM()
                {
                    PropertyName = "selectedReservations",
                    Name = x.FloorTable.TableName + " (" + x.FloorTable.MinCover + "/" + x.FloorTable.MaxCover + ")",
                    Value = x.ReservationId,
                    IsChecked = false,
                    IsDisabled = (disabledRes.Contains(x.ReservationId))
                });

            return PartialView("~/Views/TableAvailablity/CheckboxListPartial.cshtml", data);
        }

        public JsonResult UpdateExtendedReservationTime(IList<long> selectedReservations, int MinToAdd)
        {
            try
            {
                if (selectedReservations != null && selectedReservations.Count() > 0)
                {
                    var loginUser = db.Users.Find(User.Identity.GetUserId<long>());
                    var reservations = db.tabReservations.Where(r => selectedReservations.Contains(r.ReservationId)).ToList();

                    foreach (var reservation in reservations)
                    {
                        var oldDuration = reservation.Duration;
                        var newDuration = oldDuration.AddMinutesToDuration(MinToAdd);

                        reservation.Duration = newDuration;
                        reservation.TimeTo = reservation.TimeForm.AddMinutes(newDuration.GetMinutesFromDuration());
                        db.Entry(reservation).State = System.Data.Entity.EntityState.Modified;

                        db.LogEditReservation(reservation, loginUser, null);
                    }

                    db.SaveChanges();

                    ClearReservationCache();

                    return Json(new
                    {
                        Status = ResponseStatus.Success,
                        Message = "Reservations updated successfully.",
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new
                    {
                        Status = ResponseStatus.Fail,
                        Message = "Please select atleast one table.",
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(
                    new
                    {
                        Status = ResponseStatus.Fail,
                        Message = "Failed to update reservation duration."
                    }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetAvailableTablesForUpdate(Int64 resId)
        {
            var tables = new List<FloorTable>();

            var res = db.tabReservations.Single(r => !r.IsDeleted && r.ReservationId == resId);

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
            tables = db.GetAvailableFloorTables(model, out upcomingTableIds, out smallTableIds, true, true).ToList();

            var data = new
            {
                currentTableId = res.FloorTableId,
                currentTableName = res.FloorTable.TableName,
                availTables = tables.Select(x => new
                                {
                                    tableId = x.FloorTableId,
                                    tableName = x.TableName + " (" + x.MinCover + "/" + x.MaxCover + ")",
                                    minCovers = x.MinCover,
                                    maxCovers = x.MaxCover
                                }).ToList(),
                upcomingTableIds = upcomingTableIds,
                smallTableIds = smallTableIds
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Cache Methods
        public JsonResult ClearServerCache()
        {
            cache.Clear();
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region private methods

        private void ClearFloorCache()
        {
            //cache.RemoveByPattern(CacheKeys.FLOOR_TABLES_SCREEN_PATTREN);
            cache.RemoveByPattern(string.Format(CacheKeys.FLOOR_TABLES_SCREEN_COMPANY_PATTREN, User.Identity.GetDatabaseName()));
        }

        private void ClearReservationCache()
        {
            //cache.RemoveByPattern(CacheKeys.RESERVATION_BY_DATE_PATTREN);
            cache.RemoveByPattern(string.Format(CacheKeys.RESERVATION_BY_DATE_COMPANY_PATTREN, User.Identity.GetDatabaseName()));
            //cache.RemoveByPattern(CacheKeys.FILTERED_RESERVATION_PATTREN);
            cache.RemoveByPattern(string.Format(CacheKeys.FILTERED_RESERVATION_COMPANY_PATTREN, User.Identity.GetDatabaseName()));
            //cache.RemoveByPattern(CacheKeys.FLOOR_TABLES_SCREEN_PATTREN);
            cache.RemoveByPattern(string.Format(CacheKeys.FLOOR_TABLES_SCREEN_COMPANY_PATTREN, User.Identity.GetDatabaseName()));
        }

        //private IList<Reservation> GetFilteredReservations(GetReservationsParamVM options)
        //{
        //    IList<Reservation> reservations = new List<Reservation>();
        //    var clientDate = DateTime.UtcNow.ToClientTime().Date;

        //    using (var dbContext = new UsersContext())
        //    {
        //        reservations = dbContext.GetReservationByDate(options.Date)
        //            .Where(r => (r.FloorTableId > 0) ? (!r.FloorTable.IsDeleted) : (!r.MergedFloorTable.OrigionalTables.Any(t => t.FloorTable.IsDeleted))).ToList();

        //        if (options.FloorPlanId.HasValue)
        //        {
        //            reservations = reservations.Where(r => r.FloorPlanId == options.FloorPlanId.Value).ToList();
        //        }

        //        if (options.ShiftId.HasValue)
        //        {
        //            reservations = reservations.Where(r => r.FoodMenuShiftId == options.ShiftId.Value).ToList();
        //        }

        //        DateTime? startTime = null;

        //        string filter = string.Empty;

        //        if (!string.IsNullOrEmpty(options.Filter) && !string.IsNullOrEmpty(options.Time))
        //        {
        //            filter = options.Filter.Trim().ToUpper();

        //            switch (filter)
        //            {
        //                case "ALL":
        //                    {
        //                        break;
        //                    }
        //                case "SEATED":
        //                    {
        //                        options.IncludeStatusIds.Add(ReservationStatus.Seated);
        //                        options.ExcludeStatusIds.AddRange(
        //                            new List<long>
        //                        {
        //                            ReservationStatus.Cancelled,
        //                            ReservationStatus.Finished
        //                        });
        //                        options.Time = options.Time.Trim();
        //                        startTime = clientDate.Add(Convert.ToDateTime(options.Time).TimeOfDay);
        //                        break;
        //                    }
        //                case "UPCOMING":
        //                    {
        //                        options.ExcludeStatusIds.AddRange(
        //                            new List<long>
        //                        {
        //                            ReservationStatus.Seated,
        //                            ReservationStatus.Cancelled,
        //                            ReservationStatus.Finished
        //                        });
        //                        options.Time = options.Time.Trim();
        //                        startTime = clientDate.Add(Convert.ToDateTime(options.Time).TimeOfDay).AddMinutes(15);
        //                        break;
        //                    }
        //                default:
        //                    break;
        //            }
        //        }

        //        //var reservations = resList.ToList();

        //        if (options.IncludeStatusIds.Count > 0 && startTime.HasValue)
        //        {
        //            reservations = reservations.Where(r => options.IncludeStatusIds.Contains(r.StatusId.Value) || ((filter == "SEATED") ? (r.TimeForm <= startTime) : (r.TimeForm >= startTime))).ToList();
        //        }
        //        else if (options.IncludeStatusIds.Count > 0)
        //        {
        //            reservations = reservations.Where(r => options.IncludeStatusIds.Contains(r.StatusId.Value)).ToList();
        //        }
        //        else if (startTime.HasValue)
        //        {
        //            reservations = reservations.Where(r => ((filter == "SEATED") ? (r.TimeForm <= startTime) : (r.TimeForm >= startTime))).ToList();
        //        }

        //        if (options.ExcludeStatusIds.Count > 0)
        //        {
        //            reservations = reservations.Where(r => !options.ExcludeStatusIds.Contains(r.StatusId.Value)).ToList();
        //        }
        //    }

        //    return reservations.OrderBy(r => r.TimeForm).ToList();
        //}

        //private IList<ReservationListItemVM> GetFilteredReservations20150608(GetReservationsParamVM options)
        //{
        //    IList<Reservation> reservations = new List<Reservation>();
        //    var clientDate = DateTime.UtcNow.ToClientTime().Date;

        //    using (var dbContext = new UsersContext())
        //    {
        //        reservations = dbContext.GetReservationByDate(options.Date)
        //            .Where(r => (r.FloorTableId > 0) ? (!r.FloorTable.IsDeleted) : (!r.MergedFloorTable.OrigionalTables.Any(t => t.FloorTable.IsDeleted))).ToList();

        //        if (options.FloorPlanId.HasValue)
        //        {
        //            reservations = reservations.Where(r => r.FloorPlanId == options.FloorPlanId.Value).ToList();
        //        }

        //        if (options.ShiftId.HasValue)
        //        {
        //            reservations = reservations.Where(r => r.FoodMenuShiftId == options.ShiftId.Value).ToList();
        //        }

        //        DateTime? startTime = null;

        //        string filter = string.Empty;

        //        if (!string.IsNullOrEmpty(options.Filter) && !string.IsNullOrEmpty(options.Time))
        //        {
        //            filter = options.Filter.Trim().ToUpper();

        //            switch (filter)
        //            {
        //                case "ALL":
        //                    {
        //                        break;
        //                    }
        //                case "SEATED":
        //                    {
        //                        options.IncludeStatusIds.Add(ReservationStatus.Seated);
        //                        options.ExcludeStatusIds.AddRange(
        //                            new List<long>
        //                        {
        //                            ReservationStatus.Cancelled,
        //                            ReservationStatus.Finished
        //                        });
        //                        options.Time = options.Time.Trim();
        //                        startTime = clientDate.Add(Convert.ToDateTime(options.Time).TimeOfDay);
        //                        break;
        //                    }
        //                case "UPCOMING":
        //                    {
        //                        options.ExcludeStatusIds.AddRange(
        //                            new List<long>
        //                        {
        //                            ReservationStatus.Seated,
        //                            ReservationStatus.Cancelled,
        //                            ReservationStatus.Finished
        //                        });
        //                        options.Time = options.Time.Trim();
        //                        startTime = clientDate.Add(Convert.ToDateTime(options.Time).TimeOfDay).AddMinutes(15);
        //                        break;
        //                    }
        //                default:
        //                    break;
        //            }
        //        }

        //        //var reservations = resList.ToList();

        //        if (options.IncludeStatusIds.Count > 0 && startTime.HasValue)
        //        {
        //            reservations = reservations.Where(r => options.IncludeStatusIds.Contains(r.StatusId.Value) || ((filter == "SEATED") ? (r.TimeForm <= startTime) : (r.TimeForm >= startTime))).ToList();
        //        }
        //        else if (options.IncludeStatusIds.Count > 0)
        //        {
        //            reservations = reservations.Where(r => options.IncludeStatusIds.Contains(r.StatusId.Value)).ToList();
        //        }
        //        else if (startTime.HasValue)
        //        {
        //            reservations = reservations.Where(r => ((filter == "SEATED") ? (r.TimeForm <= startTime) : (r.TimeForm >= startTime))).ToList();
        //        }

        //        if (options.ExcludeStatusIds.Count > 0)
        //        {
        //            reservations = reservations.Where(r => !options.ExcludeStatusIds.Contains(r.StatusId.Value)).ToList();
        //        }
        //    }

        //    return reservations.OrderBy(r => r.TimeForm).Select(r => new ReservationListItemVM
        //    {
        //        Reservation = r,
        //        HTMLString = this.RenderPartialViewToString("~/Views/FloorPlan/ReservationListItemPartial.cshtml", r)
        //    }).ToList();
        //}

        private IList<ReservationListItemVM> GetFilteredReservations20150617(GetReservationsParamVM options)
        {
            IList<Reservation> reservations = new List<Reservation>();
            //var clientDate = DateTime.UtcNow.ToClientTime().Date;
            var clientDate = DateTime.UtcNow.ToDefaultTimeZone(User.Identity.GetDatabaseName()).Date;

            using (var dbContext = new UsersContext())
            {
                reservations = dbContext.GetReservationByDate(options.Date)
                    .Where(r => (r.FloorTableId > 0) ? (!r.FloorTable.IsDeleted) : (!r.MergedFloorTable.OrigionalTables.Any(t => t.FloorTable.IsDeleted))).ToList();

                if (options.FloorPlanId.HasValue)
                {
                    reservations = reservations.Where(r => r.FloorPlanId == options.FloorPlanId.Value).ToList();
                }

                if (options.ShiftId.HasValue)
                {
                    reservations = reservations.Where(r => r.FoodMenuShiftId == options.ShiftId.Value).ToList();
                }

                DateTime? startTime = null;

                string filter = string.Empty;

                if (!string.IsNullOrEmpty(options.Filter) && !string.IsNullOrEmpty(options.Time))
                {
                    filter = options.Filter.Trim().ToUpper();

                    if (!string.IsNullOrWhiteSpace(options.Time))
                    {
                        options.Time = options.Time.Trim();
                        clientDate = clientDate.Add(Convert.ToDateTime(options.Time).TimeOfDay);
                    }

                    switch (filter)
                    {
                        case "ALL":
                            {
                                break;
                            }
                        case "SEATED":
                            {
                                options.IncludeStatusIds.AddRange(new List<long>{
                                    ReservationStatus.Partially_Arrived,
                                    ReservationStatus.All_Arrived,
                                    ReservationStatus.Partially_Seated,
                                    ReservationStatus.Seated,
                                    ReservationStatus.Appetizer,
                                    ReservationStatus.Entree,
                                    ReservationStatus.Dessert,
                                    ReservationStatus.Table_Cleared,
                                    ReservationStatus.Check_Dropped,
                                    ReservationStatus.Check_Paid
                                });
                                options.ExcludeStatusIds.AddRange(new List<long>
                                {
                                    ReservationStatus.Finished,
                                    ReservationStatus.Cancelled,
                                    ReservationStatus.No_show
                                });
                                //options.Time = options.Time.Trim();
                                startTime = clientDate;
                                break;
                            }
                        case "UPCOMING":
                            {
                                options.ExcludeStatusIds.AddRange(new List<long>
                                {
                                    ReservationStatus.Partially_Arrived,
                                    ReservationStatus.All_Arrived,
                                    ReservationStatus.Partially_Seated,
                                    ReservationStatus.Seated,
                                    ReservationStatus.Appetizer,
                                    ReservationStatus.Entree,
                                    ReservationStatus.Dessert,
                                    ReservationStatus.Table_Cleared,
                                    ReservationStatus.Check_Dropped,
                                    ReservationStatus.Check_Paid,
                                    ReservationStatus.Seated,
                                    ReservationStatus.Cancelled,
                                    ReservationStatus.Finished,
                                    ReservationStatus.No_show
                                });
                                //options.Time = options.Time.Trim();
                                startTime = clientDate.AddMinutes(15);
                                break;
                            }
                        default:
                            break;
                    }
                }

                //var reservations = resList.ToList();

                if (options.IncludeStatusIds.Count > 0 && startTime.HasValue)
                {
                    //reservations = reservations.Where(r => options.IncludeStatusIds.Contains(r.StatusId.Value) || ((filter == "SEATED") ? (r.TimeForm <= startTime) : (r.TimeForm >= startTime))).ToList();
                    reservations = reservations.Where(r => options.IncludeStatusIds.Contains(r.StatusId.Value)
                        || ((filter == "SEATED") ? (r.TimeForm <= startTime && startTime <= r.TimeForm.AddMinutes(r.Duration.GetMinutesFromDuration())) : (r.TimeForm >= startTime))).ToList();
                }
                else if (options.IncludeStatusIds.Count > 0)
                {
                    reservations = reservations.Where(r => options.IncludeStatusIds.Contains(r.StatusId.Value)).ToList();
                }
                else if (startTime.HasValue)
                {
                    reservations = reservations.Where(r => ((filter == "SEATED") ? (r.TimeForm <= startTime) : (r.TimeForm >= startTime))).ToList();
                }

                if (options.ExcludeStatusIds.Count > 0)
                {
                    reservations = reservations.Where(r => !options.ExcludeStatusIds.Contains(r.StatusId.Value)).ToList();
                }
            }

            reservations = reservations.OrderBy(r => r.TimeForm).ToList();

            //if (options.Filter == "ALL")
            //{
            //    var currentTime = !string.IsNullOrWhiteSpace(options.Time) ? options.Date.Add(Convert.ToDateTime(options.Time).TimeOfDay) : clientDate;
            //    var sortedList = new List<Reservation>();
            //    sortedList.AddRange(reservations.SkipWhile(r => r.TimeForm < currentTime));
            //    sortedList.AddRange(reservations.TakeWhile(r => r.TimeForm < currentTime));

            //    return sortedList.Select(r => new ReservationListItemVM
            //    {
            //        Reservation = r,
            //        HTMLString = cache.Get<string>(string.Format(CacheKeys.RESERVATION_RIGHT_LIST_ITEM, r.ReservationId), () =>
            //        {
            //            return this.RenderPartialViewToString("~/Views/FloorPlan/ReservationListItemPartial.cshtml", r);
            //        })
            //    }).ToList();
            //}
            //else
            //{

            var htmlMinifier = new HtmlMinifier();

            return reservations.Select(r => new ReservationListItemVM
            {
                Reservation = r,
                HTMLString = cache.Get<string>(string.Format(CacheKeys.RESERVATION_RIGHT_LIST_ITEM, db.Database.Connection.Database, r.ReservationId), () =>
                {
                    ModelState.Clear();
                    return htmlMinifier.Minify(this.RenderPartialViewToString("~/Views/FloorPlan/ReservationListItemPartial.cshtml", r),
                        generateStatistics: false).MinifiedContent;

                    //return this.RenderPartialViewToString("~/Views/FloorPlan/ReservationListItemPartial.cshtml", r);
                })
            }).ToList();
            //}
        }

        //private IList<FloorTable> InitializeAddResOptionsNew(ReservationVM model, bool isDateChanged = false, bool considerFloor = false, bool isMerging = false)
        //{
        //    int maxCoversLimit = db.GetMaxFloorCovers();

        //    ViewBag.StatusList = db.GetStatusList();
        //    ViewBag.ShiftList = db.GetFoodMenuShifts();
        //    ViewBag.LevelList = db.tabFloorPlans.ToList().Select(fp => new
        //    {
        //        Text = "L" + fp.FLevel.Value + "-" + fp.FloorName,
        //        Value = fp.FloorPlanId

        //    });

        //    var coverList = new List<object>();

        //    for (int i = 1; i <= 30; i++)
        //    {
        //        coverList.Add(new { Value = i, Text = i + " Cover" });
        //    }

        //    ViewBag.CoverList = coverList;

        //    //ViewBag.DurationList = new List<string>() { "15MIN", "30MIN", "45MIN", "1HR", "1HR 30MIN", "2HR", "2HR 30MIN", "3HR", "3HR 30MIN", "4HR" };
        //    ViewBag.DurationList = this.GetDurationList("15MIN", "4HR");

        //    if (string.IsNullOrEmpty(model.Duration))
        //    {
        //        model.Duration = "1HR 30MIN";
        //    }

        //    // code for getting time  list
        //    var day = model.resDate.DayOfWeek.ToString().Trim();

        //    int sId = model.ShiftId;

        //    var dId = db.GetWeekDays().Single(p => p.DayName.Contains(day)).DayId;

        //    var openTime = new DateTime();
        //    var closeTime = new DateTime();
        //    //if (sId != 0)
        //    //{
        //    //    var ttime = db.tabMenuShiftHours.Single(p => p.DayId == dId && p.FoodMenuShiftId == sId);

        //    //    openTime = Convert.ToDateTime(ttime.OpenAt);
        //    //    closeTime = Convert.ToDateTime(ttime.CloseAt).AddDays(Convert.ToInt32(ttime.IsNext));
        //    //}
        //    //else
        //    //{
        //    var ttime = db.GetMenuShiftHours().Where(p => p.DayId == dId).AsEnumerable();
        //    var minOpenAt = ttime.Where(p => p.OpenAt != null).Min(p => Convert.ToDateTime(p.OpenAt));
        //    var maxCloseAt = ttime.Where(p => p.CloseAt != null).Max(p => Convert.ToDateTime(p.CloseAt).AddDays(Convert.ToInt32(p.IsNext)));

        //    openTime = Convert.ToDateTime(minOpenAt);
        //    closeTime = Convert.ToDateTime(maxCloseAt);

        //    if (!string.IsNullOrEmpty(model.Duration))
        //    {
        //        closeTime = closeTime.AddMinutes(-(model.Duration.GetMinutesFromDuration() - 15));
        //    }

        //    //}

        //    var op = openTime;
        //    var cl = closeTime;

        //    var TimeList = new List<object>();

        //    var aa = db.GetMenuShiftHours().AsEnumerable().Where(p => p.DayId == dId);

        //    while (op < cl)
        //    {
        //        var startTime = op;
        //        op = op.AddMinutes(15);

        //        int tShiftId = 0;
        //        //var timeShift = aa.Where(s => Convert.ToDateTime(s.OpenAt) <= startTime && Convert.ToDateTime(s.CloseAt).AddDays(s.IsNext.Value) >= startTime).FirstOrDefault();
        //        var openTM = new DateTime();
        //        var closeTM = new DateTime();

        //        var timeShift = aa.Where(s => (DateTime.TryParse(s.OpenAt, out openTM) && DateTime.TryParse(s.CloseAt, out closeTM)) &&
        //            startTime.Date.Add(openTM.TimeOfDay) <= startTime &&
        //            startTime.Date.Add(closeTM.TimeOfDay).AddDays(s.IsNext.Value) >= startTime).FirstOrDefault();

        //        if (timeShift != null)
        //        {
        //            tShiftId = timeShift.FoodMenuShiftId;
        //        }

        //        TimeList.Add(new
        //        {
        //            Text = startTime.ToString("hh:mm tt"),
        //            Value = new DateTime().Add(startTime.TimeOfDay).ToString("ddMMyyyyhhmmtt") + " - " + new DateTime().Add(op.TimeOfDay).ToString("ddMMyyyyhhmmtt") + " - " + tShiftId
        //        });
        //    }

        //    ViewBag.TimeList = TimeList;

        //    //  end of code for getting time  list
        //    if (isDateChanged || string.IsNullOrEmpty(model.time))
        //    {
        //        model.time = ((dynamic)TimeList[0]).Value;
        //    }

        //    // code to get tables list

        //    var tt = model.time.Split('-');

        //    var startTm = model.resDate.Add(DateTime.ParseExact(tt[0].Trim(), "ddMMyyyyhhmmtt", CultureInfo.InvariantCulture).TimeOfDay);
        //    var endTime = new DateTime();
        //    if (string.IsNullOrEmpty(model.Duration))
        //    {
        //        endTime = model.resDate.Add(DateTime.ParseExact(tt[1].Trim(), "ddMMyyyyhhmmtt", CultureInfo.InvariantCulture).TimeOfDay);
        //    }
        //    else
        //    {
        //        endTime = startTm.AddMinutes(model.Duration.GetMinutesFromDuration());
        //    }

        //    var reservation = db.tabReservations.SingleOrDefault(r => !r.IsDeleted && r.ReservationId == model.ReservationId);

        //    if (reservation != null)
        //    {
        //        model.EdtTableId = reservation.FloorTableId;
        //    }

        //    IList<FloorTable> table = null;

        //    var array = new string[] { "Sofa", "Chair", "SofaTable", "Wall", "SolidWall", "GlassWall", "BarTable", "Fence", "Pillar" };

        //    if (considerFloor)
        //    {
        //        if (model.FloorPlanId == 0)
        //        {
        //            model.FloorPlanId = 1;
        //        }

        //        maxCoversLimit = db.GetMaxFloorCovers(model.FloorPlanId);

        //        var floorPlan = db.tabFloorPlans.Include("FloorTables").Where(f => f.FloorPlanId == model.FloorPlanId).Single();
        //        table = floorPlan.FloorTables.Where(t => t.IsDeleted == false).AsEnumerable().Where(t => !array.Contains(t.TableName.Split('-')[0])).ToList();
        //    }
        //    else
        //    {
        //        table = db.tabFloorTables.Where(t => t.IsDeleted == false).ToList().Where(t => !array.Contains(t.TableName.Split('-')[0])).ToList();
        //    }
        //    //var sid = Db.tabFoodMenuShift.Single(p => p.MenuShift == shift).FoodMenuShiftId;

        //    var resList = db.GetReservationByDate(model.resDate);

        //    var rejectedTables = new List<long>();

        //    // check if status is FINISHED, CANCELLED, CANCELLED2

        //    var rejectedStatus = new List<long>()
        //                             {
        //                                 ReservationStatus.Finished,
        //                                 ReservationStatus.Cancelled
        //                                 //ReservationStatus.Cancelled_2
        //                             };

        //    resList = resList.Where(r => !rejectedStatus.Contains(r.StatusId.Value)).ToList();


        //    if (reservation != null)
        //    {
        //        resList = resList.Where(r => r.ReservationId != reservation.ReservationId).ToList();
        //        if (reservation.ReservationDate.Date == model.resDate.Date && startTm == reservation.TimeForm && reservation.Duration.Trim() == model.Duration.Trim())
        //        {
        //            if (reservation.MergedFloorTableId > 0)
        //            {
        //                var orgTables = reservation.MergedFloorTable.OrigionalTables.Select(ot => ot.FloorTable).ToList();
        //                ViewBag.SelectedTables = orgTables;
        //            }
        //        }
        //    }

        //    foreach (var item in resList)
        //    {
        //        var resStart = item.TimeForm;
        //        var resEnd = item.TimeForm.AddMinutes(item.Duration.GetMinutesFromDuration());

        //        if ((resStart <= startTm && resEnd >= endTime) || (resStart >= startTm && resEnd <= endTime) || (resStart < startTm && resEnd > startTm) || (resStart < endTime && resEnd > endTime)) //(resStart >= startTm && resStart < endTime) || (resEnd <= endTime && resEnd > startTm)
        //        {
        //            if (item.FloorTableId == 0 && item.MergedFloorTableId > 0)
        //            {
        //                foreach (var origionalTbl in item.MergedFloorTable.OrigionalTables)
        //                {
        //                    rejectedTables.Add(origionalTbl.FloorTableId);
        //                }
        //            }
        //            else
        //            {
        //                rejectedTables.Add(item.FloorTableId);
        //            }
        //        }
        //    }

        //    foreach (var tbl in table)
        //    {
        //        if (tbl.MaxCover < model.Covers)
        //        {
        //            if (!rejectedTables.Contains(tbl.FloorTableId))
        //            {
        //                rejectedTables.Add(tbl.FloorTableId);
        //            }
        //        }
        //    }

        //    table = table.Where(t => !rejectedTables.Contains(t.FloorTableId)).ToList();

        //    // Ends here

        //    if (model.Covers == 0)
        //    {
        //        model.Covers = 1;
        //    }

        //    //if (model.ShiftId == 0)
        //    //{
        //    model.ShiftId = Convert.ToInt32(tt[2]);
        //    //}

        //    if (string.IsNullOrEmpty(model.Status))
        //    {
        //        model.Status = ReservationStatus.Not_confirmed.ToString();
        //    }

        //    if (string.IsNullOrEmpty(model.tableIdd) || !table.Any(t => t.FloorTableId == Convert.ToInt64(model.tableIdd)))
        //    {
        //        if (table != null && table.Count() > 0 && model.Covers <= maxCoversLimit) // table != null && table.Count() > 0
        //        {
        //            model.tableIdd = table.First().FloorTableId.ToString();
        //        }
        //        else
        //        {
        //            model.tableIdd = "0";
        //            ViewBag.LevelList = new List<object>()
        //                { 
        //                    new
        //                    {
        //                        Text = "-No Level-",
        //                        Value = 0
        //                    }
        //                };
        //        }
        //    }

        //    var fTblId = Convert.ToInt64(model.tableIdd);

        //    if (!isMerging)
        //    {
        //        model.FloorPlanId = (table != null && table.Count() > 0 && model.Covers <= maxCoversLimit) ? db.tabFloorTables.Find(fTblId).FloorPlanId : 0;
        //    }

        //    if (model.MergeTableId.HasValue && model.MergeTableId.Value > 0) // (table == null || table.Count() == 0) && 
        //    {
        //        var mergedTable = db.tabMergedFloorTables.Find(model.MergeTableId.Value);

        //        var flrTable = new FloorTable
        //        {
        //            FloorTableId = 0,
        //            TableName = mergedTable.TableName
        //        };

        //        table = new List<FloorTable>()
        //        {
        //            new FloorTable
        //            {
        //                FloorTableId = 0,
        //                TableName = mergedTable.TableName,
        //                FloorPlan = mergedTable.FloorPlan,
        //                MinCover = mergedTable.MinCover,
        //                MaxCover = mergedTable.MaxCover
        //            }
        //        };

        //        ViewBag.LevelList = new List<object>()
        //                { 
        //                    new
        //                    {
        //                        Text = "L" + mergedTable.FloorPlan.FLevel.Value + "-" + mergedTable.FloorPlan.FloorName,
        //                        Value = 0
        //                    }
        //                };
        //    }

        //    ViewBag.MaxAvailCovers = maxCoversLimit;

        //    return table.OrderBy(t => t.FloorPlan.FLevel).ThenBy(t => t.TableName, new AlphaNumericComparer()).ToList();
        //}

        //private IList<FloorTable> InitializeAddResOptionsNew20150224(ReservationVM model, bool isDateChanged = false, bool considerFloor = false, bool isMerging = false)
        //{
        //    int maxCoversLimit = db.GetMaxFloorCovers();

        //    ViewBag.StatusList = db.GetStatusList();
        //    ViewBag.ShiftList = db.GetFoodMenuShifts();
        //    ViewBag.LevelList = db.tabFloorPlans.ToList().Select(fp => new
        //    {
        //        Text = "L" + fp.FLevel.Value + "-" + fp.FloorName,
        //        Value = fp.FloorPlanId

        //    });

        //    var coverList = new List<object>();

        //    for (int i = 1; i <= 30; i++)
        //    {
        //        coverList.Add(new { Value = i, Text = i + " Cover" });
        //    }

        //    ViewBag.CoverList = coverList;

        //    ViewBag.DurationList = this.GetDurationList("15MIN", "4HR");

        //    if (string.IsNullOrEmpty(model.Duration))
        //    {
        //        //model.Duration = "1HR 30MIN";
        //        model.Duration = "2HR"; //2015-07-01 Leigh's request
        //    }

        //    // code for getting time  list
        //    var day = model.resDate.DayOfWeek.ToString().Trim();

        //    int sId = model.ShiftId;

        //    var dId = db.GetWeekDays().Single(p => p.DayName.Contains(day)).DayId;

        //    var openTime = new DateTime();
        //    var closeTime = new DateTime();

        //    var ttime = db.GetMenuShiftHours().Where(p => p.DayId == dId).AsEnumerable();
        //    var minOpenAt = ttime.Where(p => p.OpenAt != null).Min(p => Convert.ToDateTime(p.OpenAt));
        //    var maxCloseAt = ttime.Where(p => p.CloseAt != null).Max(p => Convert.ToDateTime(p.CloseAt).AddDays(Convert.ToInt32(p.IsNext)));

        //    openTime = Convert.ToDateTime(minOpenAt);
        //    closeTime = Convert.ToDateTime(maxCloseAt);

        //    if (!string.IsNullOrEmpty(model.Duration))
        //    {
        //        closeTime = closeTime.AddMinutes(-(model.Duration.GetMinutesFromDuration() - 15));
        //    }

        //    var op = openTime;
        //    var cl = closeTime;

        //    var TimeList = new List<object>();

        //    var aa = db.GetMenuShiftHours().AsEnumerable().Where(p => p.DayId == dId);

        //    while (op < cl)
        //    {
        //        var startTime = op;
        //        op = op.AddMinutes(15);

        //        int tShiftId = 0;

        //        var openTM = new DateTime();
        //        var closeTM = new DateTime();

        //        var timeShift = aa.Where(s => (DateTime.TryParse(s.OpenAt, out openTM) && DateTime.TryParse(s.CloseAt, out closeTM)) &&
        //            startTime.Date.Add(openTM.TimeOfDay) <= startTime &&
        //            startTime.Date.Add(closeTM.TimeOfDay).AddDays(s.IsNext.Value) >= startTime).FirstOrDefault();

        //        if (timeShift != null)
        //        {
        //            tShiftId = timeShift.FoodMenuShiftId;
        //        }

        //        TimeList.Add(new
        //        {
        //            Text = startTime.ToString("hh:mm tt"),
        //            Value = new DateTime().Add(startTime.TimeOfDay).ToString("ddMMyyyyhhmmtt") + " - " + new DateTime().Add(op.TimeOfDay).ToString("ddMMyyyyhhmmtt") + " - " + tShiftId
        //        });
        //    }

        //    ViewBag.TimeList = TimeList;

        //    //  end of code for getting time  list
        //    if (isDateChanged || string.IsNullOrEmpty(model.time))
        //    {
        //        model.time = ((dynamic)TimeList[0]).Value;
        //    }

        //    // code to get tables list

        //    var tt = model.time.Split('-');

        //    var startTm = model.resDate.Add(DateTime.ParseExact(tt[0].Trim(), "ddMMyyyyhhmmtt", CultureInfo.InvariantCulture).TimeOfDay);
        //    var endTime = new DateTime();
        //    if (string.IsNullOrEmpty(model.Duration))
        //    {
        //        endTime = model.resDate.Add(DateTime.ParseExact(tt[1].Trim(), "ddMMyyyyhhmmtt", CultureInfo.InvariantCulture).TimeOfDay);
        //    }
        //    else
        //    {
        //        endTime = startTm.AddMinutes(model.Duration.GetMinutesFromDuration());
        //    }

        //    var reservation = db.tabReservations.SingleOrDefault(r => !r.IsDeleted && r.ReservationId == model.ReservationId);

        //    if (reservation != null)
        //    {
        //        model.EdtTableId = reservation.FloorTableId;
        //    }

        //    IList<FloorTable> table = null;

        //    var array = new string[] { "Sofa", "Chair", "SofaTable", "Wall", "SolidWall", "GlassWall", "BarTable", "Fence", "Pillar" };

        //    if (considerFloor)
        //    {
        //        if (model.FloorPlanId == 0)
        //        {
        //            model.FloorPlanId = 1;
        //        }

        //        maxCoversLimit = db.GetMaxFloorCovers(model.FloorPlanId);

        //        var floorPlan = db.tabFloorPlans.Include("FloorTables").Where(f => f.FloorPlanId == model.FloorPlanId).Single();
        //        table = floorPlan.FloorTables.Where(t => t.IsDeleted == false).AsEnumerable().Where(t => !array.Contains(t.TableName.Split('-')[0])).ToList();
        //    }
        //    else
        //    {
        //        table = db.tabFloorTables.Where(t => t.IsDeleted == false).ToList().Where(t => !array.Contains(t.TableName.Split('-')[0])).ToList();
        //    }

        //    var resList = db.GetReservationByDate(model.resDate);

        //    var rejectedTables = new List<long>();

        //    // check if status is FINISHED, CANCELLED, CANCELLED2

        //    var rejectedStatus = new List<long>()
        //                             {
        //                                 ReservationStatus.Finished,
        //                                 ReservationStatus.Cancelled
        //                                 //ReservationStatus.Cancelled_2
        //                             };

        //    resList = resList.Where(r => !rejectedStatus.Contains(r.StatusId.Value)).ToList();


        //    if (reservation != null)
        //    {
        //        resList = resList.Where(r => r.ReservationId != reservation.ReservationId).ToList();
        //        if (reservation.ReservationDate.Date == model.resDate.Date && startTm == reservation.TimeForm && reservation.Duration.Trim() == model.Duration.Trim())
        //        {
        //            if (reservation.MergedFloorTableId > 0)
        //            {
        //                var orgTables = reservation.MergedFloorTable.OrigionalTables.Select(ot => ot.FloorTable).ToList();
        //                ViewBag.SelectedTables = orgTables;
        //            }
        //        }
        //    }

        //    foreach (var item in resList)
        //    {
        //        var resStart = item.TimeForm;
        //        var resEnd = item.TimeForm.AddMinutes(item.Duration.GetMinutesFromDuration());

        //        if ((resStart <= startTm && resEnd >= endTime)
        //            || (resStart >= startTm && resEnd <= endTime)
        //            || (resStart < startTm && resEnd > startTm)
        //            || (resStart < endTime && resEnd > endTime)) //(resStart >= startTm && resStart < endTime) || (resEnd <= endTime && resEnd > startTm)
        //        {
        //            if (item.FloorTableId == 0 && item.MergedFloorTableId > 0)
        //            {
        //                foreach (var origionalTbl in item.MergedFloorTable.OrigionalTables)
        //                {
        //                    rejectedTables.Add(origionalTbl.FloorTableId);
        //                }
        //            }
        //            else
        //            {
        //                rejectedTables.Add(item.FloorTableId);
        //            }
        //        }
        //    }

        //    if (table != null && table.Count() > 0)
        //        maxCoversLimit = table.Where(t => !rejectedTables.Contains(t.FloorTableId)).Max(t => t.MaxCover);

        //    foreach (var tbl in table)
        //    {
        //        if (tbl.MaxCover < model.Covers)
        //        {
        //            if (!rejectedTables.Contains(tbl.FloorTableId))
        //            {
        //                rejectedTables.Add(tbl.FloorTableId);
        //            }
        //        }
        //    }

        //    table = table.Where(t => !rejectedTables.Contains(t.FloorTableId)).ToList();

        //    /**** Table availability feature enabled  start here *****/

        //    var availList = db.tabTableAvailabilities
        //        .Include("TableAvailabilityFloorTables")
        //        .Include("TableAvailabilityWeekDays")
        //        .Where(ta => ta.StartDate <= model.resDate && model.resDate <= ta.EndDate
        //        && ta.TableAvailabilityWeekDays.Any(taw => taw.DayId == dId)).ToList();

        //    var blockList = db.GetFloorTableBlockTimeList(model.resDate);

        //    table = table.Where(t => !availList.CheckAvailStatus(model.resDate, startTm, endTime, t, 2)
        //        && !blockList.IsTableBlocked(t.FloorTableId, startTm, endTime)).ToList();

        //    /**** Table availability feature enabled end here *****/

        //    // Ends here

        //    if (model.Covers == 0)
        //    {
        //        model.Covers = 1;
        //    }

        //    model.ShiftId = Convert.ToInt32(tt[2]);

        //    if (string.IsNullOrEmpty(model.Status))
        //    {
        //        model.Status = ReservationStatus.Not_confirmed.ToString();
        //    }

        //    if (string.IsNullOrEmpty(model.tableIdd) || !table.Any(t => t.FloorTableId == Convert.ToInt64(model.tableIdd)))
        //    {
        //        if (table != null && table.Count() > 0 && model.Covers <= maxCoversLimit)
        //        {
        //            model.tableIdd = table.First().FloorTableId.ToString();
        //        }
        //        else
        //        {
        //            model.tableIdd = "0";
        //            ViewBag.LevelList = new List<object>()
        //                { 
        //                    new
        //                    {
        //                        Text = "-No Level-",
        //                        Value = 0
        //                    }
        //                };
        //        }
        //    }

        //    var fTblId = Convert.ToInt64(model.tableIdd);

        //    if (!isMerging)
        //    {
        //        if (table != null && table.Count() > 0 && model.Covers <= maxCoversLimit)
        //        {
        //            model.FloorPlanId = db.tabFloorTables.Find(fTblId).FloorPlanId;
        //        }
        //        else
        //            model.FloorPlanId = 0; // 
        //    }

        //    if (model.MergeTableId.HasValue && model.MergeTableId.Value > 0)
        //    {
        //        var mergedTable = db.tabMergedFloorTables.Find(model.MergeTableId.Value);

        //        var flrTable = new FloorTable
        //        {
        //            FloorTableId = 0,
        //            TableName = mergedTable.TableName
        //        };

        //        table = new List<FloorTable>()
        //        {
        //            new FloorTable
        //            {
        //                FloorTableId = 0,
        //                TableName = mergedTable.TableName,
        //                FloorPlan = mergedTable.FloorPlan,
        //                MinCover = mergedTable.MinCover,
        //                MaxCover = mergedTable.MaxCover
        //            }
        //        };

        //        ViewBag.LevelList = new List<object>()
        //                { 
        //                    new
        //                    {
        //                        Text = "L" + mergedTable.FloorPlan.FLevel.Value + "-" + mergedTable.FloorPlan.FloorName,
        //                        Value = 0
        //                    }
        //                };
        //    }

        //    ViewBag.MaxAvailCovers = maxCoversLimit;

        //    return table.OrderBy(t => t.FloorPlan.FLevel).ThenBy(t => t.TableName, new AlphaNumericComparer()).ToList();
        //}
        private IList<FloorTable> InitializeAddResOptionsNew20151215(ReservationVM model, bool isDateChanged = false, bool considerFloor = false, bool isMerging = false)
        {
            int maxCoversLimit = db.GetMaxFloorCovers();

            ViewBag.StatusList = db.GetStatusList();
            ViewBag.ShiftList = db.GetFoodMenuShifts();
            ViewBag.LevelList = db.tabFloorPlans.ToList().Select(fp => new
            {
                Text = "L" + fp.FLevel.Value + "-" + fp.FloorName,
                Value = fp.FloorPlanId

            });

            var coverList = new List<object>();

            for (int i = 1; i <= 90; i++)
            {
                coverList.Add(new { Value = i, Text = i + " Cover" });
            }

            ViewBag.CoverList = coverList;

            ViewBag.DurationList = this.GetDurationList("15MIN", "4HR");

            if (string.IsNullOrEmpty(model.Duration))
            {
                model.Duration = "1HR 30MIN";
                //model.Duration = "2HR"; //2015-07-01 Leigh's request
            }

            // code for getting time  list
            var day = model.resDate.DayOfWeek.ToString().Trim();

            int sId = model.ShiftId;

            var dId = db.GetWeekDays().Single(p => p.DayName.Contains(day)).DayId;

            var openTime = new DateTime();
            var closeTime = new DateTime();

            var ttime = db.GetMenuShiftHours().Where(p => p.DayId == dId).AsEnumerable();
            var minOpenAt = ttime.Where(p => p.OpenAt != null).Min(p => Convert.ToDateTime(p.OpenAt));
            var maxCloseAt = ttime.Where(p => p.CloseAt != null).Max(p => Convert.ToDateTime(p.CloseAt).AddDays(Convert.ToInt32(p.IsNext)));

            openTime = Convert.ToDateTime(minOpenAt);
            closeTime = Convert.ToDateTime(maxCloseAt);

            if (!string.IsNullOrEmpty(model.Duration))
            {
                closeTime = closeTime.AddMinutes(-(model.Duration.GetMinutesFromDuration() - 15));
            }

            var op = openTime;
            var cl = closeTime;

            var TimeList = new List<object>();

            var aa = db.GetMenuShiftHours().AsEnumerable().Where(p => p.DayId == dId);

            while (op < cl)
            {
                var startTime = op;
                op = op.AddMinutes(15);

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
                    Value = new DateTime().Add(startTime.TimeOfDay).ToString("ddMMyyyyhhmmtt") + " - " + new DateTime().Add(op.TimeOfDay).ToString("ddMMyyyyhhmmtt") + " - " + tShiftId
                });
            }

            ViewBag.TimeList = TimeList;

            //  end of code for getting time  list
            if (isDateChanged || string.IsNullOrEmpty(model.time))
            {
                model.time = ((dynamic)TimeList[0]).Value;
            }

            // code to get tables list

            var tt = model.time.Split('-');

            var startTm = model.resDate.Add(DateTime.ParseExact(tt[0].Trim(), "ddMMyyyyhhmmtt", CultureInfo.InvariantCulture).TimeOfDay);
            var endTime = new DateTime();
            if (string.IsNullOrEmpty(model.Duration))
            {
                endTime = model.resDate.Add(DateTime.ParseExact(tt[1].Trim(), "ddMMyyyyhhmmtt", CultureInfo.InvariantCulture).TimeOfDay);
            }
            else
            {
                endTime = startTm.AddMinutes(model.Duration.GetMinutesFromDuration());
            }

            var reservation = db.tabReservations.SingleOrDefault(r => !r.IsDeleted && r.ReservationId == model.ReservationId);

            if (reservation != null)
            {
                model.EdtTableId = reservation.FloorTableId;
            }

            IList<FloorTable> table = null;

            var array = new string[] { "Sofa", "Chair", "SofaTable", "Wall", "SolidWall", "GlassWall", "BarTable", "Fence", "Pillar" };

            if (considerFloor)
            {
                if (model.FloorPlanId == 0)
                {
                    model.FloorPlanId = 1;
                }

                maxCoversLimit = db.GetMaxFloorCovers(model.FloorPlanId);

                var floorPlan = db.tabFloorPlans.Include("FloorTables").Where(f => f.FloorPlanId == model.FloorPlanId).Single();
                table = floorPlan.FloorTables.Where(t => t.IsDeleted == false).AsEnumerable().Where(t => !array.Contains(t.TableName.Split('-')[0])).ToList();
            }
            else
            {
                table = db.tabFloorTables.Where(t => t.IsDeleted == false).ToList().Where(t => !array.Contains(t.TableName.Split('-')[0])).ToList();
            }

            var resList = db.GetReservationByDate(model.resDate);

            var rejectedTables = new List<long>();

            // check if status is FINISHED, CANCELLED, CANCELLED2

            var rejectedStatus = new List<long>()
                                     {
                                         ReservationStatus.Finished,
                                         ReservationStatus.Cancelled
                                         //ReservationStatus.Cancelled_2
                                     };

            resList = resList.Where(r => !rejectedStatus.Contains(r.StatusId.Value)).ToList();


            if (reservation != null)
            {
                resList = resList.Where(r => r.ReservationId != reservation.ReservationId).ToList();
                if (reservation.ReservationDate.Date == model.resDate.Date && startTm == reservation.TimeForm && reservation.Duration.Trim() == model.Duration.Trim())
                {
                    if (reservation.MergedFloorTableId > 0)
                    {
                        var orgTables = reservation.MergedFloorTable.OrigionalTables.Select(ot => ot.FloorTable).ToList();
                        ViewBag.SelectedTables = orgTables;
                    }
                }
            }

            foreach (var item in resList)
            {
                var resStart = item.TimeForm;
                var resEnd = item.TimeForm.AddMinutes(item.Duration.GetMinutesFromDuration());

                if ((resStart <= startTm && resEnd >= endTime)
                    || (resStart >= startTm && resEnd <= endTime)
                    || (resStart < startTm && resEnd > startTm)
                    || (resStart < endTime && resEnd > endTime)) //(resStart >= startTm && resStart < endTime) || (resEnd <= endTime && resEnd > startTm)
                {
                    if (item.FloorTableId == 0 && item.MergedFloorTableId > 0)
                    {
                        foreach (var origionalTbl in item.MergedFloorTable.OrigionalTables)
                        {
                            rejectedTables.Add(origionalTbl.FloorTableId);
                        }
                    }
                    else
                    {
                        rejectedTables.Add(item.FloorTableId);
                    }
                }
            }

            /**** Enable Merge table feature enabled  start here *****/

            var isManualMerge = false;

            if (model.enableMerging)
            {
                foreach (var tbl in table)
                {
                    if (tbl.MaxCover >= model.Covers && !rejectedTables.Contains(tbl.FloorTableId))
                    {
                        rejectedTables.Add(tbl.FloorTableId);
                        isManualMerge = true;
                    }
                }
            }

            /**** Enable Merge table feature enabled  end here *****/

            if (table != null && table.Count() > 0)
                maxCoversLimit = table.Where(t => !rejectedTables.Contains(t.FloorTableId)).Any() ? table.Where(t => !rejectedTables.Contains(t.FloorTableId)).Max(t => t.MaxCover) : 1;

            foreach (var tbl in table)
            {
                if (tbl.MaxCover < model.Covers)
                {
                    if (!rejectedTables.Contains(tbl.FloorTableId))
                    {
                        rejectedTables.Add(tbl.FloorTableId);
                    }
                }
            }

            table = table.Where(t => !rejectedTables.Contains(t.FloorTableId)).ToList();

            /**** Table availability feature enabled  start here *****/

            var availList = db.tabTableAvailabilities
                .Include("TableAvailabilityFloorTables")
                .Include("TableAvailabilityWeekDays")
                .Where(ta => ta.StartDate <= model.resDate && model.resDate <= ta.EndDate
                && ta.TableAvailabilityWeekDays.Any(taw => taw.DayId == dId)).ToList();

            var blockList = db.GetFloorTableBlockTimeList(model.resDate);

            table = table.Where(t => !availList.CheckAvailStatus(model.resDate, startTm, endTime, t, 2)
                && !blockList.IsTableBlocked(t.FloorTableId, startTm, endTime)).ToList();

            /**** Table availability feature enabled end here *****/

            // Ends here

            if (model.Covers == 0)
            {
                model.Covers = 1;
            }

            model.ShiftId = Convert.ToInt32(tt[2]);

            if (string.IsNullOrEmpty(model.Status))
            {
                model.Status = ReservationStatus.Not_confirmed.ToString();
            }

            if (string.IsNullOrEmpty(model.tableIdd) || !table.Any(t => t.FloorTableId == Convert.ToInt64(model.tableIdd)))
            {
                if (table != null && table.Count() > 0 && model.Covers <= maxCoversLimit)
                {
                    model.tableIdd = table.OrderBy(c=>c.SeatingPriority).First().FloorTableId.ToString();
                }
                else
                {
                    model.tableIdd = "0";
                    ViewBag.LevelList = new List<object>()
                        { 
                            new
                            {
                                Text = "-No Level-",
                                Value = 0
                            }
                        };
                }
            }

            var fTblId = Convert.ToInt64(model.tableIdd);

            if (!isMerging)
            {
                if (table != null && table.Count() > 0 && model.Covers <= maxCoversLimit)
                {
                    model.FloorPlanId = db.tabFloorTables.Find(fTblId).FloorPlanId;
                }
                else
                    model.FloorPlanId = 0; // 
            }
            else
            {
            }

            if (model.MergeTableId.HasValue && model.MergeTableId.Value > 0)
            {
                var mergedTable = db.tabMergedFloorTables.Find(model.MergeTableId.Value);

                var flrTable = new FloorTable
                {
                    FloorTableId = 0,
                    TableName = mergedTable.TableName
                };

                table = new List<FloorTable>()
                {
                    new FloorTable
                    {
                        FloorTableId = 0,
                        TableName = mergedTable.TableName,
                        FloorPlan = mergedTable.FloorPlan,
                        MinCover = mergedTable.MinCover,
                        MaxCover = mergedTable.MaxCover
                    }
                };

                ViewBag.LevelList = new List<object>()
                        { 
                            new
                            {
                                Text = "L" + mergedTable.FloorPlan.FLevel.Value + "-" + mergedTable.FloorPlan.FloorName,
                                Value = 0
                            }
                        };
            }

            ViewBag.MaxAvailCovers = maxCoversLimit;
            ViewBag.IsAutoMerge = !isManualMerge && (maxCoversLimit < model.Covers);

            return table.OrderBy(t => t.FloorPlan.FLevel).ThenBy(t => t.TableName, new AlphaNumericComparer()).ToList();
        }

        /// i comment this code date 15-12-2015 and make new  fuction (InitializeAddResOptionsNew20151215)
        //private IList<FloorTable> InitializeAddResOptionsNew20150512(ReservationVM model, bool isDateChanged = false, bool considerFloor = false, bool isMerging = false)
        //{
        //    int maxCoversLimit = db.GetMaxFloorCovers();

        //    ViewBag.StatusList = db.GetStatusList();
        //    ViewBag.ShiftList = db.GetFoodMenuShifts();
        //    ViewBag.LevelList = db.tabFloorPlans.ToList().Select(fp => new
        //    {
        //        Text = "L" + fp.FLevel.Value + "-" + fp.FloorName,
        //        Value = fp.FloorPlanId

        //    });

        //    var coverList = new List<object>();

        //    for (int i = 1; i <= 90; i++)
        //    {
        //        coverList.Add(new { Value = i, Text = i + " Cover" });
        //    }

        //    ViewBag.CoverList = coverList;

        //    ViewBag.DurationList = this.GetDurationList("15MIN", "4HR");

        //    if (string.IsNullOrEmpty(model.Duration))
        //    {
        //        model.Duration = "1HR 30MIN";
        //        //model.Duration = "2HR"; //2015-07-01 Leigh's request
        //    }

        //    // code for getting time  list
        //    var day = model.resDate.DayOfWeek.ToString().Trim();

        //    int sId = model.ShiftId;

        //    var dId = db.GetWeekDays().Single(p => p.DayName.Contains(day)).DayId;

        //    var openTime = new DateTime();
        //    var closeTime = new DateTime();

        //    var ttime = db.GetMenuShiftHours().Where(p => p.DayId == dId).AsEnumerable();
        //    var minOpenAt = ttime.Where(p => p.OpenAt != null).Min(p => Convert.ToDateTime(p.OpenAt));
        //    var maxCloseAt = ttime.Where(p => p.CloseAt != null).Max(p => Convert.ToDateTime(p.CloseAt).AddDays(Convert.ToInt32(p.IsNext)));

        //    openTime = Convert.ToDateTime(minOpenAt);
        //    closeTime = Convert.ToDateTime(maxCloseAt);

        //    if (!string.IsNullOrEmpty(model.Duration))
        //    {
        //        closeTime = closeTime.AddMinutes(-(model.Duration.GetMinutesFromDuration() - 15));
        //    }

        //    var op = openTime;
        //    var cl = closeTime;

        //    var TimeList = new List<object>();

        //    var aa = db.GetMenuShiftHours().AsEnumerable().Where(p => p.DayId == dId);

        //    while (op < cl)
        //    {
        //        var startTime = op;
        //        op = op.AddMinutes(15);

        //        int tShiftId = 0;

        //        var openTM = new DateTime();
        //        var closeTM = new DateTime();

        //        var timeShift = aa.Where(s => (DateTime.TryParse(s.OpenAt, out openTM) && DateTime.TryParse(s.CloseAt, out closeTM)) &&
        //            startTime.Date.Add(openTM.TimeOfDay) <= startTime &&
        //            startTime.Date.Add(closeTM.TimeOfDay).AddDays(s.IsNext.Value) >= startTime).FirstOrDefault();

        //        if (timeShift != null)
        //        {
        //            tShiftId = timeShift.FoodMenuShiftId;
        //        }

        //        TimeList.Add(new
        //        {
        //            Text = startTime.ToString("hh:mm tt"),
        //            Value = new DateTime().Add(startTime.TimeOfDay).ToString("ddMMyyyyhhmmtt") + " - " + new DateTime().Add(op.TimeOfDay).ToString("ddMMyyyyhhmmtt") + " - " + tShiftId
        //        });
        //    }

        //    ViewBag.TimeList = TimeList;

        //    //  end of code for getting time  list
        //    if (isDateChanged || string.IsNullOrEmpty(model.time))
        //    {
        //        model.time = ((dynamic)TimeList[0]).Value;
        //    }

        //    // code to get tables list

        //    var tt = model.time.Split('-');

        //    var startTm = model.resDate.Add(DateTime.ParseExact(tt[0].Trim(), "ddMMyyyyhhmmtt", CultureInfo.InvariantCulture).TimeOfDay);
        //    var endTime = new DateTime();
        //    if (string.IsNullOrEmpty(model.Duration))
        //    {
        //        endTime = model.resDate.Add(DateTime.ParseExact(tt[1].Trim(), "ddMMyyyyhhmmtt", CultureInfo.InvariantCulture).TimeOfDay);
        //    }
        //    else
        //    {
        //        endTime = startTm.AddMinutes(model.Duration.GetMinutesFromDuration());
        //    }

        //    var reservation = db.tabReservations.SingleOrDefault(r => !r.IsDeleted && r.ReservationId == model.ReservationId);

        //    if (reservation != null)
        //    {
        //        model.EdtTableId = reservation.FloorTableId;
        //    }

        //    IList<FloorTable> table = null;

        //    var array = new string[] { "Sofa", "Chair", "SofaTable", "Wall", "SolidWall", "GlassWall", "BarTable", "Fence", "Pillar" };

        //    if (considerFloor)
        //    {
        //        if (model.FloorPlanId == 0)
        //        {
        //            model.FloorPlanId = 1;
        //        }

        //        maxCoversLimit = db.GetMaxFloorCovers(model.FloorPlanId);

        //        var floorPlan = db.tabFloorPlans.Include("FloorTables").Where(f => f.FloorPlanId == model.FloorPlanId).Single();
        //        table = floorPlan.FloorTables.Where(t => t.IsDeleted == false).AsEnumerable().Where(t => !array.Contains(t.TableName.Split('-')[0])).ToList();
        //    }
        //    else
        //    {
        //        table = db.tabFloorTables.Where(t => t.IsDeleted == false).ToList().Where(t => !array.Contains(t.TableName.Split('-')[0])).ToList();
        //    }

        //    var resList = db.GetReservationByDate(model.resDate);

        //    var rejectedTables = new List<long>();

        //    // check if status is FINISHED, CANCELLED, CANCELLED2

        //    var rejectedStatus = new List<long>()
        //                             {
        //                                 ReservationStatus.Finished,
        //                                 ReservationStatus.Cancelled
        //                                 //ReservationStatus.Cancelled_2
        //                             };

        //    resList = resList.Where(r => !rejectedStatus.Contains(r.StatusId.Value)).ToList();


        //    if (reservation != null)
        //    {
        //        resList = resList.Where(r => r.ReservationId != reservation.ReservationId).ToList();
        //        if (reservation.ReservationDate.Date == model.resDate.Date && startTm == reservation.TimeForm && reservation.Duration.Trim() == model.Duration.Trim())
        //        {
        //            if (reservation.MergedFloorTableId > 0)
        //            {
        //                var orgTables = reservation.MergedFloorTable.OrigionalTables.Select(ot => ot.FloorTable).ToList();
        //                ViewBag.SelectedTables = orgTables;
        //            }
        //        }
        //    }

        //    foreach (var item in resList)
        //    {
        //        var resStart = item.TimeForm;
        //        var resEnd = item.TimeForm.AddMinutes(item.Duration.GetMinutesFromDuration());

        //        if ((resStart <= startTm && resEnd >= endTime)
        //            || (resStart >= startTm && resEnd <= endTime)
        //            || (resStart < startTm && resEnd > startTm)
        //            || (resStart < endTime && resEnd > endTime)) //(resStart >= startTm && resStart < endTime) || (resEnd <= endTime && resEnd > startTm)
        //        {
        //            if (item.FloorTableId == 0 && item.MergedFloorTableId > 0)
        //            {
        //                foreach (var origionalTbl in item.MergedFloorTable.OrigionalTables)
        //                {
        //                    rejectedTables.Add(origionalTbl.FloorTableId);
        //                }
        //            }
        //            else
        //            {
        //                rejectedTables.Add(item.FloorTableId);
        //            }
        //        }
        //    }

        //    /**** Enable Merge table feature enabled  start here *****/

        //    var isManualMerge = false;

        //    if (model.enableMerging)
        //    {
        //        foreach (var tbl in table)
        //        {
        //            if (tbl.MaxCover >= model.Covers && !rejectedTables.Contains(tbl.FloorTableId))
        //            {
        //                rejectedTables.Add(tbl.FloorTableId);
        //                isManualMerge = true;
        //            }
        //        }
        //    }

        //    /**** Enable Merge table feature enabled  end here *****/

        //    if (table != null && table.Count() > 0)
        //        maxCoversLimit = table.Where(t => !rejectedTables.Contains(t.FloorTableId)).Any() ? table.Where(t => !rejectedTables.Contains(t.FloorTableId)).Max(t => t.MaxCover) : 1;

        //    foreach (var tbl in table)
        //    {
        //        if (tbl.MaxCover < model.Covers)
        //        {
        //            if (!rejectedTables.Contains(tbl.FloorTableId))
        //            {
        //                rejectedTables.Add(tbl.FloorTableId);
        //            }
        //        }
        //    }

        //    table = table.Where(t => !rejectedTables.Contains(t.FloorTableId)).ToList();

        //    /**** Table availability feature enabled  start here *****/

        //    var availList = db.tabTableAvailabilities
        //        .Include("TableAvailabilityFloorTables")
        //        .Include("TableAvailabilityWeekDays")
        //        .Where(ta => ta.StartDate <= model.resDate && model.resDate <= ta.EndDate
        //        && ta.TableAvailabilityWeekDays.Any(taw => taw.DayId == dId)).ToList();

        //    var blockList = db.GetFloorTableBlockTimeList(model.resDate);

        //    table = table.Where(t => !availList.CheckAvailStatus(model.resDate, startTm, endTime, t, 2)
        //        && !blockList.IsTableBlocked(t.FloorTableId, startTm, endTime)).ToList();

        //    /**** Table availability feature enabled end here *****/

        //    // Ends here

        //    if (model.Covers == 0)
        //    {
        //        model.Covers = 1;
        //    }

        //    model.ShiftId = Convert.ToInt32(tt[2]);

        //    if (string.IsNullOrEmpty(model.Status))
        //    {
        //        model.Status = ReservationStatus.Not_confirmed.ToString();
        //    }

        //    if (string.IsNullOrEmpty(model.tableIdd) || !table.Any(t => t.FloorTableId == Convert.ToInt64(model.tableIdd)))
        //    {
        //        if (table != null && table.Count() > 0 && model.Covers <= maxCoversLimit)
        //        {
        //            model.tableIdd = table.First().FloorTableId.ToString();
        //        }
        //        else
        //        {
        //            model.tableIdd = "0";
        //            ViewBag.LevelList = new List<object>()
        //                { 
        //                    new
        //                    {
        //                        Text = "-No Level-",
        //                        Value = 0
        //                    }
        //                };
        //        }
        //    }

        //    var fTblId = Convert.ToInt64(model.tableIdd);

        //    if (!isMerging)
        //    {
        //        if (table != null && table.Count() > 0 && model.Covers <= maxCoversLimit)
        //        {
        //            model.FloorPlanId = db.tabFloorTables.Find(fTblId).FloorPlanId;
        //        }
        //        else
        //            model.FloorPlanId = 0; // 
        //    }
        //    else
        //    {
        //    }

        //    if (model.MergeTableId.HasValue && model.MergeTableId.Value > 0)
        //    {
        //        var mergedTable = db.tabMergedFloorTables.Find(model.MergeTableId.Value);

        //        var flrTable = new FloorTable
        //        {
        //            FloorTableId = 0,
        //            TableName = mergedTable.TableName
        //        };

        //        table = new List<FloorTable>()
        //        {
        //            new FloorTable
        //            {
        //                FloorTableId = 0,
        //                TableName = mergedTable.TableName,
        //                FloorPlan = mergedTable.FloorPlan,
        //                MinCover = mergedTable.MinCover,
        //                MaxCover = mergedTable.MaxCover
        //            }
        //        };

        //        ViewBag.LevelList = new List<object>()
        //                { 
        //                    new
        //                    {
        //                        Text = "L" + mergedTable.FloorPlan.FLevel.Value + "-" + mergedTable.FloorPlan.FloorName,
        //                        Value = 0
        //                    }
        //                };
        //    }

        //    ViewBag.MaxAvailCovers = maxCoversLimit;
        //    ViewBag.IsAutoMerge = !isManualMerge && (maxCoversLimit < model.Covers);

        //    return table.OrderBy(t => t.FloorPlan.FLevel).ThenBy(t => t.TableName, new AlphaNumericComparer()).ToList();
        //}

        private List<Reservation> GetEndingReservationList(DateTime endResDate, int timeInMin, int minToAdd, out  List<long> disabledList)
        {
            disabledList = new List<long>();
            //var now = endResDate.Date.AddTicks(DateTime.UtcNow.ToClientTime().TimeOfDay.Ticks);
            var now = endResDate.Date.AddMinutes(timeInMin);

            var totalResToday = db.GetReservationByDate(now.Date).Where(r => r.FloorTableId > 0).ToList();

            var reservation = totalResToday.ToList();
            var resTables = reservation.Select(r => r.FloorTableId).ToArray();

            var rejectedStatus = new List<long>()
                                     {
                                         ReservationStatus.Finished,
                                         ReservationStatus.Cancelled
                                     };

            reservation = reservation.Where(r => !rejectedStatus.Contains(r.StatusId.Value)).ToList();
            reservation = reservation.Where(r => r.TimeForm.AddMinutes(r.Duration.GetMinutesFromDuration()) > now &&
                r.TimeForm.AddMinutes(r.Duration.GetMinutesFromDuration()) < now.AddMinutes(5)).ToList();

            foreach (var res in reservation)
            {
                var anyUpcomingReservation = totalResToday.Any(r => r.FloorTableId == res.FloorTableId && r.TimeForm > now &&
                    r.TimeForm < now.AddMinutes(minToAdd));

                if (anyUpcomingReservation)
                {
                    disabledList.Add(res.ReservationId);
                }
            }

            //foreach (var tbl in resTables)
            //{
            //    var upcomingReservation = totalResToday.Where(r => r.FloorTableId == tbl && r.TimeForm > now &&
            //        r.TimeForm < now.AddMinutes(minToAdd)).ToList();

            //    var anyUpcomingReservation = upcomingReservation.Count > 0;

            //    if (anyUpcomingReservation)
            //    {
            //        var upResIds = upcomingReservation.Select(r => r.ReservationId).Distinct().ToList();
            //        disabledList.AddRange(upResIds);
            //    }
            //}

            return reservation;

        }

        private List<string> GetDurationList(string startTime, string endTime)
        {
            var durationList = new List<string>();

            var startTimeValue = startTime.GetMinutesFromDuration();
            var endTimeValue = endTime.GetMinutesFromDuration();

            while (startTimeValue <= endTimeValue)
            {
                startTimeValue += 15;
                durationList.Add(startTime);
                startTime = startTime.AddMinutesToDuration(15);
            }

            return durationList;
        }

        #endregion

        #region Controller overridden methods

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            ModelState.Clear();
            base.Initialize(requestContext);
        }

        #endregion
    }
}
