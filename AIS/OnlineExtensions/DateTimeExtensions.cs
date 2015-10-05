using AIS.Models;
using AIS.Helpers.Caching;
using AISModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AIS.OnlineExtensions
{
    public static class DateTimeExtensions
    {
        private static ICacheManager cache = new MemoryCacheManager();

        public static List<DateTime> GetDaysInWeek(this DateTime Date, DayOfWeek firstdayofweek)
        {
            List<DateTime> d = new List<DateTime>();

            int days = Date.DayOfWeek - firstdayofweek;
            DateTime dt = Date.AddDays(-days);
            d.Add(dt);
            d.AddRange(new DateTime[] { dt.AddDays(1), dt.AddDays(2), dt.AddDays(3), dt.AddDays(4), dt.AddDays(5), dt.AddDays(6) });

            return d;
        }

        public static bool IsNextDay(this DateTime dateTime)
        {
            return (new DateTime().TimeOfDay <= dateTime.TimeOfDay && dateTime.TimeOfDay < new DateTime().AddHours(4).TimeOfDay);
        }

        public static DateTime ToClientTime(this DateTime dt)
        {
            // read the value from session
            var timeOffSet = HttpContext.Current.Session["timezoneoffset"];

            if (timeOffSet != null)
            {
                var offset = int.Parse(timeOffSet.ToString());
                dt = dt.AddMinutes(-1 * offset);

                return dt;
            }

            // if there is no offset in session return the datetime in server timezone
            return dt.ToLocalTime();
        }

        public static DateTime ToDefaultTimeZone(this DateTime dt,string companyName)
        {
            string settingkey = string.Format(CacheKeys.SETTING_BY_NAME_KEY,companyName, "TimeZone");

            var timeZoneSetting = cache.Get<Setting>(settingkey, () =>
            {
                Setting setting;
                using (var _db = new UsersContext(companyName))
                {
                    var query = _db.tabSettings.AsQueryable();
                    query = query.Where(t => t.Name == "TimeZone");
                    query = query.OrderBy(t => t.SettingId);
                    var settings = query.ToList();

                    setting = settings.FirstOrDefault();
                }

                return setting;
            });

            var systemTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneSetting.Value);
            return TimeZoneInfo.ConvertTimeFromUtc(dt, systemTimeZone);
        }

        public static string TimeAgo(this DateTime dt)
        {
            var Now = DateTime.UtcNow.ToClientTime();

            if (dt > Now)
            {
                return "about sometime from now";
            }

            TimeSpan span = Now - dt;

            if (span.TotalSeconds <= 5)
            {
                return "a moment ago";
            }

            if (span.TotalSeconds < 60)
            {
                return String.Format("{0} secs ago", span.Seconds);
            }

            if (span.TotalMinutes < 60)
            {
                return String.Format("{0} {1}, {2} sec ago", span.Minutes, (span.Minutes == 1 ? "min" : "mins"), span.Seconds);
            }

            if (span.TotalHours < 24)
            {
                return String.Format("{0} {1}, {2} {3} ago", span.Hours, (span.Hours == 1 ? "hr" : "hrs"), span.Minutes, (span.Minutes == 1 ? "min" : "mins"));
            }

            if (span.TotalDays > 0)
            {
                return String.Format("{0} {1}, {2} {3} ago", span.Days, (span.Days == 1 ? "day" : "days"), span.Hours, (span.Hours == 1 ? "hour" : "hours"));
            }

            //if (span.Days > 30)
            //{
            //    int months = (span.Days / 30);
            //    if (span.Days % 31 != 0)
            //    {
            //        months += 1;
            //    }

            //    return String.Format("{0}{1} {2}{3} ago", months, (months == 1 ? "month" : "months"), span.Days, (span.Days == 1 ? "day" : "days"));
            //}

            //if (span.Days > 365)
            //{
            //    int years = (span.Days / 365);
            //    if (span.Days % 365 != 0)
            //    {
            //        years += 1;
            //    }

            //    int months = (span.Days / 30);
            //    if (span.Days % 31 != 0)
            //    {
            //        months += 1;
            //    }

            //    return String.Format("{0}{1} {2}{3} ago", years, (years == 1 ? "yr" : "yrs"), months, (months == 1 ? "month" : "months"));
            //}

            return string.Empty;
        }

        public static string AsOrdinal(this Int32 number)
        {
            if (number < 0)
                throw new ArgumentOutOfRangeException("number");

            var work = number.ToString("n0");

            var modOf100 = number % 100;

            if (modOf100 == 11 || modOf100 == 12 || modOf100 == 13)
                return work + "th";

            switch (number % 10)
            {
                case 1:
                    work += "st"; break;
                case 2:
                    work += "nd"; break;
                case 3:
                    work += "rd"; break;
                default:
                    work += "th"; break;
            }

            return work;
        }

        public static IList<int> GetWeekDaysIncludedInRange(this IList<WeekDays> dbWeekdays, DateTime startDate, DateTime endDate)
        {
            IList<int> daysIncluded = new List<int>();
            int numberOfDays = (endDate - startDate).Days;
            for (int days = 0; days <= numberOfDays; days++)
            {
                DateTime date = startDate.AddDays(days);
                var dayId = dbWeekdays.First(d => d.DayName.Contains(date.DayOfWeek.ToString())).DayId;

                if (!daysIncluded.Contains(dayId))
                    daysIncluded.Add(dayId);
            }

            return daysIncluded;
        }
    }
}