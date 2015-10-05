using AIS.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace AISModels
{
    [Table("ReservationServer")]
    public class ReservationServer
    {
        [Key, ForeignKey("Reservation")]
        public Int64 ReservationId { get; set; }
        public virtual Reservation Reservation { get; set; }

        public Int64 ServerId { get; set; }
        [ForeignKey("ServerId")]
        public virtual UserProfile Server { get; set; }
    }
}
