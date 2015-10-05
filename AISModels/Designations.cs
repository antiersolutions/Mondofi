using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using AIS.Models;

namespace AISModels
{
    [Table("Designations")]
    public class Designations
    {
        [Key]
        public int DesignationId { get; set; }

        [Required]
        [DisplayName("Designation")]
        [Remote("IsExist", "Designation", AdditionalFields = "DesignationId", ErrorMessage = "Designation already exists.")]
        public string Desgination { get; set; }

        public bool IsAssignable { get; set; }

        public virtual ICollection<UserProfile> Staff { get; set; }
    }
}
