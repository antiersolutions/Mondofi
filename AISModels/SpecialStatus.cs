using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace AISModels
{
    [Table("SpecialStatus")]   
    public class SpecialStatus
    {
        [Key]
        public int SpecialStatusId { get; set; }

        [Required]
        [Remote("IsExist", "SpecialStatus", AdditionalFields = "SpecialStatusId", ErrorMessage = "Status already exists.")]
        [StringLength(50, ErrorMessage = "Special Status name must be under 50 characters")]
        public string Status { get; set; }
    }
}
