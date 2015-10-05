using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace AIS.Models
{
    public class ReservationVM
    {
        public Int64 ReservationId { get; set; }

        public Int64 FloorPlanId { get; set; }

        public Int64? MergeTableId { get; set; }

        [Required(ErrorMessage = " ")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = " ")]
        public string LastName { get; set; }

        [Required(ErrorMessage = " ")]
        [RegularExpression("^[0-9]{10}$", ErrorMessage = "Please enter a 10 digit phone number.")]
        public string MobileNumber { get; set; }

        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                             @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                             @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$",
                             ErrorMessage = "Email is not valid")]
        public string Email { get; set; }

        public Int32 Covers { get; set; }

        public Int32 ShiftId { get; set; }

        public string time { get; set; }

        public string tableIdd { get; set; }

        public string TablePositionLeft { get; set; }

        public string TablePositionTop { get; set; }

        public DateTime resDate { get; set; }

        public string Status { get; set; }

        public string Duration { get; set; }

        public Int64? EdtTableId { get; set; }

        public Int64 WaitingId { get; set; }

        public string GuestNote { get; set; }

        public string ReservationNote { get; set; }

        public int? PIN { get; set; }

        public bool enableMerging { get; set; }
    }
}