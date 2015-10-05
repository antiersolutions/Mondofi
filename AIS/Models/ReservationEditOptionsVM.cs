using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AIS.Models
{
    public class ReservationEditOptionsVM
    {
        public Int64 ReservationId { get; set; }

        public long StatusId { get; set; }

        public int ShiftId { get; set; }

        public int Covers { get; set; }

        public DateTime Date { get; set; }

        public string Time { get; set; }

        public string Duration { get; set; }

        public Int64 TableId { get; set; }
    }
}