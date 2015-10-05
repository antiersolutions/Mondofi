using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AIS.Filters;
using AISModels;
using AIS.Models;
using System.Data;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using AIS.Extensions;
using System.Web.Script.Serialization;
using AIS.Helpers;
using AIS.Helpers.Caching;
using System.Data.Entity;
using Microsoft.AspNet.Identity;

namespace AIS.Controllers
{
    [Authorize]
    public class FloorController : Controller
    {
        UsersContext Db = new UsersContext();
        private ICacheManager cache = new MemoryCacheManager();
        //
        // GET: /Floor/

        public ActionResult Index()
        {
            if (Session["EditFloorId"] == null)
            {
                var model = Db.tabFloorPlans.Include("FloorTables").Include("Sections").ToList();
                return View(model);
            }
            else
            {
                var floorId = (Int64)Session["EditFloorId"];
                return RedirectToAction("Edit", new { id = floorId });
            }
        }

        public ActionResult NewFloorPlan()
        {
            var UserId = User.Identity.GetUserId<long>();

            var floor_Name = "Temp Floor " + ((Db.tabFloorPlans.Count() != 0) ? Db.tabFloorPlans.Max(p => p.FloorPlanId) : 0);
            //var floor_Name = "TempFloor" ;

            if (Session["TempFloorUId"] == null)
            {
                this.ManageTempTables();

                var model = new TempFloorPlan()
                {
                    UserId = UserId,
                    Guid = Guid.NewGuid().ToString(),
                    FloorName = floor_Name,
                    CreatedOn = DateTime.UtcNow.ToClientTime()
                };

                Db.tabTempFloorPlans.Add(model);
                Db.SaveChanges();

                Session["TempFloorUId"] = model.Guid;

                return View(model);
            }
            else
            {
                var Uid = (string)Session["TempFloorUId"];
                var model = Db.tabTempFloorPlans.Include("TempFloorTables").Where(tf => tf.Guid == Uid).SingleOrDefault();

                if (model == null)
                {
                    model = new TempFloorPlan()
                    {
                        UserId = UserId,
                        Guid = Guid.NewGuid().ToString(),
                        FloorName = floor_Name,
                        CreatedOn = DateTime.UtcNow.ToClientTime()
                    };

                    Db.tabTempFloorPlans.Add(model);
                    Db.SaveChanges();
                }

                if (model.TempFloorTables != null)
                {
                    foreach (var item in model.TempFloorTables)
                    {
                        FloorTable copyTable = new FloorTable();
                        CopyHelper.Copy(typeof(TempFloorTable), item, typeof(FloorTable), copyTable);

                        item.TableDesign = this.RenderPartialViewToString("GetFloorItemTemplate", copyTable);

                        copyTable = null;
                    }
                }

                Session["TempFloorUId"] = model.Guid;

                return View(model);
            }
        }

        public ActionResult IsFloorPlanExistAdd(string FloorName)
        {
            var obj = new object();

            if (Db.tabFloorPlans.Any(p => p.FloorName.Trim() == FloorName.Trim()))
            {
                obj = new { success = false, msz = "Floor plan name already exist." };
            }
            else
            {
                obj = new { success = true };
            }

            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        public ActionResult IsFloorPlanExistEdit(string FloorName, Int64 floorId)
        {
            var obj = new object();

            if (Db.tabFloorPlans.Any(p => p.FloorName.Trim() == FloorName.Trim() && p.FloorPlanId != floorId))
                obj = new { success = false, msz = "Floor plan name already exist." };
            else
                obj = new { success = true };

            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateBackgroundScale(Int64 floorId, decimal value)
        {
            var obj = new object();
            try
            {
                var floor = Db.tabFloorPlans.Single(p => p.FloorPlanId == floorId);

                if (floor != null)
                {
                    floor.BackgroundScale = value;
                    Db.Entry(floor).State = EntityState.Modified;
                    Db.SaveChanges();

                    obj = new { success = true };
                }
                else
                {
                    obj = new { success = false, msz = "Floor not found." };
                }
            }
            catch (Exception ex)
            {
                obj = new { success = false, msz = ex.Message };
            }

            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult IsTableNameExist(string tableName)
        //{
        //    var obj = new object();

        //    if (Db.tabFloorTables.Any(p => p.TableName.Trim() == tableName.Trim()))
        //        obj = new { success = false, msz = "This table name already exist." };
        //    else
        //        obj = new { success = true };

        //    return Json(obj, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult CurrentFloorPlan()
        {
            return View();
        }

        public ActionResult FloorPlan(ReservationCustomerVM obj)
        {
            if (obj.date == null)
                ViewBag.ccdate = DateTime.Now.ToString("dddd, dd MMM, yy");
            else
                ViewBag.ccdate = Convert.ToDateTime(obj.date).ToString("dddd, dd MMM, yy");

            if (obj.fname == null)
                ViewBag.ccfname = string.Empty;
            else
                ViewBag.ccfname = obj.fname;

            if (obj.shiftId == null)
            {
                ViewBag.ccshift = string.Empty;
            }
            else
            {
                ViewBag.ccshift = Db.tabFoodMenuShift.Where(p => p.FoodMenuShiftId == obj.shiftId).SingleOrDefault().MenuShift;
            }

            if (obj.lname == null)
                ViewBag.cclname = string.Empty;
            else
                ViewBag.cclname = obj.lname;

            if (obj.phoneNo == null)
                ViewBag.ccphoneNo = string.Empty;
            else
                ViewBag.ccphoneNo = obj.phoneNo;

            if (obj.email == null)
                ViewBag.ccemail = string.Empty;
            else
                ViewBag.ccemail = obj.email;



            ViewBag.shiftDdl = new SelectList(Db.tabFoodMenuShift, "FoodMenuShiftId", "MenuShift");
            var floorPlans = Db.tabFloorPlans;

            var rec = floorPlans.Include("FloorTables").Where(p => p.IsActive == true).SingleOrDefault();

            if (rec != null)
            {
                ViewBag.Floors = floorPlans;
                return View(rec);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        public ActionResult _FloorPlanPartial(DateTime? startTime, DateTime? endTime, string shift)
        {
            var model = Db.tabFloorPlans.Include("FloorTables").Include("Sections").Where(p => p.IsActive == true).SingleOrDefault();

            int Covers = 0;

            if (model != null)
            {
                this.AttachFloorItemHtmlDesign(model);

                int? shiftId = null;

                if (!string.IsNullOrEmpty(shift))
                {
                    shift = shift.Trim();

                    if (shift.ToLower() == "all")
                    {
                        shiftId = 0;
                    }
                    else
                    {
                        var shiftDB = Db.tabFoodMenuShift.Where(s => s.MenuShift.Equals(shift)).FirstOrDefault();

                        if (shiftDB != null)
                        {
                            shiftId = shiftDB.FoodMenuShiftId;
                        }
                    }
                }

                foreach (var item in model.FloorTables)
                {
                    int coverCount = 0;

                    this.ChangeChairColor(item);
                    this.CheckReservations20141222(item, startTime, endTime, shiftId, out coverCount);

                    Covers += coverCount;
                }

                ViewBag.Covers = Covers;

                return PartialView(model);
            }
            else
            {
                ViewBag.Covers = Covers;

                return PartialView(new FloorPlan());
            }
        }

        public ActionResult UpdateFloorPlan(DateTime date, string time, string shift)
        {
            var model = Db.tabFloorPlans.Include("FloorTables").Include("Sections").Where(p => p.IsActive == true).SingleOrDefault();

            this.AttachFloorItemHtmlDesign(model);

            int Covers = 0;

            var tt = time.Split('-');

            var startTime = date.Date.Add(DateTime.ParseExact(tt[0].Trim(), "ddMMyyyyhhmmtt", CultureInfo.InvariantCulture).TimeOfDay);
            var endTime = date.Date.Add(DateTime.ParseExact(tt[1].Trim(), "ddMMyyyyhhmmtt", CultureInfo.InvariantCulture).TimeOfDay);

            int? shiftId = null;

            if (!string.IsNullOrEmpty(shift))
            {
                shift = shift.Trim();

                if (shift.ToLower() == "all")
                {
                    shiftId = 0;
                }
                else
                {
                    var shiftDB = Db.tabFoodMenuShift.Where(s => s.MenuShift.Equals(shift)).FirstOrDefault();

                    if (shiftDB != null)
                    {
                        shiftId = shiftDB.FoodMenuShiftId;
                    }
                }
            }

            foreach (var item in model.FloorTables)
            {
                int coverCount = 0;

                this.ChangeChairColor(item);
                this.CheckReservations20141222(item, startTime, endTime, shiftId, out coverCount);

                Covers += coverCount;
            }

            ViewBag.Covers = Covers;

            return PartialView("_FloorPlanPartial", model);
        }

        public ActionResult UpdateFloorPlanOnTimeSlide(DateTime date, string time, string shift)
        {
            var model = Db.tabFloorPlans.Include("FloorTables").Include("Sections").Where(p => p.IsActive == true).SingleOrDefault();

            this.AttachFloorItemHtmlDesign(model);

            int Covers = 0;

            time = time.Trim();

            var startTime = date.Date.Add(Convert.ToDateTime(time).TimeOfDay);
            var endTime = startTime.AddMinutes(15);

            int? shiftId = null;

            if (!string.IsNullOrEmpty(shift))
            {
                shift = shift.Trim();

                if (shift.ToLower() == "all")
                {
                    shiftId = 0;
                }
                else
                {
                    var shiftDB = Db.tabFoodMenuShift.Where(s => s.MenuShift.Equals(shift)).FirstOrDefault();

                    if (shiftDB != null)
                    {
                        shiftId = shiftDB.FoodMenuShiftId;
                    }
                }
            }

            foreach (var item in model.FloorTables)
            {
                int coverCount = 0;

                this.ChangeChairColor(item);
                this.CheckReservations20141222(item, startTime, endTime, shiftId, out coverCount);

                Covers += coverCount;
            }

            ViewBag.Covers = Covers;

            return PartialView("_FloorPlanPartial", model);
        }

        public ActionResult ReservationAdd(string cover)
        {
            if (!string.IsNullOrEmpty(cover))
            {
                ViewBag.cover = Convert.ToInt32(cover);
            }
            else
            {
                ViewBag.cover = 1;
            }
            return PartialView();
        }

        public ActionResult ReservationList()
        {
            return PartialView();
        }

        #region Old code ... new code in FloorItemController
        //[HttpPost]
        //public JsonResult AddTempElement(TableDesignVM table)
        //{
        //    try
        //    {
        //        table.UniqueId = Guid.NewGuid().ToString("N");

        //        var tbl = new TempFloorTable();

        //        tbl.FloorPlanId = table.FloorId;
        //        tbl.TableName = "T-0";
        //        tbl.Angle = table.Angle;
        //        tbl.MinCover = table.MinCover;
        //        tbl.MaxCover = table.MaxCover;
        //        tbl.TTop = "150px";
        //        tbl.TLeft = "36px";
        //        tbl.TableDesign = " ";
        //        tbl.UpdatedOn = DateTime.UtcNow;
        //        tbl.CreatedOn = DateTime.UtcNow;

        //        Db.tabTempFloorTables.Add(tbl);
        //        Db.SaveChanges();

        //        var tblDB = Db.tabTempFloorTables.Include("TempFloorPlan").Where(t => t.FloorTableId == tbl.FloorTableId).Single();

        //        table.TempFloorTableId = tblDB.FloorTableId;
        //        tblDB.TableDesign = this.RenderPartialViewToString("AddFloorElementPartial", table);
        //        tblDB.TableName = table.TableName;

        //        Db.Entry(tblDB).State = EntityState.Modified;
        //        Db.SaveChanges();

        //        var array = new string[] { "Sofa", "Chair", "SofaTable", "Wall", "SolidWall", "GlassWall", "BarTable", "Fence", "Pillar" };

        //        var totalTables = Db.tabTempFloorTables.AsEnumerable().Where(p => !array.Contains(p.TableName.Split('-')[0]) && p.FloorPlanId == tblDB.FloorPlanId).Count();

        //        return Json(new { Status = "success", ItemId = tblDB.FloorTableId, totalTables = totalTables, tableDesign = tblDB.TableDesign, HtmlId = table.HtmlId });
        //    }
        //    catch (Exception)
        //    {
        //        return Json(new { Status = "fail", ItemId = 0 });
        //    }
        //}

        //[HttpPost]
        //public JsonResult AddItem(TempFloorTable table)
        //{
        //    try
        //    {
        //        table.CreatedOn = DateTime.UtcNow;
        //        table.UpdatedOn = DateTime.UtcNow;
        //        Db.tabTempFloorTables.Add(table);
        //        Db.SaveChanges();

        //        var array = new string[] { "Sofa", "Chair", "SofaTable", "Wall", "SolidWall", "GlassWall", "BarTable", "Fence", "Pillar" };

        //        var totalTables = Db.tabTempFloorTables.AsEnumerable().Where(p => !array.Contains(p.TableName.Split('-')[0]) && p.TempFloorPlanId == table.TempFloorPlanId).Count();

        //        return Json(new { Status = "success", ItemId = table.TempFloorTableId, totalTables = totalTables });
        //    }
        //    catch (Exception)
        //    {
        //        return Json(new { Status = "fail", ItemId = 0 });
        //    }
        //}

        //[HttpPost]
        //public JsonResult UpdateItem(TempFloorTable table)
        //{
        //    try
        //    {
        //        var tbl = Db.tabTempFloorTables.Find(table.TempFloorTableId);
        //        tbl.TableName = table.TableName;
        //        tbl.TableDesign = table.TableDesign;
        //        tbl.Angle = table.Angle;
        //        tbl.MinCover = table.MinCover;
        //        tbl.MaxCover = table.MaxCover;
        //        tbl.TTop = table.TTop;
        //        tbl.TBottom = table.TBottom;
        //        tbl.TLeft = table.TLeft;
        //        tbl.TRight = table.TRight;
        //        tbl.UpdatedOn = DateTime.UtcNow;

        //        Db.Entry(tbl).State = EntityState.Modified;
        //        Db.SaveChanges();

        //        var array = new string[] { "Sofa", "Chair", "SofaTable", "Wall", "SolidWall", "GlassWall", "BarTable", "Fence", "Pillar" };

        //        var totalTables = Db.tabTempFloorTables.AsEnumerable().Where(p => !array.Contains(p.TableName.Split('-')[0]) && p.TempFloorPlanId == table.TempFloorPlanId).Count();

        //        return Json(new { Status = "success", ItemId = table.TempFloorTableId, totalTables = totalTables });
        //    }
        //    catch (Exception)
        //    {
        //        return Json(new { Status = "fail", ItemId = 0 });
        //    }
        //}

        //[HttpPost]
        //public JsonResult DeleteTempTable(Int64 id)
        //{
        //    try
        //    {
        //        var tbl = Db.tabTempFloorTables.Find(id);

        //        Db.tabTempFloorTables.Remove(tbl);
        //        Db.SaveChanges();

        //        return Json(new { Status = "success" });
        //    }
        //    catch (Exception)
        //    {
        //        return Json(new { Status = "fail" });
        //    }
        //}
        #endregion

        private void ManageTempTables()
        {
            var id = User.Identity.GetUserId<long>();
            var tempPlans = Db.tabTempFloorPlans.Where(p => p.UserId == id).ToList();

            foreach (var plan in tempPlans)
            {
                Db.Database.ExecuteSqlCommand("DELETE FROM TEMPFLOORTABLE WHERE TempFloorPlanId = {0}", plan.FloorPlanId);
                Db.tabTempFloorPlans.Remove(plan);
                Db.SaveChanges();
            }
        }

        public ActionResult SaveFloor(TempFloorPlan model)
        {
            var fp = Db.tabTempFloorPlans.Include("TempFloorTables").Where(p => p.FloorPlanId == model.FloorPlanId).Single();

            var isFloorActive = Db.tabFloorPlans;
            int[] lvl = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            int flvl;
            if (isFloorActive.Count() == 0)
            {
                flvl = 1;
            }
            else
            {
                flvl = lvl.Where(p => !isFloorActive.Select(s => s.FLevel).ToList().Contains(p)).FirstOrDefault();
            }

            var floorPlan = new FloorPlan()
            {
                CreatedOn = fp.CreatedOn,
                FloorName = model.FloorName,
                PhotoPath = fp.PhotoPath,
                UserId = fp.UserId,
                FLevel = flvl,
                IsActive = isFloorActive.Count() == 0 ? true : false,

                UpdatedBy = User.Identity.GetUserId<long>(),
                UpdatedOn = DateTime.UtcNow,
            };

            Db.tabFloorPlans.Add(floorPlan);

            foreach (var item in fp.TempFloorTables)
            {
                var floorTable = new FloorTable()
                {
                    Angle = item.Angle,
                    CreatedOn = item.CreatedOn,
                    FloorPlanId = floorPlan.FloorPlanId,
                    TableDesign = item.TableDesign,
                    MaxCover = item.MaxCover,
                    MinCover = item.MinCover,
                    TableName = item.TableName,
                    TBottom = item.TBottom,
                    TLeft = item.TLeft,
                    TRight = item.TRight,
                    TTop = item.TTop,
                    UpdatedOn = item.UpdatedOn,
                    HtmlId = item.HtmlId,
                    Shape = item.Shape,
                    Size = item.Size,
                    IsTemporary = false
                };

                Db.tabFloorTables.Add(floorTable);

                //StringBuilder design = new StringBuilder();
                //design.Append(item.TableDesign.Substring(0, item.TableDesign.LastIndexOf("<input")));
                //design.Append("<input id=\"FloorTableId\" name=\"FloorTableId\" type=\"hidden\" value=\"" + floorTable.FloorTableId + "\"/></div>");

                //floorTable.TableDesign = design.ToString();

                //Db.Entry(floorTable).State = EntityState.Modified;
            }

            Db.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult CancelTempFloor(Int64 FloorPlanId)
        {
            Db.Database.ExecuteSqlCommand("DELETE FROM TempFloorTable WHERE TempFloorPlanId = {0}; DELETE FROM TempFloorPlan  WHERE TempFloorPlanId = {0};", FloorPlanId);
            Db.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult CloseEdit(Int64 FloorPlanId, string FloorName)
        {
            if (Session["EditFloorId"] != null)
            {
                Session["EditFloorId"] = null;
            }

            var floor = Db.tabFloorPlans.Find(FloorPlanId);
            floor.FloorName = FloorName;

            floor.UpdatedBy = User.Identity.GetUserId<long>();
            floor.UpdatedOn = DateTime.UtcNow;

            Db.Entry(floor).State = EntityState.Modified;
            Db.SaveChanges();

            ClearFloorCache();

            return RedirectToAction("Index");
        }

        public ActionResult Edit(Int64 id)
        {
            var floor = Db.tabFloorPlans.Include("FloorTables").Where(p => p.FloorPlanId == id).Single();

            floor.FloorTables = floor.FloorTables.Where(p => p.IsDeleted == false).ToList();

            this.AttachFloorItemHtmlDesign(floor);

            if (Session["EditFloorId"] == null)
            {
                Session["EditFloorId"] = id;
            }

            return View("Edit", floor);
        }

        #region Old code ... new code in FloorItemController
        //[HttpPost]
        //public JsonResult AddFloorElement(TableDesignVM table)
        //{
        //    try
        //    {
        //        table.UniqueId = Guid.NewGuid().ToString("N");

        //        var tbl = new FloorTable();

        //        tbl.FloorPlanId = table.FloorId;
        //        tbl.TableName = "T-0";
        //        tbl.Angle = table.Angle;
        //        tbl.MinCover = table.MinCover;
        //        tbl.MaxCover = table.MaxCover;
        //        tbl.TTop = "150px";
        //        tbl.TLeft = "36px";
        //        tbl.TableDesign = " ";
        //        tbl.UpdatedOn = DateTime.UtcNow;
        //        tbl.CreatedOn = DateTime.UtcNow;

        //        Db.tabFloorTables.Add(tbl);
        //        Db.SaveChanges();

        //        var tblDB = Db.tabFloorTables.Include("FloorPlan").Where(t => t.FloorTableId == tbl.FloorTableId).Single();

        //        table.FloorTableId = tblDB.FloorTableId;
        //        tblDB.TableDesign = this.RenderPartialViewToString("AddFloorElementPartial", table);
        //        tblDB.TableName = table.TableName;

        //        Db.Entry(tblDB).State = EntityState.Modified;
        //        Db.SaveChanges();

        //        var array = new string[] { "Sofa", "Chair", "SofaTable", "Wall", "SolidWall", "GlassWall", "BarTable", "Fence", "Pillar" };

        //        var totalTables = Db.tabFloorTables.AsEnumerable().Where(p => !array.Contains(p.TableName.Split('-')[0]) && p.FloorPlanId == tblDB.FloorPlanId).Count();

        //        return Json(new { Status = "success", ItemId = tblDB.FloorTableId, totalTables = totalTables, tableDesign = tblDB.TableDesign, HtmlId = table.HtmlId });
        //    }
        //    catch (Exception)
        //    {
        //        return Json(new { Status = "fail", ItemId = 0 });
        //    }
        //}

        //[HttpPost]
        //public JsonResult AddFloorItem(FloorTable table)
        //{
        //    try
        //    {
        //        table.CreatedOn = DateTime.UtcNow;
        //        table.UpdatedOn = DateTime.UtcNow;

        //        Db.tabFloorTables.Add(table);
        //        Db.SaveChanges();

        //        var tbl = Db.tabFloorTables.Include("FloorPlan").Where(t => t.FloorTableId == table.FloorTableId).Single();

        //        tbl.FloorPlan.UpdatedBy = User.Identity.GetUserId<long>()
        //        tbl.FloorPlan.UpdatedOn = DateTime.UtcNow;

        //        Db.SaveChanges();

        //        var array = new string[] { "Sofa", "Chair", "SofaTable", "Wall", "SolidWall", "GlassWall", "BarTable", "Fence", "Pillar" };

        //        var totalTables = Db.tabFloorTables.AsEnumerable().Where(p => !array.Contains(p.TableName.Split('-')[0]) && p.FloorPlanId == tbl.FloorPlanId).Count();

        //        return Json(new { Status = "success", ItemId = tbl.FloorTableId, totalTables = totalTables });
        //    }
        //    catch (Exception)
        //    {
        //        return Json(new { Status = "fail", ItemId = 0 });
        //    }
        //}

        //[HttpPost]
        //public JsonResult UpdateFloorItem(FloorTable table)
        //{
        //    try
        //    {
        //        var tbl = Db.tabFloorTables.Find(table.FloorTableId);

        //        tbl.FloorPlan.UpdatedBy = User.Identity.GetUserId<long>()
        //        tbl.FloorPlan.UpdatedOn = DateTime.UtcNow;


        //        tbl.TableName = table.TableName;
        //        tbl.TableDesign = table.TableDesign;
        //        tbl.Angle = table.Angle;
        //        tbl.MinCover = table.MinCover;
        //        tbl.MaxCover = table.MaxCover;
        //        tbl.TTop = table.TTop;
        //        tbl.TBottom = table.TBottom;
        //        tbl.TLeft = table.TLeft;
        //        tbl.TRight = table.TRight;
        //        tbl.UpdatedOn = DateTime.UtcNow;

        //        Db.Entry(tbl).State = EntityState.Modified;
        //        Db.SaveChanges();

        //        var array = new string[] { "Sofa", "Chair", "SofaTable", "Wall", "SolidWall", "GlassWall", "BarTable", "Fence", "Pillar" };

        //        var totalTables = Db.tabFloorTables.AsEnumerable().Where(p => !array.Contains(p.TableName.Split('-')[0]) && p.FloorPlanId == tbl.FloorPlanId).Count();

        //        return Json(new { Status = "success", ItemId = tbl.FloorTableId, totalTables = totalTables });
        //    }
        //    catch (Exception)
        //    {
        //        return Json(new { Status = "fail", ItemId = 0 });
        //    }
        //}

        //[HttpPost]
        //public JsonResult DeleteTable(Int64 id)
        //{
        //    try
        //    {
        //        var tbl = Db.tabFloorTables.Find(id);

        //        Db.tabFloorTables.Remove(tbl);
        //        Db.SaveChanges();

        //        return Json(new { Status = "success" });
        //    }
        //    catch (Exception)
        //    {
        //        return Json(new { Status = "fail" });
        //    }
        //}

        #endregion

        public JsonResult GetReservationUser(DateTime date, Int64 FloorPlanId, string shift, string time, string status)
        {
            ModelState.Clear();

            int sId = 0;

            Int64? statusId = null;
            if (!string.IsNullOrEmpty(status))
            {
                if (status.ToLower() == "upcoming")
                {
                    status = "partially-arrived";
                }
                else if (status.ToLower() == "seated")
                {
                    status = "seated";
                }
                statusId = Db.Status.Where(p => p.StatusName.ToLower() == status.ToLower()).SingleOrDefault().StatusId;
            }

            if (!string.IsNullOrEmpty(shift) && shift.Trim().ToLower() != "all")
            {
                sId = Db.tabFoodMenuShift.Single(p => p.MenuShift == shift).FoodMenuShiftId;
            }

            var record = Db.tabReservations.Include("FloorTable").Where(r => !r.IsDeleted).Where(p => p.ReservationDate == date && p.FloorPlanId == FloorPlanId && (statusId == null ? true : p.StatusId == statusId)).ToList();

            if (sId != 0)
            {
                record = record.Where(p => p.FoodMenuShiftId == sId).ToList();
            }

            if (!string.IsNullOrEmpty(time))
            {
                time = time.Trim();
                var startTime = date.Date.Add(Convert.ToDateTime(time).TimeOfDay);
                record = record.Where(p => p.TimeForm == startTime).ToList();
            }

            var result = record.Select(p => new
            {
                resId = p.ReservationId,
                name = p.Customers.FirstName + " " + p.Customers.LastName,
                Covers = p.Covers,
                TableName = p.FloorTable.TableName,
                status = p.Status == null ? "Not-confirmed" : p.Status.StatusName,
                time = p.TimeForm.ToString("hh:mm tt"),
                popUp = this.RenderPartialViewToString("~/Views/Floor/ReservationEditPopUpPartial.cshtml", p)
            }).OrderBy(u => u.time).ToList();


            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetWaitingUsers(DateTime seletedDate, DateTime? date, Int64 FloorPlanId, string shift)
        {
            if (date.HasValue && seletedDate.Date == DateTime.UtcNow.Date)
            {
                int sId = 0;

                if (shift.Trim().ToLower() != "all")
                {
                    sId = Db.tabFoodMenuShift.Single(p => p.MenuShift == shift).FoodMenuShiftId;
                }

                var record = Db.tabReservations.Include("FloorTable").Where(r => !r.IsDeleted).Where(p => p.FloorPlanId == FloorPlanId).AsEnumerable().Where(p => p.ReservationDate == date.Value.Date && p.TimeForm > date.Value && p.TimeForm < date.Value.AddMinutes(15)).ToList();

                if (sId != 0)
                {
                    record = record.Where(p => p.FoodMenuShiftId == sId).ToList();
                }

                var result = record.Select(p => new
                {
                    name = p.Customers.FirstName + " " + p.Customers.LastName,
                    Covers = p.Covers,
                    TableName = p.FloorTable.TableName,
                    time = p.TimeForm.ToString("hh:mm tt"),
                    popUp = this.RenderPartialViewToString("ReservationEditPopUpPartial", p)
                }).OrderBy(u => u.time).ToList();

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ReservationSave(DateTime date, string fname, string lname, string shift, string mobileNo, string email, int cover, string time, Int64 tableId, Int64 floorId, string TopTablePosition, string LeftTablePosition)
        {
            email = email.Trim();
            mobileNo = mobileNo.Trim();
            shift = shift.Trim();

            var tt = time.Split('-');

            var startTime = date.Add(DateTime.ParseExact(tt[0].Trim(), "ddMMyyyyhhmmtt", CultureInfo.InvariantCulture).TimeOfDay);
            var endTime = date.Add(DateTime.ParseExact(tt[1].Trim(), "ddMMyyyyhhmmtt", CultureInfo.InvariantCulture).TimeOfDay);

            var customer = Db.tabCustomers.Where(c => c.PhoneNumbers.Any(cn => cn.PhoneNumbers.Contains(mobileNo))).FirstOrDefault();

            int shiftId = Convert.ToInt32(tt[2]);
            //if (shift == "All")
            //{
            //    var sh = Db.tabMenuShiftHours.AsEnumerable();

            //    if (endTime.Date == startTime.AddDays(1).Date)
            //    {
            //        DateTime openAt = new DateTime();
            //        DateTime closeAt = new DateTime();

            //        //var shifts = sh.Where(s => (DateTime.TryParse(s.OpenAt, out openAt) && (startTime.TimeOfDay >= openAt.TimeOfDay)) && (DateTime.TryParse(s.CloseAt, out closeAt) && (startTime.TimeOfDay <= closeAt.AddMinutes(-15).TimeOfDay))).FirstOrDefault();
            //        shiftId = 1;
            //    }
            //    else
            //{
            //        DateTime openAt = new DateTime();
            //        DateTime closeAt = new DateTime();
            //        var shifts = sh.Where(s => (DateTime.TryParse(s.OpenAt, out openAt) && (startTime.TimeOfDay >= openAt.TimeOfDay)) && (DateTime.TryParse(s.CloseAt, out closeAt) && (startTime.TimeOfDay <= closeAt.AddMinutes(-15).TimeOfDay))).FirstOrDefault();

            //        shiftId = shifts.FoodMenuShiftId;
            //    }
            //}
            //else
            //{
            //    shiftId = Db.tabFoodMenuShift.Where(s => s.MenuShift.Contains(shift)).Single().FoodMenuShiftId;
            //}

            if (customer != null)
            {
                var reservation = new Reservation()
                {
                    FloorPlanId = floorId,
                    Covers = cover,
                    CustomerId = customer.CustomerId,
                    FoodMenuShiftId = shiftId,
                    ReservationDate = date,
                    TimeForm = startTime,
                    TimeTo = endTime,
                    FloorTableId = tableId,
                    StatusId = 13,
                    UserId = User.Identity.GetUserId<long>(),
                    TablePositionLeft = LeftTablePosition,
                    TablePositionTop = TopTablePosition
                };

                Db.tabReservations.Add(reservation);

            }
            else
            {
                var cust = new Customers()
                {
                    FirstName = fname,
                    LastName = lname,
                    DateCreated = DateTime.UtcNow,
                    DateOfBirth = DateTime.UtcNow,
                    Address1 = "1",
                    Address2 = "2",
                    Anniversary = DateTime.UtcNow,
                };

                Db.tabCustomers.Add(cust);

                if (!string.IsNullOrEmpty(email))
                {
                    var cemail = new CustomersEmails()
                    {
                        CustomerId = cust.CustomerId,
                        Email = email,
                        EmailTypeId = 1
                    };
                    Db.tabCustomersEmails.Add(cemail);
                }


                var cphone = new CustomersPhoneNumbers()
                {
                    CustomerId = cust.CustomerId,
                    PhoneNumbers = mobileNo,
                    PhoneTypeId = 1
                };

                Db.tabCustomersPhoneNumbers.Add(cphone);

                var reservation = new Reservation()
                {
                    FloorPlanId = floorId,
                    Covers = cover,
                    CustomerId = cust.CustomerId,
                    FoodMenuShiftId = shiftId,
                    ReservationDate = date,
                    TimeForm = startTime,
                    TimeTo = endTime,
                    FloorTableId = tableId,
                    StatusId = 13,
                    UserId = User.Identity.GetUserId<long>(),
                    TablePositionLeft = LeftTablePosition,
                    TablePositionTop = TopTablePosition
                };
                Db.tabReservations.Add(reservation);
            }

            Db.SaveChanges();
            return null;
        }

        private void ChangeChairColor(AISModels.FloorTable table)
        {
            var design = table.TableDesign;
            var section = table.Section;

            if (section != null)
            {
                var sectionColor = section.Color;

                var seats = this.GetMatchedTagsFromHtml(design, String.Format("<div[^>]*?class=([\"'])[^>]*{0}[^>]*\\1[^>]*>(\r*?\n*?)(.*?)</div>", "seat "));

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

        // Old Reservation code to show Popup , assign section color and Show reservation for a Table.

        //private void CheckReservations(AISModels.FloorTable table, DateTime? startTime, DateTime? endTime)
        //{
        //    var design = table.TableDesign;

        //    var TableName = this.GetMatchedTagsFromHtml(design, String.Format("<h3>(.*?)</h3>")).FirstOrDefault();

        //    if (!string.IsNullOrEmpty(TableName))
        //    {
        //        Reservation reservation = null;

        //        if (startTime.HasValue)
        //        {
        //            if (startTime.HasValue && endTime.HasValue)
        //            {
        //                reservation = table.Reservations.Where(r => r.TimeForm <= startTime.Value && r.TimeTo >= endTime.Value).FirstOrDefault();
        //            }
        //            else
        //            {
        //                var start = startTime.Value.Date.AddTicks(DateTime.UtcNow.TimeOfDay.Ticks);
        //                reservation = table.Reservations.Where(r => r.TimeForm <= start && r.TimeTo >= start).FirstOrDefault();
        //            }
        //        }
        //        else
        //        {
        //            reservation = table.Reservations.Where(r => r.TimeForm <= DateTime.UtcNow && r.TimeTo >= DateTime.UtcNow).FirstOrDefault();
        //        }

        //        if (reservation != null)
        //        {
        //            //table.TableDesign = design.Replace(TableName, TableName + "<img alt=\"\" src=\"/images/red-s.png\" class=\"table-img\">");

        //            design = design.Replace(TableName, TableName + "<img alt=\"\" src=\"/images/red-s.png\" class=\"table-img\">");

        //            var regex = new Regex(@"([\w-]+)\s*:\s*([^;]+)");
        //            var match = regex.Match(GetMatchedTagsFromHtml(design, String.Format("<div[^>]*?class=([\"'])[^>]*{0}[^>]*\\1[^>]*>", "table-main")).FirstOrDefault());
        //            while (match.Success)
        //            {
        //                var key = match.Groups[1].Value;
        //                var value = match.Groups[2].Value;
        //                if (key == "top")
        //                {
        //                    design = design.Replace(value, reservation.TablePositionTop);
        //                }

        //                if (key == "left")
        //                {
        //                    design = design.Replace(value, reservation.TablePositionLeft);
        //                }

        //                match = match.NextMatch();
        //            }

        //            var popup = this.RenderPartialViewToString("ReservationPopupPartial", reservation);

        //            StringBuilder designer = new StringBuilder();
        //            designer.Append(design.Substring(0, design.Length - 6));
        //            designer.Append(popup + "</div>");


        //            table.TableDesign = designer.ToString();
        //        }
        //        else
        //        {
        //            table.TableDesign = design.Replace(TableName, TableName + "<img alt=\"\" src=\"/images/green-a.png\" class=\"table-img\">");
        //        }
        //    }
        //}

        private void CheckReservations(AISModels.FloorTable table, DateTime? startTime, DateTime? endTime, int? shiftId, out int coverCount)
        {
            coverCount = 0;
            var design = table.TableDesign;

            var TableName = this.GetMatchedTagsFromHtml(design, String.Format("<h3(.*?)>(.*?)</h3>")).FirstOrDefault();

            if (!string.IsNullOrEmpty(TableName))
            {
                IEnumerable<Reservation> reservation = table.Reservations.Where(r => !r.IsDeleted && r.FloorPlanId == table.FloorPlanId).AsEnumerable();

                if (!shiftId.HasValue)
                {
                    if (startTime.HasValue)
                    {
                        if (startTime.HasValue && endTime.HasValue)
                        {
                            reservation = reservation.Where(r => r.TimeForm <= startTime.Value && r.TimeTo >= endTime.Value).AsEnumerable();
                        }
                        else
                        {
                            var start = startTime.Value.Date.AddTicks(DateTime.UtcNow.TimeOfDay.Ticks);
                            reservation = reservation.Where(r => r.TimeForm <= start && r.TimeTo >= start).AsEnumerable();
                        }
                    }
                    else
                    {
                        reservation = reservation.Where(r => r.TimeForm <= DateTime.UtcNow && r.TimeTo >= DateTime.UtcNow).AsEnumerable();
                    }
                }
                else
                {
                    if (startTime.HasValue)
                    {
                        reservation = reservation.Where(r => r.ReservationDate == startTime.Value.Date).AsEnumerable();
                    }
                    else
                    {
                        reservation = reservation.Where(r => r.ReservationDate == DateTime.UtcNow.Date).AsEnumerable();
                    }

                    if (shiftId.Value != 0)
                    {
                        reservation = reservation.Where(r => r.FoodMenuShiftId == shiftId.Value).AsEnumerable();
                    }
                }

                if (reservation.Count() > 0)
                {
                    //table.TableDesign = design.Replace(TableName, TableName + "<img alt=\"\" src=\"/images/red-s.png\" class=\"table-img\">");

                    design = design.Replace(TableName, TableName + "<img alt=\"\" src=\"/images/red-s.png\" class=\"table-img\">");

                    //var regex = new Regex(@"([\w-]+)\s*:\s*([^;]+)");
                    //var match = regex.Match(GetMatchedTagsFromHtml(design, String.Format("<div[^>]*?class=([\"'])[^>]*{0}[^>]*\\1[^>]*>", "table-main")).FirstOrDefault());
                    //while (match.Success)
                    //{
                    //    var key = match.Groups[1].Value;
                    //    var value = match.Groups[2].Value;
                    //    if (key == "top")
                    //    {
                    //        design = design.Replace(value, reservation.TablePositionTop);
                    //    }

                    //    if (key == "left")
                    //    {
                    //        design = design.Replace(value, reservation.TablePositionLeft);
                    //    }

                    //    match = match.NextMatch();
                    //}

                    var popup = this.RenderPartialViewToString("ReservationListPartial", reservation);

                    StringBuilder designer = new StringBuilder();
                    designer.Append(design.Substring(0, design.LastIndexOf("</div>")));
                    designer.Append(popup + "</div>");

                    table.TableDesign = designer.ToString();

                    coverCount = reservation.Sum(r => r.Covers);
                }
                else
                {
                    table.TableDesign = design.Replace(TableName, TableName + "<img alt=\"\" src=\"/images/green-a.png\" class=\"table-img\">");
                }
            }
        }

        private IEnumerable<string> GetMatchedTagsFromHtml(string sourceString, string subString) //((<div.*>)(.*)(<\\/div>))* -> Regex for Div tags
        {
            return System.Text.RegularExpressions.Regex.Matches(sourceString, subString).Cast<System.Text.RegularExpressions.Match>().Select(m => m.Value);
        }

        public JsonResult updateFloorTableAndLevel(string floorTable, string floorLevel)
        {
            try
            {
                var fl = (new JavaScriptSerializer()).Deserialize<List<updateFloorPlanLevelVM>>(floorLevel);
                var fT = (new JavaScriptSerializer()).Deserialize<List<updateFloorTableVM>>(floorTable);


                foreach (var item in fl)
                {
                    var fp = Db.tabFloorPlans.Single(p => p.FloorPlanId == item.floorPlanId);

                    fp.FLevel = item.level;
                    fp.UpdatedOn = DateTime.UtcNow;
                    fp.UpdatedBy = User.Identity.GetUserId<long>();
                }

                foreach (var item in fT)
                {
                    Db.tabFloorTables.Single(p => p.FloorTableId == item.floorTableId).IsTemporary = item.isTemp;
                }

                Db.SaveChanges();

                ClearFloorCache();

                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult updateFloorActive(Int64 floorPlanId)
        {
            try
            {
                Db.Database.ExecuteSqlCommand("UPDATE FloorPlan SET IsActive='false';");

                Db.tabFloorPlans.Single(p => p.FloorPlanId == floorPlanId).IsActive = true;

                Db.SaveChanges();
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
                throw;
            }
            finally
            {
                ClearFloorCache();
            }
        }

        private void AttachFloorItemHtmlDesign(FloorPlan plan)
        {
            var floorItems = plan.FloorTables;

            foreach (var item in floorItems)
            {
                item.TableDesign = this.RenderPartialViewToString("GetFloorItemTemplate", item);
            }
        }

        private void ClearFloorCache()
        {
            //cache.RemoveByPattern(CacheKeys.FLOOR_TABLES_SCREEN_PATTREN);
            cache.RemoveByPattern(string.Format(CacheKeys.FLOOR_TABLES_SCREEN_COMPANY_PATTREN,User.Identity.GetDatabaseName()));
        }

        protected override void Dispose(bool disposing)
        {
            Db.Dispose();
            base.Dispose(disposing);
        }
    }
}
