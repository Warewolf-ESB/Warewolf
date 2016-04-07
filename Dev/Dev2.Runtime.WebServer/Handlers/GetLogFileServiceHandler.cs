using Dev2.Common;
using Dev2.Runtime.WebServer.Responses;

namespace Dev2.Runtime.WebServer.Handlers
{
    public class GetLogFileServiceHandler : AbstractWebRequestHandler
    {

        public override void ProcessRequest(ICommunicationContext ctx)
        {
            var result = GetFileFromPath(EnvironmentVariables.ServerLogFile);

            ctx.Send(result);
        }

        static IResponseWriter GetFileFromPath(string filePath)
        {
            return new FileResponseWriter(filePath);
        }
    }
}