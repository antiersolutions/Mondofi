using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AISModels
{
    [Table("TableAvailability")]
    public class TableAvailability
    {
        [Key]
        public Int64 TableAvailabilityId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = " ")]
        public string StartTime { get; set; }

        [Required(ErrorMessage = " ")]
        public string EndTime { get; set; }

        public int AvailablityStatusId { get; set; }
        public virtual AvailablityStatus AvailablityStatus { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public Int64 CreatedBy { get; set; }

        public Int64? UpdatedBy { get; set; }

        public virtual ICollection<TableAvailabilityFloorTable> TableAvailabilityFloorTables { get; set; }
        public virtual ICollection<TableAvailabilityWeekDay> TableAvailabilityWeekDays { get; set; }
    }

    [Table("TableAvailabilityFloorTable")]
    public class TableAvailabilityFloorTable
    {
        [Key]
        public Int64 TableAvailabilityFloorTableId { get; set; }

        public Int64 TableAvailabilityId { get; set; }
        public Int64 FloorTableId { get; set; }

        public virtual TableAvailability TableAvailability { get; set; }
        public virtual FloorTable FloorTable { get; set; }
    }

    [Table("TableAvailabilityWeekDay")]
    public class TableAvailabilityWeekDay
    {
        [Key]
        public Int64 TableAvailabilityWeekDayId { get; set; }

        public Int64 TableAvailabilityId { get; set; }
        public int DayId { get; set; }

        public virtual TableAvailability TableAvailability { get; set; }
        public virtual WeekDays WeekDays { get; set; }
    }

    [Table("AvailablityStatus")]
    public class AvailablityStatus
    {
        [Key]
        public int AvailablityStatusId { get; set; }
        public string Status { get; set; }
    }
}
