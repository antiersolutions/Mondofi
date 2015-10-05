using AIS.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace AIS.OnlineExtensions
{
    public static class CalendarExtensions
    {
        public static void GetStartEndTimeList(this UsersContext db, string start, string end, out string minValue, out string maxValue)
        {
            //var startDate = Convert.ToDateTime(start, CultureInfo.CurrentCulture);
            //var endDate = Convert.ToDateTime(end, CultureInfo.CurrentCulture);

            var startDate = Convert.ToDateTime(start);
            var endDate = Convert.ToDateTime(end);

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

            var dropDownMin = dayList.Where(d => d.MenuShiftHours.Where(s => DateTime.TryParse(s.OpenAt, out open)).Count() > 0).Min(d => d.MenuShiftHours.Where(s => s.OpenAt != null).Min(p => Convert.ToDateTime(p.OpenAt)));

            var dropDownMax = dayList.Where(d => d.MenuShiftHours.Where(s => DateTime.TryParse(s.CloseAt, out close)).Count() > 0).Max(d => d.MenuShiftHours.Where(s => s.OpenAt != null).Max(p => Convert.ToDateTime(p.CloseAt).AddDays(p.IsNext.Value)));

            minValue = dropDownMin.ToString("h:mm tt");
            maxValue = dropDownMax.ToString("h:mm tt");
        }
    }
}