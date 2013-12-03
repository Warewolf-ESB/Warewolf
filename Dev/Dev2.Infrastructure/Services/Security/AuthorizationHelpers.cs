using System;

namespace Dev2.Services.Security
{
    public static class AuthorizationHelpers
    {
        public static string GetReason(this AuthorizationContext value, bool isAuthorized)
        {
            if(isAuthorized)
            {
                return string.Empty;
            }

            var field = value.GetType().GetField(value.ToString());
            var attribute = Attribute.GetCustomAttribute(field, typeof(ReasonAttribute)) as ReasonAttribute;

            return attribute == null ? string.Empty : attribute.Reason;
        }
    }
}