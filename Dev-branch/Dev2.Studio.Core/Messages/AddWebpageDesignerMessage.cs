using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Messages
{
    public class AddWebpageDesignerMessage : IMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public AddWebpageDesignerMessage(IWebActivity webActivity)
        {
            WebActivity = webActivity;
        }

        public IWebActivity WebActivity { get; set; }
    }
}