using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Web.Mvc;
using AIS.Models;
using System.Web;
using System.Linq;

namespace AIS.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class CheckClientTimeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Request.Cookies.AllKeys.Contains("timezoneoffset"))
            {
                filterContext.HttpContext.Session["timezoneoffset"] = filterContext.HttpContext.Request.Cookies["timezoneoffset"].Value;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}