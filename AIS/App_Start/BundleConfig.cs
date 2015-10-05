using System.Web;
using System.Web.Optimization;

namespace AIS
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        //public static void RegisterBundles(BundleCollection bundles)
        //{
        //    bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
        //                "~/Scripts/jquery-{version}.js"));

        //    bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
        //                "~/Scripts/jquery.validate*"));

        //    // Use the development version of Modernizr to develop with and learn from. Then, when you're
        //    // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
        //    bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
        //                "~/Scripts/modernizr-*"));

        //    bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
        //              "~/Scripts/bootstrap.js",
        //              "~/Scripts/respond.js"));

        //    bundles.Add(new StyleBundle("~/Content/css").Include(
        //              "~/Content/bootstrap.css",
        //              "~/Content/site.css"));

        //    // Set EnableOptimizations to false for debugging. For more information,
        //    // visit http://go.microsoft.com/fwlink/?LinkId=301862
        //    BundleTable.EnableOptimizations = true;
        //}

        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*",
                        "~/Scripts/Extensions/ValidationExtensions.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                        "~/Content/themes/base/jquery.ui.core.css",
                        "~/Content/themes/base/jquery.ui.resizable.css",
                        "~/Content/themes/base/jquery.ui.selectable.css",
                        "~/Content/themes/base/jquery.ui.accordion.css",
                        "~/Content/themes/base/jquery.ui.autocomplete.css",
                        "~/Content/themes/base/jquery.ui.button.css",
                        "~/Content/themes/base/jquery.ui.dialog.css",
                        "~/Content/themes/base/jquery.ui.slider.css",
                        "~/Content/themes/base/jquery.ui.tabs.css",
                        "~/Content/themes/base/jquery.ui.datepicker.css",
                        "~/Content/themes/base/jquery.ui.progressbar.css",
                        "~/Content/themes/base/jquery.ui.theme.css"));
            bundles.Add(new StyleBundle("~/AIS/css").Include(
                "~/css/style.css",
                "~/css/jquery.onoff.css",
                "~/css/jquery.mCustomScrollbar.css",
                "~/css/CustomPopUp.css"));

            bundles.Add(new StyleBundle("~/AIS/alertifyCSS").Include(
                        "~/Content/alertifyjs/css/alertify.css",
                        "~/Content/alertifyjs/css/alertify.min.css",
                        "~/Content/alertifyjs/css/themes/default.css",
                        "~/Content/alertifyjs/css/themes/default.min.css"));

            bundles.Add(new ScriptBundle("~/AIS/jquery").Include(
                "~/js/jquery.mCustomScrollbar.concat.min.js"));
            //"~/js/jquery.mCustomScrollbar.concat.js"));

            bundles.Add(new ScriptBundle("~/AIS/alertifyJS").Include(
                        "~/Content/alertifyjs/alertify.js",
                        "~/Content/alertifyjs/alertify.min.js"));
        }
    }
}
