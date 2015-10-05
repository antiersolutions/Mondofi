using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AISModels;

namespace AIS.Models
{
    public class WeekVM
    {
        public List<TimeData> TimeData { get; set; }
    }

    public class wDay
    {
        public DateTime day { get; set; }
        public List<Reservation> Reservations { get; set; }
    }

    public class TimeData
    {
        public TimeSpan time { get; set; }
        public List<wDay> wDay { get; set; }
    }
}