using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISModels
{
    [Table("CustomersRestrictions")]    
    public class CustomersRestrictions
    {
        [Key]
        public int Id { get; set; }

        public Int64? CustomerId { get; set; }
        public virtual Customers Customers { get; set; }

        public int? RestrictionId { get; set; }
        public virtual Restrictions Restrictions { get; set; }  
    }
}
