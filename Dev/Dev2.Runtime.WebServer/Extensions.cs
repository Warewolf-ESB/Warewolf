using System;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using Dev2.Common;

namespace Dev2.Runtime.WebServer
{
    public static class Extensions
    {
        public static bool IsAuthenticated(this IPrincipal user)
        {
            return user != null && user.Identity.IsAuthenticated;
        }

        public static Encoding GetContentEncoding(this HttpContent content)
        {
            var encoding = content == null ? String.Empty : content.Headers.ContentEncoding.FirstOrDefault();
            if(!String.IsNullOrEmpty(encoding))
            {
                try
                {
                    return Encoding.GetEncoding(encoding);
                }
                catch(Exception ex)
                {
                    ServerLogger.LogError(ex);
                }
            }
            return Encoding.UTF8;
        }

        public static string GetResourceID(this HttpRequestMessage request)
        {
            var rid = request.GetQueryNameValuePairs().FirstOrDefault(p => "rid".Equals(p.Key, StringComparison.InvariantCultureIgnoreCase));
            return string.IsNullOrEmpty(rid.Value) ? null : rid.Value;
        }
    }
}
