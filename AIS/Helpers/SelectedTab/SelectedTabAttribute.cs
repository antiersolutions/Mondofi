using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AIS.Helpers.Fakes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class SelectedTabAttribute : ActionFilterAttribute
    {
        private string _selectedTab;

        public SelectedTabAttribute(string selectedTab)
        {
            this._selectedTab = selectedTab;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpCookie selectedTabCookie = new HttpCookie("SelectedTab");

            selectedTabCookie.Value = _selectedTab;
            selectedTabCookie.Expires = DateTime.UtcNow.AddMonths(-1);

            // Add the cookie.
            filterContext.HttpContext.Response.Cookies.Add(selectedTabCookie);
            
            base.OnActionExecuting(filterContext);
        }

    }
}
