using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace AIS.Models
{
    public class CustomerViewModel
    {
        public Int64? CustomerId { get; set; }

        public DateTime DateCreated { get; set; }

        [Required(ErrorMessage = "*")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "*")]
        public string Lastname { get; set; }

        [Required(ErrorMessage = "*")]
        public string PhoneNumbers { get; set; }

        //[Required(ErrorMessage = "*")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Email address is not valid")]
        public string Emails { get; set; }

        public string[] specialList { get; set; }
        public string[] allergyList { get; set; }
        public string[] restrictionList { get; set; }

        [Required(ErrorMessage = "*")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "*")]
        public DateTime Anniversary { get; set; }

        [Required(ErrorMessage = "*")]
        public string Address1 { get; set; }

        [Required(ErrorMessage = "*")]
        public string Address2 { get; set; }

        //[Required(ErrorMessage = "*")]
        public string Notes { get; set; }

        //[Required(ErrorMessage = "*")]
        public string CityName { get; set; }


        public string PhotoPath { get; set; }


    }
}