using AIS.Models;
using AISModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace AIS.Helpers.Caching
{
    public static class DBExtensions
    {
        private static ICacheManager cache = new MemoryCacheManager();

        public static IList<Reservation> GetReservationByDate(this UsersContext db, DateTime date)
        {
            string key = string.Format(CacheKeys.RESERVATION_BY_DATE, db.Database.Connection.Database, date.Ticks);

            return cache.Get<IList<Reservation>>(key, () =>
            {
                // using (var dBContext = new UsersContext())
                // {
                //return db.tabReservations
                //    //.Include("FloorTable.FloorTableServer.Server.ServingTables")
                //    .Include("FloorPlan")
                //    .Include("ReservationServer")
                //    .Include("Status")
                //    //.Include("MergedFloorTable.OrigionalTables.FloorTable.FloorTableServer.Server.ServingTables")
                //    .Include("Customers.PhoneNumbers")
                //    .Include("Customers.SpecialStatus.SpecialStatus")
                //    .Include("Customers.Allergies.Allergies")
                //    .Include("FoodMenuShift")
                //    .Where(r => !r.IsDeleted && r.ReservationDate == date).ToList();
                var normalReservations = db.tabReservations
               .Include("FloorTable.FloorTableServer.Server.ServingTables")
               .Include("FloorPlan")
               .Include("ReservationServer")
               .Include("Status")
                    // .Include("MergedFloorTable.OrigionalTables.FloorTable.FloorTableServer.Server.ServingTables")
               .Include("Customers.PhoneNumbers")
               .Include("Customers.SpecialStatus.SpecialStatus")
               .Include("Customers.Allergies.Allergies")
               .Include("FoodMenuShift").Where(r => !r.IsDeleted && r.FloorTableId > 0 && r.ReservationDate == date).ToList();

                var mergedReservations = db.tabReservations
                    //.Include("FloorTable.FloorTableServer.Server.ServingTables")
                 .Include("FloorPlan")
                 .Include("ReservationServer")
                 .Include("Status")
                 .Include("MergedFloorTable.OrigionalTables.FloorTable.FloorTableServer.Server.ServingTables")
                 .Include("Customers.PhoneNumbers")
                 .Include("Customers.SpecialStatus.SpecialStatus")
                 .Include("Customers.Allergies.Allergies")
                 .Include("FoodMenuShift").Where(r => !r.IsDeleted && r.FloorTableId == 0 && r.ReservationDate == date).ToList();


                var list = new List<Reservation>(normalReservations);
                list.AddRange(mergedReservations);
                return list;
                // }
            });
        }

        public static IList<Reservation> GetReservationByDateRange(this UsersContext db, DateTime startDate, DateTime endDate, bool includeDeleted = false)
        {
            string key = string.Format(CacheKeys.RESERVATION_BY_DATE_RANGE, db.Database.Connection.Database, startDate.Ticks, endDate.Ticks, includeDeleted);

            return cache.Get<IList<Reservation>>(key, 15, () =>
            {
                // using (var dBContext = new UsersContext())
                // {
                //var query = db.tabReservations
                //    //.Include("FloorTable.FloorTableServer.Server.ServingTables")
                //    .Include("FloorPlan")
                //    .Include("ReservationServer")
                //    .Include("Status")
                //    //.Include("MergedFloorTable.OrigionalTables.FloorTable.FloorTableServer.Server.ServingTables")
                //    .Include("Customers.PhoneNumbers")
                //    .Include("Customers.SpecialStatus.SpecialStatus")
                //    .Include("Customers.Allergies.Allergies")
                //    .Include("FoodMenuShift")
                //    .Where(r => r.ReservationDate >= startDate && r.ReservationDate <= endDate);

                //if (!includeDeleted)
                //    query = query.Where(r => !r.IsDeleted);

                //return query.ToList();

                var normalReservations = db.tabReservations
               .Include("FloorTable.FloorTableServer.Server.ServingTables")
               .Include("FloorPlan")
               .Include("ReservationServer")
               .Include("Status")
                    // .Include("MergedFloorTable.OrigionalTables.FloorTable.FloorTableServer.Server.ServingTables")
               .Include("Customers.PhoneNumbers")
               .Include("Customers.SpecialStatus.SpecialStatus")
               .Include("Customers.Allergies.Allergies")
               .Include("FoodMenuShift").Where(r => r.ReservationDate >= startDate && r.ReservationDate <= endDate);
                if (!includeDeleted)
                    normalReservations = normalReservations.Where(r => !r.IsDeleted);

                var mergedReservations = db.tabReservations
                    //.Include("FloorTable.FloorTableServer.Server.ServingTables")
                 .Include("FloorPlan")
                 .Include("ReservationServer")
                 .Include("Status")
                 .Include("MergedFloorTable.OrigionalTables.FloorTable.FloorTableServer.Server.ServingTables")
                 .Include("Customers.PhoneNumbers")
                 .Include("Customers.SpecialStatus.SpecialStatus")
                 .Include("Customers.Allergies.Allergies")
                 .Include("FoodMenuShift").Where(r => r.ReservationDate >= startDate && r.ReservationDate <= endDate);
                if (!includeDeleted)
                    mergedReservations = mergedReservations.Where(r => !r.IsDeleted);

                var list = new List<Reservation>(normalReservations);
                list.AddRange(mergedReservations);

                return list;
                // }
            });
        }

        public static IList<Waiting> GetWaitingByDate(this UsersContext db, DateTime date)
        {
            string key = string.Format(CacheKeys.WAITING_BY_DATE, db.Database.Connection.Database, date.Ticks);

            return cache.Get<IList<Waiting>>(key, () =>
            {
                return db.tabWaitings
                    .Include("Customer.Emails")
                    .Include("Customer.PhoneNumbers")
                    .Include("Customer.SpecialStatus.SpecialStatus")
                    .Include("Customer.Allergies.Allergies")
                    .Where(r => r.WaitingDate == date).ToList();
            });
        }

        public static IList<WeekDays> GetWeekDays(this UsersContext db)
        {
            return cache.Get<IList<WeekDays>>(string.Format(CacheKeys.WEEKDAYS_COMPANY_PATTERN, db.Database.Connection.Database), () =>
            {
                return db.tabWeekDays.ToList();
            });
        }

        public static IList<Status> GetStatusList(this UsersContext db)
        {
            //CacheKeys.RESERVATION_STATUS_PATTERN
            return cache.Get<IList<Status>>(string.Format(CacheKeys.RESERVATION_STATUS_COMAPNY_PATTERN, db.Database.Connection.Database), () =>
            {
                return db.Status.ToList();
            });
        }

        public static IList<FoodMenuShift> GetFoodMenuShifts(this UsersContext db)
        {
            return cache.Get<IList<FoodMenuShift>>(string.Format(CacheKeys.FOOD_MENUSHIFT_COMPANY_PATTERN, db.Database.Connection.Database), () =>
            {
                return db.tabFoodMenuShift.ToList();
            });
        }

        public static IList<MenuShiftHours> GetMenuShiftHours(this UsersContext db)
        {
            return cache.Get<IList<MenuShiftHours>>(string.Format(CacheKeys.FOOD_MENUSHIFT_SHIFTHOURS, db.Database.Connection.Database), () =>
            {
                return db.tabMenuShiftHours.ToList();
            });
        }

        public static int GetMaxFloorCovers(this UsersContext db, Int64? floorPlanId = null)
        {
            string key = string.Format(CacheKeys.FLOOR_PLAN_MAX_COVERS, db.Database.Connection.Database, floorPlanId.HasValue ? floorPlanId.Value : 0);

            return cache.Get(key, () =>
            {
                int result = 0;
                var tableQuery = db.tabFloorTables.AsQueryable();

                if (floorPlanId.HasValue)
                    tableQuery = tableQuery.Where(t => t.FloorPlanId == floorPlanId);

                try
                {
                    result = tableQuery.Max(t => t.MaxCover);
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains(""))
                    {
                        throw;
                    }
                }

                return result;
            });
        }

        public static int GetMaxAvailableCovers(this UsersContext db, DateTime datetime, Int64? floorPlanId = null)
        {
            string key = string.Format(CacheKeys.FLOOR_PLAN_MAX_COVERS, db.Database.Connection.Database, floorPlanId.HasValue ? floorPlanId.Value : 0);

            return cache.Get(key, () =>
            {
                var tableQuery = db.tabFloorTables.AsQueryable();

                if (floorPlanId.HasValue)
                    tableQuery = tableQuery.Where(t => t.FloorPlanId == floorPlanId);

                return tableQuery.Max(t => t.MaxCover);
            });
        }

        public static IList<UserProfile> GetCachedStaffList(this UsersContext db)
        {
            return cache.Get(string.Format(CacheKeys.STAFF_LIST, db.Database.Connection.Database), () =>
               {
                   using (var dbContext = new UsersContext())
                   {
                       return ((DbSet<UserProfile>)dbContext.Users)
                            .Include("Designation")
                            .Include("PhoneNumbers")
                            .Include("ServingTables")
                            .Include("ServingReservations")
                            .Where(u => (u.DesignationId.HasValue && u.Designation.IsAssignable) &&
                            (u.Availability.HasValue && u.Availability == true)).ToList();
                   }
               });

        }

        public static IList<FloorTableBlock> GetFloorTableBlockTimeList(this UsersContext db, DateTime date)
        {
            string key = string.Format(CacheKeys.FLOOR_TABLES_BLOCK_BY_DATE, db.Database.Connection.Database, date.Ticks);

            return cache.Get<IList<FloorTableBlock>>(key, () =>
            {
                return db.tabFloorTableBlocks
                    .Include("FloorTable")
                    .Where(b => b.BlockDate == date).ToList();
            });
        }

        public static MessageTemplate GetMessageTemplateByName(this UsersContext db, string templateName)
        {
            string key = string.Format(CacheKeys.MESSAGETEMPLATES_BY_NAME_KEY, db.Database.Connection.Database, templateName);

            return cache.Get<MessageTemplate>(key, () =>
            {
                var query = db.tabMessageTemplates.AsQueryable();
                query = query.Where(t => t.Name == templateName);
                query = query.OrderBy(t => t.MessageTemplateId);
                var templates = query.ToList();

                return templates.FirstOrDefault();
            });
        }

        public static Setting GetSettingByName(this UsersContext db, string settingName)
        {
            string key = string.Format(CacheKeys.SETTING_BY_NAME_KEY, db.Database.Connection.Database, settingName);

            return cache.Get<Setting>(key, () =>
            {
                var query = db.tabSettings.AsQueryable();
                query = query.Where(t => t.Name == settingName);
                query = query.OrderBy(t => t.SettingId);
                var settings = query.ToList();

                return settings.FirstOrDefault();
            });
        }

        public static TimeZoneInfo GetDefaultTimeZone(this UsersContext db)
        {
            string timezone = db.GetSettingByName("TimeZone").Value;
            return TimeZoneInfo.FindSystemTimeZoneById(timezone);
        }

        public static DayOpenCloseTime GetOpenAndCloseTime(this DayOfWeek day, string companyName)
        {

            if (string.IsNullOrWhiteSpace(companyName))
                throw new Exception("company name missing.");

            string key = string.Format(CacheKeys.FOOD_MENUSHIFT_OPEN_CLOSE_DAYTIME, companyName, day);

            return cache.Get<DayOpenCloseTime>(key, () =>
            {
                using (var db = new UsersContext(companyName))
                {
                    var dayName = day.ToString();
                    var dayId = db.GetWeekDays().Single(p => p.DayName.Contains(dayName)).DayId;
                    var ttime = db.GetMenuShiftHours().Where(p => p.DayId == dayId).AsEnumerable();
                    var minOpenAt = ttime.Where(p => p.OpenAt != null).Min(p => Convert.ToDateTime(p.OpenAt));
                    var maxCloseAt = ttime.Where(p => p.CloseAt != null).Max(p => Convert.ToDateTime(p.CloseAt).AddDays(Convert.ToInt32(p.IsNext)));

                    return new DayOpenCloseTime
                    {
                        Day = day,
                        OpenTime = minOpenAt,
                        CloseTime = maxCloseAt
                    };
                }
            });
        }

        public static IList<FloorTable> GetFloorTable(this UsersContext db)
        {
            return cache.Get<IList<FloorTable>>(string.Format(CacheKeys.FLOOR_TABLES_ONLY, db.Database.Connection.Database), () =>
            {
                return db.tabFloorTables.ToList();
            });
        }

        public static IList<TableAvailability> GettabTableAvailabilities(this UsersContext db)
        {
            return cache.Get<IList<TableAvailability>>(string.Format(CacheKeys.FLOOR_TABLES_SCREEN_PATTREN_TableAvailability, db.Database.Connection.Database), () =>
            {
                return db.tabTableAvailabilities.Include("TableAvailabilityFloorTables")
                .Include("TableAvailabilityWeekDays").ToList();

            });
        }


    }

    public class DayOpenCloseTime
    {
        public DayOfWeek Day { get; set; }
        public DateTime OpenTime { get; set; }
        public DateTime CloseTime { get; set; }
    }
}