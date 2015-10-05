using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Security;


namespace AISModels
{

    [Table("Address")]
    public class Address
    {
        [Key]
        public long AddressId { get; set; }

        [Required]
        [Display(Name = "State")]
        public string State { get; set; }

        [Required]
        [Display(Name = "City")]
        public string City { get; set; }

        [Required]
        [Display(Name = "PostalCode")]
        public string PostalCode { get; set; }


        public long CountryId { get; set; }
        public virtual Country country { get; set; }



    }


}
