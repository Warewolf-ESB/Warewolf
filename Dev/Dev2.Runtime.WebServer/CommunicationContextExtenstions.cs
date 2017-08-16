using Dev2.Common;

namespace Dev2.Runtime.WebServer
{
    internal static class CommunicationContextExtenstions
    {
        public static string GetServiceName(this ICommunicationContext ctx)
        {
            return ctx.Request.BoundVariables["servicename"];
        }
        
        public static string GetWorkspaceID(this ICommunicationContext ctx)
        {
            return ctx.Request.QueryString["wid"];
        }

        public static string GetDataListID(this ICommunicationContext ctx)
        {
            return ctx.Request.QueryString[GlobalConstants.DLID];
        }

        public static string GetBookmark(this ICommunicationContext ctx)
        {
            return ctx.Request.BoundVariables["bookmark"];
        }

        public static string GetInstanceID(this ICommunicationContext ctx)
        {
            return ctx.Request.BoundVariables["instanceid"];
        }

        public static string GetWebsite(this ICommunicationContext ctx)
        {
            return ctx.Request.BoundVariables["website"];
        }

        public static string GetPath(this ICommunicationContext ctx)
        {
            return ctx.Request.BoundVariables["path"];
        }

        public static string GetClassName(this ICommunicationContext ctx)
        {
            return ctx.Request.BoundVariables["name"];
        }

        public static string GetMethodName(this ICommunicationContext ctx)
        {
            return ctx.Request.BoundVariables["action"];
        }
    }
}
