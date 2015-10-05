using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AISModels
{
    [Table("TempFloorTable")]
    public class TempFloorTable
    {
        [Key]
        [Column("TempFloorTableId")]
        public Int64 FloorTableId { get; set; }

        [Column("TempFloorPlanId")]
        public Int64 FloorPlanId { get; set; }
        [ForeignKey("FloorPlanId")]
        public virtual TempFloorPlan FloorPlan { get; set; }

        public string TableName { get; set; }

        public string HtmlId { get; set; }

        public string Shape { get; set; }

        public string Size { get; set; }

        public int MinCover { get; set; }
        
        public int MaxCover { get; set; }
        
        public int Angle { get; set; }
        
        public string TTop { get; set; }

        public string TRight { get; set; }

        public string TBottom { get; set; }

        public string TLeft { get; set; }

        public string TableDesign { get; set; }
        
        public DateTime CreatedOn { get; set; }
        
        public DateTime UpdatedOn { get; set; }
    }
}
