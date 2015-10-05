using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Teamberry.Models.Company
{
    public class CompanyVM
    {
        [Required]
        [Display(Name = "Comapny Name")]
        public string ComapnyName { get; set; }

        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        public bool IsActive { get; set; }

        public bool IsTrial { get; set; }

        [Required]
        public string ContactNo { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.Web.Mvc.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}