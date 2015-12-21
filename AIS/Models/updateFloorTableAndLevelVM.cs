using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AIS.Models
{
    public class updateFloorTableVM
    {
        public Int64 floorTableId { get; set; }
        public bool isTemp { get; set; }
    }

    public class SeatingPriorityTable
    {
        public Int64 id { get; set; }
        public int priority { get; set; }
    }

    public class updateFloorPlanLevelVM
    {
        public Int64 floorPlanId { get; set; }
        public int level { get; set; }
    }
}