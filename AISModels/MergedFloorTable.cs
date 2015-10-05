using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using AIS.Models;

namespace AISModels
{
    [Table("MergedFloorTable")]
    public class MergedFloorTable
    {
        [Key]
        [Column("MergedFloorTableId")]
        public Int64 FloorTableId { get; set; }

        public Int64 FloorPlanId { get; set; }
        public virtual FloorPlan FloorPlan { get; set; }

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

        public Int64 CreatedBy { get; set; }
        [ForeignKey("CreatedBy")]
        public virtual UserProfile UserProfile { get; set; }

        public virtual ICollection<MergedTableOrigionalTable> OrigionalTables { get; set; }

        public virtual ICollection<Reservation> Reservations { get; set; }

        [NotMapped]
        public bool? IsTemporary { get; set; }
    }


    [Table("MergedTableOrigionalTable")]
    public class MergedTableOrigionalTable
    {
        [Key]
        public Int64 MergedTableOrigionalTableId { get; set; }

        public Int64 MergedFloorTableId { get; set; }
        [ForeignKey("MergedFloorTableId")]
        public virtual MergedFloorTable MergedFloorTable { get; set; }

        public Int64 FloorTableId { get; set; }
        [ForeignKey("FloorTableId")]
        public virtual FloorTable FloorTable { get; set; }
    }
}
