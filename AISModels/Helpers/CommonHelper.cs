using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using AIS.Models;
using System.Web;

namespace AIS.Helpers
{
    public static class CommonHelper
    {
        public static string getCurrentUserConnectionString()
        {
            string dataBaseName = null;
            string _serverName = System.Web.Configuration.WebConfigurationManager.AppSettings["serverName"].ToString();
            string _userName = System.Web.Configuration.WebConfigurationManager.AppSettings["userName"].ToString();
            string _password = System.Web.Configuration.WebConfigurationManager.AppSettings["localpassword"].ToString();

            if (HttpContext.Current != null)
            {
                try
                {
                    dataBaseName = HttpContext.Current.GetOwinContext().Authentication.User.Identity.GetDatabaseName();


                    return "Data Source=" + _serverName + ";Initial Catalog=" + dataBaseName + ";User ID=" + _userName + ";Password=" + _password + ";";
                }
                catch
                {
                    return "Data Source=" + _serverName + ";Initial Catalog=" + dataBaseName + ";User ID=" + _userName + ";Password=" + _password + ";";
                }
            }
            else
            {
                throw new Exception("HttpContext.Current is null");
            }
        }

        public static string getCurrentUserConnectionString(string dataBaseName)
        {
            string _serverName = System.Web.Configuration.WebConfigurationManager.AppSettings["serverName"].ToString();
            string _userName = System.Web.Configuration.WebConfigurationManager.AppSettings["userName"].ToString();
            string _password = System.Web.Configuration.WebConfigurationManager.AppSettings["localpassword"].ToString();
            //return "Data Source=DOTNET15;Initial Catalog=" + dataBaseName + ";User ID=sa;Password=1212";
            return "Data Source=" + _serverName + ";Initial Catalog=" + dataBaseName + ";User ID=" + _userName + ";Password=" + _password + ";";
        }

        //public static string getMasterConnectionString()
        //{
        //    return "Data Source=;Initial Catalog=" + enObj.Decrypt("Sf+HXLVZOGCm5OFUgNxxtap8cVzryVnMFMFJ09LqyrM=") + ";User ID=" + enObj.Decrypt("8MevBJ/wPuLRzxU0FYUi6w==") + ";Password=" + enObj.Decrypt("/qqZSBt1vdIlZRNR+9/+DA==") + ";";
        //}
    }
}
