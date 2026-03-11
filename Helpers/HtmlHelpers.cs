using Microsoft.AspNetCore.Mvc.Rendering;

namespace Sandoohouse.Helpers;

public static class HtmlHelpers
{
    public static string IsActive(this IHtmlHelper htmlHelper, string controller, string action = null)
    {
        var routeData = htmlHelper.ViewContext.RouteData;

        var currentController = routeData.Values["controller"]?.ToString();
        var currentAction = routeData.Values["action"]?.ToString();

        if (action == null)
            return controller == currentController ? "active" : "";

        return controller == currentController && action == currentAction ? "active" : "";
    }
}