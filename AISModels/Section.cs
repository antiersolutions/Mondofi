using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AISModels
{
    [Table("Section")]
    public class Section
    {
        [Key]
        public Int64 SectionId { get; set; }

        public Int64 FloorPlanId { get; set; }
        public virtual FloorPlan FloorPlan { get; set; }

        public int SLevel { get; set; }

        public string Name { get; set; }

        public string Color { get; set; }

        public virtual ICollection<FloorTable> FloorTable { get; set; }
    }
}
