using AISModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AIS.Models
{
    public class ReservationListItemVM
    {
        public Reservation Reservation { get; set; }
        public string HTMLString { get; set; }
    }

    public class CachedReservationItemListVM
    {
        public IList<ReservationListItemVM> ReservationListItems { get; set; }
        public int Covers { get; set; }
    }
}