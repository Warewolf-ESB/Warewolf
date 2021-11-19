using System.Net;
using System.Web.Mvc;
using System.Web.Optimization;
#if NETFRAMEWORK 
using System.Web.Routing;
#endif

namespace Dev2.Web2
{
    public class MvcApplication
#if NETFRAMEWORK 
        : System.Web.HttpApplication
#endif
    {
        protected
#if !NETFRAMEWORK
            static
#endif
            void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
#if NETFRAMEWORK
            RouteConfig.RegisterRoutes(RouteTable.Routes);
#endif
            BundleConfig.RegisterBundles(BundleTable.Bundles);            
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;            
        }
    }
}