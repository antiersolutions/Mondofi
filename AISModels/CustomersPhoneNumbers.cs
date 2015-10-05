using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AISModels
{
    [Table("CustomersPhoneNumbers")]    
    public class CustomersPhoneNumbers
    {
        [Key]
        public int Id { get; set; }
        public Int64 CustomerId { get; set; }
        public virtual Customers Customers { get; set; }
        public int PhoneTypeId { get; set; }
        public virtual PhoneTypes PhoneTypes { get; set; }
        
        public string PhoneNumbers { get; set; }
        
    }
}
