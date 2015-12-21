using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AISModels;
using System.Text;
using AIS.Extensions;
using System.Web.Mvc;
using AIS.Models;
using System.Text.RegularExpressions;
using System.Globalization;
using AIS.Helpers;
using System.Drawing;
using AIS.Helpers.Caching;

namespace AIS.OnlineExtensions
{
    public static class FloorExtensions
    {
        private static Random random = new Random();

        public static void AssignSectionColor(this FloorTable table)
        {
            var design = table.TableDesign;

            //if (table.Section == null)
            //{
            //    using (var db = new UsersContext())
            //    {
            //        table.Section = db.tabSections.Find(table.SectionId);
            //    }
            //}

            var section = table.Section;

            if (section != null)
            {
                var sectionColor = section.Color;

                var seats = GetMatchedTagsFromHtml(design, String.Format("<div[^>]*?class=([\"'])[^>]*{0}[^>]*\\1[^>]*>(\r*?\n*?)(.*?)</div>", "seat "));

                foreach (var seat in seats)
                {
                    if (seat.IndexOf("style") > 0)
                    {
                        design = design.Replace(seat, seat.Replace("style=\"", "style=\"background:" + sectionColor + ";"));
                    }
                    else
                    {
                        design = design.Replace(seat, seat.Replace("<div", "<div style=\"background:" + sectionColor + ";\""));
                    }
                }

                table.TableDesign = design;
            }
        }

        public static string GetDullRandomColor()
        {
            return ColorTranslator.ToHtml(Color.FromArgb(random.Next(150, 255), random.Next(150, 255), random.Next(150, 255)));
        }

        public static string GetBrightRandomColor()
        {
            return ColorTranslator.ToHtml(Color.FromArgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)));
        }

        //public static void CheckReservations(this Controller controller, FloorTable table,
        //    DateTime? startTime, DateTime? endTime, int? shiftId, out int coverCount)
        //{
        //    coverCount = 0;
        //    var design = table.TableDesign;

        //    var TableName = GetMatchedTagsFromHtml(design, String.Format("<h3(.*?)>(.*?)</h3>")).FirstOrDefault();

        //    if (!string.IsNullOrEmpty(TableName))
        //    {
        //        IEnumerable<Reservation> reservation = table.Reservations.Where(r => !r.IsDeleted).AsEnumerable();
        //        IEnumerable<Reservation> mergedreservation = table.MergedTables.Select(m => m.MergedFloorTable).SelectMany(mt => mt.Reservations)
        //            .Where(r => !r.IsDeleted).AsEnumerable();

        //        if (!shiftId.HasValue)
        //        {
        //            if (startTime.HasValue)
        //            {
        //                if (startTime.HasValue && endTime.HasValue)
        //                {
        //                    reservation = reservation.Where(r => r.TimeForm <= startTime.Value && r.TimeForm.AddMinutes(r.Duration.GetMinutesFromDuration()) >= endTime.Value).AsEnumerable();
        //                    mergedreservation = mergedreservation.Where(r => r.TimeForm <= startTime.Value && r.TimeForm.AddMinutes(r.Duration.GetMinutesFromDuration()) >= endTime.Value).AsEnumerable();
        //                }
        //                else
        //                {
        //                    var start = startTime.Value.Date.AddTicks(DateTime.UtcNow.ToClientTime().TimeOfDay.Ticks);
        //                    reservation = reservation.Where(r => r.TimeForm <= start && r.TimeForm.AddMinutes(r.Duration.GetMinutesFromDuration()) >= start).AsEnumerable();
        //                    mergedreservation = mergedreservation.Where(r => r.TimeForm <= start && r.TimeForm.AddMinutes(r.Duration.GetMinutesFromDuration()) >= start).AsEnumerable();
        //                }
        //            }
        //            else
        //            {
        //                reservation = reservation.Where(r => r.TimeForm <= DateTime.UtcNow.ToClientTime() && r.TimeForm.AddMinutes(r.Duration.GetMinutesFromDuration()) >= DateTime.UtcNow.ToClientTime()).AsEnumerable();
        //                mergedreservation = mergedreservation.Where(r => r.TimeForm <= DateTime.UtcNow.ToClientTime() && r.TimeForm.AddMinutes(r.Duration.GetMinutesFromDuration()) >= DateTime.UtcNow.ToClientTime()).AsEnumerable();
        //            }
        //        }
        //        else
        //        {
        //            if (startTime.HasValue)
        //            {
        //                reservation = reservation.Where(r => r.ReservationDate == startTime.Value.Date).AsEnumerable();
        //                mergedreservation = mergedreservation.Where(r => r.ReservationDate == startTime.Value.Date).AsEnumerable();
        //            }
        //            else
        //            {
        //                reservation = reservation.Where(r => r.ReservationDate == DateTime.UtcNow.ToClientTime().Date).AsEnumerable();
        //                mergedreservation = mergedreservation.Where(r => r.ReservationDate == DateTime.UtcNow.ToClientTime().Date).AsEnumerable();
        //            }

        //            if (shiftId.Value != 0)
        //            {
        //                reservation = reservation.Where(r => r.FoodMenuShiftId == shiftId.Value).AsEnumerable();
        //                mergedreservation = mergedreservation.Where(r => r.FoodMenuShiftId == shiftId.Value).AsEnumerable();
        //            }
        //        }

        //        // check if status is FINISHED, CANCELLED, CANCELLED2

        //        var rejectedStatus = new List<long>()
        //                             {
        //                                 ReservationStatus.Finished,
        //                                 ReservationStatus.Cancelled
        //                                 //ReservationStatus.Cancelled_2
        //                             };

        //        reservation = reservation.Where(r => !rejectedStatus.Contains(r.StatusId.Value));
        //        mergedreservation = mergedreservation.Where(r => !rejectedStatus.Contains(r.StatusId.Value));

        //        // Ends here

        //        if (reservation.Count() > 0 || mergedreservation.Count() > 0)
        //        {
        //            var allRes = new List<Reservation>();

        //            if (reservation.Count() > 0)
        //                allRes.AddRange(reservation);

        //            if (mergedreservation.Count() > 0)
        //                allRes.AddRange(mergedreservation);

        //            allRes = allRes.OrderBy(r => r.TimeForm).ToList();

        //            //table.TableDesign = design.Replace(TableName, TableName + "<img alt=\"\" src=\"/images/red-s.png\" class=\"table-img\">");

        //            design = design.Replace(TableName, TableName + "<img alt=\"\" src=\"/images/red-s.png\" class=\"table-img\">");

        //            if (allRes.First().FloorTableId > 0)
        //            {
        //                var regex = new Regex(@"([\w-]+)\s*:\s*([^;]+)");
        //                var match = regex.Match(GetMatchedTagsFromHtml(design, String.Format("<div[^>]*?class=([\"'])[^>]*{0}[^>]*\\1[^>]*>", "table-main")).FirstOrDefault());
        //                while (match.Success)
        //                {
        //                    var key = match.Groups[1].Value;
        //                    var value = match.Groups[2].Value;
        //                    if (key == "top")
        //                    {
        //                        design = design.Replace(value, allRes.First().TablePositionTop);
        //                    }

        //                    if (key == "left")
        //                    {
        //                        design = design.Replace(value, allRes.First().TablePositionLeft);
        //                    }

        //                    match = match.NextMatch();
        //                }
        //            }

        //            var popup = controller.RenderPartialViewToString("~/Views/Floor/ReservationListPartial.cshtml", allRes);

        //            StringBuilder designer = new StringBuilder();
        //            designer.Append(design.Substring(0, design.LastIndexOf("</div>")));
        //            designer.Append(popup + "</div>");

        //            table.TableDesign = designer.ToString();

        //            coverCount = allRes.Sum(r => r.Covers);
        //        }
        //        else
        //        {
        //            table.TableDesign = design.Replace(TableName, TableName + "<img alt=\"\" src=\"/images/green-a.png\" class=\"table-img\">");
        //        }
        //    }
        //}

        public static void CheckReservations20141222(this Controller controller, FloorTable table,
            DateTime? startTime, DateTime? endTime, int? shiftId, out int coverCount)
        {
            coverCount = 0;
            var design = table.TableDesign;

            var TableName = GetMatchedTagsFromHtml(design, String.Format("<h3(.*?)>(.*?)</h3>")).FirstOrDefault();

            if (!string.IsNullOrEmpty(TableName))
            {
                IQueryable<Reservation> reservation = table.Reservations.Where(r => !r.IsDeleted).AsQueryable();
                IQueryable<Reservation> mergedreservation = table.MergedTables.Select(m => m.MergedFloorTable).SelectMany(mt => mt.Reservations)
                    .Where(r => !r.IsDeleted).AsQueryable();
                IQueryable<Reservation> upcomingReservation = reservation.AsQueryable();

                if (!shiftId.HasValue)
                {
                    if (startTime.HasValue)
                    {
                        if (startTime.HasValue && endTime.HasValue)
                        {
                            var preStart = startTime.Value.AddMinutes(90);
                            reservation = reservation.Where(r => r.TimeForm <= startTime.Value && r.TimeTo >= endTime.Value);
                            mergedreservation = mergedreservation.Where(r => r.TimeForm <= startTime.Value && r.TimeForm.AddMinutes(r.Duration.GetMinutesFromDuration()) >= endTime.Value);
                            upcomingReservation = upcomingReservation.Where(r => r.TimeForm <= preStart && r.TimeForm >= startTime.Value);
                        }
                        else
                        {
                            var start = startTime.Value.Date.AddTicks(DateTime.UtcNow.ToClientTime().TimeOfDay.Ticks);
                            var preStart = start.AddMinutes(90);
                            reservation = reservation.Where(r => r.TimeForm <= start && r.TimeTo >= start);
                            mergedreservation = mergedreservation.Where(r => r.TimeForm <= start && r.TimeTo >= start);
                            upcomingReservation = upcomingReservation.Where(r => r.TimeForm <= preStart && r.TimeForm >= start);
                        }
                    }
                    else
                    {
                        var now = DateTime.UtcNow.ToClientTime();
                        var preStart = now.AddMinutes(90);
                        reservation = reservation.Where(r => r.TimeForm <= now && r.TimeTo >= now);
                        mergedreservation = mergedreservation.Where(r => r.TimeForm <= now && r.TimeTo >= now);
                        upcomingReservation = upcomingReservation.Where(r => r.TimeForm <= preStart && r.TimeForm >= now);
                    }
                }
                else
                {
                    if (startTime.HasValue)
                    {
                        var preStart = startTime.Value.AddMinutes(90);
                        reservation = reservation.Where(r => r.ReservationDate == startTime.Value.Date);
                        mergedreservation = mergedreservation.Where(r => r.ReservationDate == startTime.Value.Date);
                        upcomingReservation = upcomingReservation.Where(r => r.TimeForm <= preStart && r.TimeForm >= startTime.Value);
                    }
                    else
                    {
                        var now = DateTime.UtcNow.ToClientTime();
                        var preStart = now.AddMinutes(90);
                        reservation = reservation.Where(r => r.ReservationDate == DateTime.UtcNow.ToClientTime().Date);
                        mergedreservation = mergedreservation.Where(r => r.ReservationDate == DateTime.UtcNow.ToClientTime().Date);
                        upcomingReservation = upcomingReservation.Where(r => r.TimeForm <= preStart && r.TimeForm >= now);
                    }

                    if (shiftId.Value != 0)
                    {
                        reservation = reservation.Where(r => r.FoodMenuShiftId == shiftId.Value);
                        mergedreservation = mergedreservation.Where(r => r.FoodMenuShiftId == shiftId.Value);
                    }
                }

                // check if status is FINISHED, CANCELLED, CANCELLED2

                var rejectedStatus = new List<long?>()
                                     {
                                         ReservationStatus.Finished,
                                         ReservationStatus.Cancelled
                                         //ReservationStatus.Cancelled_2
                                     };

                var reservationList = reservation.Where(r => !rejectedStatus.Contains(r.StatusId)).ToList();
                var mergedreservationList = mergedreservation.Where(r => !rejectedStatus.Contains(r.StatusId)).ToList();
                var upcomingReservationList = upcomingReservation.Where(r => !rejectedStatus.Contains(r.StatusId)).ToList();

                // Ends here

                if (reservationList.Count > 0 || mergedreservationList.Count > 0)
                {
                    var allRes = new List<Reservation>();

                    if (reservationList.Count() > 0)
                        allRes.AddRange(reservationList);

                    if (mergedreservationList.Count() > 0)
                        allRes.AddRange(mergedreservationList);

                    allRes = allRes.OrderBy(r => r.TimeForm).ToList();

                    //table.TableDesign = design.Replace(TableName, TableName + "<img alt=\"\" src=\"/images/red-s.png\" class=\"table-img\">");

                    design = design.Replace(TableName, TableName + "<img alt=\"\" src=\"/images/red-s.png\" class=\"table-img\">");

                    if (allRes.First().FloorTableId > 0)
                    {
                        var regex = new Regex(@"([\w-]+)\s*:\s*([^;]+)");
                        var match = regex.Match(GetMatchedTagsFromHtml(design, String.Format("<div[^>]*?class=([\"'])[^>]*{0}[^>]*\\1[^>]*>", "table-main")).FirstOrDefault());
                        while (match.Success)
                        {
                            var key = match.Groups[1].Value;
                            var value = match.Groups[2].Value;
                            if (key == "top")
                            {
                                design = design.Replace(value, allRes.First().TablePositionTop);
                            }

                            if (key == "left")
                            {
                                design = design.Replace(value, allRes.First().TablePositionLeft);
                            }

                            match = match.NextMatch();
                        }
                    }

                    var popup = controller.RenderPartialViewToString("~/Views/Floor/ReservationListPartial.cshtml", allRes);

                    StringBuilder designer = new StringBuilder();
                    designer.Append(design.Substring(0, design.LastIndexOf("</div>")));
                    designer.Append(popup + "</div>");

                    table.TableDesign = designer.ToString();

                    coverCount = allRes.Sum(r => r.Covers);
                }
                else if (upcomingReservationList.Count > 0)
                {
                    design = design.Replace(TableName, TableName + "<img alt=\"\" src=\"/images/upcom-s.png\" class=\"table-img\">");

                    var popup = controller.RenderPartialViewToString("~/Views/Floor/ReservationListPartial.cshtml", upcomingReservationList);

                    StringBuilder designer = new StringBuilder();
                    designer.Append(design.Substring(0, design.LastIndexOf("</div>")));
                    designer.Append(popup + "</div>");

                    table.TableDesign = designer.ToString();
                }
                else
                {
                    table.TableDesign = design.Replace(TableName, TableName + "<img alt=\"\" src=\"/images/green-a.png\" class=\"table-img\">");
                }
            }
        }

        public static void CheckReservations20141222(this Controller controller, IList<Reservation> todayReservations, FloorTable table,
            DateTime? startTime, DateTime? endTime, int? shiftId, out int coverCount)
        {
            coverCount = 0;
            var now = DateTime.UtcNow.ToDefaultTimeZone(controller.User.Identity.GetDatabaseName());
            var design = table.TableDesign;

            var TableName = GetMatchedTagsFromHtml(design, String.Format("<h3(.*?)>(.*?)</h3>")).FirstOrDefault();

            if (!string.IsNullOrEmpty(TableName))
            {
                IList<Reservation> reservation = todayReservations.Where(r => r.FloorTableId == table.FloorTableId).ToList();
                IList<Reservation> mergedreservation = todayReservations.Where(r => r.FloorTableId == 0
                    && r.MergedFloorTable.OrigionalTables.Any(t => t.FloorTableId == table.FloorTableId)).ToList();
                IList<Reservation> upcomingReservation = reservation.ToList();

                if (!shiftId.HasValue)
                {
                    if (startTime.HasValue)
                    {
                        if (startTime.HasValue && endTime.HasValue)
                        {
                            var preStart = startTime.Value.AddMinutes(90);
                            reservation = reservation.Where(r => r.TimeForm <= startTime.Value && r.TimeTo >= endTime.Value).ToList();
                            mergedreservation = mergedreservation.Where(r => r.TimeForm <= startTime.Value && r.TimeForm.AddMinutes(r.Duration.GetMinutesFromDuration()) >= endTime.Value).ToList();
                            upcomingReservation = upcomingReservation.Where(r => r.TimeForm <= preStart && r.TimeForm >= startTime.Value).ToList();
                        }
                        else
                        {
                            var start = startTime.Value.Date.AddTicks(now.TimeOfDay.Ticks);
                            var preStart = start.AddMinutes(90);
                            reservation = reservation.Where(r => r.TimeForm <= start && r.TimeTo >= start).ToList();
                            mergedreservation = mergedreservation.Where(r => r.TimeForm <= start && r.TimeTo >= start).ToList();
                            upcomingReservation = upcomingReservation.Where(r => r.TimeForm <= preStart && r.TimeForm >= start).ToList();
                        }
                    }
                    else
                    {
                        //var now = DateTime.UtcNow.ToClientTime();
                        var preStart = now.AddMinutes(90);
                        reservation = reservation.Where(r => r.TimeForm <= now && r.TimeTo >= now).ToList();
                        mergedreservation = mergedreservation.Where(r => r.TimeForm <= now && r.TimeTo >= now).ToList();
                        upcomingReservation = upcomingReservation.Where(r => r.TimeForm <= preStart && r.TimeForm >= now).ToList();
                    }
                }
                else
                {
                    if (startTime.HasValue)
                    {
                        var preStart = startTime.Value.AddMinutes(90);
                        reservation = reservation.Where(r => r.ReservationDate == startTime.Value.Date).ToList();
                        mergedreservation = mergedreservation.Where(r => r.ReservationDate == startTime.Value.Date).ToList();
                        upcomingReservation = upcomingReservation.Where(r => r.TimeForm <= preStart && r.TimeForm >= startTime.Value).ToList();
                    }
                    else
                    {
                        //var now = DateTime.UtcNow.ToClientTime();
                        var preStart = now.AddMinutes(90);
                        reservation = reservation.Where(r => r.ReservationDate == now.Date).ToList();
                        mergedreservation = mergedreservation.Where(r => r.ReservationDate == now.Date).ToList();
                        upcomingReservation = upcomingReservation.Where(r => r.TimeForm <= preStart && r.TimeForm >= now).ToList();
                    }

                    if (shiftId.Value != 0)
                    {
                        reservation = reservation.Where(r => r.FoodMenuShiftId == shiftId.Value).ToList();
                        mergedreservation = mergedreservation.Where(r => r.FoodMenuShiftId == shiftId.Value).ToList();
                    }
                }

                // check if status is FINISHED, CANCELLED, CANCELLED2

                var rejectedStatus = new List<long?>()
                                     {
                                         ReservationStatus.Finished,
                                         ReservationStatus.Cancelled
                                         //ReservationStatus.Cancelled_2
                                     };

                reservation = reservation.Where(r => !rejectedStatus.Contains(r.StatusId)).ToList();
                mergedreservation = mergedreservation.Where(r => !rejectedStatus.Contains(r.StatusId)).ToList();
                upcomingReservation = upcomingReservation.Where(r => !rejectedStatus.Contains(r.StatusId)).ToList();

                // Ends here

                if (reservation.Count > 0 || mergedreservation.Count > 0)
                {
                    var allRes = new List<Reservation>();

                    if (reservation.Count() > 0)
                        allRes.AddRange(reservation);

                    if (mergedreservation.Count() > 0)
                        allRes.AddRange(mergedreservation);

                    allRes = allRes.OrderBy(r => r.TimeForm).ToList();

                    //table.TableDesign = design.Replace(TableName, TableName + "<img alt=\"\" src=\"/images/red-s.png\" class=\"table-img\">");

                    design = design.Replace(TableName, TableName + "<img alt=\"\" src=\"/images/red-s.png\" class=\"table-img\">");

                    if (allRes.First().FloorTableId > 0)
                    {
                        var regex = new Regex(@"([\w-]+)\s*:\s*([^;]+)");
                        var match = regex.Match(GetMatchedTagsFromHtml(design, String.Format("<div[^>]*?class=([\"'])[^>]*{0}[^>]*\\1[^>]*>", "table-main")).FirstOrDefault());
                        while (match.Success)
                        {
                            var key = match.Groups[1].Value;
                            var value = match.Groups[2].Value;
                            if (key == "top")
                            {
                                design = design.Replace(value, allRes.First().TablePositionTop);
                            }

                            if (key == "left")
                            {
                                design = design.Replace(value, allRes.First().TablePositionLeft);
                            }

                            match = match.NextMatch();
                        }
                    }

                    var popup = controller.RenderPartialViewToString("~/Views/Floor/ReservationListPartial.cshtml", allRes);

                    StringBuilder designer = new StringBuilder();
                    designer.Append(design.Substring(0, design.LastIndexOf("</div>")));
                    designer.Append(popup + "</div>");

                    table.TableDesign = designer.ToString();

                    coverCount = allRes.Sum(r => r.Covers);
                }
                else if (upcomingReservation.Count > 0)
                {
                    design = design.Replace(TableName, TableName + "<img alt=\"\" src=\"/images/upcom-s.png\" class=\"table-img\">");

                    var popup = controller.RenderPartialViewToString("~/Views/Floor/ReservationListPartial.cshtml", upcomingReservation);

                    StringBuilder designer = new StringBuilder();
                    designer.Append(design.Substring(0, design.LastIndexOf("</div>")));
                    designer.Append(popup + "</div>");

                    table.TableDesign = designer.ToString();
                }
                else
                {
                    table.TableDesign = design.Replace(TableName, TableName + "<img alt=\"\" src=\"/images/green-a.png\" class=\"table-img\">");
                }
            }
        }

        //public static void CheckReservations20150310(this Controller controller, IList<TableAvailability> availList, IList<FloorTableBlock> blockList,
        //    FloorTable table, string duration, DateTime? startTime, DateTime? endTime, int? shiftId, out int coverCount)
        //{
        //    coverCount = 0;
        //    var design = table.TableDesign;

        //    var TableName = GetMatchedTagsFromHtml(design, String.Format("<h3(.*?)>(.*?)</h3>")).FirstOrDefault();

        //    if (!string.IsNullOrEmpty(TableName))
        //    {
        //        IQueryable<Reservation> reservation = table.Reservations.Where(r => !r.IsDeleted).AsQueryable();
        //        IQueryable<Reservation> mergedreservation = table.MergedTables.Select(m => m.MergedFloorTable).SelectMany(mt => mt.Reservations)
        //            .Where(r => !r.IsDeleted).AsQueryable();
        //        IQueryable<Reservation> upcomingReservation = reservation.AsQueryable();
        //        IQueryable<Reservation> upcomingMergedreservation = mergedreservation.AsQueryable();

        //        if (!shiftId.HasValue)
        //        {
        //            if (startTime.HasValue)
        //            {
        //                if (startTime.HasValue && endTime.HasValue)
        //                {
        //                    var preStart = startTime.Value.AddMinutes(90);
        //                    reservation = reservation.Where(r => r.TimeForm <= startTime.Value && r.TimeTo >= endTime.Value);
        //                    mergedreservation = mergedreservation.Where(r => r.TimeForm <= startTime.Value && r.TimeForm.AddMinutes(r.Duration.GetMinutesFromDuration()) >= endTime.Value);
        //                    upcomingReservation = upcomingReservation.Where(r => r.TimeForm <= preStart && r.TimeForm >= startTime.Value);
        //                    upcomingMergedreservation = upcomingMergedreservation.Where(r => r.TimeForm <= preStart && r.TimeForm >= startTime.Value);
        //                }
        //                else
        //                {
        //                    var start = startTime.Value.Date.AddTicks(DateTime.UtcNow.ToClientTime().TimeOfDay.Ticks);
        //                    var preStart = start.AddMinutes(90);
        //                    reservation = reservation.Where(r => r.TimeForm <= start && r.TimeTo >= start);
        //                    mergedreservation = mergedreservation.Where(r => r.TimeForm <= start && r.TimeTo >= start);
        //                    upcomingReservation = upcomingReservation.Where(r => r.TimeForm <= preStart && r.TimeForm >= start);
        //                    upcomingMergedreservation = upcomingMergedreservation.Where(r => r.TimeForm <= preStart && r.TimeForm >= start);
        //                }
        //            }
        //            else
        //            {
        //                var now = DateTime.UtcNow.ToClientTime();
        //                var preStart = now.AddMinutes(90);
        //                reservation = reservation.Where(r => r.TimeForm <= now && r.TimeTo >= now);
        //                mergedreservation = mergedreservation.Where(r => r.TimeForm <= now && r.TimeTo >= now);
        //                upcomingReservation = upcomingReservation.Where(r => r.TimeForm <= preStart && r.TimeForm >= now);
        //                upcomingMergedreservation = upcomingMergedreservation.Where(r => r.TimeForm <= preStart && r.TimeForm >= now);
        //            }
        //        }
        //        else
        //        {
        //            if (startTime.HasValue)
        //            {
        //                var preStart = startTime.Value.AddMinutes(90);
        //                reservation = reservation.Where(r => r.ReservationDate == startTime.Value.Date);
        //                mergedreservation = mergedreservation.Where(r => r.ReservationDate == startTime.Value.Date);
        //                upcomingReservation = upcomingReservation.Where(r => r.TimeForm <= preStart && r.TimeForm >= startTime.Value);
        //                upcomingMergedreservation = upcomingMergedreservation.Where(r => r.TimeForm <= preStart && r.TimeForm >= startTime.Value);
        //            }
        //            else
        //            {
        //                var now = DateTime.UtcNow.ToClientTime();
        //                var preStart = now.AddMinutes(90);
        //                reservation = reservation.Where(r => r.ReservationDate == DateTime.UtcNow.ToClientTime().Date);
        //                mergedreservation = mergedreservation.Where(r => r.ReservationDate == DateTime.UtcNow.ToClientTime().Date);
        //                upcomingReservation = upcomingReservation.Where(r => r.TimeForm <= preStart && r.TimeForm >= now);
        //                upcomingMergedreservation = upcomingMergedreservation.Where(r => r.TimeForm <= preStart && r.TimeForm >= now);
        //            }

        //            if (shiftId.Value != 0)
        //            {
        //                reservation = reservation.Where(r => r.FoodMenuShiftId == shiftId.Value);
        //                mergedreservation = mergedreservation.Where(r => r.FoodMenuShiftId == shiftId.Value);
        //            }
        //        }

        //        // check if status is FINISHED, CANCELLED, CANCELLED2

        //        var rejectedStatus = new List<long?>()
        //                             {
        //                                 ReservationStatus.Finished,
        //                                 ReservationStatus.Cancelled
        //                                 //ReservationStatus.Cancelled_2
        //                             };

        //        var reservationList = reservation.Where(r => !rejectedStatus.Contains(r.StatusId)).ToList();
        //        var mergedreservationList = mergedreservation.Where(r => !rejectedStatus.Contains(r.StatusId)).ToList();
        //        var upcomingReservationList = upcomingReservation.Where(r => !rejectedStatus.Contains(r.StatusId)).ToList();
        //        var upcomingMergedReservationList = upcomingMergedreservation.Where(r => !rejectedStatus.Contains(r.StatusId)).ToList();
        //        upcomingReservationList.AddRange(upcomingMergedReservationList);

        //        // Ends here

        //        if (reservationList.Count > 0 || mergedreservationList.Count > 0)
        //        {
        //            var allRes = new List<Reservation>();

        //            if (reservationList.Count() > 0)
        //                allRes.AddRange(reservationList);

        //            if (mergedreservationList.Count() > 0)
        //                allRes.AddRange(mergedreservationList);

        //            allRes = allRes.OrderBy(r => r.TimeForm).ToList();

        //            //table.TableDesign = design.Replace(TableName, TableName + "<img alt=\"\" src=\"/images/red-s.png\" class=\"table-img\">");

        //            design = design.Replace(TableName, TableName + "<img alt=\"\" src=\"/images/red-s.png\" class=\"table-img\">");

        //            if (allRes.First().FloorTableId > 0)
        //            {
        //                var regex = new Regex(@"([\w-]+)\s*:\s*([^;]+)");
        //                var match = regex.Match(GetMatchedTagsFromHtml(design, String.Format("<div[^>]*?class=([\"'])[^>]*{0}[^>]*\\1[^>]*>", "table-main")).FirstOrDefault());
        //                while (match.Success)
        //                {
        //                    var key = match.Groups[1].Value;
        //                    var value = match.Groups[2].Value;
        //                    if (key == "top")
        //                    {
        //                        design = design.Replace(value, allRes.First().TablePositionTop);
        //                    }

        //                    if (key == "left")
        //                    {
        //                        design = design.Replace(value, allRes.First().TablePositionLeft);
        //                    }

        //                    match = match.NextMatch();
        //                }
        //            }

        //            var popup = controller.RenderPartialViewToString("~/Views/Floor/ReservationListPartial.cshtml", allRes);

        //            StringBuilder designer = new StringBuilder();
        //            designer.Append(design.Substring(0, design.LastIndexOf("</div>")));
        //            designer.Append(popup + "</div>");

        //            table.TableDesign = designer.ToString();

        //            coverCount = allRes.Sum(r => r.Covers);
        //        }
        //        else if (upcomingReservationList.Count > 0)
        //        {
        //            design = design.Replace(TableName, TableName + "<img alt=\"\" src=\"/images/upcom-s.png\" class=\"table-img\">");

        //            var popup = controller.RenderPartialViewToString("~/Views/Floor/ReservationListPartial.cshtml", upcomingReservationList);

        //            StringBuilder designer = new StringBuilder();
        //            designer.Append(design.Substring(0, design.LastIndexOf("</div>")));
        //            designer.Append(popup + "</div>");

        //            table.TableDesign = designer.ToString();
        //        }
        //        else
        //        {
        //            if (endTime.HasValue && !string.IsNullOrWhiteSpace(duration))
        //                endTime = startTime.Value.AddMinutes(duration.GetMinutesFromDuration());

        //            if (startTime.HasValue && endTime.HasValue && (availList.CheckAvailStatus(startTime.Value.Date, startTime.Value, endTime.Value, table, 2)
        //                    || blockList.IsTableBlocked(table.FloorTableId, startTime.Value, endTime.Value)))
        //                table.TableDesign = design.Replace(TableName, TableName + "<img alt=\"\" src=\"/images/free-s.png\" class=\"table-img\">");
        //            else
        //                table.TableDesign = design.Replace(TableName, TableName + "<img alt=\"\" src=\"/images/green-a.png\" class=\"table-img\">");
        //        }
        //    }
        //}

        public static void CheckReservations20150622(this Controller controller, IList<Reservation> todayReservations, IList<TableAvailability> availList, IList<FloorTableBlock> blockList,
            FloorTable table, string duration, DateTime? startTime, DateTime? endTime, int? shiftId, out int coverCount)
        {
            coverCount = 0;
            var deafultTimeZoneTime = DateTime.UtcNow.ToDefaultTimeZone(controller.User.Identity.GetDatabaseName());
            var design = table.TableDesign;

            var TableName = GetMatchedTagsFromHtml(design, String.Format("<h3(.*?)>(.*?)</h3>")).FirstOrDefault();

            if (!string.IsNullOrEmpty(TableName))
            {
                IList<Reservation> reservation = todayReservations.Where(r => r.FloorTableId == table.FloorTableId).ToList();
                IList<Reservation> mergedreservation = todayReservations.Where(r => r.FloorTableId == 0
                    && r.MergedFloorTable.OrigionalTables.Any(t => t.FloorTableId == table.FloorTableId)).ToList();
                IList<Reservation> upcomingReservation = reservation.ToList();
                IList<Reservation> upcomingMergedreservation = mergedreservation.ToList();

                if (!shiftId.HasValue)
                {
                    if (startTime.HasValue)
                    {
                        if (startTime.HasValue && endTime.HasValue)
                        {
                            var preStart = startTime.Value.AddMinutes(90);
                            reservation = reservation.Where(r => r.TimeForm <= startTime.Value && r.TimeTo >= endTime.Value).ToList();
                            mergedreservation = mergedreservation.Where(r => r.TimeForm <= startTime.Value && r.TimeForm.AddMinutes(r.Duration.GetMinutesFromDuration()) >= endTime.Value).ToList();
                            upcomingReservation = upcomingReservation.Where(r => r.TimeForm <= preStart && r.TimeForm >= startTime.Value).ToList();
                            upcomingMergedreservation = upcomingMergedreservation.Where(r => r.TimeForm <= preStart && r.TimeForm >= startTime.Value).ToList();
                        }
                        else
                        {
                            var start = startTime.Value.Date.AddTicks(deafultTimeZoneTime.TimeOfDay.Ticks);
                            var preStart = start.AddMinutes(90);
                            reservation = reservation.Where(r => r.TimeForm <= start && r.TimeTo >= start).ToList();
                            mergedreservation = mergedreservation.Where(r => r.TimeForm <= start && r.TimeTo >= start).ToList();
                            upcomingReservation = upcomingReservation.Where(r => r.TimeForm <= preStart && r.TimeForm >= start).ToList();
                            upcomingMergedreservation = upcomingMergedreservation.Where(r => r.TimeForm <= preStart && r.TimeForm >= start).ToList();
                        }
                    }
                    else
                    {
                        var now = deafultTimeZoneTime;
                        var preStart = now.AddMinutes(90);
                        reservation = reservation.Where(r => r.TimeForm <= now && r.TimeTo >= now).ToList();
                        mergedreservation = mergedreservation.Where(r => r.TimeForm <= now && r.TimeTo >= now).ToList();
                        upcomingReservation = upcomingReservation.Where(r => r.TimeForm <= preStart && r.TimeForm >= now).ToList();
                        upcomingMergedreservation = upcomingMergedreservation.Where(r => r.TimeForm <= preStart && r.TimeForm >= now).ToList();
                    }
                }
                else
                {
                    if (startTime.HasValue)
                    {
                        var preStart = startTime.Value.AddMinutes(90);
                        reservation = reservation.Where(r => r.ReservationDate == startTime.Value.Date).ToList();
                        mergedreservation = mergedreservation.Where(r => r.ReservationDate == startTime.Value.Date).ToList();
                        upcomingReservation = upcomingReservation.Where(r => r.TimeForm <= preStart && r.TimeForm >= startTime.Value).ToList();
                        upcomingMergedreservation = upcomingMergedreservation.Where(r => r.TimeForm <= preStart && r.TimeForm >= startTime.Value).ToList();
                    }
                    else
                    {
                        var now = deafultTimeZoneTime;
                        var preStart = now.AddMinutes(90);
                        reservation = reservation.Where(r => r.ReservationDate == now.Date).ToList();
                        mergedreservation = mergedreservation.Where(r => r.ReservationDate == now.Date).ToList();
                        upcomingReservation = upcomingReservation.Where(r => r.TimeForm <= preStart && r.TimeForm >= now).ToList();
                        upcomingMergedreservation = upcomingMergedreservation.Where(r => r.TimeForm <= preStart && r.TimeForm >= now).ToList();
                    }

                    if (shiftId.Value != 0)
                    {
                        reservation = reservation.Where(r => r.FoodMenuShiftId == shiftId.Value).ToList();
                        mergedreservation = mergedreservation.Where(r => r.FoodMenuShiftId == shiftId.Value).ToList();
                    }
                }

                // check if status is FINISHED, CANCELLED, CANCELLED2

                var rejectedStatus = new List<long?>()
                                     {
                                         ReservationStatus.Finished,
                                         ReservationStatus.Cancelled
                                         //ReservationStatus.Cancelled_2
                                     };

                var reservationList = reservation.Where(r => !rejectedStatus.Contains(r.StatusId)).ToList();
                var mergedreservationList = mergedreservation.Where(r => !rejectedStatus.Contains(r.StatusId)).ToList();
                var upcomingReservationList = upcomingReservation.Where(r => !rejectedStatus.Contains(r.StatusId)).ToList();
                var upcomingMergedReservationList = upcomingMergedreservation.Where(r => !rejectedStatus.Contains(r.StatusId)).ToList();
                upcomingReservationList.AddRange(upcomingMergedReservationList);

                // Ends here

                if (reservationList.Count > 0 || mergedreservationList.Count > 0)
                {
                    var allRes = new List<Reservation>();

                    if (reservationList.Count() > 0)
                        allRes.AddRange(reservationList);

                    if (mergedreservationList.Count() > 0)
                        allRes.AddRange(mergedreservationList);

                    allRes = allRes.OrderBy(r => r.TimeForm).ToList();

                    //table.TableDesign = design.Replace(TableName, TableName + "<img alt=\"\" src=\"/images/red-s.png\" class=\"table-img\">");

                    design = design.Replace(TableName, TableName + "<img alt=\"\" src=\"/images/red-s.png\" class=\"table-img\">");

                    if (allRes.First().FloorTableId > 0)
                    {
                        var regex = new Regex(@"([\w-]+)\s*:\s*([^;]+)");
                        var match = regex.Match(GetMatchedTagsFromHtml(design, String.Format("<div[^>]*?class=([\"'])[^>]*{0}[^>]*\\1[^>]*>", "table-main")).FirstOrDefault());
                        while (match.Success)
                        {
                            var key = match.Groups[1].Value;
                            var value = match.Groups[2].Value;
                            if (key == "top")
                            {
                                design = design.Replace(value, allRes.First().TablePositionTop);
                            }

                            if (key == "left")
                            {
                                design = design.Replace(value, allRes.First().TablePositionLeft);
                            }

                            match = match.NextMatch();
                        }
                    }

                    var popup = controller.RenderPartialViewToString("~/Views/Floor/ReservationListPartial.cshtml", allRes);

                    StringBuilder designer = new StringBuilder();
                    designer.Append(design.Substring(0, design.LastIndexOf("</div>")));
                    designer.Append(popup + "</div>");

                    table.TableDesign = designer.ToString();

                    coverCount = allRes.Sum(r => r.Covers);
                }
                else if (upcomingReservationList.Count > 0)
                {
                    design = design.Replace(TableName, TableName + "<img alt=\"\" src=\"/images/upcom-s.png\" class=\"table-img\">");

                    var popup = controller.RenderPartialViewToString("~/Views/Floor/ReservationListPartial.cshtml", upcomingReservationList);

                    StringBuilder designer = new StringBuilder();
                    designer.Append(design.Substring(0, design.LastIndexOf("</div>")));
                    designer.Append(popup + "</div>");

                    table.TableDesign = designer.ToString();
                }
                else
                {
                    if (endTime.HasValue && !string.IsNullOrWhiteSpace(duration))
                        endTime = startTime.Value.AddMinutes(duration.GetMinutesFromDuration());

                    if (startTime.HasValue && endTime.HasValue && (availList.CheckAvailStatus(startTime.Value.Date, startTime.Value, endTime.Value, table, 2)
                            || blockList.IsTableBlocked(table.FloorTableId, startTime.Value, endTime.Value)))
                        table.TableDesign = design.Replace(TableName, TableName + "<img alt=\"\" src=\"/images/free-s.png\" class=\"table-img\">");
                    else
                        table.TableDesign = design.Replace(TableName, TableName + "<img alt=\"\" src=\"/images/green-a.png\" class=\"table-img\">");
                }
            }
        }

        public static void AttachFloorItemHtmlDesign(this Controller controller, FloorPlan plan, ICacheManager cache)
        {
            var floorItems = plan.FloorTables;

            foreach (var item in floorItems)
            {
                item.TableDesign = cache.Get<string>(string.Format(CacheKeys.FLOOR_PLAN_TABLE_DESIGN, controller.User.Identity.GetDatabaseName(), item.FloorTableId), () =>
                {
                    return controller.RenderPartialViewToString("~/Views/Floor/GetFloorItemTemplate.cshtml", item);
                });
            }
        }

        //public static void CheckForMergedTable(this UsersContext db, Controller controller,
        //    ICollection<FloorTable> tables, Int64 floorId, DateTime? startTime, DateTime? endTime)
        //{
        //    DateTime date = (startTime.HasValue ? startTime.Value.Date : DateTime.UtcNow.ToClientTime().Date);

        //    IEnumerable<Reservation> reservations = db.tabReservations.Where(r => !r.IsDeleted && r.FloorPlanId == floorId && r.ReservationDate == date).AsEnumerable();

        //    if (startTime.HasValue)
        //    {
        //        if (startTime.HasValue && endTime.HasValue)
        //        {
        //            reservations = reservations.Where(r => r.TimeForm <= startTime.Value && r.TimeForm.AddMinutes(r.Duration.GetMinutesFromDuration()) >= endTime.Value).AsEnumerable();
        //        }
        //        else
        //        {
        //            var start = startTime.Value.Date.AddTicks(DateTime.UtcNow.ToClientTime().TimeOfDay.Ticks);
        //            reservations = reservations.Where(r => r.TimeForm <= start && r.TimeForm.AddMinutes(r.Duration.GetMinutesFromDuration()) >= start).AsEnumerable();
        //        }
        //    }
        //    else
        //    {
        //        reservations = reservations.Where(r => r.TimeForm <= DateTime.UtcNow.ToClientTime() && r.TimeForm.AddMinutes(r.Duration.GetMinutesFromDuration()) >= DateTime.UtcNow.ToClientTime()).AsEnumerable();
        //    }

        //    if (reservations != null && reservations.Count() > 0)
        //    {
        //        foreach (var res in reservations.ToList())
        //        {
        //            if (res.FloorTableId == 0 && res.MergedFloorTableId > 0)
        //            {
        //                var randomColor = GetDullRandomColor();

        //                foreach (var tbl in res.MergedFloorTable.OrigionalTables)
        //                {
        //                    var tble = tables.SingleOrDefault(t => t.FloorTableId == tbl.FloorTable.FloorTableId);
        //                    var TableName = GetMatchedTagsFromHtml(tble.TableDesign, String.Format("<h3(.*?)>(.*?)</h3>")).FirstOrDefault();

        //                    var match = GetMatchedTagsFromHtml(tble.TableDesign, String.Format("<div[^>]*?class=([\"'])[^>]*{0}[^>]*\\1[^>]*>", "(quan-2-other1|quan-2-1|quan-4-1)")).First();

        //                    //tble.TableDesign = tble.TableDesign.Replace(match, match.Replace("class=\"", "class=\"isMerged "));

        //                    if (match.IndexOf("style") > 0)
        //                    {
        //                        tble.TableDesign = tble.TableDesign.Replace(match, match.Replace("class=\"", "class=\"isMerged ").Replace("style=\"", "style=\"background:" + randomColor + " !important; "));
        //                    }
        //                    else
        //                    {
        //                        tble.TableDesign = tble.TableDesign.Replace(match, match.Replace("class=\"", "class=\"isMerged ").Replace("<div", "<div style=\"background:" + randomColor + " !important;\""));
        //                    }

        //                    tble.TableDesign = tble.TableDesign.Replace(TableName, TableName + "<img alt=\"\" src=\"/images/red-s.png\" class=\"table-img\">");
        //                }

        //                //var floorTbl = new FloorTable();
        //                //CopyHelper.Copy(typeof(MergedFloorTable), res.MergedFloorTable, typeof(FloorTable), floorTbl);

        //                //floorTbl.TableDesign = controller.RenderPartialViewToString("~/Views/Floor/GetFloorItemTemplate.cshtml", floorTbl);
        //                //floorTbl.Reservations = new List<Reservation>();
        //                //res.FloorTable = floorTbl;
        //                //floorTbl.Reservations.Add(res);

        //                //tables.Add(floorTbl);
        //            }
        //        }
        //    }
        //}

        public static void CheckForMergedTable20141222(this UsersContext db, Controller controller,
            ICollection<FloorTable> tables, Int64 floorId, DateTime? startTime, DateTime? endTime)
        {
            var defaultTimeZoneTime = DateTime.UtcNow.ToDefaultTimeZone(db.Database.Connection.Database);
            //DateTime date = (startTime.HasValue ? startTime.Value.Date : DateTime.UtcNow.ToClientTime().Date);
            DateTime date = (startTime.HasValue ? startTime.Value.Date : defaultTimeZoneTime.Date);

            IQueryable<Reservation> reservations = db.tabReservations.Where(r => !r.IsDeleted && r.FloorPlanId == floorId && r.ReservationDate == date).AsQueryable();

            if (startTime.HasValue)
            {
                if (startTime.HasValue && endTime.HasValue)
                {
                    reservations = reservations.Where(r => r.TimeForm <= startTime.Value && r.TimeTo >= endTime.Value);
                }
                else
                {
                    //var start = startTime.Value.Date.AddTicks(DateTime.UtcNow.ToClientTime().TimeOfDay.Ticks);
                    var start = startTime.Value.Date.AddTicks(defaultTimeZoneTime.TimeOfDay.Ticks);
                    reservations = reservations.Where(r => r.TimeForm <= start && r.TimeTo >= start);
                }
            }
            else
            {
                //reservations = reservations.Where(r => r.TimeForm <= DateTime.UtcNow.ToClientTime() && r.TimeTo >= DateTime.UtcNow.ToClientTime());
                reservations = reservations.Where(r => r.TimeForm <= defaultTimeZoneTime && r.TimeTo >= defaultTimeZoneTime);
            }

            // check if status is FINISHED, CANCELLED, CANCELLED2

            var rejectedStatus = new List<long?>()
                                     {
                                         ReservationStatus.Finished,
                                         ReservationStatus.Cancelled
                                         //ReservationStatus.Cancelled_2
                                     };

            reservations = reservations.Where(r => !rejectedStatus.Contains(r.StatusId));

            var reservationList = reservations.ToList();

            if (reservationList != null && reservationList.Count > 0)
            {
                foreach (var res in reservationList)
                {
                    if (res.FloorTableId == 0 && res.MergedFloorTableId > 0)
                    {
                        var randomColor = GetDullRandomColor();

                        foreach (var tbl in res.MergedFloorTable.OrigionalTables)
                        {
                            var tble = tables.SingleOrDefault(t => t.FloorTableId == tbl.FloorTable.FloorTableId);
                            var TableName = GetMatchedTagsFromHtml(tble.TableDesign, String.Format("<h3(.*?)>(.*?)</h3>")).FirstOrDefault();

                            var match = GetMatchedTagsFromHtml(tble.TableDesign, String.Format("<div[^>]*?class=([\"'])[^>]*{0}[^>]*\\1[^>]*>", "(quan-2-other1|quan-2-1|quan-4-1)")).First();

                            //tble.TableDesign = tble.TableDesign.Replace(match, match.Replace("class=\"", "class=\"isMerged "));

                            if (match.IndexOf("style") > 0)
                            {
                                tble.TableDesign = tble.TableDesign.Replace(match, match.Replace("class=\"", "class=\"isMerged ").Replace("style=\"", "style=\"background:" + randomColor + " !important; "));
                            }
                            else
                            {
                                tble.TableDesign = tble.TableDesign.Replace(match, match.Replace("class=\"", "class=\"isMerged ").Replace("<div", "<div style=\"background:" + randomColor + " !important;\""));
                            }

                            tble.TableDesign = tble.TableDesign.Replace(TableName, TableName + "<img alt=\"\" src=\"/images/red-s.png\" class=\"table-img\">");
                        }

                        //var floorTbl = new FloorTable();
                        //CopyHelper.Copy(typeof(MergedFloorTable), res.MergedFloorTable, typeof(FloorTable), floorTbl);

                        //floorTbl.TableDesign = controller.RenderPartialViewToString("~/Views/Floor/GetFloorItemTemplate.cshtml", floorTbl);
                        //floorTbl.Reservations = new List<Reservation>();
                        //res.FloorTable = floorTbl;
                        //floorTbl.Reservations.Add(res);

                        //tables.Add(floorTbl);
                    }
                }
            }
        }

        public static IEnumerable<string> GetMatchedTagsFromHtml(string sourceString, string subString) //((<div.*>)(.*)(<\\/div>))* -> Regex for Div tags
        {
            return System.Text.RegularExpressions.Regex.Matches(sourceString, subString).Cast<System.Text.RegularExpressions.Match>().Select(m => m.Value);
        }

        public static void GetFloorItemCount(this IEnumerable<FloorTable> tables, Int64 floorPlanId,
            out int totalTables, out int totalMinCovers, out int totalMaxCovers)
        {
            totalTables = totalMaxCovers = totalMinCovers = 0;

            var array = new string[] { "Sofa", "Chair", "SofaTable", "Wall", "SolidWall", "GlassWall", "BarTable", "Fence", "Pillar" };

            var selectedTables = tables.Where(t => t.IsDeleted == false).AsEnumerable()
                .Where(p => !array.Contains(p.TableName.Split('-')[0]) && p.FloorPlanId == floorPlanId);
            totalTables = selectedTables.Count();
            totalMaxCovers = selectedTables.Sum(t => t.MaxCover);
            totalMinCovers = selectedTables.Sum(t => t.MinCover);
        }

        public static void GetFloorItemCount(this IEnumerable<TempFloorTable> tables, Int64 floorPlanId,
            out int totalTables, out int totalMinCovers, out int totalMaxCovers)
        {
            totalTables = totalMaxCovers = totalMinCovers = 0;

            var array = new string[] { "Sofa", "Chair", "SofaTable", "Wall", "SolidWall", "GlassWall", "BarTable", "Fence", "Pillar" };

            var selectedTables = tables.AsEnumerable().Where(p => !array.Contains(p.TableName.Split('-')[0]) && p.FloorPlanId == floorPlanId);
            totalTables = selectedTables.Count();
            totalMaxCovers = selectedTables.Sum(t => t.MaxCover);
            totalMinCovers = selectedTables.Sum(t => t.MinCover);
        }

        public static IEnumerable<FloorTable> GetAvailableFloorTables(this UsersContext db, Reservation reservation,
            out IList<Int64> upcomingTableIds, out IList<Int64> smallTableIds, out ReservationVM model,
            bool considerFloor = false, bool considerCovers = true, long ExceptionalMergeTableId = 0)
        {
            model = new ReservationVM
            {
                ReservationId = reservation.ReservationId,
                resDate = reservation.ReservationDate,
                Duration = reservation.Duration,
                Covers = reservation.Covers,
                time = new DateTime().Add(reservation.TimeForm.TimeOfDay).ToString("ddMMyyyyhhmmtt") +
                           " - " +
                           new DateTime().Add(reservation.TimeForm.AddMinutes(15).TimeOfDay).ToString("ddMMyyyyhhmmtt") +
                           " - " +
                           reservation.FoodMenuShiftId
            };

            return db.GetAvailableFloorTables(model, out upcomingTableIds, out smallTableIds, considerFloor, considerCovers, ExceptionalMergeTableId);
        }

        public static IEnumerable<FloorTable> GetAvailableFloorTables(this UsersContext db, ReservationVM model,
            out IList<Int64> upcomingTableIds, out IList<Int64> smallTableIds,
            bool considerFloor = false, bool considerCovers = true, long ExceptionalMergeTableId = 0)
        {
            IEnumerable<FloorTable> tables = null;
            var rejectedTables = new List<long>();
            upcomingTableIds = new List<Int64>();
            smallTableIds = new List<Int64>();

            // code to get tables list
            var tt = model.time.Split('-');

            var startTm = model.resDate.Add(DateTime.ParseExact(tt[0].Trim(), "ddMMyyyyhhmmtt", CultureInfo.InvariantCulture).TimeOfDay);
            var endTime = new DateTime();
            if (string.IsNullOrEmpty(model.Duration))
            {
                endTime = model.resDate.Add(DateTime.ParseExact(tt[1].Trim(), "ddMMyyyyhhmmtt", CultureInfo.InvariantCulture).TimeOfDay);
            }
            else
            {
                endTime = startTm.AddMinutes(model.Duration.GetMinutesFromDuration());
            }

            var reservation = db.tabReservations.SingleOrDefault(r => !r.IsDeleted && r.ReservationId == model.ReservationId);

            if (reservation != null)
            {
                model.EdtTableId = reservation.FloorTableId;
            }

            var array = new string[] { "Sofa", "Chair", "SofaTable", "Wall", "SolidWall", "GlassWall", "BarTable", "Fence", "Pillar" };

            if (considerFloor)
            {
                if (model.FloorPlanId == 0)
                {
                    model.FloorPlanId = 1;
                }

                var floorPlan = db.tabFloorPlans.Include("FloorTables").Where(f => f.FloorPlanId == model.FloorPlanId).Single();
                tables = floorPlan.FloorTables.Where(t => t.IsDeleted == false).AsEnumerable().Where(t => !array.Contains(t.TableName.Split('-')[0]));
            }
            else
            {
                tables = db.tabFloorTables.Where(t => t.IsDeleted == false).ToList().Where(t => !array.Contains(t.TableName.Split('-')[0]));
            }

            var resList = db.tabReservations.Where(r => !r.IsDeleted &&
                r.ReservationDate == model.resDate
                ).ToList();

            // check if status is FINISHED, CANCELLED, CANCELLED2
            var rejectedStatus = new List<long>()
                                     {
                                         ReservationStatus.Finished,
                                         ReservationStatus.Cancelled
                                         //ReservationStatus.Cancelled_2
                                     };

            resList = resList.Where(r => !rejectedStatus.Contains(r.StatusId.Value)).ToList();

            foreach (var item in resList)
            {
                var resStart = item.TimeForm;
                var resEnd = item.TimeForm.AddMinutes(item.Duration.GetMinutesFromDuration());

                if ((resStart <= startTm && resEnd >= endTime) || (resStart >= startTm && resEnd <= endTime) || (resStart < startTm && resEnd > startTm) || (resStart < endTime && resEnd > endTime)) //((resStart >= startTm && resStart < endTime) || (resEnd <= endTime && resEnd > startTm))
                {
                    if (item.FloorTableId == 0 && item.MergedFloorTableId > 0 && ExceptionalMergeTableId != item.MergedFloorTableId)
                    {
                        // mergedtables = item.MergedFloorTable.OrigionalTables.Select(ot => ot.FloorTable).ToList();

                        foreach (var origionalTbl in item.MergedFloorTable.OrigionalTables)
                        {
                            //rejectedTables.Add(origionalTbl.FloorTableId);
                            upcomingTableIds.Add(origionalTbl.FloorTableId);
                        }
                    }
                    else
                    {
                        //rejectedTables.Add(item.FloorTableId);
                        upcomingTableIds.Add(item.FloorTableId);
                    }
                }
            }

            if (reservation != null)
            {
                if (reservation.ReservationDate.Date == model.resDate.Date && startTm == reservation.TimeForm && reservation.Duration.Trim() == model.Duration.Trim())
                {
                    //rejectedTables.Remove(reservation.FloorTableId);
                    upcomingTableIds.Remove(reservation.FloorTableId);
                }
            }

            if (considerCovers)
            {
                foreach (var tbl in tables)
                {
                    if (tbl.MaxCover < model.Covers)
                    {
                        //if (!rejectedTables.Contains(tbl.FloorTableId))
                        if (!smallTableIds.Contains(tbl.FloorTableId) && !upcomingTableIds.Contains(tbl.FloorTableId))
                        {
                            //rejectedTables.Add(tbl.FloorTableId);
                            smallTableIds.Add(tbl.FloorTableId);
                        }
                    }
                }
            }

            //if (model.enableMerging)
            //{
            //    foreach (var tbl in tables)
            //    {
            //        if (tbl.MaxCover >= model.Covers && !rejectedTables.Contains(tbl.FloorTableId))
            //        {
            //            rejectedTables.Add(tbl.FloorTableId);
            //        }
            //    }
            //}

            rejectedTables.AddRange(upcomingTableIds);
            rejectedTables.AddRange(smallTableIds);

            tables = tables.Where(t => !rejectedTables.Contains(t.FloorTableId));

            return tables;
        }

        public static IList<object> GetTimeListForReservation(this UsersContext db, ReservationVM model)
        {
            var day = model.resDate.DayOfWeek;

            int sId = model.ShiftId;

            var dId = db.tabWeekDays.AsEnumerable().Single(p => p.DayName.Trim() == day.ToString().Trim()).DayId;

            var openTime = new DateTime();
            var closeTime = new DateTime();

            var ttime = db.tabMenuShiftHours.Where(p => p.DayId == dId).AsEnumerable();

            openTime = Convert.ToDateTime(ttime.Min(p => Convert.ToDateTime(p.OpenAt)));
            closeTime = Convert.ToDateTime(ttime.Max(p => Convert.ToDateTime(p.CloseAt).AddDays(Convert.ToInt32(p.IsNext))));

            if (!string.IsNullOrEmpty(model.Duration))
            {
                closeTime = closeTime.AddMinutes(-(model.Duration.GetMinutesFromDuration() - 15));
            }

            var op = openTime;
            var cl = closeTime;

            var TimeList = new List<object>();

            var aa = db.tabMenuShiftHours.AsEnumerable().Where(p => p.DayId == dId);

            while (op < cl)
            {
                var startTime = op;
                op = op.AddMinutes(15);

                int tShiftId = 0;
                var timeShift = aa.Where(s => Convert.ToDateTime(s.OpenAt) <= startTime && Convert.ToDateTime(s.CloseAt).AddDays(s.IsNext.Value) >= startTime).FirstOrDefault();
                if (timeShift != null)
                {
                    tShiftId = timeShift.FoodMenuShiftId;
                }

                TimeList.Add(new
                {
                    Text = startTime.ToString("hh:mm tt"),
                    Value = new DateTime().Add(startTime.TimeOfDay).ToString("ddMMyyyyhhmmtt") + " - " + new DateTime().Add(op.TimeOfDay).ToString("ddMMyyyyhhmmtt") + " - " + tShiftId
                });
            }

            return TimeList;
        }

        public static bool CheckAvailStatus(this IList<TableAvailability> availList, DateTime date, DateTime startTime, DateTime endTime, FloorTable table, int checkStatusId)
        {
            var tblAvailList = availList.Where(ta => ta.TableAvailabilityFloorTables.Any(taf => taf.FloorTableId == table.FloorTableId)).ToList();

            tblAvailList = tblAvailList.Where(ta => ta.CheckAvail(date, startTime, endTime)).OrderByDescending(ta => ta.CreatedOn).ToList();

            if (tblAvailList.Any())
            {
                var availablity = tblAvailList.FirstOrDefault();
                return checkStatusId == availablity.AvailablityStatusId;
            }
            else
            {
                return false;
            }
        }
        public static bool CheckAvailStatusOnline18122015(this IList<TableAvailability> availList, DateTime date, DateTime startTime, DateTime endTime, FloorTable table, int checkStatusId)
        {
            var tblAvailList = availList.Where(ta => ta.TableAvailabilityFloorTables.Any(taf => taf.FloorTableId == table.FloorTableId)).ToList();

            if (table.TableName == "T-34" || table.TableName == "T-42")
            {
                var a = table;
            }

            bool isAvailable = true;
            var startTm = startTime;
            var endTm = endTime;

            tblAvailList = tblAvailList.Where(ta => ta.CheckAvailOnline(date, startTime, endTime)).OrderByDescending(ta => ta.CreatedOn).ToList();

            while ((startTm < endTm) && isAvailable)
            {
                tblAvailList = tblAvailList.Where(ta => ta.CheckAvailOnline(date, startTm, startTm.AddMinutes(15))).OrderByDescending(ta => ta.CreatedOn).ToList();

                if (tblAvailList.Any())
                {
                    var availablity = tblAvailList.FirstOrDefault();
                    isAvailable = checkStatusId == availablity.AvailablityStatusId;
                }
                else
                {
                    isAvailable = false;
                }

                startTm = startTm.AddMinutes(15);
            }

            return isAvailable;
        }
        
        public static bool CheckAvailStatusOnline(this IList<TableAvailability> availList, DateTime date, DateTime startTime, DateTime endTime, FloorTable table, int checkStatusId)
        {
            var tblAvailList = availList.Where(ta => ta.TableAvailabilityFloorTables.Any(taf => taf.FloorTableId == table.FloorTableId)).ToList();

            if (table.TableName == "T-34" || table.TableName == "T-42")
            {
                var a = table;
            }

            bool isAvailable = true;
            var startTm = startTime;
            var endTm = endTime;

            tblAvailList = tblAvailList.Where(ta => ta.CheckAvailOnline(date, startTime, endTime)).OrderByDescending(ta => ta.CreatedOn).ToList();

            while ((startTm < endTm) && isAvailable)
            {
                tblAvailList = tblAvailList.Where(ta => ta.CheckAvailOnline(date, startTm, endTime.AddMinutes(15))).OrderByDescending(ta => ta.CreatedOn).ToList();

                if (tblAvailList.Any())
                {
                    var availablity = tblAvailList.FirstOrDefault();
                    isAvailable = checkStatusId == availablity.AvailablityStatusId;
                }
                else
                {
                    isAvailable = false;
                }

                startTm = startTm.AddMinutes(15);
            }

            return isAvailable;
        }

        public static bool CheckAvail(this TableAvailability ta, DateTime date, DateTime startTime, DateTime endTime)
        {
            var TAStartTime = date.Add(DateTime.ParseExact(ta.StartTime.Trim(), "h:mm tt", CultureInfo.InvariantCulture).TimeOfDay);
            var TAEndTime = date.Add(DateTime.ParseExact(ta.EndTime.Trim(), "h:mm tt", CultureInfo.InvariantCulture).TimeOfDay);

            if ((TAStartTime <= startTime && TAEndTime >= endTime)
                || (TAStartTime >= startTime && TAEndTime <= endTime)
                || (TAStartTime < startTime && TAEndTime > startTime)
                || (TAStartTime < endTime && TAEndTime > endTime))
                return true;
            else
                return false;
        }

        public static bool CheckAvailOnline(this TableAvailability ta, DateTime date, DateTime startTime, DateTime endTime)
        {
            var TAStartTime = date.Add(DateTime.ParseExact(ta.StartTime.Trim(), "h:mm tt", CultureInfo.InvariantCulture).TimeOfDay);
            var TAEndTime = date.Add(DateTime.ParseExact(ta.EndTime.Trim(), "h:mm tt", CultureInfo.InvariantCulture).TimeOfDay);

            //if ((TAStartTime <= startTime && TAEndTime >= endTime))
            if (((TAStartTime <= startTime && TAEndTime >= endTime)
                || (TAStartTime >= startTime && TAEndTime <= endTime)
                || (TAStartTime < startTime && TAEndTime > startTime)
                || (TAStartTime < endTime && TAEndTime > endTime)))
                return true;
            else
                return false;
        }

        public static bool IsTableBlocked(this IList<FloorTableBlock> blockList, long tableId, DateTime startTime, DateTime endTime)
        {
            return blockList.Any(b => b.FloorTableId == tableId
                && ((b.BlockFrom <= startTime && b.BlockTo >= endTime)
               || (b.BlockFrom >= startTime && b.BlockTo <= endTime)
               || (b.BlockFrom < startTime && b.BlockTo > startTime)
               || (b.BlockFrom < endTime && b.BlockTo > endTime)));
        }
    }
}