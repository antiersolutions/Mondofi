using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Drawing;
using AIS.Models;
using System.Data;
using System.Data.Entity;

namespace AIS.Controllers
{
    [Authorize]
    public class UploadFileController : Controller
    {
        UsersContext db = new UsersContext();
        //
        // GET: /UploadFile/

        [HttpPost]
        [AllowAnonymous]
        public string uploadimage(HttpPostedFileBase fileData)
        {
            if (fileData != null)
            {
                var filename = "";
                if (fileData != null && fileData.ContentLength > 0)
                {
                    if (fileData.ContentLength < 524288)
                    {
                        var FileExtension = Path.GetExtension(fileData.FileName).Substring(1);
                        if (FileExtension.ToLower() == "jpg" || FileExtension.ToLower() == "png" || FileExtension.ToLower() == "gif")
                        {
                            var img = Image.FromStream(fileData.InputStream, true, true);

                            if (img.Height != img.Width)
                            {
                                return "resolution";
                            }

                            filename = Path.GetFileName(fileData.FileName);
                            string uid = getUniqueID();
                            filename = uid + "_" + filename;
                            var path = Path.Combine(Server.MapPath("~/Content/UserData"), filename);
                            fileData.SaveAs(path);

                            return filename.ToString();
                        }
                        else
                        {
                            return "extentions";

                        }
                    }
                    else
                    {
                        return "size";
                    }
                }
                return filename.ToString();
            }
            else
            {
                return "error";
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public string UploadFloorBackground(Int64 userId, Int64? TempFloorId, Int64? FloorId, HttpPostedFileBase fileData)
        {
            if (fileData != null)
            {
                var filename = "";
                if (fileData != null && fileData.ContentLength > 0)
                {
                    var location = Server.MapPath("~/Content/UserData/" + userId);

                    if (!Directory.Exists(location))
                    {
                        Directory.CreateDirectory(location);
                    }

                    if (fileData.ContentLength < 4147228)
                    {
                        var FileExtension = Path.GetExtension(fileData.FileName).Substring(1);
                        if (FileExtension.ToLower() == "jpg" || FileExtension.ToLower() == "png" || FileExtension.ToLower() == "gif")
                        {
                            var img = Image.FromStream(fileData.InputStream, true, true);

                            if (img.Height != 768 || img.Width != 1024)
                            {
                                return "resolution";
                            }

                            filename = Path.GetFileName(fileData.FileName);
                            string uid = getUniqueID();
                            filename = uid + "_" + filename;
                            var path = Path.Combine(location, filename);
                            fileData.SaveAs(path);

                            if (TempFloorId.HasValue)
                            {
                                var Floor = db.tabTempFloorPlans.Find(TempFloorId.Value);

                                if (!string.IsNullOrEmpty(Floor.PhotoPath))
                                {
                                    System.IO.File.Delete(Server.MapPath(Floor.PhotoPath));
                                }

                                Floor.PhotoPath = "/Content/UserData/" + userId + "/" + filename.ToString();
                                db.Entry(Floor).State = EntityState.Modified;
                            }

                            if (FloorId.HasValue)
                            {
                                var Floor = db.tabFloorPlans.Find(FloorId.Value);

                                if (!string.IsNullOrEmpty(Floor.PhotoPath))
                                {
                                    System.IO.File.Delete(Server.MapPath(Floor.PhotoPath));
                                }

                                Floor.PhotoPath = "/Content/UserData/" + userId + "/" + filename.ToString();
                                db.Entry(Floor).State = EntityState.Modified;
                            }

                            db.SaveChanges();

                            return filename.ToString();
                        }
                        else
                        {
                            return "extentions";

                        }
                    }
                    else
                    {
                        return "size";
                    }
                }

                return "/Content/UserData/" + userId + "/" + filename.ToString();
            }
            else
            {
                return "error";
            }
        }



        [HttpPost]
        [AllowAnonymous]
        public string uploadLogoimage(HttpPostedFileBase fileData)
        {
            if (fileData != null)
            {
                var filename = "";
                if (fileData != null && fileData.ContentLength > 0)
                {
                    if (fileData.ContentLength < 524288)
                    {
                        var FileExtension = Path.GetExtension(fileData.FileName).Substring(1);
                        if (FileExtension.ToLower() == "jpg" || FileExtension.ToLower() == "png" || FileExtension.ToLower() == "gif")
                        {
                            var img = Image.FromStream(fileData.InputStream, true, true);

                            if (img.Height != 91 && img.Width!=114 )
                            {
                                return "resolution";
                            }

                            filename = Path.GetFileName(fileData.FileName);
                            string uid = getUniqueID();
                            filename = uid + "_" + filename;
                            var path = Path.Combine(Server.MapPath("~/Content/UserData"), filename);
                            var logoPath = "/Content/UserData/"+filename;
                            var logoSetting = db.tabSettings.Where(s => s.Name.Contains("Logo")).Single();
                            logoSetting.Value = logoPath;
                            db.Entry(logoSetting).State = System.Data.Entity.EntityState.Modified;

                            db.SaveChanges();

                            fileData.SaveAs(path);

                            return filename.ToString();
                        }
                        else
                        {
                            return "extentions";

                        }
                    }
                    else
                    {
                        return "size";
                    }
                }
                return filename.ToString();
            }
            else
            {
                return "error";
            }
        }



        [HttpPost]
        [AllowAnonymous]
        public string uploadLogoimageReser(HttpPostedFileBase fileData)
        {
            if (fileData != null)
            {
                var filename = "";
                if (fileData != null && fileData.ContentLength > 0)
                {
                    if (fileData.ContentLength < 524288)
                    {
                        var FileExtension = Path.GetExtension(fileData.FileName).Substring(1);
                        if (FileExtension.ToLower() == "jpg" || FileExtension.ToLower() == "png" || FileExtension.ToLower() == "gif")
                        {
                            var img = Image.FromStream(fileData.InputStream, true, true);

                            if (img.Height != 95 && img.Width != 202)
                            {
                                return "resolution";
                            }

                            filename = Path.GetFileName(fileData.FileName);
                            string uid = getUniqueID();
                            filename = uid + "_" + filename;
                            var path = Path.Combine(Server.MapPath("~/Content/UserData"), filename);
                            var logoPath = "/Content/UserData/" + filename;
                            var logoSetting = db.tabSettings.Where(s => s.Name.Contains("OnlineResosL")).Single();
                            logoSetting.Value = logoPath;
                            db.Entry(logoSetting).State = System.Data.Entity.EntityState.Modified;

                            db.SaveChanges();

                            fileData.SaveAs(path);

                            return filename.ToString();
                        }
                        else
                        {
                            return "extentions";

                        }
                    }
                    else
                    {
                        return "size";
                    }
                }
                return filename.ToString();
            }
            else
            {
                return "error";
            }
        }


        private string getUniqueID()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            return BitConverter.ToUInt32(buffer, 8).ToString();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
