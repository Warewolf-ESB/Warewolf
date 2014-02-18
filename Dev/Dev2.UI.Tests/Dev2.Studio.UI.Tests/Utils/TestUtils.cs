using netDumbster.smtp;

namespace Dev2.Studio.UI.Tests.Utils
{
    public class TestUtils
    {
        public static SimpleSmtpServer StartEmailServer()
        {
            var server = SimpleSmtpServer.Start(25);
            return server;
        }

        public static void StopEmailServer(SimpleSmtpServer server)
        {
            if(server != null)
            {
                server.Stop();
            }
        }
    }
}
