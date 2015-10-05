using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AIS.Models
{
    public class GetReservationsParamVM
    {
        public GetReservationsParamVM()
        {
            IncludeStatusIds = new List<long>();
            ExcludeStatusIds = new List<long>();
        }

        public long? FloorPlanId { get; set; }

        public DateTime Date { get; set; }

        public int? ShiftId { get; set; }

        public string Time { get; set; }

        public string Filter { get; set; }

        public List<long> IncludeStatusIds { get; set; }

        public List<long> ExcludeStatusIds { get; set; }
    }
}