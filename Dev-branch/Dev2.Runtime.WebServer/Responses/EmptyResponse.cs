namespace Unlimited.Applications.WebServer.Responses
{
    public class StatusCommunicationResponseWriter : CommunicationResponseWriter
    {
        private int _statusCode;

        public StatusCommunicationResponseWriter(int statusCode = 204)
        {
            _statusCode = statusCode;
        }

        public override void Write(ICommunicationContext context)
        {
            base.Write(context);
            context.Response.Status = (System.Net.HttpStatusCode)_statusCode;
        }
    }
}