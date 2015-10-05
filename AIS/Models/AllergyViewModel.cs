using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AIS.Models
{
    public class AllergyViewModel
    {
        public int? AllergyId { get; set; }
        public string Allergy { get; set; }
        public bool Ischecked { get; set; }
    }
}