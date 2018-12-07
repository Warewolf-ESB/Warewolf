using System.Web.Mvc;
using System.Web.Routing;

namespace Dev2.Web2
{
    public class RouteConfig
    {
        protected RouteConfig()
        {
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Audit", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
