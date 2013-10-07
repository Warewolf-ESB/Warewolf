namespace Unlimited.Framework.Plugins
{
    /// <summary>
    /// Used to test the nullToEmpty property of Plugins ;)
    /// </summary>
    public class EmailPlugin
    {

        public string SampleSend(string host, string port, string from, string to, string subject, string body)
        {
            if (from == null)
            {
                return "Anonymous email sent";
            }

            return "Email sent from " + from;
        }
    }
}
