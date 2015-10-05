using AISModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AIS.Models
{
    public class FloorPlanVM
    {
        public FloorPlan FloorPlan { get; set; }

        public Int64 SelectedFloorId { get; set; }

        public int Covers { get; set; }
    }
}