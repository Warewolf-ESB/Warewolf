#pragma warning disable
 using Dev2.Common;

namespace Dev2.Runtime.WebServer
{
    static class CommunicationContextExtenstions
    {
        public static string GetServiceName(this ICommunicationContext ctx) => ctx.Request.BoundVariables["servicename"];

        public static string GetWorkspaceID(this ICommunicationContext ctx)
        {
            return ctx.Request.QueryString["wid"];
        }

        public static string GetDataListID(this ICommunicationContext ctx) => ctx.Request.QueryString[GlobalConstants.DLID];

        public static string GetBookmark(this ICommunicationContext ctx) => ctx.Request.BoundVariables["bookmark"];

        public static string GetInstanceID(this ICommunicationContext ctx) => ctx.Request.BoundVariables["instanceid"];

        public static string GetWebsite(this ICommunicationContext ctx) => ctx.Request.BoundVariables["website"];

        public static string GetPath(this ICommunicationContext ctx) => ctx.Request.BoundVariables["path"];

        public static string GetClassName(this ICommunicationContext ctx) => ctx.Request.BoundVariables["name"];

        public static string GetMethodName(this ICommunicationContext ctx) => ctx.Request.BoundVariables["action"];
    }
}
