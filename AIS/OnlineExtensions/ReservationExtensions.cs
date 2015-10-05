using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AIS.Models;
using AISModels;

namespace AIS.OnlineExtensions
{
    public static class ReservationExtensions
    {
        public static int GetMinutesFromDuration(this string duration)
        {
            var minutes = 0;

            if (duration.ToUpper().Contains("MIN") == true && duration.ToUpper().Contains("HR"))
            {
                var newTime = duration.Split(' ');
                var hr = Convert.ToInt32(newTime[0].Replace("HR", ""));
                var min = Convert.ToInt32(newTime[1].Replace("MIN", ""));
                minutes = (hr * 60) + (min);
            }
            else
            {
                if (duration.ToUpper().Contains("MIN"))
                {
                    var newTime = duration.Split(' ');
                    var min = Convert.ToInt32(newTime[0].Replace("MIN", ""));
                    minutes = (min);
                }
                else
                {
                    var newTime = duration.Split(' ');
                    var hr = Convert.ToInt32(newTime[0].Replace("HR", ""));
                    minutes = (hr * 60);
                }
            }

            return minutes;
        }

        public static string AddMinutesToDuration(this string duration, int minutes)
        {
            var durationInMins = duration.GetMinutesFromDuration();
            durationInMins += minutes;

            int min = durationInMins % 60;
            int hr = (durationInMins - min) / 60;

            return string.Format("{0} {1}", (hr > 0 ? hr + "HR" : string.Empty), (min > 0 ? min + "MIN" : string.Empty)).Trim();
        }

        public static void GetCustomerVisits(this Customers customer, out int totalVisits, out int totalReservations, out int walkins, out int noshows, out int cancellations)
        {
            totalVisits = totalReservations = walkins = noshows = cancellations = 0;

            var customerReservations = customer.Reservations.Where(r => !r.IsDeleted).ToList();

            if (customerReservations != null && customerReservations.Count > 0)
            {
                var reservations = customerReservations;
                totalReservations = reservations.Count();
                totalVisits = reservations.Count(r => r.StatusId == ReservationStatus.Finished);
                walkins = customer.Waitings.Count(w => w.ReservationId == 0);
                noshows = reservations.Count(r => r.StatusId == ReservationStatus.No_show);
                cancellations = reservations.Count(r => r.StatusId == ReservationStatus.Cancelled); //  || r.StatusId == ReservationStatus.Cancelled_2
            }
        }

        public static void LogAddReservation(this UsersContext db, Reservation reservation, UserProfile loginUser, UserProfile pinUser)
        {
            string comment;
            var audit = new ReservationAudit();

            if (pinUser != null)
            {
                comment = string.Format(ReservationComments.Added_New_Reservation_UsingPIN,
                    loginUser.FirstName + " " + loginUser.LastName,
                    pinUser.UserCode);

                audit.PinUserId = pinUser.Id;
            }
            else
                comment = string.Format(ReservationComments.Added_New_Reservation,
                    loginUser.FirstName + " " + loginUser.LastName);

            audit.Comment = comment;
            audit.ReservationId = reservation.ReservationId;
            audit.LoginUserId = loginUser.Id;
            audit.CreatedOn = DateTime.UtcNow;
            audit.Action = "Created";
            audit.TimeForm = reservation.TimeForm;
            audit.TableName = reservation.FloorTableId > 0 ? db.tabFloorTables.Find(reservation.FloorTableId).TableName : db.tabMergedFloorTables.Find(reservation.MergedFloorTableId).TableName;
            audit.Covers = reservation.Covers;
            audit.Notes = reservation.ReservationNote ?? string.Empty;
            audit.TimeTo = reservation.TimeTo;
            audit.StatusId = (int)reservation.StatusId.Value;

            db.tabReservationAudits.Add(audit);
        }

        public static void LogEditReservation(this UsersContext db, Reservation reservation, UserProfile loginUser, UserProfile pinUser)
        {
            string comment;
            var audit = new ReservationAudit();

            if (pinUser != null)
            {
                comment = string.Format(ReservationComments.Updated_Reservation_UsingPIN,
                    loginUser.FirstName + " " + loginUser.LastName,
                    pinUser.UserCode);

                audit.PinUserId = pinUser.Id;
            }
            else
                comment = string.Format(ReservationComments.Updated_Reservation,
                    loginUser.FirstName + " " + loginUser.LastName);

            audit.Comment = comment;
            audit.ReservationId = reservation.ReservationId;
            audit.LoginUserId = loginUser.Id;
            audit.CreatedOn = DateTime.UtcNow;
            audit.Action = "Edited";
            audit.TimeForm = reservation.TimeForm;
            audit.TableName = reservation.FloorTableId > 0 ? db.tabFloorTables.Find(reservation.FloorTableId).TableName : db.tabMergedFloorTables.Find(reservation.MergedFloorTableId).TableName;
            audit.Covers = reservation.Covers;
            audit.Notes = reservation.ReservationNote ?? string.Empty;
            audit.TimeTo = reservation.TimeTo;
            audit.StatusId = (int)reservation.StatusId.Value;

            db.tabReservationAudits.Add(audit);
        }

        public static void LogDeleteReservation(this UsersContext db, Reservation reservation, UserProfile loginUser, UserProfile pinUser)
        {
            string comment;
            var audit = new ReservationAudit();

            if (pinUser != null)
            {
                comment = string.Format(ReservationComments.Deleted_Reservation_UsingPIN,
                    loginUser.FirstName + " " + loginUser.LastName,
                    pinUser.UserCode);

                audit.PinUserId = pinUser.Id;
            }
            else
                comment = string.Format(ReservationComments.Deleted_Reservation,
                    loginUser.FirstName + " " + loginUser.LastName);

            audit.Comment = comment;
            audit.ReservationId = reservation.ReservationId;
            audit.LoginUserId = loginUser.Id;
            audit.CreatedOn = DateTime.UtcNow;
            audit.Action = "Deleted";
            audit.TimeForm = reservation.TimeForm;
            audit.TableName = reservation.FloorTableId > 0 ? db.tabFloorTables.Find(reservation.FloorTableId).TableName : db.tabMergedFloorTables.Find(reservation.MergedFloorTableId).TableName;
            audit.Covers = reservation.Covers;
            audit.Notes = reservation.ReservationNote ?? string.Empty;
            audit.TimeTo = reservation.TimeTo;
            audit.StatusId = (int)reservation.StatusId.Value;

            db.tabReservationAudits.Add(audit);
        }
    }
}