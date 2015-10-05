using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AIS.Models
{
    public class RestrictionViewModel
    {
        public int? RestrictionId { get; set; }
        public string Restriction { get; set; }
        public bool Ischecked { get; set; }
    }
}