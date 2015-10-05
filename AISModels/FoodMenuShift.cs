using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISModels
{
    [Table("FoodMenuShift")]    
    public class FoodMenuShift
    {
        [Key]
        public int FoodMenuShiftId { get; set; }
        public string MenuShift { get; set; }

        public virtual ICollection<MenuShiftHours> MenuShiftHours { get; set; }
    }
}
