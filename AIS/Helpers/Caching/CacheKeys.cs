using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AIS.Helpers.Caching
{
    public static class CacheKeys
    {
        #region Reservation keys

        /// <summary>
        /// {0}: DataBase Name
        /// {1}: {0}: DataBase Name
        /// </summary>
        public static string RESERVATION_BY_DATE = "AIS.Reservation.Date.{0}.{1}";
        /// <summary>
        /// {0}: DataBase Name
        /// {1}: Start Date ticks
        /// {2}: Start Date ticks
        /// {3}: Include Deleted
        /// </summary>
        public static string RESERVATION_BY_DATE_RANGE = "AIS.Reservation.Date.{0}.{1}.TO.{2}.Deleted.{3}";
        /// <summary>
        /// [0}: DataBase Name
        /// </summary>
        public static string RESERVATION_BY_DATE_COMPANY_PATTREN = "AIS.Reservation.Date.{0}";
        public static string RESERVATION_BY_DATE_PATTREN = "AIS.Reservation.Date.";
        /// <summary>
        /// {0}: DataBase Name
        /// {1}: Date ticks
        /// {2}: Time
        /// {3}: Filter
        /// {4}: FloorPlanId
        /// {5}: ShiftId
        /// </summary>
        public static string FILTERED_RESERVATION = "AIS.Reservations.Filtered.{0}.{1}.{2}.{3}.{4}.{5}";
        /// <summary>
        /// [0}: DataBase Name
        /// </summary>
        public static string FILTERED_RESERVATION_COMPANY_PATTREN = "AIS.Reservations.Filtered.{0}";
        public static string FILTERED_RESERVATION_PATTREN = "AIS.Reservations.Filtered.";

        /// <summary>
        /// {0}: DataBase Name
        /// {1}: ReservationId
        /// </summary>
        public static string RESERVATION_RIGHT_LIST_ITEM = "AIS.Reservations.Filtered.{0}.List.Item.{1}";

        #endregion

        #region Floor keys

        /// <summary>
        /// {0}: DataBase Name
        /// {1}: Date ticks
        /// {2}: Time
        /// {3}: Shift
        /// {4}: FloorPlanId
        /// </summary>
        public static string FLOOR_TABLES_SCREEN = "AIS.Floor.Tables.{0}.{1}.{2}.{3}.{4}";
        public static string FLOOR_TABLES_ONLY = "AIS.Floor.Tables.OnlyTables";
        /// <summary>
        /// {0}: DataBase Name
        /// {1}: FloorPlanId
        /// </summary>
        public static string FLOOR_PLAN_MAX_COVERS = "AIS.Floor.Tables.{0}.MaxCovers.{1}";
        /// <summary>
        /// {0}: DataBase Name
        /// {1}: FloorPlanId
        /// </summary>
        public static string FLOOR_PLAN_TABLE_DESIGN = "AIS.Floor.Tables.{0}.Design.{1}";
        /// <summary>
        /// {0}: DataBase Name
        /// </summary>
        public static string FLOOR_TABLES_SCREEN_COMPANY_PATTREN = "AIS.Floor.Tables.{0}";
        public static string FLOOR_TABLES_SCREEN_PATTREN = "AIS.Floor.Tables.";

        /// <summary>
        /// [0}: DataBase Name
        /// {1}: Date ticks
        /// </summary>
        public static string FLOOR_TABLES_BLOCK_BY_DATE = "AIS.Floor.Tables.{0}.{1}";

        #endregion

        #region Staff keys
        /// <summary>
        /// {0}: DataBase Name
        /// </summary>
        public static string STAFF_LIST = "AIS.Staff.{0}.StaffList";
        /// <summary>
        /// {0}: DataBase Name
        /// {1}: userId
        /// </summary>
        public static string STAFF_LIST_ITEM = "AIS.Staff.{0}.StaffList.Item.{1}";
        /// <summary>
        /// {0}: DataBase Name
        /// {1}: userId
        /// </summary>
        public static string STAFF_LIST_ITEM_TABLES_CHECKLIST = "AIS.Staff.{0}.StaffList.Item.Tables.Checklist.{1}";
        /// <summary>
        /// {0}: DataBase Name
        /// {1}: userId
        /// </summary>
        public static string STAFF_LIST_ITEM_SECTIONS_CHECKLIST = "AIS.Staff.{0}.StaffList.Item.Sections.Checklist.{1}";
        /// <summary>
        /// {0}: DataBase Name
        /// </summary>
        public static string STAFF_COMPANY_PATTREN = "AIS.Staff.{0}";
        public static string STAFF_PATTREN = "AIS.Staff.";

        #endregion

        #region WeekDays
        /// <summary>
        /// {0}: DataBase Name
        /// </summary>
        public static string WEEKDAYS_COMPANY_PATTERN = "AIS.WeekDays.{0}";
        public static string WEEKDAYS_PATTERN = "AIS.WeekDays";

        #endregion

        #region FoodMenuShifts
        /// <summary>
        /// {0}: DataBase Name
        /// </summary>
        public static string FOOD_MENUSHIFT_COMPANY_PATTERN = "AIS.FoodMenuShifts.{0}";
        public static string FOOD_MENUSHIFT_PATTERN = "AIS.FoodMenuShifts";
       /// <summary>
        /// {0}: DataBase Name
       /// </summary>
        public static string FOOD_MENUSHIFT_SHIFTHOURS = "AIS.FoodMenuShifts.{0}.MenuShiftHours";
        /// <summary>
        /// {0}: DataBase Name
        /// {1}: Day
        /// </summary>
        public static string FOOD_MENUSHIFT_OPEN_CLOSE_DAYTIME = "AIS.FoodMenuShifts.{0}.OpenCloseTime.{1}";

        #endregion

        #region Reservation Status
        /// <summary>
        /// {0}: DataBase Name
        /// </summary>
          public static string RESERVATION_STATUS_COMAPNY_PATTERN = "AIS.Status.Res.{0}";
        public static string RESERVATION_STATUS_PATTERN = "AIS.Status.Res";

        #endregion

        #region waitings
        /// <summary>
        /// {0}: DataBase Name
        /// </summary>
        public static string WAITING_COMPANY_PATTERN = "AIS.Waiting.{0}";
        public static string WAITING_PATTERN = "AIS.Waiting.";
        /// <summary>
        /// {0}: DataBase Name
        /// {1}: DataBase Name
        /// </summary>
        public static string WAITING_BY_DATE = "AIS.Waiting.{0}.Date.{1}";
        public static string WAITING_LIST = "AIS.Waiting.List";
        /// <summary>
        /// {0}: DataBase Name
        /// {1}: watingId
        /// </summary>
        public static string WAITING_RIGHT_LIST_ITEM = "AIS.Waiting.{0}.Right.List.Item.{1}";

        #endregion

        #region message Templates

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : DataBaseName
        /// {1} : template name
       /// </remarks>
        public static string MESSAGETEMPLATES_BY_NAME_KEY = "AIS.messagetemplate.{0}.name-{1}";
        /// <summary>
        /// {0} : DataBaseName
        /// Key pattern to clear cache
        /// </summary>
        public static string MESSAGETEMPLATES_COMPANY_PATTERN_KEY = "AIS.messagetemplate.{0}";
        public static string MESSAGETEMPLATES_PATTERN_KEY = "AIS.messagetemplate.";

        #endregion

        #region Settings
        /// <summary>
        /// {0} : DataBaseName
        /// </summary>
        public static string SETTING_BY_NAME_COMPANY_PATTERN = "AIS.setting.{0}";
        public static string SETTING_BY_NAME_PATTERN = "AIS.setting";
        
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {1} : setting name
        /// {0} : DataBaseName
        /// </remarks>
        public static string SETTING_BY_NAME_KEY = "AIS.setting.{0}.name-{1}";

        #endregion
    }
}