using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AISModels;

namespace AIS.Models
{
    public class TableDesignVM
    {
        public string TableName { get; set; }
        public int MinCover { get; set; }
        public int MaxCover { get; set; }
        public string Shape { get; set; }
        public string Size { get; set; }
        public int Angle { get; set; }
        public Int64 FloorTableId { get; set; }
        public Int64 TempFloorTableId { get; set; }
        public Int64 FloorId { get; set; }
        public string UniqueId { get; set; }
        public string HtmlId { get; set; }
        public bool IsTemp { get; set; }
    }
}