using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AIS.Models;

namespace AISModels
{
    [Table("UserPhones")]   
    public class UserPhones
    {
        [Key]
        public Int64 UserPhoneId { get; set; }
        public long UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual UserProfile UserProfile { get; set; }
        public int PhoneTypeId { get; set; }
        [ForeignKey("PhoneTypeId")]
        public virtual PhoneTypes PhoneType { get; set; }

        [Required]
        [DisplayName("Phone Number")]
        //[StringLength(10, MinimumLength = 10, ErrorMessage = " Enter 10 digit number.")]
        public string PhoneNumber { get; set; } 
    }
}
