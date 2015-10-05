using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AISModels
{
    [Table("FloorTimeSetting")]
    public class FloorTimeSetting
    {
        [Key]
        public Int64 FloorPlanId { get; set; }
        public virtual FloorPlan FloorPlan { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string FloorDesign { get; set; }
    }
}
