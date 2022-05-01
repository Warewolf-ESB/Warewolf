#if !NETFRAMEWORK
using System.Web.Http;
using Owin;

namespace Dev2.Web
{
    public class Startup
    {
        public static void Configuration(IAppBuilder builder)
        {
            var config = new HttpConfiguration();
            config.Routes.IgnoreRoute("Default", "{resource}.axd/{*pathInfo}");
            config.Routes.MapHttpRoute("Default", "{controller}/{action}/{id}", new { controller = "Audit", action = "Index", id = RouteParameter.Optional });
            builder.UseWebApi(config);
        }
    }
}
#endif