using System.Net.Mail;

namespace Tu.Servers
{
    // Wrapper class for IEmailServer interface to facilitate testing!
    public class EmailServer : SmtpClient, IEmailServer
    {
    }
}