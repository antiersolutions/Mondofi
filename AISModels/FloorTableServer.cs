using AIS.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace AISModels
{
    [Table("FloorTableServer")]
    public class FloorTableServer
    {
        [Key, ForeignKey("FloorTable")]
        public Int64 FloorTableId { get; set; }
        public virtual FloorTable FloorTable { get; set; }

        public Int64? ServerId { get; set; }
        [ForeignKey("ServerId")]
        public virtual UserProfile Server { get; set; }
    }
}
