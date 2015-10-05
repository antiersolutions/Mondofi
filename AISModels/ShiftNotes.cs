using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace AISModels
{
    [Table("ShiftNotes")]
    public class ShiftNotes
    {
        [Key]
        public Int64 DayShiftNotesId { get; set; }

        [Required(ErrorMessage = "*")]
        public string Notes { get; set; }

        public int? FoodMenuShiftId { get; set; }
        public virtual FoodMenuShift FoodMenuShift { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public string Type { get; set; }
    }
}
