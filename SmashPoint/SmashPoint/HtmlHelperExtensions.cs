using Microsoft.AspNetCore.Mvc.Rendering;

namespace SmashPoint
{
    public static class HtmlHelperExtensions
    {
        public static string GetSessionValue(this IHtmlHelper htmlHelper, string key)
        {
            var session = htmlHelper.ViewContext.HttpContext.Session;
            return session.GetString(key);
        }
    }
}
