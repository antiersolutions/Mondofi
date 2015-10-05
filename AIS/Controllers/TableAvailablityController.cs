using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AIS.Models;
using AIS.Models.TableAvailablity;
using System.Globalization;
using AISModels;
using System.Threading;
using AIS.Extensions;
using AIS.Helpers.Caching;
using AIS.Helpers;
using AIS.Filters;
using Microsoft.AspNet.Identity;

namespace AIS.Controllers
{
    [Authorize]
    public class TableAvailablityController : Controller
    {
        private UsersContext db = new UsersContext();
        private ICacheManager cache = new MemoryCacheManager();

        #region Table Availability Actions

        #region Index Section

        public ActionResult Index()
        {
            ViewBag.shiftDDl = new SelectList(db.tabFoodMenuShift, "FoodMenuShiftId", "MenuShift");
            return View();
        }

        public ActionResult getTableAvailablityNew(TableAvailabilityFilter filters, string date, Int64? shiftId)
        {
            var obj = new TimeAvailablityVM();
            var dat = Convert.ToDateTime(date, CultureInfo.InvariantCulture);

            var startTime = dat.AddTicks(DateTime.Parse("4:00:00").TimeOfDay.Ticks);
            var endTime = startTime.AddHours(23).AddMinutes(45);

            var array = new string[] { "Sofa", "Chair", "SofaTable", "Wall", "SolidWall", "GlassWall", "BarTable", "Fence", "Pillar" };
            var tableDB = db.tabFloorTables.Include("Reservations").Where(p => p.IsDeleted == false).ToList().Where(p => !array.Contains(p.TableName.Split('-')[0])).ToList();

            var day = dat.DayOfWeek.ToString();
            var menuShifts = db.tabMenuShiftHours.Where(s => s.WeekDays.DayName.Equals(day, StringComparison.CurrentCultureIgnoreCase)).ToList();

            var dId = db.GetWeekDays().Single(p => p.DayName.Contains(day)).DayId;
            var availList = db.tabTableAvailabilities
                .Include("TableAvailabilityFloorTables")
                .Include("TableAvailabilityWeekDays")
                .Where(ta => ta.StartDate <= dat && dat <= ta.EndDate
                && ta.TableAvailabilityWeekDays.Any(taw => taw.DayId == dId)).ToList();

            var model = new TableAvailNewVM();
            model.Tables = new List<Table>();

            var cday = db.tabWeekDays.Where(p => p.DayName == dat.DayOfWeek.ToString());
            var nday = db.tabWeekDays.Where(p => p.DayName == dat.AddDays(1).DayOfWeek.ToString());

            var shiftStime = new DateTime();
            var shiftEtime = new DateTime();

            if (shiftId.HasValue)
            {
                var dayId = db.tabWeekDays.AsEnumerable().Single(p => p.DayName == dat.DayOfWeek.ToString()).DayId;

                var dday = db.tabMenuShiftHours.Where(p => p.FoodMenuShiftId == shiftId && p.DayId == dayId).SingleOrDefault();

                var open = dday.OpenAt;
                var close = dday.CloseAt;

                shiftStime = dat.AddTicks(DateTime.Parse(open).TimeOfDay.Ticks);



                if (shiftId == 4 && close.Contains("AM"))
                {
                    shiftEtime = dat.AddDays(1).AddTicks(DateTime.Parse(close).TimeOfDay.Ticks);
                }
                else
                {
                    shiftEtime = dat.AddTicks(DateTime.Parse(close).TimeOfDay.Ticks);
                }
            }

            foreach (var tbl in tableDB)
            {
                var table = new Table();
                table.FloorTable = tbl;
                table.AvailStatus = new List<AvailStatus>();

                var time = startTime;

                while (time <= endTime)
                {
                    var AStatus = new AvailStatus();
                    AStatus.time = time;
                    if (shiftId.HasValue == true)
                    {
                        if (shiftStime <= time && shiftEtime > time)
                        {
                            bool isStartRes = false;
                            AStatus.Reservation = this.CheckReservationStatus(tbl, time, endTime, out isStartRes);
                            AStatus.Status = this.CheckAvailStatus(time, AStatus.Reservation, availList, tbl);
                            AStatus.shiftId = this.CheckShift(time, menuShifts);
                            AStatus.IsResStart = isStartRes;
                        }
                        else
                        {
                            AStatus.Reservation = null;
                            AStatus.Status = 0;
                            AStatus.shiftId = 0;
                        }
                    }
                    else
                    {
                        bool isStartRes = false;
                        AStatus.Reservation = this.CheckReservationStatus(tbl, time, endTime, out isStartRes);
                        AStatus.Status = this.CheckAvailStatus(time, AStatus.Reservation, availList, tbl);
                        AStatus.shiftId = this.CheckShift(time, menuShifts);
                        AStatus.IsResStart = isStartRes;
                    }

                    table.AvailStatus.Add(AStatus);

                    time = time.AddMinutes(15);
                }
                model.Tables.Add(table);
            }

            model.Tables = model.Tables
                .OrderBy(t => t.FloorTable.FloorPlan.FLevel)
                .ThenBy(t => t.FloorTable.SectionId)
                .ThenBy(t => t.FloorTable.TableName, new AlphaNumericComparer())
                .ToList();

            this.FilterTables(model, filters);

            return PartialView(model);
        }

        #endregion

        #region Edit Section

        [HttpGet]
        public ActionResult Edit(EditAvailVM model)
        {
            //var startTime = DateTime.Parse("3:45:00");
            //var str = new List<string>();
            //var endTime = startTime.AddHours(23).AddMinutes(45);

            //while (startTime <= endTime)
            //{
            //    var dt = startTime.AddMinutes(15);
            //    str.Add(dt.ToString("h:mm tt"));
            //    startTime = startTime.AddMinutes(15);
            //}
            string minValue = string.Empty;
            string maxValue = string.Empty;

            var str = this.GetStartEndTimeList(model.StartDate, model.EndDate, out minValue, out maxValue);

            ViewBag.Time = str;

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(TableAvailability model, int[] selectedWeekDays, long[] selectedTables, int selectedAvailability)
        {
            model.CreatedOn = DateTime.UtcNow.ToClientTime();
            model.CreatedBy = User.Identity.GetUserId<long>();

            model.TableAvailabilityWeekDays = selectedWeekDays.Select(d => new TableAvailabilityWeekDay
            {
                DayId = d,
                TableAvailabilityId = model.TableAvailabilityId
            }).ToList();

            model.TableAvailabilityFloorTables = selectedTables.Select(t => new TableAvailabilityFloorTable
            {
                FloorTableId = t,
                TableAvailabilityId = model.TableAvailabilityId
            }).ToList();

            model.AvailablityStatusId = selectedAvailability;

            db.tabTableAvailabilities.Add(model);
            db.SaveChanges();

            this.ClearAvailabilityCache();

            return RedirectToAction("Index");
        }

        public PartialViewResult GetDayofWeekList(string selectedDay, DateTime startDate, DateTime endDate, bool? isCheckAll)
        {
            bool checkAll = false;

            if (isCheckAll.HasValue)
            {
                checkAll = isCheckAll.Value;
            }

            var weekDays = db.GetWeekDays();
            var availDays = weekDays.GetWeekDaysIncludedInRange(startDate, endDate);

            return PartialView("CheckboxListPartial",
                weekDays.Select(x => new CheckListVM()
                {
                    PropertyName = "selectedWeekDays",
                    Name = x.DayName,
                    Value = x.DayId,
                    IsChecked = ((x.DayName.Equals(selectedDay) || checkAll) ? true : false),
                    IsDisabled = (!availDays.Contains(x.DayId))
                }));
        }

        [HttpPost]
        public PartialViewResult GetDayofWeekList(int[] selectedWeekDays, DateTime startDate, DateTime endDate, bool? isCheckAll)
        {
            bool checkAll = false;

            if (isCheckAll.HasValue)
            {
                checkAll = isCheckAll.Value;
            }

            var weekDays = db.GetWeekDays();
            var availDays = weekDays.GetWeekDaysIncludedInRange(startDate, endDate);
            selectedWeekDays = (selectedWeekDays ?? new int[0]).Where(swd => availDays.Contains(swd)).ToArray();

            return PartialView("CheckboxListPartial",
                weekDays.Select(x => new CheckListVM()
                {
                    PropertyName = "selectedWeekDays",
                    Name = x.DayName,
                    Value = x.DayId,
                    IsChecked = ((selectedWeekDays.Contains(x.DayId) || checkAll) ? true : false),
                    IsDisabled = (!availDays.Contains(x.DayId))
                }));
        }

        public PartialViewResult GetAvailableTablesList(long selectedTable, bool? isCheckAll)
        {
            bool checkAll = false;

            if (isCheckAll.HasValue)
            {
                checkAll = isCheckAll.Value;
            }

            var array = new string[] { "Sofa", "Chair", "SofaTable", "Wall", "SolidWall", "GlassWall", "BarTable", "Fence", "Pillar" };
            var tables = db.tabFloorTables.Include("FloorPlan").Where(t => t.IsDeleted == false).AsEnumerable().Where(p => !array.Contains(p.TableName.Split('-')[0]) && p.IsDeleted == false);

            var data = tables.GroupBy(p => p.FloorPlan).Select(p => new AlphabeticalMapping<CheckListVM>()
            {
                FirstLetter = p.Select(t => t.FloorPlan.FLevel).First() + "-" + p.Key.FloorName,
                Items = p.Select(t => new CheckListVM()
                {
                    PropertyName = "selectedTables",
                    Name = t.TableName,
                    Value = t.FloorTableId,
                    IsChecked = ((t.FloorTableId.Equals(selectedTable) || checkAll) ? true : false),
                    IsDisabled = false
                }).OrderBy(t => t.Name, new AlphaNumericComparer()).ToList()
            }).ToList();
            return PartialView(data);
        }

        public PartialViewResult GetSeatingAvailabilityList(int selectedAvail, bool? isCheckAll, bool? isFilter)
        {
            bool checkAll = false;

            if (isCheckAll.HasValue)
            {
                checkAll = isCheckAll.Value;
            }

            var availStatus = db.tabAvailablityStatus.ToList();

            if (isFilter.HasValue && isFilter.Value == true)
            {
                availStatus.Add(new AvailablityStatus { AvailablityStatusId = 4, Status = "Reserved" });
            }

            return PartialView("CheckboxListPartial", availStatus.Select(x => new CheckListVM()
            {
                PropertyName = "selectedAvailability",
                Name = x.Status,
                Value = x.AvailablityStatusId,
                IsChecked = ((x.AvailablityStatusId.Equals(selectedAvail) || checkAll) ? true : false),
                IsDisabled = false
            }));
        }

        public PartialViewResult UpdateTimeRange(EditAvailVM model)
        {
            ModelState.Clear();

            string minValue = string.Empty;
            string maxValue = string.Empty;

            var str = this.GetStartEndTimeList(model.StartDate, model.EndDate, out minValue, out maxValue);

            ViewBag.Time = str;

            return PartialView("TimeRangePartial", model);
        }

        #endregion

        #region Filter Section

        public PartialViewResult GetfilterPartial(string StartDate, string EndDate)
        {
            string minValue = string.Empty;
            string maxValue = string.Empty;

            var str = this.GetStartEndTimeList(StartDate, EndDate, out minValue, out maxValue);

            ViewBag.Time = str;
            ViewBag.MinTime = minValue;
            ViewBag.MaxTime = maxValue;

            return PartialView("FilterPartial");
        }

        public PartialViewResult GetTableClassList(bool? isCheckAll)
        {
            bool checkAll = false;

            if (isCheckAll.HasValue)
            {
                checkAll = isCheckAll.Value;
            }

            var tableClasses = new List<CheckListVM>();

            tableClasses.Add(new CheckListVM()
            {
                PropertyName = "selectedTableClass",
                Name = "Permanent",
                Value = 1,
                IsChecked = (checkAll) ? true : false,
                IsDisabled = false
            });

            tableClasses.Add(new CheckListVM()
            {
                PropertyName = "selectedTableClass",
                Name = "Temporary",
                Value = 2,
                IsChecked = (checkAll) ? true : false,
                IsDisabled = false
            });

            return PartialView("CheckboxListPartial", tableClasses);
        }

        #endregion

        #endregion

        #region private methods

        private void ClearAvailabilityCache()
        {
            //cache.RemoveByPattern(CacheKeys.RESERVATION_BY_DATE_PATTREN);
            cache.RemoveByPattern(string.Format(CacheKeys.RESERVATION_BY_DATE_COMPANY_PATTREN, User.Identity.GetDatabaseName()));
            //cache.RemoveByPattern(CacheKeys.FILTERED_RESERVATION_PATTREN);
            cache.RemoveByPattern(string.Format(CacheKeys.FILTERED_RESERVATION_COMPANY_PATTREN, User.Identity.GetDatabaseName()));
            //cache.RemoveByPattern(CacheKeys.FLOOR_TABLES_SCREEN_PATTREN);
            cache.RemoveByPattern(string.Format(CacheKeys.FLOOR_TABLES_SCREEN_COMPANY_PATTREN, User.Identity.GetDatabaseName()));
        }

        private Reservation CheckReservationStatus(FloorTable table, DateTime startTime, DateTime endTime, out bool isResStart)
        {
            isResStart = false;

            if (startTime.Date == endTime.Date)
            {
                startTime = startTime.AddDays(-1);
            }

            //var res = table.Reservations.Where(r => !r.IsDeleted && r.ReservationDate == startTime.Date && r.TimeForm <= startTime && startTime < r.TimeTo).FirstOrDefault();

            var reservations = db.GetReservationByDate(startTime.Date);

            var res = reservations.Where(r => !r.IsDeleted
                && (r.FloorTableId == 0 ? r.MergedFloorTable.OrigionalTables.Any(t => t.FloorTableId == table.FloorTableId) : r.FloorTableId == table.FloorTableId)
                && r.ReservationDate == startTime.Date
                && (r.TimeForm <= startTime && startTime < r.TimeTo)).FirstOrDefault();

            if (res != null
                && (res.FloorTableId == 0 ? res.MergedFloorTable.OrigionalTables.First().FloorTableId == table.FloorTableId : true))
            {
                isResStart = res.TimeForm == startTime;
            }

            return res;
        }

        private int CheckAvailStatus(DateTime startTime, Reservation res, List<TableAvailability> availablilies, FloorTable table)
        {
            if (res != null)
            {
                return 4;
            }

            if (availablilies != null && availablilies.Count > 0)
            {
                var avails = availablilies.Where(ta =>
                    (ta.StartDate <= ((startTime.TimeOfDay.TotalMinutes < 240) ? startTime.AddDays(-1).Date : startTime.Date) && ta.EndDate >= ((startTime.TimeOfDay.TotalMinutes < 240) ? startTime.AddDays(-1).Date : startTime.Date))
                    && ta.TableAvailabilityFloorTables.Any(t => t.FloorTableId.Equals(table.FloorTableId)))
                    .ToList();

                avails = avails.Where(ta => CheckAvail(ta, startTime)).OrderByDescending(ta => ta.CreatedOn).ToList();

                //foreach (var avail in avails)
                //{
                //    var TAstartTime = startTime.Date.AddTicks(Convert.ToDateTime(avail.StartTime).TimeOfDay.Ticks);
                //    var TAendTime = startTime.Date.AddTicks(Convert.ToDateTime(avail.EndTime).TimeOfDay.Ticks);

                //    if (!(TAstartTime <= startTime && TAendTime > startTime))
                //    {
                //        avails.Remove(avail);
                //    }
                //}


                //avails = avails.Where(ta =>
                //                (startTime.Date.AddTicks(Convert.ToDateTime(ta.StartTime).TimeOfDay.Ticks) <= startTime && startTime.Date.AddTicks(Convert.ToDateTime(ta.EndTime).TimeOfDay.Ticks) > startTime)
                //                && ta.TableAvailabilityWeekDays.Any(w => w.WeekDays.DayName.Equals(startTime.DayOfWeek.ToString()))).ToList();

                if (avails.Any())
                {
                    var availablity = avails.FirstOrDefault();
                    return availablity.AvailablityStatusId;
                }
                else
                {
                    return 0;
                }
            }

            return 0;
        }

        private bool CheckAvail(TableAvailability avail, DateTime startTime)
        {
            var TAstartTime = startTime.Date.AddTicks(Convert.ToDateTime(avail.StartTime).TimeOfDay.Ticks);
            var TAendTime = startTime.Date.AddTicks(Convert.ToDateTime(avail.EndTime).TimeOfDay.Ticks);

            TAstartTime = (TAstartTime.TimeOfDay.TotalMinutes < 240 && !(startTime.TimeOfDay.TotalMinutes < 240)) ? TAstartTime.AddDays(1) : ((startTime.TimeOfDay.TotalMinutes < 240) && !(TAstartTime.TimeOfDay.TotalMinutes < 240) ? TAstartTime.AddDays(-1) : TAstartTime);
            TAendTime = (TAendTime.TimeOfDay.TotalMinutes < 240 && !(startTime.TimeOfDay.TotalMinutes < 240)) ? TAendTime.AddDays(1) : ((startTime.TimeOfDay.TotalMinutes < 240) && !(TAendTime.TimeOfDay.TotalMinutes < 240) ? TAendTime.AddDays(-1) : TAendTime);

            if (!(TAstartTime <= startTime && startTime < TAendTime))
            {
                return false;
            }

            return true;
        }

        private int CheckShift(DateTime time, List<MenuShiftHours> menuShifts)
        {
            DateTime openAt = new DateTime();
            DateTime closeAt = new DateTime();

            var shift = menuShifts.Where(s => (DateTime.TryParse(s.OpenAt, out openAt) && DateTime.TryParse(s.CloseAt, out closeAt) && ((time.AddDays(s.IsNext.Value) >= time.Date.AddTicks(openAt.TimeOfDay.Ticks) && (time <= time.Date.AddTicks(closeAt.TimeOfDay.Ticks).AddMinutes(-15))) || (time >= time.Date.AddTicks(openAt.TimeOfDay.Ticks) && (time <= time.Date.AddTicks(closeAt.TimeOfDay.Ticks).AddMinutes(-15).AddDays(s.IsNext.Value)))))).FirstOrDefault();

            if (shift == null)
            {
                return 0;
            }
            else
            {
                return shift.FoodMenuShiftId;
            }
        }

        private List<string> GetStartEndTimeList(string start, string end, out string minValue, out string maxValue)
        {
            //var startDate = Convert.ToDateTime(start, CultureInfo.CurrentCulture);
            //var endDate = Convert.ToDateTime(end, CultureInfo.CurrentCulture);

            var startDate = DateTime.ParseExact(start, "MM/dd/yyyy", CultureInfo.InvariantCulture);
            var endDate = DateTime.ParseExact(end, "MM/dd/yyyy", CultureInfo.InvariantCulture);

            var dayIN = new List<string>();

            var firstDate = startDate;
            while (firstDate.Date <= endDate.Date)
            {
                dayIN.Add(firstDate.DayOfWeek.ToString().ToUpper());
                firstDate = firstDate.AddDays(1);
            }

            var open = new DateTime();
            var close = new DateTime();

            var dayList = db.tabWeekDays.Include("MenuShiftHours").ToList().Where(d => dayIN.Contains(d.DayName.ToUpper())).ToList();

            //var dropDownMin = dayList.Where(d => d.MenuShiftHours.Where(s => DateTime.TryParse(s.OpenAt, out open)).Count() > 0).Max(d => d.MenuShiftHours.Where(s => DateTime.TryParse(s.OpenAt, out open)).Max(s => Convert.ToDateTime(s.OpenAt)));
            //var dropDownMax = dayList.Where(d => d.MenuShiftHours.Where(s => DateTime.TryParse(s.CloseAt, out close)).Count() > 0).Min(d => d.MenuShiftHours.Where(s => DateTime.TryParse(s.CloseAt, out close)).Min(s => Convert.ToDateTime(s.CloseAt).AddDays(s.IsNext.Value)));

            var dropDownMin = dayList.Where(d => d.MenuShiftHours.Where(s => DateTime.TryParse(s.OpenAt, out open)).Count() > 0).Max(d => d.MenuShiftHours.Where(s => s.OpenAt != null).Min(p => Convert.ToDateTime(p.OpenAt)));

            var dropDownMax = dayList.Where(d => d.MenuShiftHours.Where(s => DateTime.TryParse(s.CloseAt, out close)).Count() > 0).Min(d => d.MenuShiftHours.Where(s => s.OpenAt != null).Max(p => Convert.ToDateTime(p.CloseAt).AddDays(p.IsNext.Value)));


            var startTime = dropDownMin;
            var str = new List<string>();
            var endTime = dropDownMax;

            while (startTime <= endTime)
            {
                var dt = startTime;
                str.Add(dt.ToString("h:mm tt"));
                startTime = startTime.AddMinutes(15);
            }

            minValue = dropDownMin.ToString("h:mm tt");
            maxValue = dropDownMax.ToString("h:mm tt");

            return str;

        }

        private void FilterTables(TableAvailNewVM model, TableAvailabilityFilter filters)
        {

            if (filters.selectedTables.Count > 0)
            {
                model.Tables.RemoveAll(t => !filters.selectedTables.Contains(t.FloorTable.FloorTableId));
            }

            if (filters.selectedTableClass.Count > 0)
            {
                model.Tables.RemoveAll(t => !filters.selectedTableClass.Contains((t.FloorTable.IsTemporary.HasValue ? ((t.FloorTable.IsTemporary.Value == true) ? 2 : 1) : 2)));
            }

            foreach (var table in model.Tables)
            {
                if (!string.IsNullOrEmpty(filters.StartTime) && !string.IsNullOrEmpty(filters.EndTime))
                {
                    var date = table.AvailStatus.First().time.Date;

                    var startTime = date.AddTicks(Convert.ToDateTime(filters.StartTime).TimeOfDay.Ticks);
                    var endTime = date.AddTicks(Convert.ToDateTime(filters.EndTime).TimeOfDay.Ticks);

                    startTime = startTime.TimeOfDay.TotalMinutes < 240 ? startTime.AddDays(1) : startTime;
                    endTime = endTime.TimeOfDay.TotalMinutes < 240 ? endTime.AddDays(1) : endTime;

                    foreach (var AStatus in table.AvailStatus)
                    {
                        bool isRejected = false;

                        if (AStatus.time < startTime || AStatus.time > endTime)
                        {
                            isRejected = true;
                        }

                        if (isRejected)
                        {
                            AStatus.Reservation = null;
                            AStatus.Status = 0;
                            AStatus.shiftId = 0;
                        }
                    }
                }

                foreach (var AStatus in table.AvailStatus)
                {
                    bool isRejected = false;

                    if (filters.selectedAvailability.Count > 0)
                    {
                        isRejected = (filters.selectedAvailability.Contains(AStatus.Status) || AStatus.Status == 0) ? false : true;
                    }

                    if (isRejected)
                    {
                        AStatus.Reservation = null;
                        AStatus.Status = 0;
                        AStatus.shiftId = 0;
                    }
                }
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
