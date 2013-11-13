namespace Unlimited.Framework.Plugins
{
    /// <summary>
    /// Used to test the nullToEmpty property of Plugins ;)
    /// </summary>
    public class EmailPlugin
    {
        public string DummySent(object data)
        {
            return string.Format("<zzz>Email sent:{0}</zzz>", data);
        }
        public string SampleSend(string host, int port, string from, string to, string subject, string body)
        {
            if (from == null)
            {
                return "<result>Anonymous email sent</result>";
            }

            return "<result>Email sent from " + from+"</result>";
        }
    }
}
