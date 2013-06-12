using Dev2.Studio.Interfaces.Messages;

namespace Dev2.Studio.Core.Messages
{
    public class ShowHelpTabMessage:IMessage
    {
        public string HelpLink { get; set; }

        public ShowHelpTabMessage(string helpLink)
        {
            HelpLink = helpLink;
        }
    }
}