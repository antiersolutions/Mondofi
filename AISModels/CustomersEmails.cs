using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;


namespace AISModels
{
    [Table("CustomersEmails")]    
    public class CustomersEmails
    {
        [Key]
        public int Id { get; set; }
        public Int64? CustomerId { get; set; }
        public virtual Customers Customers { get; set; }

        [Required]
        [DisplayName("Email")]
        public string Email { get; set; }

        public int? EmailTypeId { get; set; }
        public virtual EmailTypes EmailTypes { get; set; } 
    }
}
