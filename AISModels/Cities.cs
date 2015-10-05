using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.Web.Mvc;

namespace AISModels
{
    [Table("Cities")]
    public class Cities
    {
        [Key]
        public int CityId { get; set; }

        [Required]
        [DisplayName("City")]
        [Remote("IsExist", "City", AdditionalFields = "CityId", ErrorMessage = "City already exists.")]
        [StringLength(50, ErrorMessage = "City name must be under 50 characters")]
        public string City { get; set; }
    }
}
