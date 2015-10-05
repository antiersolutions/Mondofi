using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace AIS.Models
{
    public class UserRegisterViewModel
    {
        public long? UserId { get; set; }


        [Required(ErrorMessage = " *")]
        public string Password { get; set; }

        [Required(ErrorMessage = " *")]
        [System.ComponentModel.DataAnnotations.Compare("Password")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = " *")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = " *")]
        public string LastName { get; set; }

        [Required(ErrorMessage = " *")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Email address is not valid")]
        [Remote("doesEmailExist", "User", HttpMethod = "POST", AdditionalFields = "InitialEmail", ErrorMessage = "User name already exists. Please enter a different user name.")]
        public string EmailAddress { get; set; }

        public int? DesignationId { get; set; }

        //[Required(ErrorMessage = " *")]
        public string PhotoPath { get; set; }

        [Required(ErrorMessage = " *")]
        public bool Availability { get; set; }

        [Required(ErrorMessage = " *")]
        public bool IsAdmin { get; set; }

        [Required(ErrorMessage = " *")]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumbers { get; set; }

        public string InitialEmail { get; set; }

        public int UserCode { get; set; }

        [Required(ErrorMessage = " *")]
        public bool EnablePIN { get; set; }

        public bool IsPasswordChanged { get; set; }
    }
    public class AlphabeticalMapping<T>
    {
        public string FirstLetter { get; set; }
        public List<T> Items { get; set; }
    }
}