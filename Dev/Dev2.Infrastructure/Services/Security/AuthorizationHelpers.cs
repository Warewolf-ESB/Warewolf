using System;

namespace Dev2.Services.Security
{
    public static class AuthorizationHelpers
    {
        public static string GetReason(this AuthorizationContext value, bool isAuthorized)
        {
            //
            // MUST return null and NOT empty string as the result is used as TargetNullValue in bindings!
            //
            if(isAuthorized)
            {
                return null;
            }

            var field = value.GetType().GetField(value.ToString());
            var attribute = Attribute.GetCustomAttribute(field, typeof(ReasonAttribute)) as ReasonAttribute;

            return attribute == null || string.IsNullOrEmpty(attribute.Reason) ? null : attribute.Reason;
        }
    }
}