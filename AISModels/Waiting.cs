using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISModels
{
    [Table("Waiting")]
    public class Waiting
    {
        [Key]
        public Int64 WaitingId { get; set; }

        public DateTime WaitingDate { get; set; }

        public Int64 CustomerId { get; set; }
        public virtual Customers Customer { get; set; }

        public int Covers { get; set; }

        public string Notes { get; set; }

        public Int64 ReservationId { get; set; }
        public virtual Reservation Reservation { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
