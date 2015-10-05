using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AISModels;
using System.Web.Script.Serialization;
using AIS.Models;
using System.Data;
using AIS.Helpers.Caching;
using System.Data.Entity;

namespace AIS.Controllers
{
    [Authorize]
    public class SectionController : Controller
    {
        private UsersContext db = new UsersContext();
        private ICacheManager cache = new MemoryCacheManager();

        public ActionResult Index(Int64 id)
        {
            ViewBag.floorId = id;
            var record = db.tabSections.Where(p => p.FloorPlanId == id).ToList();
            if (record == null || record.Count == 0)
            {
                var floor = db.tabFloorPlans.Find(id);

                record = new List<Section>() 
                { 
                    new Section() 
                    {
                        FloorPlanId = floor.FloorPlanId,
                        FloorPlan = floor
                    }
                };
            }
            return View(record);
        }

        public ActionResult Assign(Int64 id)
        {
            var floor = db.tabFloorPlans.Include("Sections").Include("FloorTables").Where(f => f.FloorPlanId == id).Single();
            ViewBag.floorId = floor.FloorPlanId;
            return View(floor);
        }

        //public ActionResult Save(string str, string floorId)
        //{
        //    try
        //    {
        //        db.Database.ExecuteSqlCommand("DELETE from Section where FloorPlanId = {0}", floorId);
        //        db.SaveChanges();

        //        List<Section> ite = (new JavaScriptSerializer()).Deserialize<List<Section>>(str);
        //        foreach (var item in ite)
        //        {
        //            var sec = new Section()
        //            {
        //                Color = item.Color,
        //                Name = item.Name,
        //                SLevel = item.SLevel,
        //                FloorPlanId = Convert.ToInt64(floorId)
        //            };
        //            db.tabSections.Add(sec);
        //        }
        //        db.SaveChanges();
        //        return Json(new { success = true, msz = "Section saved successfully." }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception)
        //    {
        //        return Json(new { success = false, msz = "Some error occur." }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        public ActionResult Save(string str, Int64 floorId)
        {
            var floor = db.tabFloorPlans.Include("Sections").Include("FloorTables").Where(f => f.FloorPlanId == floorId).Single();

            try
            {
                List<Section> sections = (new JavaScriptSerializer()).Deserialize<List<Section>>(str);

                var DBSections = floor.Sections.ToList();

                foreach (var section in sections)
                {
                    if (DBSections.Any(s => s.SectionId == section.SectionId))
                    {
                        var dbsection = DBSections.Where(p => p.SectionId == section.SectionId).Single();
                        dbsection.Color = section.Color;
                        dbsection.Name = section.Name;
                        dbsection.SLevel = section.SLevel;

                        db.Entry(dbsection).State = EntityState.Modified;
                    }
                    else
                    {
                        var sec = new Section()
                        {
                            Color = section.Color,
                            Name = section.Name,
                            SLevel = section.SLevel,
                            FloorPlanId = floorId
                        };

                        db.tabSections.Add(sec);
                    }
                }

                db.SaveChanges();

                foreach (var dbsection in DBSections)
                {
                    if (!sections.Any(p => p.SectionId == dbsection.SectionId))
                    {
                        if (dbsection.FloorTable != null)
                        {
                            var tables = dbsection.FloorTable.ToList();

                            foreach (var table in tables)
                            {
                                table.SectionId = 0;
                                db.Entry(table).State = EntityState.Modified;
                            }
                        }

                        db.tabSections.Remove(dbsection);
                    }
                }

                db.SaveChanges();
                return Json(new { success = true, msz = "Section updated successfully." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false, msz = "Some error occur." }, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                ClearFloorCache();
            }
        }

        public ActionResult addTableSection(string addIds, string remIds)
        {
            try
            {
                var rec = (new JavaScriptSerializer()).Deserialize<List<ids>>(addIds);

                if (!string.IsNullOrEmpty(remIds))
                {
                    var removeIds = remIds.Split(',');
                    foreach (var item in removeIds)
                    {
                        var record = db.tabFloorTables.AsEnumerable().Single(p => p.FloorTableId == Convert.ToInt64(item));
                        record.SectionId = 0;
                    }
                    db.SaveChanges();
                }

                foreach (var item in rec)
                {
                    var record = db.tabFloorTables.AsEnumerable().Single(p => p.FloorTableId == item.tId);
                    record.SectionId = item.secId;
                }
                db.SaveChanges();
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                ClearFloorCache();
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        private void ClearFloorCache()
        {
            //cache.RemoveByPattern(CacheKeys.FLOOR_TABLES_SCREEN_PATTREN);
            cache.RemoveByPattern(string.Format(CacheKeys.FLOOR_TABLES_SCREEN_COMPANY_PATTREN, User.Identity.GetDatabaseName()));
        }

        private class ids
        {
            public Int64 tId { get; set; }
            public Int64 secId { get; set; }
        }
    }
}
