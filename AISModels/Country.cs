using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Security;


namespace AISModels
{

    [Table("Country")]
    public class Country
    {
        [Key]
        public long CountryId { get; set; }

        [Required]
        [Display(Name = "Country Name")]
        public string CountryName { get; set; }


    }


}
