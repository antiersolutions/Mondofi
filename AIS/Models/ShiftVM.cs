using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AIS.Models
{
    public class WeekDayShift
    {
        public int DayId { get; set; }
        public string BreakfastOpen { get; set; }
        public string BreakfastClose { get; set; }
        public string BrunchOpen { get; set; }
        public string BrunchClose { get; set; }
        public string LunchOpen { get; set; }
        public string LunchClose { get; set; }
        public string DinnerOpen { get; set; }
        public string DinnerClose { get; set; }
    }


    public class TimeIntervalVM
    {
        public string text { get; set; }
        public string timeVal { get; set; }
    }
}