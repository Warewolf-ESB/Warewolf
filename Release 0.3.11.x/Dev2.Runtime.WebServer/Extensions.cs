using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using Dev2.Common;

namespace Dev2.Runtime.WebServer
{
    public static class Extensions
    {
        public static Encoding GetContentEncoding(this HttpContent content)
        {
            var encoding = content == null ? string.Empty : content.Headers.ContentEncoding.FirstOrDefault();
            if(!string.IsNullOrEmpty(encoding))
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
    }
}
