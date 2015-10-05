using AIS.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WebMarkupMin.Mvc.ActionFilters;

namespace AIS
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            ViewEngines.Engines.Clear(); // Remove view engines
            var engine = new RazorViewEngine();
            //engine.ViewLocationCache = new TwoLevelViewCache(engine.ViewLocationCache);
            ViewEngines.Engines.Add(engine); //Added a new razor view engine

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ConfigureModeAttribute();
        }


        private void ConfigureModeAttribute()
        {
            var excludedFloorPlanActions = new string[] {
            "getjsonallreservationlist",
            "getendingreservationpopup",
            "updateextendedreservationtime",
            "getavailabletablesforupdate",
            "clearservercache"};

            var excludedStaffActions = new string[] { 
            "updateserver"};

            var excludedWaitingActions = new string[]{
            "deletewaiting",
            "savewaiting",
            "updatewaiting"};

            //Configure a conditional filter
            var conditions = new Func<ControllerContext, ActionDescriptor, object>[] {
                    ( c, a ) => !c.IsChildAction ? new CompressContentAttribute() : null,
                    
                    //FloorPlan Controller
                    ( c, a ) => a.ControllerDescriptor.ControllerName.Contains("FloorPlan") && !c.IsChildAction 
                        && !excludedFloorPlanActions.Contains( a.ActionName.ToLower())
                        ? new MinifyHtmlAttribute() : null,

                    //Staff Controller
                    ( c, a ) => a.ControllerDescriptor.ControllerName.Contains("Staff") && !c.IsChildAction 
                        && !excludedStaffActions.Contains( a.ActionName.ToLower())
                        ? new MinifyHtmlAttribute() : null,

                    //Waiting Controller
                    ( c, a ) => a.ControllerDescriptor.ControllerName.Contains("Waiting") && !c.IsChildAction 
                        && !excludedWaitingActions.Contains( a.ActionName.ToLower())
                        ? new MinifyHtmlAttribute() : null,
            };

            var provider = new ConditionalFilterProvider(conditions);

            // This line adds the filter we created above
            FilterProviders.Providers.Add(provider);
        }
    }
}
