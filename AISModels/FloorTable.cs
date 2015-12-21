using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AISModels
{
    [Table("FloorTable")]
    public class FloorTable
    {
        [Key]
        public Int64 FloorTableId { get; set; }

        public Int64 FloorPlanId { get; set; }
        public virtual FloorPlan FloorPlan { get; set; }

        public Int64 SectionId { get; set; }
        public virtual Section Section { get; set; }

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

        public bool? IsTemporary { get; set; }

        public bool IsDeleted { get; set; }

        //public bool IsBlocked { get; set; }

        //public DateTime LastBlockedOn { get; set; }

        public DateTime? DeletedOn { get; set; }

        public int  SeatingPriority { get; set; }

        public virtual FloorTableServer FloorTableServer { get; set; }

        public virtual ICollection<Reservation> Reservations { get; set; }

        public virtual ICollection<MergedTableOrigionalTable> MergedTables { get; set; }
    }
}
