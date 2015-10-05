using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AIS.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace AISModels
{
    [Table("TempFloorPlan")]
    public class TempFloorPlan
    {
        [Key]
        [Column("TempFloorPlanId")]
        public Int64 FloorPlanId { get; set; }

        public Int64 UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual UserProfile UserProfile { get; set; }

        public string FloorName { get; set; }

        public string PhotoPath { get; set; }

        public string Guid { get; set; }

        public DateTime CreatedOn { get; set; }

        public virtual ICollection<TempFloorTable> TempFloorTables { get; set; }
    }
}
