using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;


namespace AISModels
{
    [Table("Allergies")]
    public class Allergies
    {
       [Key]
       public int AllergyId { get; set; }

       [Required]
       [Display(Name = "Allergy")]
       [Remote("IsExist", "Allergy", AdditionalFields = "AllergyId", ErrorMessage = "Allergy already exists.")]
       [StringLength(100, ErrorMessage = "Allergy name must be under 100 characters")]
       public string Allergy{get;set;}
    }
}
