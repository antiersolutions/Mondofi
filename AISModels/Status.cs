using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AISModels
{
    [Table("Status")]
    public class Status
    {
        [Key]
        public Int64 StatusId { get; set; }

        public string StatusName { get; set; }
    }


    public static class ReservationStatus
    {                                               /******     ******/                                    /**** old list *****/
        public const int Not_confirmed = 1;        // Not_confirmed = 1;                //All_Arrived = 1;                     
        public const int Confirmed = 2;            // Confirmed = 2;                    //Appetizer = 2;          
        public const int Online_Booking = 3;
        public const int Left_message = 4;         // Left_message = 3;                 //Cancelled = 3;          
        public const int No_answer = 5;            // No_answer = 4;                   //Cancelled_2 = 4;        
        public const int Wrong_Number = 6;         // Wrong_Number = 5;                 //Check_Dropped = 5;      
        public const int Running_Late = 7;         // Running_Late = 6;                 //Check_Paid = 6;         
        public const int Partially_Arrived = 8;    // Partially_Arrived = 7;            //Confirmed = 7;          
        public const int All_Arrived = 9;          // All_Arrived = 8;                  //Entree = 8;             
        public const int Paged = 10;               // Paged = 9;                        //Finished = 9;           
        public const int Partially_Seated = 11;    // Seated = 10;                      //Left_message = 10;      
        public const int Seated = 12;              // Partially_Seated = 11;            //No_answere = 11;        
        public const int Appetizer = 13;           // Appetizer = 12;                   //No_show = 12;           
        public const int Entree = 14;              // Entree = 13;                      //Not_confirmed = 13;     
        public const int Dessert = 15;             // Dessert = 14;                     //Paged = 14;             
        public const int Table_Cleared = 16;       // Table_Cleared = 15;               //Partially_Arrived = 15; 
        public const int Check_Dropped = 17;       // Check_Dropped = 16;               //Partially_Seated = 16;  
        public const int Check_Paid = 18;          // Check_Paid = 17;                  //Running_Late = 17;      
        public const int Finished = 19;            // Finished = 18;                    //Seated = 18;            
        public const int Cancelled = 20;           // Cancelled = 19;                   //Table_Cleared = 19;     
       // public const int Cancelled_2 = 20;       //nt Cancelled_2 = 20;                 //Table_Cleared_11 = 20;   
        public const int No_show = 21;             // No_show = 21;                     //Wrong_Number = 21;      
    };
}
