using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace AISModels
{
    public class EmailTypes
    {
        [Key]
        public int EmailTypeId { get; set; }
        [Required]
        [DisplayName("Email")]
        public string EmailType { get; set; }
    }
}
