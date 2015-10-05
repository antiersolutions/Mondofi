using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AIS.Models
{
    public class CheckListVM
    {
        public string PropertyName { get; set; }
        public string Icon { get; set; }
        public string Group { get; set; }
        public string Name { get; set; }
        public long Value { get; set; }
        public bool IsChecked { get; set; }
        public bool IsDisabled { get; set; }
    }
}