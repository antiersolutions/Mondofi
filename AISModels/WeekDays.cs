using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISModels
{
    [Table("WeekDays")]
    public class WeekDays
    {
        [Key]
        public int DayId { get; set; }
        public string DayName { get; set; }

        public virtual ICollection<MenuShiftHours> MenuShiftHours { get; set; }
    }
}
