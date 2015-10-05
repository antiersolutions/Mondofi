using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace AIS.Models
{
    public class WaitingVM
    {
        public Int64 WaitingId { get; set; }

        [Required(ErrorMessage = " ")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = " ")]
        public string LastName { get; set; }

        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                             @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                             @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$",
                             ErrorMessage = "Email is not valid")]
        public string Email { get; set; }

        [Required(ErrorMessage = " ")]
        [RegularExpression("^[0-9]{10}$", ErrorMessage = "Please enter a 10 digit phone number.")]
        public string MobileNumber { get; set; }

        public int Covers { get; set; }

        public DateTime WaitDate { get; set; }

        public string Notes { get; set; }

        public string GuestNote { get; set; }
    }
}