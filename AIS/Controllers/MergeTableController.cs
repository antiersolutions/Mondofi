using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AISModels;
using AIS.Extensions;
using AIS.Models;
using System.Data;
using AIS.Helpers;
using System.Globalization;
using Microsoft.AspNet.Identity;
using System.Data.Entity;

namespace AIS.Controllers
{
    [Authorize]
    public class MergeTableController : Controller
    {
        private UsersContext db = new UsersContext();

        #region MergeTable old Methods

        public ActionResult MergeTablePartial(ReservationVM model)
        {
            ViewBag.LevelList = db.tabFloorPlans.ToList().Select(fp => new
            {
                Text = "L" + fp.FLevel.Value + "-" + fp.FloorName,
                Value = fp.FloorPlanId

            });

            var coverList = new List<object>();

            for (int i = 7; i <= 16; i++)
            {
                coverList.Add(new { Value = i, Text = i + " Cover" });
            }

            ViewBag.CoverList = coverList;

            ViewBag.TimeList = db.GetTimeListForReservation(model);

            var tempTables = db.tabMergedFloorTables
                .Include("OrigionalTables")
                .Where(mt => mt.CreatedBy == User.Identity.GetUserId<long>() && !db.tabReservations
                    .Any(r => !r.IsDeleted && r.MergedFloorTableId == mt.FloorTableId))
                .ToList();

            if (tempTables.Count() > 0)
            {
                foreach (var tbl in tempTables)
                {
                    foreach (var otbl in tbl.OrigionalTables.ToList())
                    {
                        db.tabMergedTableOrigionalTables.Remove(otbl);
                    }

                    db.tabMergedFloorTables.Remove(tbl);
                }

                db.SaveChanges();
            }

            return PartialView(model);
        }

        public ActionResult GetMergedTable(List<long> selectedTables, int covers)
        {
            ModelState.Clear();

            var tableName = string.Empty;
            var tableMinCovers = 0;
            var tableMaxCovers = 0;
            var floorId = 0L;

            if (selectedTables != null)
            {
                var tables = db.tabFloorTables.Where(t => selectedTables.Contains(t.FloorTableId)).ToList();

                foreach (var tbl in tables)
                {
                    tableName += tbl.TableName + " & ";
                    tableMinCovers += tbl.MinCover;
                    tableMaxCovers += tbl.MaxCover;
                }

                tableName = tableName.Remove(tableName.Length - 3);
                floorId = tables.First().FloorPlanId;
            }
            else
            {
                return Content("<h1>Preview Here</h1>");
            }

            if (tableMaxCovers < covers)
            {
                return Content("<h1>Max Covers should be more than " + covers + ".</h1>");
            }
            else if (tableMaxCovers > 16)
            {
                return Content("<h1>Max Covers should be less than 16.</h1>");
            }

            var table = new FloorTable()
            {
                FloorPlanId = floorId,
                HtmlId = "table" + Guid.NewGuid().ToString("N"),
                TableName = tableName,
                Angle = 0,
                MinCover = tableMinCovers,
                MaxCover = tableMaxCovers,
                Size = "random",
                Shape = "RANDOM",
                TTop = "0",
                TLeft = "0",
                TableDesign = " ",
                IsTemporary = true,
            };

            ViewBag.SelectedTables = selectedTables;

            return PartialView("MergedTableTemplate", table);
        }

        public ActionResult GetTablesFreeToMerge(ReservationVM model)
        {
            ModelState.Clear();

            this.UpdateReservationModel(model);

            IList<Int64> upcomingTableIds;
            IList<Int64> smallTableIds;
            var tables = db.GetAvailableFloorTables(model, out upcomingTableIds, out smallTableIds, true, false);
            var data = tables.Select(x => new CheckListVM()
                {
                    PropertyName = "selectedTables",
                    Name = x.TableName + " (" + x.MinCover + "/" + x.MaxCover + ")",
                    Value = x.FloorTableId,
                    IsChecked = false,
                    IsDisabled = false
                });
            return PartialView("~/Views/TableAvailablity/CheckboxListPartial.cshtml", data);
        }

        public ActionResult AddMergedTable(FloorTable table, List<long> selectedTables)
        {
            ModelState.Clear();

            try
            {
                var tbl = new MergedFloorTable();

                CopyHelper.Copy(typeof(FloorTable), table, typeof(MergedFloorTable), tbl);

                tbl.TTop = table.TTop = "150px";
                tbl.TLeft = table.TLeft = "36px";
                tbl.TableDesign = " ";
                tbl.IsTemporary = table.IsTemporary = false;
                tbl.CreatedOn = table.CreatedOn = DateTime.UtcNow.ToDefaultTimeZone(User.Identity.GetDatabaseName());
                tbl.CreatedBy = User.Identity.GetUserId<long>();

                var originalTables = db.tabFloorTables.Where(t => selectedTables.Contains(t.FloorTableId)).ToList();

                foreach (var origionalTable in originalTables)
                {
                    var mergedTables = new MergedTableOrigionalTable();
                    mergedTables.MergedFloorTableId = tbl.FloorTableId;
                    mergedTables.FloorTableId = origionalTable.FloorTableId;

                    db.tabMergedTableOrigionalTables.Add(mergedTables);
                }

                db.tabMergedFloorTables.Add(tbl);
                db.SaveChanges();

                table.FloorTableId = tbl.FloorTableId;

                ViewBag.SelectedTables = selectedTables;

                tbl.TableDesign = this.RenderPartialViewToString("~/Views/MergeTable/MergedTableTemplate.cshtml", table);

                return Json(new
                {
                    Status = ResponseStatus.Success,
                    ItemId = tbl.FloorTableId,
                    Template = tbl.TableDesign,
                    TablesToRemove = string.Join(",#", originalTables.Select(t => t.HtmlId))
                });
            }
            catch (Exception)
            {
                return Json(new { Status = ResponseStatus.Fail, ItemId = 0 });
            }
        }

        public ActionResult UpdateMergedTable(FloorTable table, Int64 MergedFloorTableId, List<long> selectedTables)
        {
            ModelState.Clear();

            try
            {
                var tbl = db.tabMergedFloorTables.Find(table.FloorTableId);

                tbl.FloorTableId = table.FloorTableId = MergedFloorTableId;
                tbl.FloorPlanId = table.FloorPlanId;
                tbl.TableName = table.TableName;
                tbl.Angle = table.Angle;
                tbl.MinCover = table.MinCover;
                tbl.HtmlId = table.HtmlId;
                tbl.Shape = table.Shape;
                tbl.Size = table.Size;
                tbl.MaxCover = table.MaxCover;
                tbl.TTop = table.TTop;
                tbl.TLeft = table.TLeft;
                tbl.TableDesign = " ";
                tbl.IsTemporary = table.IsTemporary = false;

                //var originalTables = db.tabFloorTables.Where(t => selectedTables.Contains(t.FloorTableId)).ToList();

                //foreach (var origionalTable in originalTables)
                //{
                //    var mergedTables = new MergedTableOrigionalTable();
                //    mergedTables.MergedFloorTableId = tbl.FloorTableId;
                //    mergedTables.FloorTableId = origionalTable.FloorTableId;

                //    db.tabMergedTableOrigionalTables.Add(mergedTables);
                //}

                db.Entry(tbl).State = EntityState.Modified;
                db.SaveChanges();

                ViewBag.SelectedTables = selectedTables;

                tbl.TableDesign = this.RenderPartialViewToString("~/Views/MergeTable/MergedTableTemplate.cshtml", table);

                return Json(new
                {
                    Status = ResponseStatus.Success,
                    ItemId = tbl.FloorTableId,
                    Template = tbl.TableDesign,
                    HtmlId = tbl.HtmlId
                    //TablesToRemove = string.Join(",#", originalTables.Select(t => t.HtmlId))
                });
            }
            catch (Exception)
            {
                return Json(new { Status = ResponseStatus.Fail, ItemId = 0 });
            }
        }

        #endregion

        #region MergeTable new Methods

        public JsonResult GetJSONTablesFreeToMerge(ReservationVM model)
        {
            ModelState.Clear();

            this.CleanUpTempMergeTable();
            this.UpdateReservationModel(model);

            IList<Int64> upcomingTableIds;
            IList<Int64> smallTableIds;
            var tables = new List<FloorTable>();
            var mergedtablesList = new List<FloorTable>();

            if (model.ReservationId > 0)
            {
                var tt = model.time.Split('-');
                var startTm = model.resDate.Add(DateTime.ParseExact(tt[0].Trim(), "ddMMyyyyhhmmtt", CultureInfo.InvariantCulture).TimeOfDay);
                var reservation = db.tabReservations.SingleOrDefault(r => !r.IsDeleted && r.ReservationId == model.ReservationId);

                if (reservation != null)
                {
                    if (reservation.MergedFloorTableId > 0 && reservation.ReservationDate.Date == model.resDate.Date && startTm == reservation.TimeForm && reservation.Duration.Trim() == model.Duration.Trim())
                    {
                        mergedtablesList = db.tabMergedFloorTables.Find(reservation.MergedFloorTableId).OrigionalTables.Select(ot => ot.FloorTable).ToList();
                        tables = db.GetAvailableFloorTables(model, out upcomingTableIds, out smallTableIds, true, false).ToList();
                    }
                    else if (reservation.MergedFloorTableId > 0)
                    {
                        tables = db.GetAvailableFloorTables(model, out upcomingTableIds, out smallTableIds, true, false, reservation.MergedFloorTableId).ToList();
                        mergedtablesList = new List<FloorTable>();
                    }
                    else
                    {
                        tables = db.GetAvailableFloorTables(model, out upcomingTableIds, out smallTableIds, true, false).ToList();
                    }
                }
                else
                {
                    tables = db.GetAvailableFloorTables(model, out upcomingTableIds, out smallTableIds, true, false).ToList();
                }
            }
            else
            {
                tables = db.GetAvailableFloorTables(model, out upcomingTableIds, out smallTableIds, true, false).ToList();
            }

            var data = new
            {
                availTables = tables.Select(x => new
                                {
                                    tableId = x.FloorTableId,
                                    tableName = x.TableName + " (" + x.MinCover + "/" + x.MaxCover + ")",
                                    minCovers = x.MinCover,
                                    maxCovers = x.MaxCover
                                }).ToList(),
                mergedTables = mergedtablesList.Select(x => new
                                {
                                    tableId = x.FloorTableId,
                                    tableName = x.TableName + " (" + x.MinCover + "/" + x.MaxCover + ")",
                                    minCovers = x.MinCover,
                                    maxCovers = x.MaxCover
                                }).ToList()
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddMergedTableNew(List<long> selectedTables, int covers, int? PIN)
        {
            ModelState.Clear();

            var tbl = new MergedFloorTable();

            var tableName = string.Empty;
            var tableMinCovers = 0;
            var tableMaxCovers = 0;
            var floorId = 0L;

            var tables = db.tabFloorTables.Where(t => selectedTables.Contains(t.FloorTableId)).ToList();

            foreach (var table in tables)
            {
                tableName += table.TableName + " & ";
                tableMinCovers += table.MinCover;
                tableMaxCovers += table.MaxCover;
            }

            tableName = tableName.Remove(tableName.Length - 3);
            floorId = tables.First().FloorPlanId;


            //if (tableMaxCovers < covers)
            //{
            //    return Json(new
            //    {
            //        Status = ResponseStatus.Fail,
            //        ItemId = 0,
            //        Message = "Max Covers should be more than " + covers + "."
            //    });
            //}
            //else if (tableMinCovers > covers)
            //{
            //    return Json(new
            //    {
            //        Status = ResponseStatus.Fail,
            //        ItemId = 0,
            //        Message = "Min Covers should be less than or equal to " + covers + "."
            //    });
            //}


            tbl.FloorPlanId = floorId;
            tbl.HtmlId = "table" + Guid.NewGuid().ToString("N");
            tbl.TableName = tableName;
            tbl.Angle = 0;
            tbl.MinCover = tableMinCovers;
            tbl.MaxCover = tableMaxCovers;
            tbl.Size = "random";
            tbl.Shape = "RANDOM";
            tbl.TTop = "0px";
            tbl.TLeft = "0px";
            tbl.TableDesign = " ";
            tbl.IsTemporary = false;
            tbl.CreatedOn = DateTime.UtcNow.ToDefaultTimeZone(User.Identity.GetDatabaseName());
            tbl.CreatedBy = User.Identity.GetUserId<long>();

            var originalTables = db.tabFloorTables.Where(t => selectedTables.Contains(t.FloorTableId)).ToList();

            foreach (var origionalTable in originalTables)
            {
                var mergedTables = new MergedTableOrigionalTable();
                mergedTables.MergedFloorTableId = tbl.FloorTableId;
                mergedTables.FloorTableId = origionalTable.FloorTableId;

                db.tabMergedTableOrigionalTables.Add(mergedTables);
            }

            db.tabMergedFloorTables.Add(tbl);
            db.SaveChanges();

            ViewBag.SelectedTables = originalTables;

            return Json(new
            {
                Status = ResponseStatus.Success,
                ItemId = tbl.FloorTableId,
                PIN = PIN
            });
        }

        public ActionResult UpdateMergedTableNew(Int64 MergedFloorTableId, List<long> selectedTables, int covers, int? PIN)
        {
            ModelState.Clear();
            var tableName = string.Empty;
            var tableMinCovers = 0;
            var tableMaxCovers = 0;

            var tables = db.tabFloorTables.Where(t => selectedTables.Contains(t.FloorTableId)).ToList();

            foreach (var table in tables)
            {
                tableName += table.TableName + " & ";
                tableMinCovers += table.MinCover;
                tableMaxCovers += table.MaxCover;
            }

            tableName = tableName.Remove(tableName.Length - 3);

            //if (tableMaxCovers < covers)
            //{
            //    return Json(new
            //    {
            //        Status = ResponseStatus.Fail,
            //        ItemId = 0,
            //        Message = "Max Covers should be more than " + covers + "."
            //    });
            //}
            //else if (tableMinCovers > covers)
            //{
            //    return Json(new
            //    {
            //        Status = ResponseStatus.Fail,
            //        ItemId = 0,
            //        Message = "Min Covers should be less than or equal to " + covers + "."
            //    });
            //}

            var tbl = db.tabMergedFloorTables.Find(MergedFloorTableId);
            tbl.TableName = tableName;
            tbl.MinCover = tableMinCovers;
            tbl.MaxCover = tableMaxCovers;

            var mtblMap = tbl.OrigionalTables.ToList();
            var mtblTables = mtblMap.Select(mt => mt.FloorTable).ToList();

            foreach (var origionalTable in tables)
            {
                if (!mtblTables.Contains(origionalTable))
                {
                    var mergedTables = new MergedTableOrigionalTable();
                    mergedTables.MergedFloorTableId = tbl.FloorTableId;
                    mergedTables.FloorTableId = origionalTable.FloorTableId;

                    db.tabMergedTableOrigionalTables.Add(mergedTables);
                }
            }

            foreach (var oldMergedtable in mtblTables)
            {
                if (!tables.Contains(oldMergedtable))
                {
                    db.tabMergedTableOrigionalTables.Remove(mtblMap.Where(ot => ot.FloorTableId == oldMergedtable.FloorTableId).Single());
                }
            }


            db.Entry(tbl).State = EntityState.Modified;
            db.SaveChanges();

            ViewBag.SelectedTables = tables;

            return Json(new
            {
                Status = ResponseStatus.Success,
                ItemId = tbl.FloorTableId,
                PIN = PIN
            });
        }

        #endregion

        #region private methods

        private void UpdateReservationModel(ReservationVM model)
        {
            if (model.ReservationId > 0)
            {
                var res = db.tabReservations.SingleOrDefault(r => !r.IsDeleted && r.ReservationId == model.ReservationId);
                model.Email = ((res.Customers.Emails.Count() > 0) ? res.Customers.Emails.First().Email : string.Empty);
                model.FirstName = res.Customers.FirstName;
                model.LastName = res.Customers.LastName;
                model.MobileNumber = res.Customers.PhoneNumbers.First().PhoneNumbers;
                model.ShiftId = res.FoodMenuShiftId;
                model.Status = res.StatusId.ToString();
                model.TablePositionLeft = res.TablePositionLeft;
                model.TablePositionTop = res.TablePositionTop;
            }
        }

        private void CleanUpTempMergeTable()
        {
            var id = User.Identity.GetUserId<long>();
            var tempTablesQuery = db.tabMergedFloorTables
                .Include("OrigionalTables")
                .Where(mt => mt.CreatedBy == id && !db.tabReservations
                    .Any(r => !r.IsDeleted && r.MergedFloorTableId == mt.FloorTableId)).AsQueryable();

            var tempTables = tempTablesQuery.ToList();

            if (tempTables.Count > 0)
            {
                foreach (var tbl in tempTables)
                {
                    var orgTbls = tbl.OrigionalTables.ToList();
                    foreach (var otbl in orgTbls)
                    {
                        db.tabMergedTableOrigionalTables.Remove(otbl);
                    }

                    db.tabMergedFloorTables.Remove(tbl);
                }

                db.SaveChanges();
            }
        }

        #endregion

        #region Controller overridden methods

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        #endregion
    }
}
