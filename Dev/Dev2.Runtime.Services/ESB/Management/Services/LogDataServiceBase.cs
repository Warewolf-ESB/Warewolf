#pragma warning disable
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Web;
using Dev2.Common.Interfaces.Enums;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class LogDataServiceBase
    {
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Administrator;
        T GetValue<T>(string key, Dictionary<string, StringBuilder> values)
        {
            var toReturn = default(T);
            if (values.TryGetValue(key, out StringBuilder value))
            {
                var item = value.ToString();
                return (T)Convert.ChangeType(item, typeof(T));
            }
            return toReturn;
        }
    }
}