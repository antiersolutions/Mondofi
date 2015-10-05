using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using AIS.Models;

namespace AISModels
{
    [Table("Reservation")]
    public class Reservation
    {
        [Key]
        public Int64 ReservationId { get; set; }

        public DateTime ReservationDate { get; set; }

        public Int64 UserId { get; set; }

        public Int64 CustomerId { get; set; }
        public virtual Customers Customers { get; set; }

        public Int64 FloorPlanId { get; set; }
        public virtual FloorPlan FloorPlan { get; set; }

        public Int32 Covers { get; set; }

        //public Int64 ShiftId { get; set; }
        [Column("ShiftId")]
        public int FoodMenuShiftId { get; set; }
        public virtual FoodMenuShift FoodMenuShift { get; set; }

        public DateTime TimeForm { get; set; }
        public DateTime TimeTo { get; set; }

        public Int64 FloorTableId { get; set; }
        public virtual FloorTable FloorTable { get; set; }

        public Int64 MergedFloorTableId { get; set; }
        [ForeignKey("MergedFloorTableId")]
        public virtual MergedFloorTable MergedFloorTable { get; set; }

        public string TablePositionLeft { get; set; }
        public string TablePositionTop { get; set; }

        public Int64? StatusId { get; set; }
        public virtual Status Status { get; set; }

        public string Duration { get; set; }

        public string ReservationNote { get; set; }

        public DateTime CreatedOn { get; set; }

        public Int64? UpdatedBy { get; set; }
        [ForeignKey("UpdatedBy")]
        public virtual UserProfile UpdatedByUser { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public bool IsDeleted { get; set; }

        public virtual ReservationServer ReservationServer { get; set; }

        public virtual ICollection<ReservationAudit> ReservationAudits { get; set; }
    }
}
