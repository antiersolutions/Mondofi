using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISModels
{
    [Table("MenuShiftHours")]
    public class MenuShiftHours
    {
        [Key]
        public int ShiftHourId { get; set; }
        public int DayId { get; set; }
        public virtual WeekDays WeekDays { get; set; }
        public int FoodMenuShiftId { get; set; }
        public virtual FoodMenuShift FoodMenuShift { get; set; }
        public string OpenAt { get; set; }
        public string CloseAt { get; set; }
        public int? IsNext { get; set; }

    }
}
