using AIS.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace AISModels
{
    [Table("ReservationAudit")]
    public class ReservationAudit
    {
        [Key]
        public Int64 ReservationAuditId { get; set; }

        public Int64 ReservationId { get; set; }

        public DateTime TimeForm { get; set; }

        public DateTime TimeTo { get; set; }

        public Int32 Covers { get; set; }

        public string TableName { get; set; }

        public int StatusId { get; set; }

        public string Notes { get; set; }

        public string Action { get; set; }

        public string Comment { get; set; }

        public DateTime CreatedOn { get; set; }

        public Int64 LoginUserId { get; set; }

        public Int64? PinUserId { get; set; }

        [ForeignKey("LoginUserId")]
        public virtual UserProfile LoginUser { get; set; }

        [ForeignKey("PinUserId")]
        public virtual UserProfile PinUser { get; set; }

        public virtual Reservation Reservation { get; set; }
    }

    public static class ReservationComments
    {
        /// <summary>
        /// {0}: login username
        /// </summary>
        public static string Added_New_Reservation = "{0} has added a new reservation.";

        /// <summary>
        /// {0}: login username
        /// {1}: pin
        /// </summary>
        public static string Added_New_Reservation_UsingPIN = "{0} has added a new reservation using pin '{1}'";

        /// <summary>
        /// {0}: login username
        /// </summary>
        public static string Updated_Reservation = "{0} has updated a reservation.";

        /// <summary>
        /// {0}: login username
        /// {1}: pin
        /// </summary>
        public static string Updated_Reservation_UsingPIN = "{0} has updated a reservation using pin '{1}'";

        /// <summary>
        /// {0}: login username
        /// </summary>
        public static string Deleted_Reservation = "{0} has deleted a reservation.";

        /// <summary>
        /// {0}: login username
        /// {1}: pin
        /// </summary>
        public static string Deleted_Reservation_UsingPIN = "{0} has deleted a reservation using pin '{1}'";
    }
}
