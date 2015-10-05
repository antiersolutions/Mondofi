using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Text;
using AISModels;

namespace AIS.OnlineExtensions
{
    public static class ControllerExtensions
    {
        /// <summary>
        /// Renders the specified partial view to a string.
        /// </summary>
        /// <param name="controller">The current controller instance.</param>
        /// <param name="viewName">The name of the partial view.</param>
        /// <param name="model">The model.</param>
        /// <returns>The partial view as a string.</returns>
        public static string RenderPartialViewToString(this Controller controller, string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = controller.ControllerContext.RouteData.GetRequiredString("action");
            }

            controller.ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                // Find the partial view by its name and the current controller context.
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);

                // Create a view context.
                var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);

                // Render the view using the StringWriter object.
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }

        /// <summary>
        /// Renders the specified partial view to a string.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="model"></param>
        /// <param name="Context"></param>
        /// <returns></returns>
        public static string RenderPartialToString(string view, object model, ControllerContext Context)
        {
            if (string.IsNullOrEmpty(view))
            {
                view = Context.RouteData.GetRequiredString("action");
            }

            ViewDataDictionary ViewData = new ViewDataDictionary();

            TempDataDictionary TempData = new TempDataDictionary();

            ViewData.Model = model;

            using (StringWriter sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(Context, view);

                ViewContext viewContext = new ViewContext(Context, viewResult.View, ViewData, TempData, sw);

                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }
    }
}