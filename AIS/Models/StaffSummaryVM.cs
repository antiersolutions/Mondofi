using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AIS.Models
{
    public class StaffSummaryVM
    {
        public UserProfile Server { get; set; }
        public int TablesAssigned { get; set; }
        public int TablesSeated { get; set; }
        public int CoversSeated { get; set; }
    }
}