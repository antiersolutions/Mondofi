using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AISModels;
using AIS.Models;
using AIS.Extensions;
using System.Data;
using AIS.Filters;
using System.Net;
using AIS.Helpers;
using AIS.Helpers.Caching;
using Microsoft.AspNet.Identity;
using System.Data.Entity;

namespace AIS.Controllers
{
    [Authorize]
    public class FloorItemController : Controller
    {
        UsersContext Db = new UsersContext();
        private ICacheManager cache = new MemoryCacheManager();

        #region Temp Floor Items

        [HttpPost]
        public JsonResult AddTempItem(TempFloorTable table)
        {
            ViewBag.IsTemp = true;

            try
            {
                table.HtmlId = "table" + Guid.NewGuid().ToString("N");
                table.TableName = "T-0";
                table.Angle = table.Angle;
                table.MinCover = table.MinCover;
                table.MaxCover = table.MaxCover;
                table.TTop = "150px";
                table.TLeft = "36px";
                table.TableDesign = " ";
                table.UpdatedOn = DateTime.UtcNow.ToDefaultTimeZone(User.Identity.GetDatabaseName());
                table.CreatedOn = DateTime.UtcNow.ToDefaultTimeZone(User.Identity.GetDatabaseName());

                Db.tabTempFloorTables.Add(table);
                Db.SaveChanges();

                var tblDB = Db.tabTempFloorTables.Include("FloorPlan").Where(t => t.FloorTableId == table.FloorTableId).Single();

                tblDB.TableName = this.AssignItemName(tblDB.FloorTableId, tblDB.Shape);

                FloorTable copyTable = new FloorTable();
                CopyHelper.Copy(typeof(TempFloorTable), table, typeof(FloorTable), copyTable);

                tblDB.TableDesign = this.RenderPartialViewToString("~/Views/Floor/GetFloorItemTemplate.cshtml", copyTable);

                Db.Entry(tblDB).State = EntityState.Modified;
                Db.SaveChanges();

                var totalTables = 0;
                var totalMaxCovers = 0;
                var totalMinCovers = 0;

                Db.tabTempFloorTables.GetFloorItemCount(tblDB.FloorPlanId, out totalTables, out totalMinCovers, out totalMaxCovers);

                return Json(new
                {
                    Status = ResponseStatus.Success,
                    ItemId = tblDB.FloorTableId,
                    totalTables = totalTables,
                    totalMaxCovers = totalMaxCovers,
                    totalMinCovers = totalMinCovers,
                    Template = tblDB.TableDesign,
                    HtmlId = table.HtmlId
                });
            }
            catch (Exception)
            {
                return Json(new { Status = ResponseStatus.Fail, ItemId = 0 });
            }
        }

        [HttpPost]
        public JsonResult UpdateTempItem(TempFloorTable table)
        {
            ViewBag.IsTemp = true;

            try
            {
                var tbl = Db.tabTempFloorTables.Find(table.FloorTableId);
                tbl.TableName = table.TableName;

                FloorTable copyTable = new FloorTable();
                CopyHelper.Copy(typeof(TempFloorTable), table, typeof(FloorTable), copyTable);

                tbl.TableDesign = this.RenderPartialViewToString("~/Views/Floor/GetFloorItemTemplate.cshtml", copyTable);
                tbl.Angle = table.Angle;
                //tbl.MinCover = table.MinCover;
                //tbl.HtmlId = table.HtmlId;
                //tbl.Shape = table.Shape;
                //tbl.Size = table.Size;
                //tbl.MaxCover = table.MaxCover;
                tbl.TTop = table.TTop;
                tbl.TBottom = table.TBottom;
                tbl.TLeft = table.TLeft;
                tbl.TRight = table.TRight;
                tbl.UpdatedOn = DateTime.UtcNow.ToDefaultTimeZone(User.Identity.GetDatabaseName());

                Db.Entry(tbl).State = EntityState.Modified;
                Db.SaveChanges();

                var totalTables = 0;
                var totalMaxCovers = 0;
                var totalMinCovers = 0;

                Db.tabTempFloorTables.GetFloorItemCount(tbl.FloorPlanId, out totalTables, out totalMinCovers, out totalMaxCovers);

                return Json(new
                {
                    Status = ResponseStatus.Success,
                    ItemId = tbl.FloorTableId,
                    totalTables = totalTables,
                    totalMaxCovers = totalMaxCovers,
                    totalMinCovers = totalMinCovers
                });
            }
            catch (Exception)
            {
                return Json(new { Status = ResponseStatus.Fail, ItemId = 0 });
            }
        }

        [HttpGet]
        public JsonResult ChangeTempItem(TempFloorTable table, bool SaveChanges = false, bool CancelChanges = false)
        {
            ModelState.Clear();

            ViewBag.IsSelected = true;
            ViewBag.IsTemp = true;

            var totalTables = 0;
            var totalMaxCovers = 0;
            var totalMinCovers = 0;

            try
            {
                if (!CancelChanges)
                {
                    FloorTable copyTable = new FloorTable();
                    CopyHelper.Copy(typeof(TempFloorTable), table, typeof(FloorTable), copyTable);

                    if (SaveChanges)
                    {
                        ViewBag.IsSelected = false;

                        table.TableDesign = this.RenderPartialViewToString("~/Views/Floor/GetFloorItemTemplate.cshtml", copyTable);

                        var tblDB = Db.tabTempFloorTables.Find(table.FloorTableId);

                        tblDB.TableName = table.TableName;
                        tblDB.Shape = table.Shape;
                        tblDB.Size = table.Size;
                        tblDB.Angle = table.Angle;
                        tblDB.TableDesign = table.TableDesign;
                        tblDB.MinCover = table.MinCover;
                        tblDB.MaxCover = table.MaxCover;
                        tblDB.UpdatedOn = DateTime.UtcNow.ToDefaultTimeZone(User.Identity.GetDatabaseName());

                        Db.Entry(table).State = EntityState.Modified;
                        Db.SaveChanges();
                    }
                    else
                    {
                        table.TableDesign = this.RenderPartialViewToString("~/Views/Floor/GetFloorItemTemplate.cshtml", copyTable);
                    }

                    Db.tabTempFloorTables.GetFloorItemCount(table.FloorPlanId, out totalTables, out totalMinCovers, out totalMaxCovers);

                    var response = new
                    {
                        Status = ResponseStatus.Success,
                        HtmlId = table.HtmlId,
                        Template = table.TableDesign,
                        totalTables = totalTables,
                        totalMaxCovers = totalMaxCovers,
                        totalMinCovers = totalMinCovers,
                        IsUpdated = SaveChanges
                    };

                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ViewBag.IsSelected = false;

                    var origionalTable = Db.tabTempFloorTables.Find(table.FloorTableId);

                    FloorTable copyOrigionalTable = new FloorTable();
                    CopyHelper.Copy(typeof(TempFloorTable), origionalTable, typeof(FloorTable), copyOrigionalTable);

                    origionalTable.TableDesign = this.RenderPartialViewToString("~/Views/Floor/GetFloorItemTemplate.cshtml", copyOrigionalTable);

                    Db.tabTempFloorTables.GetFloorItemCount(table.FloorPlanId, out totalTables, out totalMinCovers, out totalMaxCovers);

                    var response = new
                    {
                        Status = ResponseStatus.Success,
                        HtmlId = origionalTable.HtmlId,
                        Template = origionalTable.TableDesign,
                        totalTables = totalTables,
                        totalMaxCovers = totalMaxCovers,
                        totalMinCovers = totalMinCovers,
                        IsUpdated = SaveChanges
                    };

                    return Json(response, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                Db.Dispose();

                Db = new UsersContext();

                ViewBag.IsSelected = false;

                var origionalTable = Db.tabTempFloorTables.Find(table.FloorTableId);

                FloorTable copyOrigionalTable = new FloorTable();
                CopyHelper.Copy(typeof(TempFloorTable), origionalTable, typeof(FloorTable), copyOrigionalTable);

                origionalTable.TableDesign = this.RenderPartialViewToString("~/Views/Floor/GetFloorItemTemplate.cshtml", copyOrigionalTable);

                Db.tabTempFloorTables.GetFloorItemCount(table.FloorPlanId, out totalTables, out totalMinCovers, out totalMaxCovers);

                var response = new
                {
                    Status = ResponseStatus.Fail,
                    HtmlId = origionalTable.HtmlId,
                    Template = origionalTable.TableDesign,
                    totalTables = totalTables,
                    totalMaxCovers = totalMaxCovers,
                    totalMinCovers = totalMinCovers,
                    IsUpdated = SaveChanges
                };

                return Json(response, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult DeleteTempItem(Int64 id)
        {
            try
            {
                var tbl = Db.tabTempFloorTables.Find(id);
                var floorId = tbl.FloorPlanId;

                Db.tabTempFloorTables.Remove(tbl);
                Db.SaveChanges();

                var totalTables = 0;
                var totalMaxCovers = 0;
                var totalMinCovers = 0;

                Db.tabTempFloorTables.GetFloorItemCount(floorId, out totalTables, out totalMinCovers, out totalMaxCovers);

                return Json(new
                {
                    Status = ResponseStatus.Success,
                    totalTables = totalTables,
                    totalMaxCovers = totalMaxCovers,
                    totalMinCovers = totalMinCovers
                });
            }
            catch (Exception)
            {
                return Json(new { Status = ResponseStatus.Fail });
            }
        }

        #endregion

        #region Floor Items

        [HttpPost]
        public JsonResult AddFloorItem(FloorTable table)
        {
            try
            {
                table.HtmlId = "table" + Guid.NewGuid().ToString("N");
                table.TableName = "T-0";
                table.Angle = table.Angle;
                table.MinCover = table.MinCover;
                table.MaxCover = table.MaxCover;
                table.TTop = "150px";
                table.TLeft = "36px";
                table.TableDesign = " ";
                table.UpdatedOn = DateTime.UtcNow.ToDefaultTimeZone(User.Identity.GetDatabaseName());
                table.CreatedOn = DateTime.UtcNow.ToDefaultTimeZone(User.Identity.GetDatabaseName());
                table.IsDeleted = false;

                Db.tabFloorTables.Add(table);
                Db.SaveChanges();

                var tblDB = Db.tabFloorTables.Include("FloorPlan").Where(t => t.FloorTableId == table.FloorTableId).Single();

                tblDB.TableName = this.AssignItemName(tblDB.FloorTableId, tblDB.Shape);
                tblDB.TableDesign = this.RenderPartialViewToString("~/Views/Floor/GetFloorItemTemplate.cshtml", tblDB);

                Db.Entry(tblDB).State = EntityState.Modified;
                Db.SaveChanges();

                var totalTables = 0;
                var totalMaxCovers = 0;
                var totalMinCovers = 0;

                Db.tabFloorTables.GetFloorItemCount(tblDB.FloorPlanId, out totalTables, out totalMinCovers, out totalMaxCovers);

                return Json(new
                {
                    Status = ResponseStatus.Success,
                    ItemId = tblDB.FloorTableId,
                    totalTables = totalTables,
                    totalMaxCovers = totalMaxCovers,
                    totalMinCovers = totalMinCovers,
                    Template = tblDB.TableDesign,
                    HtmlId = table.HtmlId
                });
            }
            catch (Exception)
            {
                return Json(new { Status = ResponseStatus.Fail, ItemId = 0 });
            }
            finally
            {
                ClearFloorCache();
            }
        }

        [HttpPost]
        public JsonResult UpdateFloorItem(FloorTable table)
        {
            try
            {
                var tbl = Db.tabFloorTables.Find(table.FloorTableId);

                tbl.FloorPlan.UpdatedBy = User.Identity.GetUserId<long>();
                tbl.FloorPlan.UpdatedOn = DateTime.UtcNow.ToDefaultTimeZone(User.Identity.GetDatabaseName());

                //tbl.TableName = table.TableName;
                tbl.TableDesign = this.RenderPartialViewToString("~/Views/Floor/GetFloorItemTemplate.cshtml", table);
                tbl.Angle = table.Angle;
                //tbl.MinCover = table.MinCover;
                //tbl.HtmlId = table.HtmlId;
                //tbl.Shape = table.Shape;
                //tbl.Size = table.Size;
                //tbl.MaxCover = table.MaxCover;
                tbl.TTop = table.TTop;
                tbl.TBottom = table.TBottom;
                tbl.TLeft = table.TLeft;
                tbl.TRight = table.TRight;
                tbl.UpdatedOn = DateTime.UtcNow.ToDefaultTimeZone(User.Identity.GetDatabaseName());

                Db.Entry(tbl).State = EntityState.Modified;
                Db.SaveChanges();

                var totalTables = 0;
                var totalMaxCovers = 0;
                var totalMinCovers = 0;

                Db.tabFloorTables.GetFloorItemCount(tbl.FloorPlanId, out totalTables, out totalMinCovers, out totalMaxCovers);

                return Json(new
                {
                    Status = ResponseStatus.Success,
                    ItemId = tbl.FloorTableId,
                    totalTables = totalTables,
                    totalMaxCovers = totalMaxCovers,
                    totalMinCovers = totalMinCovers
                });
            }
            catch (Exception)
            {
                return Json(new { Status = ResponseStatus.Fail, ItemId = 0 });
            }
            finally
            {
                ClearFloorCache();
            }
        }

        [HttpGet]
        public JsonResult ChangeFloorItem(FloorTable table, bool SaveChanges = false, bool CancelChanges = false)
        {
            ModelState.Clear();

            ViewBag.IsSelected = true;

            var totalTables = 0;
            var totalMaxCovers = 0;
            var totalMinCovers = 0;

            try
            {
                if (!CancelChanges)
                {
                    if (SaveChanges)
                    {
                        ViewBag.IsSelected = false;

                        table.TableDesign = this.RenderPartialViewToString("~/Views/Floor/GetFloorItemTemplate.cshtml", table);
                        table.AssignSectionColor();

                        var tblDB = Db.tabFloorTables.Find(table.FloorTableId);

                        tblDB.TableName = table.TableName;
                        tblDB.IsTemporary = table.IsTemporary;
                        tblDB.Shape = table.Shape;
                        tblDB.Size = table.Size;
                        tblDB.Angle = table.Angle;
                        tblDB.TableDesign = table.TableDesign;
                        tblDB.MinCover = table.MinCover;
                        tblDB.MaxCover = table.MaxCover;
                        tblDB.UpdatedOn = DateTime.UtcNow.ToDefaultTimeZone(User.Identity.GetDatabaseName());

                        Db.Entry(tblDB).State = EntityState.Modified;
                        Db.SaveChanges();

                        ClearFloorCache();
                    }
                    else
                    {
                        table.TableDesign = this.RenderPartialViewToString("~/Views/Floor/GetFloorItemTemplate.cshtml", table);
                        table.AssignSectionColor();
                    }

                    Db.tabFloorTables.GetFloorItemCount(table.FloorPlanId, out totalTables, out totalMinCovers, out totalMaxCovers);

                    var response = new
                    {
                        Status = ResponseStatus.Success,
                        HtmlId = table.HtmlId,
                        Template = table.TableDesign,
                        totalTables = totalTables,
                        totalMaxCovers = totalMaxCovers,
                        totalMinCovers = totalMinCovers,
                        IsUpdated = SaveChanges
                    };

                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ViewBag.IsSelected = false;

                    var origionalTable = Db.tabFloorTables.Find(table.FloorTableId);

                    origionalTable.TableDesign = this.RenderPartialViewToString("~/Views/Floor/GetFloorItemTemplate.cshtml", origionalTable);
                    origionalTable.AssignSectionColor();

                    Db.tabFloorTables.GetFloorItemCount(table.FloorPlanId, out totalTables, out totalMinCovers, out totalMaxCovers);

                    var response = new
                    {
                        Status = ResponseStatus.Success,
                        HtmlId = origionalTable.HtmlId,
                        Template = origionalTable.TableDesign,
                        totalTables = totalTables,
                        totalMaxCovers = totalMaxCovers,
                        totalMinCovers = totalMinCovers,
                        IsUpdated = SaveChanges
                    };

                    return Json(response, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                Db.Dispose();

                Db = new UsersContext();

                ViewBag.IsSelected = false;

                var origionalTable = Db.tabFloorTables.Find(table.FloorTableId);

                origionalTable.TableDesign = this.RenderPartialViewToString("~/Views/Floor/GetFloorItemTemplate.cshtml", origionalTable);
                origionalTable.AssignSectionColor();

                Db.tabFloorTables.AsEnumerable().GetFloorItemCount(table.FloorPlanId, out totalTables, out totalMinCovers, out totalMaxCovers);

                var response = new
                {
                    Status = ResponseStatus.Fail,
                    HtmlId = origionalTable.HtmlId,
                    Template = origionalTable.TableDesign,
                    totalTables = totalTables,
                    totalMaxCovers = totalMaxCovers,
                    totalMinCovers = totalMinCovers,
                    IsUpdated = SaveChanges
                };

                return Json(response, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult DeleteFloorItem(Int64 id)
        {
            try
            {
                var dateTimeNow = DateTime.UtcNow.ToDefaultTimeZone(User.Identity.GetDatabaseName());

                if (Db.tabReservations.Any(r => !r.IsDeleted &&
                    ((r.FloorTableId > 0) ? (r.FloorTableId == id) : (r.MergedFloorTable.OrigionalTables.Any(ot => ot.FloorTableId == id)))
                    && (r.TimeForm > dateTimeNow || r.TimeTo > dateTimeNow)))
                {
                    return Json(new
                    {
                        Status = ResponseStatus.Fail,
                        message = "This table is already in use. you can not delete this table."
                    });
                }

                var tbl = Db.tabFloorTables.Find(id);
                var floorId = tbl.FloorPlanId;

                tbl.TableName += "_" + DateTime.Now.TimeOfDay.Ticks;
                tbl.IsDeleted = true;
                tbl.DeletedOn = DateTime.UtcNow;

                //Db.tabFloorTables.Remove(tbl);
                Db.SaveChanges();

                var totalTables = 0;
                var totalMaxCovers = 0;
                var totalMinCovers = 0;

                Db.tabFloorTables.AsEnumerable()
                    .GetFloorItemCount(floorId, out totalTables, out totalMinCovers, out totalMaxCovers);

                return Json(new
                {
                    Status = ResponseStatus.Success,
                    totalTables = totalTables,
                    totalMaxCovers = totalMaxCovers,
                    totalMinCovers = totalMinCovers
                });
            }
            catch (Exception)
            {
                return Json(new { Status = ResponseStatus.Fail });
            }
            finally
            {
                ClearFloorCache();
            }
        }

        [HttpPost]
        public JsonResult IsFloorTableExist(string TName, string OriginalTName)
        {
            bool isTableExist = Db.tabFloorTables.Any(t => t.TableName.Equals(TName) && !t.TableName.Equals(OriginalTName));

            if (isTableExist)
            {
                return Json(new
                {
                    Status = ResponseStatus.Fail
                });
            }

            return Json(new
            {
                Status = ResponseStatus.Success
            });
        }

        #endregion

        #region private methods

        private void ClearFloorCache()
        {
            //cache.RemoveByPattern(CacheKeys.FLOOR_TABLES_SCREEN_PATTREN);
            cache.RemoveByPattern(string.Format(CacheKeys.FLOOR_TABLES_SCREEN_COMPANY_PATTREN, User.Identity.GetDatabaseName()));
            //cache.RemoveByPattern(CacheKeys.RESERVATION_BY_DATE_PATTREN);
            cache.RemoveByPattern(string.Format(CacheKeys.RESERVATION_BY_DATE_COMPANY_PATTREN, User.Identity.GetDatabaseName()));
            //cache.RemoveByPattern(CacheKeys.FILTERED_RESERVATION_PATTREN);
            cache.RemoveByPattern(string.Format(CacheKeys.FILTERED_RESERVATION_COMPANY_PATTREN, User.Identity.GetDatabaseName()));
            //cache.RemoveByPattern(CacheKeys.STAFF_PATTREN);
            cache.RemoveByPattern(string.Format(CacheKeys.STAFF_COMPANY_PATTREN,User.Identity.GetDatabaseName()));
        }

        private string AssignItemName(Int64 itemId, string shape)
        {
            switch (shape.ToUpper())
            {
                case "ROUND":
                case "SQUARE":
                case "RECTANGLE":
                    {
                        return "T-" + itemId;
                    }
                default:
                    {
                        return shape + "-" + itemId;
                    }
            }
        }

        #endregion

        #region Controller overridden methods

        protected override void Dispose(bool disposing)
        {
            Db.Dispose();
            base.Dispose(disposing);
        }

        #endregion
    }
}
