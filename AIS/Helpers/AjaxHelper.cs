using System.Web.Mvc;
using System.Web.Mvc.Ajax;

namespace AIS.Helpers
{
    public static class ImageActionLinkHelper
    {
        public static string ImageActionLink(this AjaxHelper helper, string imageUrl, string altText, string titleText, string actionName, string controllerName,
            object routeValues, AjaxOptions ajaxOptions, object htmlAttributes = null)
        {
            var builder = new TagBuilder("img");
            builder.MergeAttribute("src", imageUrl);
            builder.MergeAttribute("alt", altText);
            builder.MergeAttribute("title", titleText);
            var link = helper.ActionLink("[replaceme]", actionName, controllerName, routeValues, ajaxOptions, htmlAttributes);
            return link.ToHtmlString().Replace("[replaceme]", builder.ToString(TagRenderMode.SelfClosing));
        }
    }
}