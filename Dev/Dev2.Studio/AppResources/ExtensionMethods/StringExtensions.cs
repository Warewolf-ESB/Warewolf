using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dev2.Studio.AppResources.ExtensionMethods
{
    public static class StringExtensions
    {
        public static string GetManagementPayload(this string payload)
        {
            if (payload.Contains("<Dev2System.ManagmentServicePayload>"))
            {
                var startIndx = payload.IndexOf("<Dev2System.ManagmentServicePayload>", StringComparison.Ordinal);
                var length = "<Dev2System.ManagmentServicePayload>".Length;
                var endIndx = payload.IndexOf("</Dev2System.ManagmentServicePayload>", StringComparison.Ordinal);
                var l = endIndx - startIndx - length;
                return payload.Substring(startIndx + length, l);
            }
            return string.Empty;
        }
    }
}
