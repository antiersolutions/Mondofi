using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AIS.Models
{
    public class ReservationCustomerVM
    {
        public DateTime? date { get; set; }
        public Int64? shiftId { get; set; }
        public string fname { get; set; }
        public string lname { get; set; }
        public string email { get; set; }
        public string phoneNo { get; set; }
    }
}