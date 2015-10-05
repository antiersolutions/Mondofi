using AIS.Models.TableAvailablity;
using AISModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AIS.Models
{
    public class OnlineAvailTables
    {
        public OnlineAvailTables()
        {
            OtherMatches = new List<TimeSlotAvailabilities>();
        }

        public TimeSlotAvailabilities ExactMatch { get; set; }

        public IList<TimeSlotAvailabilities> OtherMatches { get; set; }
    }

    public class TimeSlotAvailabilities
    {
        public TimeSlotAvailabilities()
        {
            AvailableTables = new List<FloorTable>();
        }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public IList<FloorTable> AvailableTables { get; set; }
    }
}