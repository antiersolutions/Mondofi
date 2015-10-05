using AIS.Models;
using AISModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using AIS.Extensions;
using AIS.Helpers;
using System.Collections.ObjectModel;
using AIS.Helpers.Caching;

namespace AIS.Controllers
{
    [Authorize]
    public class StaffController : Controller
    {
        private UsersContext db = new UsersContext();
        private ICacheManager cache = new MemoryCacheManager();

        #region Staff methods

        public JsonResult UpdateServer(Int64 ServerId, string ServerColor, IList<Int64> selectedFloorTableIds, Int64? AssignTo)
        {
            string msg = string.Empty;
            string status = string.Empty;

            //if (string.IsNullOrEmpty(ServerColor) && selectedFloorTableIds != null && selectedFloorTableIds.Any())
            //{
            //    status = ResponseStatus.Fail;
            //    msg = "Please select a color for server, before saving changes";
            //}
            //else 
            if (AssignTo.HasValue)
            {
                if (this.AssignServer(ServerId, ServerColor, AssignTo.Value))
                {
                    status = ResponseStatus.Success;
                    msg = "Tables are assigned to selected server sucessfully";
                }
                else
                {
                    status = ResponseStatus.Fail;
                    msg = "An error occured assigning tables to selected server, please try again later";
                }
            }
            else
            {
                if (this.UpdateFloorTableServer(ServerId, ServerColor, selectedFloorTableIds))
                {
                    status = ResponseStatus.Success;
                    msg = "Server updated sucessfully";
                }
                else
                {
                    status = ResponseStatus.Fail;
                    msg = "An error occured while updating the server, please try again later";
                }
            }

            this.ClearStaffCache();

            return Json(
                new
                {
                    Status = status,
                    Message = msg
                },
                JsonRequestBehavior.AllowGet);
        }

        //public JsonResult UpdateServer(Int64 ServerId, Int64? SectionId, IList<Int64> selectedFloorTableIds)
        //{
        //    try
        //    {
        //        string msg = string.Empty;
        //        string status = string.Empty;

        //        if (this.UpdateFloorTableServer(ServerId, SectionId, selectedFloorTableIds))
        //        {
        //            status = ResponseStatus.Success;
        //            msg = "Server updated sucessfully";
        //        }
        //        else
        //        {
        //            status = ResponseStatus.Fail;
        //            msg = "An error occured while updating the server, please try again later";
        //        }

        //        return Json(
        //            new
        //            {
        //                Status = status,
        //                Message = msg
        //            },
        //            JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(
        //            new
        //            {
        //                Status = ResponseStatus.Fail,
        //                Message = ex.Message
        //            },
        //            JsonRequestBehavior.AllowGet);
        //    }
        //}

        #endregion

        #region Floor Plan staff methods

        public PartialViewResult GetStaffList()
        {
            var staffList = db.GetCachedStaffList();

            return PartialView("StaffListPartial", staffList);
        }

        public PartialViewResult GetStaffListItem(long id)
        {
            string key = string.Format(CacheKeys.STAFF_LIST_ITEM,User.Identity.GetDatabaseName(), id);

            var cachedStaff = db.GetCachedStaffList();
            var staff = cachedStaff.Single(s => s.Id == id);

            var selectList = cachedStaff.Where(u => (u.DesignationId.HasValue && u.Designation.IsAssignable) &&
                        (u.Availability.HasValue && u.Availability == true) &&
                        u.Id != id)
                    .Select(u => new
                    {
                        StaffId = u.Id,
                        StaffName = u.FirstName + " " + u.LastName
                    }).ToList();

            ViewBag.OtherStaff = new SelectList(selectList,
                   "StaffId",
                   "StaffName");

            return PartialView("StaffListItemPartial", staff);
        }

        public PartialViewResult GetStaffSummary()
        {
            var summary = GetStaffSummaryList();
            return PartialView("StaffSummaryPartial", summary);
        }

        public PartialViewResult GetServerTableCheckList(Int64 ServerId)
        {
            string key = string.Format(CacheKeys.STAFF_LIST_ITEM_TABLES_CHECKLIST,User.Identity.GetDatabaseName(), ServerId);

            return cache.Get<PartialViewResult>(key, () =>
            {
                var tableList = this.GetTableCheckList(ServerId);
                return PartialView("StaffCheckBoxListPartial", tableList);
            });
        }

        public PartialViewResult GetServerSectionCheckList(Int64 ServerId)
        {
            string key = string.Format(CacheKeys.STAFF_LIST_ITEM_SECTIONS_CHECKLIST,User.Identity.GetDatabaseName(), ServerId);

            return cache.Get<PartialViewResult>(key, () =>
            {
                var tableList = this.GetSectionCheckList(ServerId);
                return PartialView("StaffCheckBoxListPartial", tableList);
            });
        }

        #endregion

        #region private methods

        private void ClearStaffCache()
        {
            //cache.RemoveByPattern(CacheKeys.FLOOR_TABLES_SCREEN_PATTREN);
            cache.RemoveByPattern(string.Format(CacheKeys.FLOOR_TABLES_SCREEN_COMPANY_PATTREN, User.Identity.GetDatabaseName()));
            //cache.RemoveByPattern(CacheKeys.STAFF_PATTREN);
            cache.RemoveByPattern(string.Format(CacheKeys.STAFF_COMPANY_PATTREN, User.Identity.GetDatabaseName()));
        }

        private IList<CheckListVM> GetTableCheckList(long ServerId)
        {
            var array = new string[] { "Sofa", "Chair", "SofaTable", "Wall", "SolidWall", "GlassWall", "BarTable", "Fence", "Pillar" };
            var allTables = db.tabFloorTables.Include("FloorPlan").Where(t => t.IsDeleted == false).ToList().Where(p => !array.Contains(p.TableName.Split('-')[0]));

            var serverTables = db.Users.Find(ServerId).ServingTables.Select(st => st.FloorTable).ToList();

            return allTables.OrderBy(s => s.FloorPlan.FLevel).ThenBy(s => s.TableName, new AlphaNumericComparer()).Select(t => new CheckListVM
            {
                PropertyName = "selectedFloorTableIds",
                Icon = (t.Section != null) ? t.Section.Color : null,
                Name = "L" + t.FloorPlan.FLevel + "-" + t.TableName,
                Group = "sec" + t.SectionId,
                Value = t.FloorTableId,
                IsChecked = ((serverTables.Contains(t)) ? true : false),
                IsDisabled = false
            }).ToList();
        }

        private IList<CheckListVM> GetSectionCheckList(long ServerId)
        {
            var sectionCheckList = new List<CheckListVM>();

            var serverTables = db.Users.Find(ServerId).ServingTables.Select(st => st.FloorTable).ToList();

            var allSections = db.tabSections.Include("FloorTable").ToList();

            foreach (var section in allSections)
            {
                var isCheck = false;

                if (section.FloorTable.Any() && serverTables != null)
                {
                    isCheck = section.FloorTable.Count() == section.FloorTable.Intersect(serverTables).Count();
                }

                var checkbox = new CheckListVM
                {
                    PropertyName = "selectedSectionIds",
                    Icon = section.Color,
                    Name = "L" + section.FloorPlan.FLevel + "-" + section.Name,
                    Value = section.SectionId,
                    IsChecked = isCheck,
                    IsDisabled = false
                };

                sectionCheckList.Add(checkbox);
            }

            return sectionCheckList;
        }

        private IList<StaffSummaryVM> GetStaffSummaryList()
        {
            var now = DateTime.UtcNow.ToClientTime();

            //var activeTables = db.tabFloorTables.Where(t => t.FloorTableServer != null && t.FloorTableServer.ServerId.HasValue).ToList()
            //    .Where(t => t.Reservations
            //        .Any(r => r.TimeForm < now && now < r.TimeForm.AddMinutes(r.Duration.GetMinutesFromDuration()))).ToList();
            var activeTables = db.tabFloorTables
                .Include("FloorTableServer.Server.ServingTables")
                .Where(t => t.FloorTableServer != null && t.FloorTableServer.ServerId.HasValue).ToList();

            if (activeTables != null && activeTables.Count > 0)
            {
                var currentReservation = db.GetReservationByDate(now.Date)
                    .Where(r => r.TimeForm < now && now < r.TimeForm.AddMinutes(r.Duration.GetMinutesFromDuration())).ToList();

                var currentReservationTables = currentReservation
                    .SelectMany(r => (r.FloorTableId > 0 ? (new Collection<FloorTable> { r.FloorTable }) : r.MergedFloorTable.OrigionalTables
                        .Select(mr => mr.FloorTable))).ToList();


                return activeTables
                    .GroupBy(t => t.FloorTableServer.Server)
                    .Where(u => (u.Key.DesignationId.HasValue && u.Key.Designation.IsAssignable) && (u.Key.Availability.HasValue && u.Key.Availability == true))
                    .Select(s => new StaffSummaryVM
                {
                    Server = s.Key,
                    TablesAssigned = s.Key.ServingTables.Count(),
                    TablesSeated = s.Key.ServingTables
                    .Where(t => currentReservationTables.Any(ct => ct.FloorTableId == t.FloorTableId))
                            .Count(),
                    CoversSeated = currentReservation
                    .Where(r => s.Key.ServingTables
                        .Any(st => (r.FloorTableId > 0 ? r.FloorTableId == st.FloorTableId : r.MergedFloorTable
                            .OrigionalTables.Any(mt => mt.FloorTableId == st.FloorTableId))))
                    .Sum(r => r.FloorTableId > 0 ? r.Covers : r.MergedFloorTable.OrigionalTables
                        .Where(mt => s.Key.ServingTables
                            .Any(st => st.FloorTableId == mt.FloorTableId))
                            .Sum(t => t.FloorTable.MaxCover))
                }).ToList();
            }

            return new List<StaffSummaryVM>();
        }

        private bool UpdateFloorTableServer(Int64 serverId, string serverColor, IList<Int64> floorTableIds)
        {
            try
            {
                if (string.IsNullOrEmpty(serverColor))
                {
                    serverColor = FloorExtensions.GetBrightRandomColor();
                }

                var server = db.Users.Find(serverId);

                IList<FloorTable> assignedTables = new List<FloorTable>();

                if (floorTableIds != null && floorTableIds.Any())
                {
                    //var server = db.Users.Find(serverId);
                    server.StaffColor = serverColor;

                    assignedTables = db.tabFloorTables.Where(t => floorTableIds.Contains(t.FloorTableId)).ToList();

                    foreach (var table in assignedTables)
                    {
                        var now = DateTime.UtcNow.ToClientTime();

                        var upcomingReservations = table.Reservations.Where(r => !r.IsDeleted && r.TimeForm > now).ToList();

                        if (upcomingReservations != null)
                        {
                            foreach (var reservation in upcomingReservations)
                            {
                                if (reservation.ReservationServer == null)
                                {
                                    var resServer = new ReservationServer()
                                    {
                                        ServerId = serverId
                                    };

                                    reservation.ReservationServer = resServer;
                                }
                                else
                                {
                                    reservation.ReservationServer.ServerId = serverId;
                                }

                                db.Entry(reservation).State = System.Data.Entity.EntityState.Modified;
                            }
                        }

                        if (table.FloorTableServer == null)
                        {
                            var tableServer = new FloorTableServer()
                            {
                                ServerId = serverId
                            };

                            table.FloorTableServer = tableServer;
                        }
                        else
                        {
                            table.FloorTableServer.ServerId = serverId;
                        }

                        db.Entry(table).State = System.Data.Entity.EntityState.Modified;
                    }
                }
                else
                {
                    server.StaffColor = null;
                }

                db.Entry(server).State = System.Data.Entity.EntityState.Modified;

                var remainingTables = db.tabFloorTables.ToList().Where(t => !assignedTables.Contains(t) && (t.FloorTableServer != null && t.FloorTableServer.ServerId == serverId)).ToList();

                foreach (var tbl in remainingTables)
                {
                    tbl.FloorTableServer.ServerId = null;
                    db.Entry(tbl).State = System.Data.Entity.EntityState.Modified;
                }

                db.SaveChanges();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool AssignServer(long ServerId, string ServerColor, long AssignTo)
        {
            try
            {
                var targetServer = db.Users.Find(AssignTo);
                var sourceServer = db.Users.Find(ServerId);

                if (string.IsNullOrEmpty(targetServer.StaffColor))
                {
                    targetServer.StaffColor = ServerColor;
                }

                sourceServer.StaffColor = null;

                var sourceServerTables = sourceServer.ServingTables.ToList();

                foreach (var table in sourceServerTables)
                {
                    table.ServerId = AssignTo;
                    db.Entry(table).State = System.Data.Entity.EntityState.Modified;
                }

                db.SaveChanges();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region Controller overridden methods

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);

            CultureInfo cultureInfo = CultureInfo.GetCultureInfo("en-US");

            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        #endregion
    }
}
