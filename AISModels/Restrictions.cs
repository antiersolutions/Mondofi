using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace AISModels
{
    [Table("Restrictions")]
    public class Restrictions
    {   
        [Key]
        public int RestrictionId { get; set; }

        [Required]
        [Remote("IsExist", "Restriction", AdditionalFields = "RestrictionId", ErrorMessage = "Restriction already exists.")]
        [StringLength(50, ErrorMessage = "Restriction name must be under 50 characters")]
        public string Restriction { get; set; }
    }
}
