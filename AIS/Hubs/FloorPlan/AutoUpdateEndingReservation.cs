using AIS.Extensions;
using AIS.Models;
using AIS.Helpers.Caching;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using AISModels;
using System.Globalization;

namespace AIS.Hubs.FloorPlan
{
    public class AutoUpdateEndingReservation
    {
        private const string FAILED_TO_EXTEND_RESERVATION = "Sorry, an error occurred while extending reservation at Table \"{0}\" ending at {1}. Reason \"{2}\".";
        private const string FAILED_EXCEED_DAY_CLOSE_TIME = "Sorry, reservation at Table \"{0}\" ending at {1} can't be extended as it will exceed today's closing time.";
        private const string CANNOT_EXTEND_RESERVATION = "Sorry, the system can't extend the reservation at Table \"{0}\" ending at {1} " +
            "as the system has failed to resolve upcoming conflicted reservation starting at {2} on this table.";
        private const string CANNOT_EXTEND_CONFICT_MERGE_RESERVATION = "Sorry, the system can't extend the reservation at Table \"{0}\" ending at {1} " +
            "as there is an upcoming conflicted & merged reservation starting at {2} on this table.";
        private const string RESERVATION_EXTENDED_SUCCESSFULLY = "The reservation at Table \"{0}\" ending at {1} is extended by 15 minutes.";
        private const string RESERVATION_EXTENDED_CONFLICT_RESOLVED_SUCCESSFULLY = "The reservation at Table \"{0}\" ending at {1} is extended by 15 minutes and" +
            " the upcoming reservation at this table starting at {2} is shifted to Table \"{3}\".";

        private readonly ICacheManager _cache;
        private readonly static Lazy<AutoUpdateEndingReservation> _instance = new Lazy<AutoUpdateEndingReservation>(() => new AutoUpdateEndingReservation());
        private readonly TimeSpan BroadcastInterval = TimeSpan.FromMinutes(1);
        private readonly IHubContext _hubContext;
        private Timer _broadcastLoop;

        public AutoUpdateEndingReservation()
        {
            _cache = new MemoryCacheManager();
            // Save our hub context so we can easily use it 
            // to send to its connected clients
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<FloorPlanHub>();

            // Start the broadcast loop
            _broadcastLoop = new Timer(
                BroadcastUpdate,
                null,
                BroadcastInterval - TimeSpan.FromSeconds(DateTime.UtcNow.Second),
                BroadcastInterval);
        }

        public void BroadcastUpdate(object state)
        {
            try
            {
                SAASContext _db = new SAASContext();
                var databaseName = _db.Users.Where(c => c.Approved != null).ToList();
                foreach (var item in databaseName)
                {
                    string databasename = item.RestaurantName;
                    databasename = databasename.Replace(" ", "");
                    //databasename = "vanfish";
                    //if (databaseName != null)
                    //{
                    try
                    {
                        int minToAdd = 15;
                        int checkIfMins = 13;
                        var timeZone = _cache.Get<Setting>(string.Format(CacheKeys.SETTING_BY_NAME_KEY, databasename, "TimeZone"), () =>
                        {
                            using (var db = new UsersContext(databasename))
                            {
                                return db.GetSettingByName("TimeZone");
                            }
                        });

                        DateTime defaultClientTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(timeZone.Value));
                        if ((defaultClientTime.TimeOfDay.Minutes % 15) == checkIfMins)
                        {
                            var openCloseTimeOfDay = defaultClientTime.DayOfWeek.GetOpenAndCloseTime(databasename);

                            if (openCloseTimeOfDay.OpenTime <= defaultClientTime && defaultClientTime <= openCloseTimeOfDay.CloseTime.AddMinutes(-15))
                            {
                                using (var db = new UsersContext(databasename))
                                {
                                    List<Reservation> singleTableReservations = null;
                                    List<long> conflictedRes = new List<long>();

                                    var allResToday = db.tabReservations.Where(r => !r.IsDeleted && r.ReservationDate == defaultClientTime.Date).ToList();

                                    singleTableReservations = allResToday.Where(r => r.FloorTableId > 0).ToList();

                                    var rejectedStatus = new List<long>()
                                     {
                                         ReservationStatus.Finished,
                                         ReservationStatus.Cancelled
                                     };

                                    singleTableReservations = singleTableReservations.Where(r => !rejectedStatus.Contains(r.StatusId.Value)).ToList();
                                    singleTableReservations = singleTableReservations.Where(r => r.TimeForm.AddMinutes(r.Duration.GetMinutesFromDuration()) > defaultClientTime &&
                                        r.TimeForm.AddMinutes(r.Duration.GetMinutesFromDuration()) < defaultClientTime.AddMinutes(15 - checkIfMins)).ToList();

                                    int resCount = singleTableReservations.Count;
                                    decimal processedReservations = 0;
                                    if (resCount > 0)
                                        _hubContext.Clients.All.startProgress();

                                    foreach (var res in singleTableReservations)
                                    {
                                        var upcomingReservation = allResToday
                                            .FirstOrDefault(r => (r.TimeForm > defaultClientTime && r.TimeForm < defaultClientTime.AddMinutes(minToAdd))
                                                && (r.FloorTableId == 0 ? r.MergedFloorTable.OrigionalTables.Any(t => t.FloorTableId == res.FloorTableId) : r.FloorTableId == res.FloorTableId));

                                        if (upcomingReservation != null)
                                        {
                                            bool isMergeConflict = (upcomingReservation.FloorTableId == 0);
                                            var oldDuration = res.Duration;
                                            try
                                            {
                                                if (!isMergeConflict && TryShiftUpcomingReservationToOtherAvailableTable(db, upcomingReservation))
                                                {
                                                    try
                                                    {
                                                        ExtendReservationDuration(minToAdd, db, res, out oldDuration);

                                                        _hubContext.Clients.Group(databasename).updateEndingReservation(
                                                            string.Format(RESERVATION_EXTENDED_CONFLICT_RESOLVED_SUCCESSFULLY,
                                                            res.FloorTable.TableName,
                                                            res.TimeForm.AddMinutes(oldDuration.GetMinutesFromDuration()).ToShortTimeString(),
                                                            upcomingReservation.TimeForm.ToShortTimeString(),
                                                            upcomingReservation.FloorTable.TableName), NotifyType.INFO);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        _hubContext.Clients.Group(databasename).updateEndingReservation(
                                                            string.Format(FAILED_TO_EXTEND_RESERVATION,
                                                            res.FloorTable.TableName,
                                                            res.TimeForm.AddMinutes(oldDuration.GetMinutesFromDuration()).ToShortTimeString(),
                                                            ex.Message), NotifyType.ERROR);
                                                    }
                                                }
                                                else
                                                {
                                                    if (isMergeConflict)
                                                    {
                                                        _hubContext.Clients.Group(databasename).updateEndingReservation(
                                                            string.Format(CANNOT_EXTEND_CONFICT_MERGE_RESERVATION,
                                                            res.FloorTable.TableName,
                                                            res.TimeForm.AddMinutes(res.Duration.GetMinutesFromDuration()).ToShortTimeString(),
                                                            upcomingReservation.TimeForm.ToShortTimeString()), NotifyType.ERROR);
                                                    }
                                                    else
                                                    {
                                                        _hubContext.Clients.Group(databasename).updateEndingReservation(
                                                            string.Format(CANNOT_EXTEND_RESERVATION,
                                                            res.FloorTable.TableName,
                                                            res.TimeForm.AddMinutes(res.Duration.GetMinutesFromDuration()).ToShortTimeString(),
                                                            upcomingReservation.TimeForm.ToShortTimeString()), NotifyType.ERROR);
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                _hubContext.Clients.Group(databasename).updateEndingReservation(
                                                    string.Format(FAILED_TO_EXTEND_RESERVATION,
                                                    res.FloorTable.TableName,
                                                    res.TimeForm.AddMinutes(res.Duration.GetMinutesFromDuration()).ToShortTimeString(),
                                                    ex.Message), NotifyType.ERROR);
                                            }
                                        }
                                        else if (res.TimeForm.AddMinutes(res.Duration.GetMinutesFromDuration() + minToAdd) > openCloseTimeOfDay.CloseTime)
                                        {
                                            _hubContext.Clients.Group(databasename).updateEndingReservation(
                                               string.Format(FAILED_EXCEED_DAY_CLOSE_TIME,
                                               res.FloorTable.TableName,
                                               res.TimeForm.AddMinutes(res.Duration.GetMinutesFromDuration()).ToShortTimeString()), NotifyType.ERROR);
                                        }
                                        else
                                        {
                                            var oldDuration = res.Duration;
                                            try
                                            {
                                                ExtendReservationDuration(minToAdd, db, res, out oldDuration);
                                                _hubContext.Clients.Group(databasename).updateEndingReservation(
                                                    string.Format(RESERVATION_EXTENDED_SUCCESSFULLY,
                                                    res.FloorTable.TableName,
                                                    res.TimeForm.AddMinutes(oldDuration.GetMinutesFromDuration()).ToShortTimeString()), NotifyType.SUCCESS);
                                            }
                                            catch (Exception ex)
                                            {
                                                _hubContext.Clients.Group(databasename).updateEndingReservation(
                                                    string.Format(FAILED_TO_EXTEND_RESERVATION,
                                                    res.FloorTable.TableName,
                                                    res.TimeForm.AddMinutes(oldDuration.GetMinutesFromDuration()).ToShortTimeString(),
                                                    ex.Message), NotifyType.ERROR);
                                            }
                                        }

                                        processedReservations++;
                                        var percent = Convert.ToInt32(Math.Floor((processedReservations / resCount) * 100));
                                        _hubContext.Clients.All.updateProgress(percent);

                                        Thread.Sleep(200);
                                    }

                                    _hubContext.Clients.Group(databasename).updateProgress(100);
                                }
                            }

                            ClearReservationCache(databasename);
                        }
                    }
                    catch (Exception ex)
                    {
                        _hubContext.Clients.Group(databasename).updateEndingReservation(ex.Message, NotifyType.ERROR);
                    }
                }
            }
            catch (Exception ex)
            {
                _hubContext.Clients.All.updateEndingReservation(ex.Message, NotifyType.ERROR);
            }
        }

        private void ExtendReservationDuration(int minToAdd, UsersContext db, Reservation res, out string oldDuration)
        {
            oldDuration = res.Duration;
            var newDuration = oldDuration.AddMinutesToDuration(minToAdd);

            res.Duration = newDuration;
            res.TimeTo = res.TimeForm.AddMinutes(newDuration.GetMinutesFromDuration());
            db.Entry(res).State = System.Data.Entity.EntityState.Modified;
            //db.LogEditReservation(reservation, loginUser, null);

            db.SaveChanges();
        }

        private bool TryShiftUpcomingReservationToOtherAvailableTable(UsersContext db, Reservation upcomingReservation)
        {
            ReservationVM model;
            IList<Int64> upcomingTableIds;
            IList<Int64> smallTableIds;
            var availableTable = db.GetAvailableFloorTables(upcomingReservation, out upcomingTableIds, out smallTableIds, out model, true, true)
                .Where(t => t.FloorTableId != upcomingReservation.FloorTableId);
            //.FirstOrDefault();

            #region Check for entries in tableavailabilities
            /**** Table availability feature enabled  start here *****/

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

            var day = upcomingReservation.ReservationDate.DayOfWeek.ToString();
            var dId = db.GetWeekDays().Single(p => p.DayName.Contains(day)).DayId;

            var availList = db.tabTableAvailabilities
                .Include("TableAvailabilityFloorTables")
                .Include("TableAvailabilityWeekDays")
                .Where(ta => ta.StartDate <= model.resDate && model.resDate <= ta.EndDate
                && ta.TableAvailabilityWeekDays.Any(taw => taw.DayId == dId)).ToList();

            var blockList = db.GetFloorTableBlockTimeList(model.resDate);

            availableTable = availableTable.Where(t => availList.CheckAvailStatus(model.resDate, startTm, endTime, t, 1)
                && !blockList.IsTableBlocked(t.FloorTableId, startTm, endTime)).ToList();


            /**** Table availability feature enabled end here *****/
            #endregion

            var firstAvailableTable = availableTable.FirstOrDefault();

            if (firstAvailableTable != null)
            {
                upcomingReservation.FloorTableId = firstAvailableTable.FloorTableId;
                upcomingReservation.TablePositionLeft = firstAvailableTable.TLeft;
                upcomingReservation.TablePositionTop = firstAvailableTable.TTop;
                db.Entry(upcomingReservation).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return true;
            }

            return false;
        }

        public static AutoUpdateEndingReservation Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        private void ClearReservationCache(string databaseName)
        {
            _cache.RemoveByPattern(string.Format(CacheKeys.RESERVATION_BY_DATE_COMPANY_PATTREN, databaseName));
            _cache.RemoveByPattern(string.Format(CacheKeys.FILTERED_RESERVATION_COMPANY_PATTREN, databaseName));
            _cache.RemoveByPattern(string.Format(CacheKeys.FLOOR_TABLES_SCREEN_COMPANY_PATTREN, databaseName));
        }
    }
}