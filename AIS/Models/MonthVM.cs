using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AIS.Models
{
    public class MonthVM
    {
        public List<wDay> Week1 { get; set; }
        public List<wDay> Week2 { get; set; }
        public List<wDay> Week3 { get; set; }
        public List<wDay> Week4 { get; set; }
        public List<wDay> Week5 { get; set; }
        public List<wDay> Week6 { get; set; }
        public string nextMonth { get; set; }
        public string prevMonth { get; set; }
        public string startMonth { get; set; }
        public string endMonth { get; set; }
    }

    public class MonthCalendarTopVM
    {
        public string CalendarType { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string UrlAction { get; set; }
        public string UrlController { get; set; }
        public string TargetId { get; set; }
        public string ddlMonth { get; set; }
    }
}