using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AIS
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");


            routes.MapRoute(
              name: "DefaultOnline",
              url: "{company}/Online",
              defaults: new { controller = "Online", action = "Index" }
            );

            routes.MapRoute(
           name: "DefaultLogin",
           url: "Login",
           defaults: new { controller = "Account", action = "Login" }
         );
               routes.MapRoute(
           name: "DefaultRegister",
           url: "Register",
           defaults: new { controller = "Account", action = "Register" }
         );

               routes.MapRoute(
                  name: "Default",
                  url: "{controller}/{action}/{id}",
                  defaults: new { controller = "Mondofi", action = "Index", id = UrlParameter.Optional }
              );

          
        }
    }
}
