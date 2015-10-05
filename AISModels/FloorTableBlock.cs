using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace AISModels
{
    [Table("FloorTableBlock")]
    public class FloorTableBlock
    {
        [Key]
        public Guid FloorTableBlockId { get; set; }

        public Int64 FloorTableId { get; set; }

        public DateTime BlockDate { get; set; }

        public DateTime BlockFrom { get; set; }

        public DateTime BlockTo { get; set; }

        public virtual FloorTable FloorTable { get; set; }
    }
}
