using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AISModels;

namespace AIS.Models.TableAvailablity
{
    public class TimeAvailablityVM
    {
        public TimeAvailablityVM()
        {
            allTime = new List<DateTime>();
            isReserved = new List<bool>();
        }

        public List<DateTime> allTime { get; set; }
        public List<bool> isReserved { get; set; }
    }

    public class TableAvailNewVM
    {
        public List<Table> Tables { get; set; }
    }

    public class Table
    {
        public FloorTable FloorTable { get; set; }
        public List<AvailStatus> AvailStatus { get; set; }
    }

    public class AvailStatus
    {
        public int shiftId { get; set; }
        public int Status { get; set; }
        public DateTime time { get; set; }
        public Reservation Reservation { get; set; }
        public bool IsResStart { get; set; }
    }

    public class EditAvailVM
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string selectedWeekDays { get; set; }
        public long selectedTables { get; set; }
        public long selectedAvailability { get; set; }
    }

    public class TableAvailabilityFilter
    {
        public TableAvailabilityFilter()
        {
            selectedTables = new List<long>();
            selectedAvailability = new List<long>();
            selectedTableClass = new List<int>();
        }

        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public IList<long> selectedTables { get; set; }
        public IList<long> selectedAvailability { get; set; }
        public IList<int> selectedTableClass { get; set; }
    }
}