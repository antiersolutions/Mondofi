using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace AISModels
{
    [Table("PhoneTypes")]    
    public class PhoneTypes
    {
        [Key]
        public int PhoneTypeId { get; set; }
     
        [Required]
        [DisplayName("Phone Type")]
        [Remote("IsExist", "PhoneTypes", AdditionalFields = "PhoneTypeId", ErrorMessage = "Phone Type already exists.")]
        [StringLength(20, ErrorMessage = "Phone Type name must be under 20 characters")]
        public string PhoneType { get; set; }
    }
}
