using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISModels
{

    [Table("CustomerSpecialStatus")]    
    public class CustomerSpecialStatus
    {
        [Key]
        public int Id { get; set; }

        public Int64? CustomerId { get; set; }
        public virtual Customers Customers { get; set; }

        public int? SpecialStatusId { get; set; }
        public virtual SpecialStatus SpecialStatus { get; set; }
    }
}
