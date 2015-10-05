using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using AIS.Models;

namespace AISModels
{
    [Table("FloorPlan")]
    public class FloorPlan
    {
        [Key]
        public Int64 FloorPlanId { get; set; }

        public Int64 UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual UserProfile UserProfile { get; set; }

        public string FloorName { get; set; }

        public string PhotoPath { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedOn { get; set; }

        public int? FLevel { get; set; }

        public decimal? BackgroundScale { get; set; }

        public Int64? UpdatedBy { get; set; }
        [ForeignKey("UpdatedBy")]
        public virtual UserProfile EditerProfile { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public virtual ICollection<Section> Sections { get; set; }
        public virtual ICollection<FloorTable> FloorTables { get; set; }
        public virtual ICollection<Reservation> Reservations { get; set; }
    }
}
